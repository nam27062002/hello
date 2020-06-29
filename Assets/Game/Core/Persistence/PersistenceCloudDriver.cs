//#define MOCK_CLOUD_DISCONNECTION

using FGOL.Server;
using SimpleJSON;
using System;
using System.Collections.Generic;
public class PersistenceCloudDriver
{
    private enum ESyncStep
    {
        None,
        CheckingConnection,
        LoggingInServer,
        LoggingInSocial,
        MergingAccounts,
        GettingPersistence,
        SyncingPersistences
    };

    public enum ESyncMode
    {
        None,
        Lite,
        Full,
        UpToMerge // for merge/c with force a true
    };

    public enum EMergeState
    {
        None,
        Ok,
        Failed // An irrecoverable error arose when syncing with server. The user chose to keep playing without cloud save
    }

    private enum EState
    {
        NotLoggedIn,
        Syncing,
        LoggedIn
    };

    public PersistenceData Data { get; set; }

    private EState mState;
    private EState State
    {
        get { return mState; }
        set
        {
            mState = value;
            switch (mState)
            {
                case EState.NotLoggedIn:
                    Upload_IsAllowed = false;
                    break;
            }
        }
    }

    private PersistenceLocalDriver LocalDriver { get; set; }

    private bool mIsInSync;
    public bool IsInSync
    {
        get { return mIsInSync; }
        set
        {
            if (mIsInSync != value)
            {
                mIsInSync = value;

                if (mIsInSync)
                {
                    LatestSyncTime = GameServerManager.GetEstimatedServerTimeAsLong();
                }

                Messenger.Broadcast<bool>(MessengerEvents.PERSISTENCE_SYNC_CHANGED, mIsInSync);
            }
        }
    }

    /// <summary>
    /// Returns the time (milliseconds since 1970) at the latest successful synchronization
    /// </summary>
    public long LatestSyncTime { get; set; }    

    public PersistenceCloudDriver()
	{
		string dataName = PersistencePrefs.ActiveProfileName;        
		Data = new PersistenceData(dataName);      
		SocialPlatformManager.SharedInstance.SetOnSocialPlatformLogout(OnLogout);
		Reset();
	}

    public void Destroy()
    {        
    }

    private void Reset()
	{
		IsInSync = false;
		State = EState.NotLoggedIn;
        LatestSyncTime = 0;

        Syncer_Reset();
		Upload_Reset();

		ExtendedReset();
	}

	protected virtual void ExtendedReset() {}

	public void Setup(PersistenceLocalDriver localDriver)
	{
		LocalDriver = localDriver;	
	}

    public void Logout()
    {
        ExtendedLogout();		       
    }   

	private void OnLogout()
	{
		LocalDriver.Prefs_SocialWasLoggedInWhenQuit = false;		
	}

    protected virtual void ExtendedLogout()
    {
        SocialPlatformManager.SharedInstance.Logout();
    }

    public bool IsLoggedIn
    {
        get { return State == EState.LoggedIn; }
    }

    #region syncer
    private Stack<PersistenceSyncerRequest> m_syncerRequests;

    private PersistenceSyncerRequest Syncer_CurrentRequest
    {
        get
        {            
            return m_syncerRequests.Peek();
        }
    }

    private ESyncMode Syncer_Mode
    {
        get
        {
            return Syncer_CurrentRequest.Mode;
        }

        set
        {
            Syncer_CurrentRequest.Mode = value;
        }
    }

    public enum EErrorMode
    {
        Verbose,        
        OnlyMergeConflict,
        Silent
    };

    public enum EError
    {
        NoConnection,
        MergeConflict,
        Persistence
    }

    private static bool CanProcessError(EError error, EErrorMode errorMode)
    {
        bool returnValue = errorMode == EErrorMode.Verbose;
        if (errorMode == EErrorMode.OnlyMergeConflict)
        {
            returnValue = error == EError.MergeConflict;
        }

        return returnValue;
    }

    private EErrorMode Syncer_ErrorMode
    {
        get
        {
            return Syncer_CurrentRequest.ErrorMode;
        }

        set
        {
            Syncer_CurrentRequest.ErrorMode = value;
        }
    }

	protected bool Syncer_IsAppInit
    {
        get
        {
            return Syncer_CurrentRequest.IsAppInit;
        }

        set
        {
            Syncer_CurrentRequest.IsAppInit = value;
        }
    }

	public SocialUtils.EPlatform Syncer_PlatformId
    {
        get
        {
            return Syncer_CurrentRequest.PlatformId;
        }

        set
        {
            Syncer_CurrentRequest.PlatformId = value;
        }
    }

