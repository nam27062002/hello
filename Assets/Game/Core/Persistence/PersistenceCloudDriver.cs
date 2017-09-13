using FGOL.Server;
using SimpleJSON;
using System;
public class PersistenceCloudDriver
{
	private enum ESyncSetp
	{
		None,
		CheckingConnection,
		LoggingInServer,
		LoggingInSocial,
		GettingPersistence,
		Syncing
	};	

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

	public bool IsInSync { get; set; }
    
	public PersistenceCloudDriver()
	{
		string dataName = PersistencePrefs.ActiveProfileName;        
		Data = new PersistenceData(dataName);        
		Reset();
	}

    public void Destroy()
    {        
    }

    private void Reset()
	{
		IsInSync = false;
		State = EState.NotLoggedIn;

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

        if (State == EState.LoggedIn)
        {
            // Logs out
            State = EState.NotLoggedIn;
            IsInSync = false;
        }
    }        

    protected virtual void ExtendedLogout()
    {
        SocialPlatformManager.SharedInstance.Logout();
    }

    #region syncer
    private bool Syncer_IsSilent { get; set; }
	private bool Syncer_IsAppInit { get; set; }
	private SocialPlatformManager.ELoginResult Syncer_LogInSocialResult { get; set; }
	private Action<PersistenceStates.ESyncResult> Syncer_OnSyncDone { get; set; }

	private ESyncSetp mSyncerStep;
	private ESyncSetp Syncer_Step 
	{ 
		get 
		{
			return mSyncerStep;
		}
		set 
		{
			mSyncerStep = value;

			switch (mSyncerStep)
			{
				case ESyncSetp.CheckingConnection:
					Syncer_CheckConnection();
					break;

				case ESyncSetp.LoggingInServer:
					Syncer_LogInServer();
					break;

				case ESyncSetp.LoggingInSocial:
					Syncer_LogInSocial();
					break;

				case ESyncSetp.GettingPersistence:
					Syncer_GetPersistence();
					break;

				case ESyncSetp.Syncing:
					Syncer_Sync();
					break;
			}
		}
	}

	private PersistenceComparator mSyncerComparator;
	private PersistenceComparator Syncer_Comparator 
	{ 
		get 
		{
			if (mSyncerComparator == null)
			{
				mSyncerComparator = new HDPersistenceComparator();
			}

			return mSyncerComparator;
		}
	}

	private void Syncer_Reset()
	{
		Syncer_Step = ESyncSetp.None;
		Syncer_IsAppInit = false;
		Syncer_IsSilent = false;
		Syncer_OnSyncDone = null;
		Syncer_LogInSocialResult = SocialPlatformManager.ELoginResult.Error;
	}

	public void Sync(bool isSilent, bool isAppInit, Action<PersistenceStates.ESyncResult> onDone)
	{
		Syncer_Reset();
		Upload_IsAllowed = false;

		Syncer_OnSyncDone = onDone;
		Syncer_IsSilent = isSilent;
		Syncer_IsAppInit = isAppInit;
		State = EState.Syncing;

		Syncer_Step = ESyncSetp.CheckingConnection;
	}