    private SocialPlatformManager.ELoginResult Syncer_LogInSocialResult
    {
        get
        {
            return Syncer_CurrentRequest.LogInSocialResult;
        }

        set
        {
            Syncer_CurrentRequest.LogInSocialResult = value;
        }
    }

    public Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> Syncer_OnSyncDone
    {
        get
        {
            return Syncer_CurrentRequest.OnSyncDone;
        }

        set
        {
            Syncer_CurrentRequest.OnSyncDone = value;
        }
    }

    /// <summary>
    /// When <c>true</c> the local user server Id is forced to be linked to the social user Id used for merging accounts
    /// </summary>
    private bool Syncer_ForceMerge
    {
        get
        {
            return Syncer_CurrentRequest.ForceMerge;
        }

        set
        {
            Syncer_CurrentRequest.ForceMerge = value;
        }
    }

    private ESyncStep mSyncerStep;
	private ESyncStep Syncer_Step 
	{ 
		get 
		{
			return mSyncerStep;
		}
		set 
		{            
            PersistenceFacade.Log("(SYNCER) CLOUD " + mSyncerStep.ToString() + " ->  " + value.ToString());

            mSyncerStep = value;

			switch (mSyncerStep)
			{
				case ESyncStep.CheckingConnection:
					Syncer_CheckConnection();
					break;

				case ESyncStep.LoggingInServer:
					Syncer_LogInServer();
					break;

				case ESyncStep.LoggingInSocial:
                    if (Syncer_Mode == ESyncMode.Lite)
                    {
                        Syncer_Step = ESyncStep.GettingPersistence;
                    }
                    else
                    {
                        Syncer_LogInSocial();
                    }					
					break;

                case ESyncStep.MergingAccounts:
                    if (Syncer_ForceMerge && Syncer_LogInSocialResult != SocialPlatformManager.ELoginResult.Ok)
                    {
                        Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorSyncing, PersistenceStates.ESyncResultDetail.NoLogInSocial);
                    }
                    else
                    {
                        Syncer_MergingAccounts();
                    }
                    break;

                case ESyncStep.GettingPersistence:
                    if (Syncer_Mode == ESyncMode.UpToMerge)
                    {
                        Syncer_PerformDone(PersistenceStates.ESyncResult.Ok, PersistenceStates.ESyncResultDetail.None);
                    }
                    else
                    {
                        Syncer_GetPersistence();
                    }
					break;

				case ESyncStep.SyncingPersistences:
					Syncer_SyncPersistences();
					break;
			}
		}
	}

	private PersistenceComparator Syncer_Comparator 
	{ 
		get 
		{
            return Syncer_CurrentRequest.Comparator;
		}
	}

    private bool Syncer_NeedsToShowCloudOverridenPopup
    {
        get
        {
            return Syncer_CurrentRequest.NeedsToShowCloudOverridenPopup;
        }

        set
        {
            Syncer_CurrentRequest.NeedsToShowCloudOverridenPopup = value;
        }
    }

    private float Syncer_Timer
    {
        get
        {
            return Syncer_CurrentRequest.Timer;
        }

        set
        {
            Syncer_CurrentRequest.Timer = value;
        }
    }

    private void Syncer_Reset()
	{
        if (m_syncerRequests == null)
        {
            m_syncerRequests = new Stack<PersistenceSyncerRequest>();
            m_syncerRequests.Push(new PersistenceSyncerRequest());
        }

        Syncer_CurrentRequest.Reset();        
		Syncer_Step = ESyncStep.None;		       
    }

    public void Sync_PopRequest()
    {
        if (m_syncerRequests.Count > 1)
        {
            m_syncerRequests.Pop();
        }
    }

	public void Sync(SocialUtils.EPlatform platformId, ESyncMode mode, EErrorMode errorMode, bool isAppInit,
                     Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onDone,
                     bool forceMerge = false, bool pushRequest = false)
	{        
        PersistenceFacade.Log("(SYNC) CLOUD STARTED...");

        if (pushRequest)
        {
            m_syncerRequests.Push(new PersistenceSyncerRequest());
        }
        else
        {
            Syncer_Reset();
        }

		Upload_IsAllowed = false;

		Syncer_OnSyncDone = onDone;
		Syncer_PlatformId = platformId;
        Syncer_Mode = mode;
        Syncer_ErrorMode = errorMode;
		Syncer_IsAppInit = isAppInit;
        Syncer_ForceMerge = forceMerge;
        State = EState.Syncing;

		Syncer_Step = ESyncStep.CheckingConnection;
	}

	public void Syncer_Discard()
	{
		if (State == EState.Syncing)
		{
            if (Syncer_Step == ESyncStep.LoggingInSocial)
            {
                SocialPlatformManager.SharedInstance.Login_Discard();
            }

			Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.Cancelled);
		}
	}

    private void Syncer_CheckConnection()
	{
		Action<bool> onDone = delegate(bool success)
		{
            if (success)
			{
				Syncer_Step = ESyncStep.LoggingInServer;
			}
			else
			{
				Syncer_ProcessNoConnectionError();
			}
		};

		Syncer_ExtendedCheckConnection(onDone);
	}

	private void Syncer_LogInServer()
	{
		Action<bool> onDone = delegate(bool success)
		{
			if (success)
			{                
                Syncer_Step = ESyncStep.LoggingInSocial;                
			}
			else
			{
                Syncer_ProcessNoConnectionError();
            }
		};

		Syncer_ExtendedLogInServer(onDone);
	}

	private void Syncer_LogInSocial()
	{
        Syncer_Timer = FeatureSettingsManager.instance.SocialPlatformLoginTimeout;
        Syncer_ExtendedLogInSocial(Syncer_OnLogInSocialDone);
	}

    private void Syncer_OnLogInSocialDone(SocialPlatformManager.ELoginResult result, string persistenceMerge)
    {
        if (mSyncerStep == ESyncStep.LoggingInSocial)
        {
            Syncer_LogInSocialResult = result;            
            switch (Syncer_LogInSocialResult)
            {
                case SocialPlatformManager.ELoginResult.Error:
                    Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.NoLogInSocial);
                    break;

                case SocialPlatformManager.ELoginResult.Ok:
                    Syncer_Step = ESyncStep.GettingPersistence;
                    break;

                case SocialPlatformManager.ELoginResult.MergeLocalOrOnlineAccount:
                case SocialPlatformManager.ELoginResult.MergeDifferentAccountWithProgress:
                case SocialPlatformManager.ELoginResult.MergeDifferentAccountWithoutProgress:                                         
                    // If the user has logged in to an explicit platform and implicit logging in fails then we just stop the logging in
                    // process and we mark the implicit merge as failed because we don't want to mess up with the user's progress, which
                    // is linked to the explicit platform chosen by the suer
                    if (SocialPlatformManager.SharedInstance.IsImplicit(Syncer_PlatformId) && LocalDriver.HasEverExplicitlyLoggedIn())
                    {
                        LocalDriver.Prefs_SocialImplicitMergeState = EMergeState.Failed;
                        Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.NoLogInSocial);
                    }
                    else if (CanProcessError(EError.MergeConflict, Syncer_ErrorMode))
                    {                      
                        Data.LoadFromString(persistenceMerge);
                        Syncer_Step = ESyncStep.MergingAccounts;
                    }
                    else
                    {
                        Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.NoLogInSocial);
                    }
                    break;
            }
        }        
    }

	private void Syncer_GetPersistence()	
	{
		Action<bool> onDone = delegate(bool success)
		{
			if (success)
			{
				Syncer_Step = ESyncStep.SyncingPersistences;
			}
			else
			{
				Syncer_ProcessSyncingPersistencesError();
			}
		};

		Syncer_ExtendedGetPersistence(onDone);
	}    

	private void Syncer_MergingAccounts()
	{
        if (SocialPlatformManager.SharedInstance.IsImplicit(Syncer_PlatformId) &&
            (Syncer_LogInSocialResult == SocialPlatformManager.ELoginResult.MergeLocalOrOnlineAccount ||
            Syncer_LogInSocialResult == SocialPlatformManager.ELoginResult.MergeDifferentAccountWithProgress))
        {
            Syncer_Comparator.Compare(LocalDriver.Data, Data);            

            // Automatic logging in to a social platform lets the user overwrite the server userId linked to the social user Id in order to use
            // the current server userId because it's not as risky as letting explicit social platform such as Facebook or SIWA do it
            PersistenceFacade.Popup_OpenImplicitMergeConflict(
                Syncer_PlatformId,
                Syncer_Comparator.GetLocalProgress() as PersistenceComparatorSystem,
                Syncer_Comparator.GetCloudProgress() as PersistenceComparatorSystem,
                Syncer_OnMergeConflictOverwriteSocialUserIdWithLocalServerId,
                Syncer_OnMergeConflictUseCloud
            );
        }
        else
        {
            // Uncomment to force MergeDifferentAccountWithoutProgress popup
            //Syncer_LogInSocialResult = SocialPlatformManager.ELoginResult.MergeDifferentAccountWithoutProgress;

            switch (Syncer_LogInSocialResult)
            {
                case SocialPlatformManager.ELoginResult.MergeLocalOrOnlineAccount:
                    {
                        Syncer_ProcessMerge();
                    }
                    break;

                case SocialPlatformManager.ELoginResult.MergeDifferentAccountWithProgress:
                case SocialPlatformManager.ELoginResult.MergeDifferentAccountWithoutProgress:
                    {
                        Action onConfirm = delegate ()
                        {
                            Syncer_OnMergeConflictUseCloud();
                        };

                        Action onCancel = delegate ()
                        {
                            SocialPlatformManager.SharedInstance.Logout();
                            Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.Cancelled);
                        };

                        PersistenceFacade.Popup_OpenMergeWithADifferentAccount(Syncer_PlatformId, onConfirm, onCancel);
                    }
                    break;               
            }
        }                
	}    

    private void Syncer_SyncPersistences()
    {
        PersistenceStates.ELoadState localState = LocalDriver.Data.LoadState;
        PersistenceStates.ELoadState cloudState = Data.LoadState;

        if (localState == PersistenceStates.ELoadState.OK && cloudState == PersistenceStates.ELoadState.OK)
        {            
            PersistenceFacade.Log("(Syncer_Sync) :: local:Ok Cloud:Ok");

            PersistenceStates.EConflictState conflictState = Syncer_Comparator.Compare(LocalDriver.Data, Data);

            // If local persistence has already been loaded in game then we need to change
            // UseLocal for RecommendCloud because we need the user to confirm, so we can reload
            // the game with the new persistence
            if (LocalDriver.IsLoadedInGame && conflictState == PersistenceStates.EConflictState.UseCloud)
            {
                conflictState = PersistenceStates.EConflictState.RecommendCloud;
            }

            // Uncomment to force User decision popup
            //conflictState = PersistenceStates.EConflictState.UserDecision;
            Syncer_ProcessConflictState(conflictState);
        }
        else if (localState == PersistenceStates.ELoadState.OK && cloudState == PersistenceStates.ELoadState.Corrupted)
        {            
            PersistenceFacade.Log("(Syncer_Sync) :: local:Ok Cloud:Corrupted");
            
            Action onContinue = delegate ()
            {                
                Syncer_ResolveConflict(PersistenceStates.EConflictResult.Dismissed);
            };

            Action onOverride = delegate ()
            {
                Syncer_NeedsToShowCloudOverridenPopup = true;
                Syncer_ProcessConflictState(PersistenceStates.EConflictState.UseLocal);
            };

            PersistenceFacade.Popup_OpenCloudCorrupted(Syncer_PlatformId, onContinue, onOverride);
        }
        else if (localState == PersistenceStates.ELoadState.Corrupted && cloudState == PersistenceStates.ELoadState.OK)
        {            
            PersistenceFacade.Log("(Syncer_Sync) :: local:Corrupted Cloud:Ok");

            Action onSyncWithCloud = delegate ()
            {
                // Since local persistence is corrupted we need to override it with cloud persistence 
                Syncer_ProcessConflictState(PersistenceStates.EConflictState.UseCloud);
            };            

            PersistenceFacade.Popups_OpenLoadLocalCorruptedButCloudOkError(onSyncWithCloud);            
        }
        else if (localState == PersistenceStates.ELoadState.Corrupted && cloudState == PersistenceStates.ELoadState.Corrupted)
        {            
            PersistenceFacade.Log("(Syncer_Sync) :: local:Corrupted Cloud:Corrupted");

            Action onReset = delegate ()
            {
                Action onResetDone = delegate()
                {
                    // Since cloud persistence is corrupted we need to override cloud persistence with local persistence after resetting it
                    // to the default persistence
                    Syncer_ProcessConflictState(PersistenceStates.EConflictState.UseLocal);
                };

                LocalDriver.OverrideWithDefault(onResetDone);
            };

            PersistenceFacade.Popup_OpenLocalAndCloudCorrupted(onReset);
        }
        else
        {            
            PersistenceFacade.Log("(Syncer_Sync) :: case not supported");

            Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorSyncing, PersistenceStates.ESyncResultDetail.None);
        }
    }

    private void Syncer_OnMergeConflictOverwriteSocialUserIdWithLocalServerId(bool success)
    {       
        if (success)
        {
            LocalDriver.Prefs_SocialImplicitMergeState = EMergeState.Ok;
            Syncer_PerformDone(PersistenceStates.ESyncResult.Ok, PersistenceStates.ESyncResultDetail.None);
        }
        else
        {
            LocalDriver.Prefs_SocialImplicitMergeState = EMergeState.Failed;
            Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.NoLogInSocial);
        }                
    }

    private void Syncer_ProcessMerge()
    {
        PersistenceStates.ELoadState localState = LocalDriver.Data.LoadState;
        PersistenceStates.ELoadState cloudState = Data.LoadState;
        
        Action onError = delegate ()
        {
            // Dismiss
            Syncer_OnMergeConflictUseLocal();

            // Forces the game to restart so the flow follows a known case
            ApplicationManager.instance.NeedsToRestartFlow = true;
        };


        if (localState == PersistenceStates.ELoadState.OK && cloudState == PersistenceStates.ELoadState.OK)
        {
            // Chooses between local and cloud
            PersistenceFacade.Popup_OpenMergeConflict(Syncer_PlatformId, Syncer_OnMergeConflictUseLocal, Syncer_OnMergeConflictUseCloud);
        }
        else if (localState == PersistenceStates.ELoadState.OK && cloudState == PersistenceStates.ELoadState.Corrupted)
        {
            // Notifies cloud is not an option so confirm to keep playing local
            PersistenceFacade.Popup_OpenMergeConflictCloudCorrupted(Syncer_PlatformId, Syncer_OnMergeConflictUseLocal);
        }
        else if (localState == PersistenceStates.ELoadState.Corrupted && cloudState == PersistenceStates.ELoadState.OK)
        {
            //PersistenceFacade.Popup_OpenMergeConflictLocalCorrupted(Syncer_OnMergeConflictUseCloud);

            // This case shouldn't happen because there are previous checks that should avoid it. Just in case a generic sync error popup is shown
            // and the game is reloaded to make it follow a know flow.             
            PersistenceFacade.Popup_OpenMergeConflictLocalCorrupted(Syncer_PlatformId, onError);
        }
        else if (localState == PersistenceStates.ELoadState.Corrupted && cloudState == PersistenceStates.ELoadState.Corrupted)
        {
            /*
            Action onConfirmReset = delegate ()
            {
                Action onResetDone = delegate ()
                {
                    Syncer_OnMergeConflictUseLocal();
                };

                LocalDriver.OverrideWithDefault(onResetDone);
            };
                        
            PersistenceFacade.Popup_OpenMergeConflictLocalCorrupted(onError);
            */

            // Neither local and cloud is an option so reset cloud
            // This case shouldn't happen because there are previous checks that should avoid it. Just in case a generic sync error popup is shown
            // and the game is reloaded to make it follow a known flow.             
            PersistenceFacade.Popup_OpenMergeConflictBothCorrupted(Syncer_PlatformId, onError);
        }
        else
        {                    
            PersistenceFacade.Log("(Syncer_Sync) :: case not supported");

            // Dismiss
            Syncer_OnMergeConflictUseLocal();
        }
    }

    private void Syncer_OnMergeConflictUseLocal()
    {
        // Merge is solved with local persistence which makes the game log out from social because the social account chosen is linked to a different user
        // and the user has refused to override her account with the one linked to that social account
        GameSessionManager.SharedInstance.MergeConfirmAfterPopup(false, true);
        Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.Cancelled);
    }

    private void Syncer_OnMergeConflictUseCloud()
    {        
        PersistenceFacade.Log("(SYNCER) MERGE WITH CLOUD!!! Syncer_LogInSocialResult = " + Syncer_LogInSocialResult + " CloudData = " + ((Data == null) ? null : Data.ToString()));

        // Calety is called to overwrite the anonymous id so the game will log in server with the right account Id when reloading	
        switch (Syncer_LogInSocialResult)
        {
            case SocialPlatformManager.ELoginResult.MergeLocalOrOnlineAccount:
                GameSessionManager.SharedInstance.MergeConfirmAfterPopup(true);                
                break;

            case SocialPlatformManager.ELoginResult.MergeDifferentAccountWithProgress:
                GameSessionManager.SharedInstance.SetAnonymousPlatformUserIDFromMergeOnlineAccount();
                break;

            case SocialPlatformManager.ELoginResult.MergeDifferentAccountWithoutProgress:
                GameSessionManager.SharedInstance.ResetAnonymousPlatformUserID();
                break;

            default:                
                PersistenceFacade.LogWarning("No Syncer_LogInSocialResult " + Syncer_LogInSocialResult + " supported");
                break;
        }

        // Forces to log out from server since we're about to reload and we want to log in with the anonymous id that we've just overridden
        GameServerManager.SharedInstance.LogOut();

        // PersistencePrefs are deleted since it has to be overridden by the remote account id
        PersistencePrefs.Clear();

        // Cache is invalidated in order to make sure that the new account's information will be requested
        SocialPlatformManager.SharedInstance.CurrentPlatform_InvalidateCachedSocialInfo();

        Action onReset = delegate ()
        {
            Syncer_PerformDone(PersistenceStates.ESyncResult.NeedsToReload, PersistenceStates.ESyncResultDetail.None);
        };

        LocalDriver.Override(Data.ToString(), onReset);
    }

	private void Syncer_ProcessConflictState(PersistenceStates.EConflictState conflict)
	{		
    	PersistenceFacade.Log("(ProcessConflictState :: " + conflict);

		switch (conflict)
		{
			case PersistenceStates.EConflictState.UserDecision:
			case PersistenceStates.EConflictState.RecommendCloud:
			case PersistenceStates.EConflictState.RecommendLocal:				
				// If the user is solving a merge then the decission can't be dismiss				
				PersistenceFacade.Popups_OpenSyncConflict(conflict, Syncer_Comparator.GetLocalProgress() as PersistenceComparatorSystem, 
			                                   Syncer_Comparator.GetCloudProgress() as PersistenceComparatorSystem, true, 
			                                   Syncer_ResolveConflict);
			break;

			case PersistenceStates.EConflictState.Equal:
				Syncer_ResolveConflict(PersistenceStates.EConflictResult.Dismissed);
			break;

			case PersistenceStates.EConflictState.UseLocal:
				Syncer_ResolveConflict(PersistenceStates.EConflictResult.Local);
			break;

			case PersistenceStates.EConflictState.UseCloud:
				Syncer_ResolveConflict(PersistenceStates.EConflictResult.Cloud);
			break;

			default:
				Syncer_ResolveConflict(PersistenceStates.EConflictResult.Dismissed);
			break;
		}
	}

	private void Syncer_ResolveConflict(PersistenceStates.EConflictResult result)
    {
        switch(result)
        {
			// Overrides local persistence with cloud persistence
            case PersistenceStates.EConflictResult.Cloud:				
                PersistenceFacade.Log("(ResolveConflict) :: Resolving conflict with cloud save!");
												
				Action onDone = delegate() 
				{
					PersistenceStates.ESyncResult syncResult = (result == PersistenceStates.EConflictResult.Cloud) ? PersistenceStates.ESyncResult.NeedsToReload : PersistenceStates.ESyncResult.Ok;
					Syncer_PerformDone(syncResult, PersistenceStates.ESyncResultDetail.None);
				};
								
				LocalDriver.Override(Data.ToString(), onDone);							               
                break;

            case PersistenceStates.EConflictResult.Local:				
				Syncer_UploadLocalToCloud();				
                break;

			default:				
				Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorSyncing, PersistenceStates.ESyncResultDetail.None);
			break;
        }
    }

	private void Syncer_UploadLocalToCloud()
	{		
	    PersistenceFacade.Log("(ResolveConflict) :: Resolving conflict with local save!");
				
		Action<bool> onUploadDone = delegate(bool success)
		{
			if (success)
			{
                Action onDone = delegate ()
                {
                    Syncer_PerformDone(PersistenceStates.ESyncResult.Ok, PersistenceStates.ESyncResultDetail.None);
                };

                if (Syncer_NeedsToShowCloudOverridenPopup)
                {
                    Syncer_NeedsToShowCloudOverridenPopup = false;
                    PersistenceFacade.Popup_OpenCloudCorruptedWasOverriden(onDone);
                }
                else
                {
                    onDone();
                }				
			}
			else
			{				
				PersistenceFacade.LogWarning("(ResolveConflict) :: Upload failed");

				Action onRetry = delegate()
				{
					Syncer_UploadLocalToCloud();
				};

				Action onContinue = delegate()
				{
					Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorSyncing, PersistenceStates.ESyncResultDetail.None);
				};

				PersistenceFacade.Popup_OpenErrorWhenSyncing(onContinue, onRetry);
			}
		};

		Upload_Internal(onUploadDone);				
	}

	private void Syncer_PerformDone(PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail)
	{        
        PersistenceFacade.Log("(SYNCER) CLOUD DONE result: " + result.ToString() + " resultDetail: " + resultDetail);

        if (Syncer_Mode == ESyncMode.Full || Syncer_Mode == ESyncMode.UpToMerge)
        {
            State = (result == PersistenceStates.ESyncResult.Ok || result == PersistenceStates.ESyncResult.NeedsToReload) ? EState.LoggedIn : EState.NotLoggedIn;
        }
        else
        {
            // Cloud save is enabled if it was possible to connect to server
            if (result == PersistenceStates.ESyncResult.ErrorLogging &&
                (resultDetail == PersistenceStates.ESyncResultDetail.NoConnection || resultDetail == PersistenceStates.ESyncResultDetail.NoLogInServer))
            {
                State = EState.NotLoggedIn;
            }
            else
            {
                State = EState.LoggedIn;
            }
        }

        // Uploading is allowed simply by logging in to the server in order to maximise the user's chances of having a backup
        Upload_IsAllowed = State == EState.LoggedIn;
        IsInSync = Upload_IsAllowed;
        if (IsInSync)
        {
            LocalDriver.UpdatesAheadOfCloud = 0;
        }

        Action onDone = delegate() 
		{
            // We need to call Syncer_Reset() before calling onSyncDone because onSyncDone could call Sync() again which will set a new value to Syncer_OnSyncDone,
            // will would be reseted if Syncer_Reset() was called after calling onSyncDone
            // Syncer_OnSyncDone from being lost            
            Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onSyncDone = Syncer_OnSyncDone;
            Syncer_Reset();

            if (onSyncDone != null)
			{
                onSyncDone(result, resultDetail);
			}							
		};
        
        // If the sync is ok we need to process the new social state (reward for logging in)
        if (State == EState.LoggedIn && resultDetail == PersistenceStates.ESyncResultDetail.None && Syncer_Mode == ESyncMode.Full)
		{
			Action onUserLoggedIn = delegate()
			{
				LocalDriver.ProcessSocialState(onDone);                
			};
            
            HDTrackingManager.Instance.Notify_SocialAuthentication();

            // Logging in to an implicit platform is not considered in terms of logging in (no reward is given to the user) because
            // it hasn't been triggered by the user
            if (SocialPlatformManager.SharedInstance.IsImplicit(Syncer_PlatformId))
            {
                LocalDriver.Prefs_SocialImplicitMergeState = EMergeState.Ok;
                onDone();
            }
            else
            {
                SocialUtils.EPlatform platformId = Syncer_PlatformId;
                string currentSocialPlatformKey = SocialUtils.EPlatformToKey(platformId);
                string socialId = SocialPlatformManager.SharedInstance.GetUserID(platformId);
                LocalDriver.NotifyUserHasLoggedIn(currentSocialPlatformKey, socialId, onUserLoggedIn);
            }
		} 
		else
		{
			onDone();
		}
	}

	private void Syncer_ProcessNoConnectionError()
	{
		Action onDone = delegate() 
		{
			Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.NoConnection);
		};

		if (CanProcessError(EError.NoConnection, Syncer_ErrorMode))
		{
            PersistenceFacade.Popups_OpenErrorConnection(Syncer_PlatformId, onDone);            
		}
		else
		{
            onDone();
        }
	}

	private void Syncer_ProcessSyncingPersistencesError()
	{
		Action onDone = delegate() 
		{
			Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorSyncing, PersistenceStates.ESyncResultDetail.None);
		};

		Action onRetry = delegate() 
		{
			Syncer_Step = ESyncStep.GettingPersistence;
		};

		if (CanProcessError(EError.Persistence, Syncer_ErrorMode))
		{
            PersistenceFacade.Popup_OpenErrorWhenSyncing(onDone, onRetry);            
		}
		else
		{
            onDone();
        }
	}

	protected virtual void Syncer_ExtendedCheckConnection(Action<bool> onDone)
    {
        Action<Error> onCheckDone = delegate (Error error)
        {
            if (onDone != null)
            {
                onDone(error == null);
            }
        };

        GameServerManager.SharedInstance.CheckConnection(onCheckDone);        
    }

	protected virtual void Syncer_ExtendedLogInServer(Action<bool> onDone)
    {
        GameServerManager.SharedInstance.Auth((Error error, GameServerManager.ServerResponse response) => 
        {
            if (onDone != null)
            {
                onDone(GameServerManager.SharedInstance.IsLoggedIn());                
            }
        });
    }

	protected virtual void Syncer_ExtendedLogInSocial(Action<SocialPlatformManager.ELoginResult, string> onDone)
    {
        SocialPlatformManager.SharedInstance.Login(Syncer_PlatformId, Syncer_ErrorMode != EErrorMode.Verbose, Syncer_IsAppInit, onDone, Syncer_ForceMerge);        
    }

	protected virtual void Syncer_ExtendedGetPersistence (Action<bool> onDone)
    {
        GameServerManager.SharedInstance.GetPersistence((Error error, GameServerManager.ServerResponse response) =>
        {
            bool success = error == null;
            string persistence = null;

            if (success)
            {
                persistence = response["response"] as string;
                Data.LoadFromString(persistence);
            }

            if (onDone != null)
            {
                onDone(success);
            }
        });
    }
    #endregion
    
    #region upload
    private bool Upload_IsRunning { get; set; }
	public bool Upload_IsAllowed { get; set; }

    public bool Upload_IsEnabled
    {
        get
        {
			//return PersistencePrefs.IsCloudSaveEnabled;
			return true;	// As of 2.10, cloud save is always enabled (players no longer have the toggle to disable it)
        }

        set
        {
            PersistencePrefs.IsCloudSaveEnabled = value;
        }
    }

	private const float UPLOAD_TIME_BETWEEN_PERFORMS = 30f;
	private float Upload_TimeLeftToPerform { get; set; }

	private void Upload_Reset()
	{
		Upload_IsRunning = false;
		Upload_IsAllowed = false;
		Upload_TimeLeftToPerform = 0f;
	}

	public void Upload()
	{
		// Makes sure no upload is already running
		if (!Upload_IsRunning)
		{
			Upload_Internal(null);
		}
	}

	private void Upload_Internal(Action<bool> onDone)
	{
		int updatesAhead = LocalDriver.UpdatesAheadOfCloud;
		Action<bool> onUploadDone = delegate(bool success)
		{            
			IsInSync = success;
			Upload_IsRunning = false;

			if (success)
			{
				LocalDriver.UpdatesAheadOfCloud -= updatesAhead;
				
				PersistenceFacade.Log("Upload load persistence completed (" + LocalDriver.UpdatesAheadOfCloud + ")");

				if (LocalDriver.UpdatesAheadOfCloud < 0)
				{
					LocalDriver.UpdatesAheadOfCloud = 0;
					PersistenceFacade.LogWarning("Amount of updates doesn't make sense");
				}
			}
			else
			{				
				PersistenceFacade.LogWarning("Upload failed");
			}

			if (onDone != null)
			{
				onDone(success);
			}
		};
		
		PersistenceFacade.Log("Uploading local persistence.... (" + updatesAhead + ")");

		Upload_Perform(LocalDriver.Data.ToString(), onUploadDone);
	}

	protected virtual void Upload_Perform(string persistence, Action<bool> onDone)
    {
        GameServerManager.SharedInstance.SetPersistence(persistence, (Error error, GameServerManager.ServerResponse response) =>
        {           
            if (onDone != null)
            {
                onDone(error == null);
            }
        });
    }
	#endregion

	public void Update()
	{     
        switch (mSyncerStep)
        {
		case ESyncStep.LoggingInSocial:

			// Checks for timeout after calling the social network so we don't depend on the social network, 
			// in particular this approach lets us address HDK-1574 and HDK-2590          
			if (SocialPlatformManager.SharedInstance.IsLogInTimeoutEnabled(Syncer_PlatformId)) 
			{				
				Syncer_Timer -= Math.Min (UnityEngine.Time.deltaTime, UnityEngine.Time.maximumDeltaTime);
				if (Syncer_Timer <= 0f) 
				{
					SocialPlatformManager.SharedInstance.OnLogInTimeout(Syncer_PlatformId);
					Syncer_OnLogInSocialDone(SocialPlatformManager.ELoginResult.Error, null);
				}					
			}
            break;
        }   

		// Upload local to cloud
		if (Upload_IsAllowed && Upload_IsEnabled && !Upload_IsRunning && LocalDriver.UpdatesAheadOfCloud > 0)
		{
			Upload_TimeLeftToPerform -= UnityEngine.Time.deltaTime;
			if (Upload_TimeLeftToPerform <= 0f)
			{		
				Upload_TimeLeftToPerform = UPLOAD_TIME_BETWEEN_PERFORMS;

				// Makes sure it's not corrupted
				if (LocalDriver.Data.LoadState == PersistenceStates.ELoadState.OK)
				{
					Upload();
				}
				else
				{					
				    PersistenceFacade.LogError("Not valid data. It won't be uploaded");
				}
			}
		}
	}	
}