	public void Syncer_Discard()
	{
		if (State == EState.Syncing)
		{
            if (Syncer_Step == ESyncSetp.LoggingInSocial)
            {
                SocialPlatformManager.SharedInstance.Login_Discard();
            }

			Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging);
		}
	}

	private void Syncer_CheckConnection()
	{
		Action<bool> onDone = delegate(bool success)
		{
			if (success)
			{
				Syncer_Step = ESyncSetp.LoggingInServer;
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
				Syncer_Step = ESyncSetp.LoggingInSocial;
			}
			else
			{
				Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging);
			}
		};

		Syncer_ExtendedLogInServer(onDone);
	}

	private void Syncer_LogInSocial()
	{
		Action<SocialPlatformManager.ELoginResult, string> onDone = delegate(SocialPlatformManager.ELoginResult result, string persistenceMerge)
		{
			Syncer_LogInSocialResult = result;
			switch (Syncer_LogInSocialResult)
			{
				case SocialPlatformManager.ELoginResult.Error:
					Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging);
					break;

				case SocialPlatformManager.ELoginResult.Ok:
					Syncer_Step = ESyncSetp.GettingPersistence;
					break;

				case SocialPlatformManager.ELoginResult.NeedsToMerge:                    
                    Data.LoadFromString(persistenceMerge);                    
                    Syncer_Step = ESyncSetp.Syncing;
					break;
			}
		};

		Syncer_ExtendedLogInSocial(onDone);
	}

	private void Syncer_GetPersistence()	
	{
		Action<bool> onDone = delegate(bool success)
		{
			if (success)
			{
				Syncer_Step = ESyncSetp.Syncing;
			}
			else
			{
				Syncer_ProcessSyncingError();
			}
		};

		Syncer_ExtendedGetPersistence(onDone);
	}

	private void Syncer_Sync()
	{
		PersistenceStates.ELoadState localState = LocalDriver.Data.LoadState;
		PersistenceStates.ELoadState cloudState = Data.LoadState;
		if (localState == PersistenceStates.ELoadState.OK && cloudState == PersistenceStates.ELoadState.OK)
		{
			if (FeatureSettingsManager.IsDebugEnabled)
				PersistenceFacade.Log("(Syncer_Sync) :: local:Ok Cloud:Ok");

			PersistenceStates.EConflictState conflictState = Syncer_Comparator.Compare(LocalDriver.Data, Data);

			// If local persistence has already been loaded in game then we need to change
			// UseLocal for RecommendCloud because we need the user to confirm, so we can reload
			// the game with the new persistence
			if (LocalDriver.IsLoadedInGame && conflictState == PersistenceStates.EConflictState.UseCloud)
			{
				conflictState = PersistenceStates.EConflictState.RecommendCloud;
			} 
			else if (Syncer_LogInSocialResult == SocialPlatformManager.ELoginResult.NeedsToMerge)
			{
				// If we need to merge with social account then we need the user
				// to choose what to do explicitly, so we recommend cloud if 
				// cloud persistence is ahead, otherwise we let the user choose.
				// Anyway it's recommendable to use the cloud because when choosing
				// local the user actually logs out so she won't have cloud save
				if (conflictState == PersistenceStates.EConflictState.UseCloud || 
					conflictState == PersistenceStates.EConflictState.RecommendCloud)
				{
					conflictState = PersistenceStates.EConflictState.RecommendCloud;
				} 
				else
				{
					conflictState = PersistenceStates.EConflictState.UserDecision;
				}
			}

			Syncer_ProcessConflictState(conflictState);
		} 
		else if (localState == PersistenceStates.ELoadState.OK && cloudState == PersistenceStates.ELoadState.Corrupted)
		{
			if (FeatureSettingsManager.IsDebugEnabled)
				PersistenceFacade.Log("(Syncer_Sync) :: local:Ok Cloud:Corrupted");

			// When merging the user doesn't have access to the cloud persistence unless its platform
			// id gets overriden by the one to be able to access to the cloud, so this user is not allowed
			// to correct cloud persistence
			bool canOverride = Syncer_LogInSocialResult != SocialPlatformManager.ELoginResult.NeedsToMerge;

			Action onContinue = delegate() 
			{
				PersistenceStates.EConflictResult result = (canOverride) ? PersistenceStates.EConflictResult.Dismissed : PersistenceStates.EConflictResult.Local;
				Syncer_ResolveConflict(result);
			};

			Action onOverride = delegate() 
			{
				Syncer_ProcessConflictState(PersistenceStates.EConflictState.UseLocal);
			};

			PersistenceFacade.Popup_OpenCloudCorrupted(canOverride, onContinue, onOverride);			
		} 
		else if (localState == PersistenceStates.ELoadState.Corrupted && cloudState == PersistenceStates.ELoadState.OK)
		{
			if (FeatureSettingsManager.IsDebugEnabled)
				PersistenceFacade.Log("(Syncer_Sync) :: local:Corrupted Cloud:Ok");

			Syncer_ProcessConflictState(PersistenceStates.EConflictState.UseCloud);
		} 
		else if (localState == PersistenceStates.ELoadState.Corrupted && cloudState == PersistenceStates.ELoadState.Corrupted)
		{
			if (FeatureSettingsManager.IsDebugEnabled)
				PersistenceFacade.Log("(Syncer_Sync) :: local:Corrupted Cloud:Corrupted");

			Action onReset = delegate() 
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
			if (FeatureSettingsManager.IsDebugEnabled)
				PersistenceFacade.Log("(Syncer_Sync) :: case not supported");

			Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging);
		}
	}    

	private void Syncer_ProcessConflictState(PersistenceStates.EConflictState conflict)
	{
		if (FeatureSettingsManager.IsDebugEnabled)
			PersistenceFacade.Log("(ProcessConflictState :: " + conflict);

		switch (conflict)
		{
			case PersistenceStates.EConflictState.UserDecision:
			case PersistenceStates.EConflictState.RecommendCloud:
			case PersistenceStates.EConflictState.RecommendLocal:				
				// If the user is solving a merge then the decission can't be dismiss
				bool isDismissable = Syncer_LogInSocialResult != SocialPlatformManager.ELoginResult.NeedsToMerge;
				PersistenceFacade.Popups_OpenMerge(conflict, Syncer_Comparator.GetLocalProgress() as PersistenceComparatorSystem, 
			                                   Syncer_Comparator.GetCloudProgress() as PersistenceComparatorSystem, isDismissable, 
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
				if (FeatureSettingsManager.IsDebugEnabled)
                	PersistenceFacade.Log("(ResolveConflict) :: Resolving conflict with cloud save!");
								
				if (Syncer_LogInSocialResult == SocialPlatformManager.ELoginResult.NeedsToMerge)
				{
                    // Calety is called to override the anonymous id so the game will log in server with the right account Id when reloading	
                    GameSessionManager.SharedInstance.MergeConfirmAfterPopup(true);

                    // Forces to log out from server since we're about to reload and we want to log in with the anonymous id that we've just overridden
                    GameSessionManager.SharedInstance.LogOutFromServer(false);

                    // PersistencePrefs are deleted since it has to be overridden by the remove account id
                    PersistencePrefs.Clear();
                }

				Action onDone = delegate() 
				{
					PersistenceStates.ESyncResult syncResult = (result == PersistenceStates.EConflictResult.Cloud) ? PersistenceStates.ESyncResult.NeedsToReload : PersistenceStates.ESyncResult.Ok;
					Syncer_PerformDone(syncResult);
				};
								
				LocalDriver.Override(Data.ToString(), onDone);							               
                break;

            case PersistenceStates.EConflictResult.Local:
				if (Syncer_LogInSocialResult == SocialPlatformManager.ELoginResult.NeedsToMerge)
				{
                    // Merge is solved with local persistence which makes the game log out from social because the social account chosen is linked to a different user
                    // and the user has refused to override her account with the one linked to that social account
                    GameSessionManager.SharedInstance.MergeConfirmAfterPopup(false, true);
                    Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging);                    
                }
				else
				{
					Syncer_UploadLocalToCloud();
				}
                break;

			default:				
				Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorSyncing);
			break;
        }
    }

	private void Syncer_UploadLocalToCloud()
	{
		if (FeatureSettingsManager.IsDebugEnabled)
			PersistenceFacade.Log("(ResolveConflict) :: Resolving conflict with local save!");
				
		Action<bool> onUploadDone = delegate(bool success)
		{
			if (success)
			{
				Syncer_PerformDone(PersistenceStates.ESyncResult.Ok);
			}
			else
			{
				if (FeatureSettingsManager.IsDebugEnabled)
					PersistenceFacade.LogWarning("(ResolveConflict) :: Upload failed");

				Action onRetry = delegate()
				{
					Syncer_UploadLocalToCloud();
				};

				Action onContinue = delegate()
				{
					Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorSyncing);
				};

				PersistenceFacade.Popup_OpenErrorWhenSyncing(onContinue, onRetry);
			}
		};

		Upload_Internal(onUploadDone);				
	}

	private void Syncer_PerformDone(PersistenceStates.ESyncResult result)
	{	
		Upload_IsAllowed = result == PersistenceStates.ESyncResult.Ok;
		IsInSync = Upload_IsAllowed;
		if (IsInSync)
		{
			LocalDriver.UpdatesAheadOfCloud = 0;
		}

		if (result == PersistenceStates.ESyncResult.ErrorLogging)
		{
			State = EState.NotLoggedIn;
		} 
		else
		{
			State = EState.LoggedIn;
		}

		Action onDone = delegate() 
		{
			if (Syncer_OnSyncDone != null)
			{
				Syncer_OnSyncDone (result);
			}

			Syncer_Reset ();				
		};

		// If the sync is ok we need to process the new social state (reward for logging in)
		if (State == EState.LoggedIn)
		{
			Action onUserLoggedIn = delegate()
			{
				LocalDriver.ProcessSocialState(onDone);
			};

			LocalDriver.NotifyUserHasLoggedIn(onUserLoggedIn);
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
			Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorLogging);
		};

		if (Syncer_IsSilent)
		{
			onDone();
		}
		else
		{
			PersistenceFacade.Popups_OpenErrorConnection(onDone);
		}
	}

	private void Syncer_ProcessSyncingError()
	{
		Action onDone = delegate() 
		{
			Syncer_PerformDone(PersistenceStates.ESyncResult.ErrorSyncing);
		};

		Action onRetry = delegate() 
		{
			Syncer_Step = ESyncSetp.GettingPersistence;
		};

		if (Syncer_IsSilent)
		{
			onDone();
		}
		else
		{
			PersistenceFacade.Popup_OpenErrorWhenSyncing(onDone, onRetry);
		}
	}

	protected virtual void Syncer_ExtendedCheckConnection(Action<bool> onDone)
    {        
        GameServerManager.SharedInstance.CheckConnection((Error error, GameServerManager.ServerResponse response) => 
        {
            if (onDone != null)
            {
                onDone(error == null);
            }
        });
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
        SocialPlatformManager.SharedInstance.Login(Syncer_IsSilent, Syncer_IsAppInit, onDone);        
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
	private bool Upload_IsAllowed { get; set; }

    public bool Upload_IsEnabled
    {
        get
        {
            return PersistencePrefs.IsCloudSaveEnabled;
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

				if (FeatureSettingsManager.IsDebugEnabled)
					PersistenceFacade.Log("Upload load persistence complete (" + LocalDriver.UpdatesAheadOfCloud + ")");

				if (LocalDriver.UpdatesAheadOfCloud < 0)
				{
					LocalDriver.UpdatesAheadOfCloud = 0;
					if (FeatureSettingsManager.IsDebugEnabled)
						PersistenceFacade.LogWarning("Amount of updates doesn't make sense");
				}
			}
			else
			{
				if (FeatureSettingsManager.IsDebugEnabled)
					PersistenceFacade.LogWarning("Upload failed");
			}

			if (onDone != null)
			{
				onDone(success);
			}
		};

		if (FeatureSettingsManager.IsDebugEnabled)
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
					if (FeatureSettingsManager.IsDebugEnabled)
						PersistenceFacade.LogError("Not valid data. It won't be uploaded");
				}
			}
		}
	}	
}
