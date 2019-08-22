#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using System;
using System.Diagnostics;
using System.Globalization;

public class PersistenceFacade : IBroadcastListener
{	
	public static readonly CultureInfo JSON_FORMATTING_CULTURE = CultureInfo.InvariantCulture;
	
	private static PersistenceFacade smInstance;
	public static PersistenceFacade instance 
	{
		get 
		{
			if (smInstance == null)
			{
				smInstance = new PersistenceFacade();
				smInstance.Init();
			}

			return smInstance;
		}
	}

    private PersistenceFacadeConfig Config { get; set; }   
	
	// Makes sure it's called when using UbiBCN.SingletonMonoBehaviour
	// Call Reset insted of Init from outside
	private void Init()
	{
		// Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
		// From http://answers.unity3d.com/questions/30930/why-did-my-binaryserialzer-stop-working.html
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");

        PersistenceFacadeConfigDebug.EUserCaseId userCaseId = PersistenceFacadeConfigDebug.EUserCaseId.Production;
        //userCaseId = PersistenceFacadeConfigDebug.EUserCaseId.Settings_Local_NeverLoggedIn_Cloud_More;
        if (FeatureSettingsManager.IsDebugEnabled && userCaseId != PersistenceFacadeConfigDebug.EUserCaseId.Production)
        {
            Config = new PersistenceFacadeConfigDebug(userCaseId);
        }
        else
        {
            Config = new PersistenceFacadeConfig();
        }

		Popups_Init();

		GameServerManager.SharedInstance.Configure();

        // Tries to log in as soon as possible so the chances to have online stuff such as customizer (which may contain offers) ready when main menu is loaded are higher
        GameServerManager.SharedInstance.Auth(null);        
	}

	public void Destroy()
	{
		Popups_Destroy();
        if (Config != null)
        {
            Config.Destroy();
        }
	}

    public void Reset()
    {     
		Local_Reset();
		Cloud_Reset();
        Sync_Reset();        
    }    
    	
    public void Update()
    {
        PersistencePrefs.Update();

		Config.LocalDriver.Update();
		Config.CloudDriver.Update();    
	}

    public bool IsCloudSaveAllowed
    {
        get { return CloudDriver.Upload_IsAllowed; }
    }

    public bool IsCloudSaveEnabled
    {
        get { return CloudDriver.Upload_IsEnabled; }
        set { CloudDriver.Upload_IsEnabled = value; }
    }   

	#region sync
    public long Sync_LatestSyncTime { get { return CloudDriver.LatestSyncTime; } }
   
    public bool Sync_IsSyncing { get; set; }

    public bool Sync_IsSynced { get { return CloudDriver.IsInSync; } }

    private void Sync_Reset()
    {
        Sync_IsSyncing = false;
    }

    public void Sync_FromLaunchApplication(Action onDone)
	{
        Sync_IsSyncing = true;

        Action onLoadDone = delegate()
		{            
            Log("SYNC: Loading  local DONE! " + LocalData.LoadState);           

			// If local persistence is corrupted then we'll try to override it with cloud persistence if the user has ever logged in the social network
			if (LocalData.LoadState == PersistenceStates.ELoadState.Corrupted)
			{				
				bool logInSocialEver = !string.IsNullOrEmpty(LocalDriver.Prefs_SocialId);

				Action onReset = delegate()
				{
					Action onResetDone = delegate()
					{
						Sync_FromLaunchApplication(onDone);
					};

					LocalDriver.OverrideWithDefault(onResetDone);
				};

                Action onRetry = delegate ()
                {
                    Sync_FromLaunchApplication(onDone);
                };			

				if (logInSocialEver)
				{
					Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onConnectDone = delegate(PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail)
					{
						if (result == PersistenceStates.ESyncResult.ErrorLogging)
						{
                            // Error when accessing to cloud so a popup is prompted asking the user if she wants to try to connect to cloud again
                            // or if she prefers to override local persistence with the default one
                            Popups_OpenLoadLocalCorruptedNoAccessToCloudError(onReset, onRetry);                            
						}                        
						else
                        {
                            Config.LocalDriver.IsLoadedInGame = true;
                            Sync_OnDone(result, onDone);                            
						}
					};

                    Config.CloudDriver.Sync(false, true, onConnectDone);                    
				}
                else
                {
                    // Lets the user know that local persistence is corrupted and asks permission to reset it to the default one                    
                    Popups_OpenLoadLocalCorruptedError(onReset);
                }                
			}
			else
			{                
                Log("Ready lo load local persistence in game ");

                Config.LocalDriver.IsLoadedInGame = true;

                // We need to wait until this moment to send the first Razolytics funnel step because we need to send some information stored in the local persistence too
                HDTrackingManager.Instance.Notify_Razolytics_Funnel_Load(FunnelData_LoadRazolytics.Steps._00_start);

                // Since local is already loaded then we consider the operation done. Sync will happen in background
                if (onDone != null)
                {
                    onDone();
                }

                Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onSyncDone = delegate (PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail)
                {
                    Sync_OnDone(result, null);
                };

                // Tries to sync with cloud only if the user was logged in the social platform when she quit the app last time she played
                if (PersistencePrefs.Social_WasLoggedInWhenQuit)
                {                    
                    Config.CloudDriver.Sync(true, true, onSyncDone);
                }
                else
                {
                    onSyncDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.NoLogInSocial);
                }
			}			
		};
        
        Log("SYNC: Loading local...");

		Config.LocalDriver.Load(onLoadDone);
	}

	public void Sync_FromSettings(Action onDone)
	{
        // Check if there's already a sync being performed since only one can be performed simultaneously. this Typically happens when the sync from launching the app wasn't over yet when the game loaded so
        // the user could click on manual sync
        if (Sync_IsSyncing)
        {
            // A popup is shown so the user gets some feedback
            Popup_SyncAlreadyOn(onDone);
        }
        else
        {
            Sync_IsSyncing = true;

            Action onSaveDone = delegate ()
            {
                Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onSyncDone = delegate (PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail)
                {
                    Sync_OnDone(result, onDone);
                };

                Config.CloudDriver.Sync(false, false, onSyncDone);
            };

            Config.LocalDriver.Save(onSaveDone);
        }
	}

    public void Sync_FromReconnecting(Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onDone)
    {
        if (Sync_IsSyncing)
        {
            if (onDone != null)
            {
                onDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.Cancelled);
            }
        }
        else
        {
            Sync_IsSyncing = true;

            Action onSaveDone = delegate ()
            {
                Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onSyncDone = delegate (PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail)
                {
                    Sync_OnDone(result, null);
                    onDone(result, resultDetail);
                };

                Config.CloudDriver.Sync(true, false, onSyncDone);
            };

            Config.LocalDriver.Save(onSaveDone);
        }
    }

    private void Sync_OnDone(PersistenceStates.ESyncResult result, Action onDone)
	{
        Sync_IsSyncing = false;
        
        Log("(SYNCER) Sync_OnDone result = " + result);

        if (result == PersistenceStates.ESyncResult.NeedsToReload)
		{        
            Log("(SYNCER) RELOADS THE APP TO LOAD CLOUD PERSISTENCE");

            ApplicationManager.instance.NeedsToRestartFlow = true;
        } 
		else if (onDone != null)
		{
			onDone();
		}
	}    
	#endregion

    #region texts
    public const string TID_SOCIAL_FB_LOGIN_MAINMENU_INCENTIVIZED = "TID_SOCIAL_LOGIN_MAINMENU_INCENTIVIZED";
	
    public static void Texts_LocalizeIncentivizedSocial(Localizer text)
    {
        text.Localize(TID_SOCIAL_FB_LOGIN_MAINMENU_INCENTIVIZED, Rules_GetPCAmountToIncentivizeSocial() + "");
    }
    #endregion

    #region save

    private bool Save_IsSaving { get; set; }

	public void Save_Request(bool immediate=false)
	{
        // Makes sure that local persistence has already been loaded in game so we can be sure that default persistence is not saved 
        // if this method is called when the engine is not ready (for example, when restarting the app)
        if (Config.LocalDriver.IsLoadedInGame && !Save_IsSaving)
        {
            Save_IsSaving = true;

            Action onDone = delegate ()
            {
                Save_IsSaving = false;
            };            

            Config.LocalDriver.Save(onDone);

            if (FeatureSettingsManager.instance.IsTrackingStoreUnsentOnSaveGameEnabled)
            {
                // Makes sure tracking events won't get lost if playing offline 
                HDTrackingManager.Instance.SaveOfflineUnsentEvents();
            }
        }
	}
	#endregion

	#region local
	public PersistenceLocalDriver LocalDriver { get { return Config.LocalDriver; } }
	public PersistenceData LocalData { get { return Config.LocalDriver.Data; } }

	private void Local_Reset()
	{
		LocalDriver.Reset();

        LocalDriver.UserProfile = UsersManager.currentUser;
        LocalData.Systems_RegisterSystem(LocalDriver.UserProfile);
        
        TrackingPersistenceSystem trackingSystem = HDTrackingManager.Instance.TrackingPersistenceSystem;
        LocalDriver.TrackingPersistenceSystem = trackingSystem;
        if (trackingSystem != null)
        {
            LocalData.Systems_RegisterSystem(trackingSystem);                    
        }
	}
	#endregion

	#region cloud
	public PersistenceCloudDriver CloudDriver { get { return Config.CloudDriver; } }
	public PersistenceData CloudData { get { return Config.CloudDriver.Data; } }

	private void Cloud_Reset()
	{
		CloudData.Reset();
	}
	#endregion

    #region rules
    // This region is responsible for giving access to rules related to persistence/social networks

    /// <summary>
    /// Returns the amount of PC the user will receive if she logs in a social network (she actually has to grant the friends list permission to get the reward)
    /// </summary>
    /// <returns></returns>
    public static int Rules_GetPCAmountToIncentivizeSocial()
    {
        int returnValue = 0;
        DefinitionNode _def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
        if (_def != null)
        {
            returnValue = _def.GetAsInt("incentivizeFBGem");
        }

        return returnValue;
    }
    #endregion

    #region popups
    private static int SYNC_GENERIC_ERROR_CODE_MERGE_CLOUD_SAVE_CORRUPTED = 1;
    private static int SYNC_GENERIC_ERROR_CODE_MERGE_LOCAL_SAVE_CORRUPTED = 2;
    private static int SYNC_GENERIC_ERROR_CODE_MERGE_BOTH_SAVES_CORRUPTED = 3;
    private static int SYNC_GENERIC_ERROR_CODE_SYNC_CLOUD_SAVE_CORRUPTED = 4;
    private static int SYNC_GENERIC_ERROR_CODE_SYNC_ALREADY_PERFORMING = 5;

    // This region is responsible for opening the related to persistence popups    
    private static bool Popups_IsInited { get; set; }

    private void Popups_Init()
    {
        if (!Popups_IsInited)
        {			
			Broadcaster.AddListener(BroadcastEventType.POPUP_CLOSED, this);
            Popups_IsInited = true;
        }
    }

    private void Popups_Destroy()
    {		
		Broadcaster.RemoveListener(BroadcastEventType.POPUP_CLOSED, this);
    }
	
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.POPUP_CLOSED:
            {
                PopupManagementInfo info = (PopupManagementInfo)broadcastEventInfo;
                Popups_OnPopupClosed(info.popupController);
            }break;
        }
    }
    
    private static PopupController Popups_LoadingPopup { get; set; }

    private static bool Popups_IsLoadingPopupOpen()
    {
        return Popups_LoadingPopup != null;
    }

    /// <summary>
    /// Opens a popup to make the user wait until the response of a request related to persistence is received
    /// </summary>
    public static void Popups_OpenLoadingPopup()
    {        
        Log("Popups_OpenLoadingPopup canOpen = " + (!Popups_IsLoadingPopupOpen()));

        if (!Popups_IsLoadingPopupOpen())
        {			
            Popups_LoadingPopup = PopupManager.PopupLoading_Open();
        }
    }

    public static void Popups_CloseLoadingPopup()
    {        
        Log("Popups_CloseLoadingPopup IsOpen = " + Popups_IsLoadingPopupOpen());

        if (Popups_IsLoadingPopupOpen())
        {			
            Popups_LoadingPopup.Close(true);
        }
    }
	
    private static void Popups_OnPopupClosed(PopupController popup)
    {        
        Log("Popups_OnPopupClosed canClose = " + (popup == Popups_LoadingPopup));

        if (popup == Popups_LoadingPopup)
        {
            Popups_LoadingPopup = null;
        }
    }
    

    /// <summary>
    /// This popup is shown when the local save is corrupted
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/20%29Local+save+corrupted
    /// </summary>     
    private static void Popups_OpenLoadLocalCorruptedError(Action onReset)
	{                
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_DESC";
        config.IsButtonCloseVisible = false;
        config.OnConfirm = onReset;                
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;        

        // Back button is disabled in order to make sure that the user is aware when making such an important decision
        config.BackButtonStrategy = IPopupMessage.Config.EBackButtonStratety.None;
        PopupManager.PopupMessage_Open(config);        
	}

    private static void Popups_OpenLoadLocalCorruptedNoAccessToCloudError(Action onReset, Action onRetry)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_OFFLINE_DESC";
        config.IsButtonCloseVisible = false;
        config.OnConfirm = onRetry;
        config.OnCancel = onReset;
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.CancelButtonTid = "TID_GEN_CONTINUE";

        // Back button is disabled in order to make sure that the user is aware when making such an important decision
        config.BackButtonStrategy = IPopupMessage.Config.EBackButtonStratety.None;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popups_OpenLoadLocalCorruptedButCloudOkError(Action onDone)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_CLOUD_SAVE_DESC";
        config.IsButtonCloseVisible = false;
        config.OnConfirm = onDone;        
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;        
        
        PopupManager.PopupMessage_Open(config);        
    }

	/// <summary>
    /// This popup is shown when the access to the local save file is not authorized by the device
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/18%29No+access+to+local+data
    /// </summary>    
    public static void Popups_OpenLocalLoadPermissionError(Action onConfirm)
    {					
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOAD_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOAD_FAILED_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);                
    }

	/// <summary>
    /// This popup is shown when an error arises when saving because there's no free disk space to store the local save
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/27%29No+disk+space
    /// </summary>    
    public static void Popups_OpenLocalSaveDiskOutOfSpaceError(Action onConfirm)
    {					
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_DISABLED_SPACE_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;        
        PopupManager.PopupMessage_Open(config);     
    }

    /// <summary>
    /// This popup is shown when an error arises when saving because there's no access to disk to store the local save.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/28%29No+disk+access
    /// </summary>    
    public static void Popups_OpenLocalSavePermissionError(Action onConfirm)
    {			
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_DISABLED_ACCESS_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);     	        
    }

    /// <summary>
    /// This popup is shown when an error arises because the persistence saved is corrupted
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/28%29No+disk+access
    /// </summary>    
    public static void Popups_OpenLocalSaveCorruptedError(Action onConfirm, Action onContinue)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_CORRUPTED";
        string uid = GameSessionManager.SharedInstance.GetUID();
        if (string.IsNullOrEmpty(uid))
        {
            uid = "-1";
        }

        config.MessageParams = new string[] { "" + uid };
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;        
        config.CancelButtonTid = "TID_GEN_CONTINUE";
        config.OnCancel = onContinue;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when there's no internet connection
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/29%29No+internet+connection
    /// </summary>    
    public static void Popups_OpenErrorConnection(Action onConfirm)
    {				
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_ERROR_CONNECTION_NAME";
        config.MessageTid = "TID_SOCIAL_ERROR_CONNECTION_DESC";
        config.MessageParams = new string[] { SocialPlatformManager.SharedInstance.GetPlatformName() };
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;        
        PopupManager.PopupMessage_Open(config);              
    }

    public static void Popups_OpenLoginComplete(int rewardAmount, Action onConfirm)
    {		
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_LOGIN_COMPLETE_NAME";
        config.MessageTid = "TID_SOCIAL_LOGIN_COMPLETE_DESC";
        config.MessageParams = new string[] { rewardAmount + "" };
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);                
    }

    /// <summary>
    /// This popup is shown when the user clicks on logout button on settings popup
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/12%29Logout
    /// </summary>   
    public static void Popups_OpenLogoutWarning(bool cloudSaveEnabled, Action onConfirm, Action onCancel)
    {				
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = cloudSaveEnabled ? "TID_SAVE_WARN_CLOUD_LOGOUT_NAME" : "TID_SOCIAL_WARNING_LOGOUT_TITLE";
        config.MessageTid = cloudSaveEnabled ? "TID_SAVE_WARN_CLOUD_LOGOUT_DESC" : "TID_SOCIAL_WARNING_LOGOUT_DESC";
        config.MessageParams = new string[] { SocialPlatformManager.SharedInstance.GetPlatformName() };
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);               
    }

	/// <summary>
    /// The user is prompted with this popup so she can choose the persistence to keep when there's a conflict between the progress stored in local and the one stored in the cloud
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/29%29Sync+Conflict
    /// </summary>
    public static void Popups_OpenSyncConflict(PersistenceStates.EConflictState conflictState, PersistenceComparatorSystem local, PersistenceComparatorSystem cloud, bool dismissable, Action<PersistenceStates.EConflictResult> onResolve)
	{
		PopupController pc = PopupManager.OpenPopupInstant(PopupMerge.PATH);
        PopupMerge pm = pc.GetComponent<PopupMerge>();
        if (pm != null)
        {
            pm.Setup(conflictState, local, cloud, dismissable, onResolve);
        }               
    }

    /// <summary>
    /// This popup is shown when the user doesn't choose the recommended option in sync conflict popup.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/29%29Sync+Conflict
    /// </summary>    
    public static void Popups_OpenSyncConfirmation(Action onConfirm)
    {        
		IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_WARN_CLOUD_WRONG_CHOICE_NAME";
        config.MessageTid = "TID_SAVE_WARN_CLOUD_WRONG_CHOICE_DESC";                
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);       
    }

    public static void Popup_OpenMergeConflict(Action onLocal, Action onCloud)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_TITLE";
        config.MessageTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_DESC";
        string platformName = SocialPlatformManager.SharedInstance.GetPlatformName();
        config.MessageParams = new string[] { platformName, platformName };
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onCloud;
        config.OnCancel = onLocal;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }   

    public static void Popup_OpenSyncGenericError(int errorCode, Action onConfirm)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_SYNC_FAILED_NAME";

        // A different message is shown for this error code to address HDK-2489
        if (errorCode == SYNC_GENERIC_ERROR_CODE_SYNC_ALREADY_PERFORMING)
        {
            config.MessageTid = "TID_SOCIAL_LOGIN_ERROR";
            string platformName = SocialPlatformManager.SharedInstance.GetPlatformName();
            config.MessageParams = new string[] { platformName };            
        }
        else
        {
            config.MessageTid = "TID_SAVE_ERROR_SYNC_FAILED_DESC";
            config.MessageParams = new string[] { "" + errorCode };
        }

        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popup_OpenMergeConflictCloudCorrupted(Action onConfirm)
    {        
        // Alternative: "You can't use this facebook account because its cloud save is corrupted."
        Popup_OpenSyncGenericError(SYNC_GENERIC_ERROR_CODE_MERGE_CLOUD_SAVE_CORRUPTED, onConfirm);        
    }

    public static void Popup_OpenMergeConflictLocalCorrupted(Action onConfirm)
    {        
        // Local save corrupted when syncing
        // Alternative: "Your local save is corrupted, do you want to override it with the cloud save?"
        Popup_OpenSyncGenericError(SYNC_GENERIC_ERROR_CODE_MERGE_LOCAL_SAVE_CORRUPTED, onConfirm);        
    }

    public static void Popup_OpenMergeConflictBothCorrupted(Action onConfirm)
    {
        // Alternative "Both saves are corrupted, reset local save?"
        Popup_OpenSyncGenericError(SYNC_GENERIC_ERROR_CODE_MERGE_BOTH_SAVES_CORRUPTED, onConfirm);       
    }

    /// <summary>
    /// Called when there was an attempt to sync while a sync is already being performed
    /// </summary>
    /// <param name="onConfirm"></param>
    public static void Popup_SyncAlreadyOn(Action onConfirm)
    {
        Popup_OpenSyncGenericError(SYNC_GENERIC_ERROR_CODE_SYNC_ALREADY_PERFORMING, onConfirm);
    }

    public static void Popup_OpenMergeWithADifferentAccount(Action onConfirm, Action onCancel)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_TITLE";
        config.MessageTid = "TID_SAVE_WARN_CLOUD_SWITCH_DESC";
        string platformName = SocialPlatformManager.SharedInstance.GetPlatformName();
        config.MessageParams = new string[] { platformName };
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }    

    public static void Popup_OpenErrorWhenSyncing(Action onContinue, Action onRetry)
	{                
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_INACCESSIBLE_NAME";
        config.MessageTid = "TID_SAVE_ERROR_CLOUD_INACCESSIBLE_DESC";

        if (onRetry == null)
        {
            config.ConfirmButtonTid = "TID_GEN_CONTINUE";            
            config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
            config.OnConfirm = onContinue;            
        }
        else
        {
            config.ConfirmButtonTid = "TID_GEN_RETRY";
            config.CancelButtonTid = "TID_GEN_CONTINUE";
            config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
            config.OnConfirm = onRetry;
            config.OnCancel = onContinue;
        }

        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);                   
    }

	public static void Popup_OpenCloudCorrupted(Action onContinue, Action onOverride)
	{
        // Internal error is shown.
        Popup_OpenSyncGenericError(SYNC_GENERIC_ERROR_CODE_SYNC_CLOUD_SAVE_CORRUPTED, onContinue);
        /*            
        // Alternative: Let the user override cloud save with local save.
        // Make sure texts used by 'Popup_OpenCloudCorruptedWasOverriden()' popup stop being hardcoded when this alternative is enabled
        string msg = "TID_SAVE_ERROR_CLOUD_SAVE_CORRUPTED_DESC";

        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_CORRUPTED_NAME";
        config.MessageTid = msg;
        config.ConfirmButtonTid = "TID_GEN_UPLOAD";
        config.CancelButtonTid = "TID_GEN_CONTINUE";
        config.IsButtonCloseVisible = false;          
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onOverride;
        config.OnCancel = onContinue;        

        PopupManager.PopupMessage_Open(config);
        */
    }
    
    public static void Popup_OpenCloudCorruptedWasOverriden(Action onContinue)
    {
        // Don't worry about the hardcoded texts because this popup is not used yet since this popup is shown only if 'Popup_OpenCloudCorrupted()'
        // lets the user override cloud save with local save when cloud save is corrupted
        string msg = "Corrupted cloud save was fixed";

        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "Cloud save";
        config.MessageTid = msg;
        config.IsButtonCloseVisible = false;
        config.OnConfirm = onContinue;
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;        

        PopupManager.PopupMessage_Open(config);
    }

    public static void Popup_OpenLocalAndCloudCorrupted(Action onReset)
	{
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_BOTH_SAVE_CORRUPTED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_BOTH_SAVE_CORRUPTED_DESC";
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.IsButtonCloseVisible = false;
        config.OnConfirm = onReset;
        PopupManager.PopupMessage_Open(config);        
	}   

    public static void Popups_OpenCloudSync(Action onConfirm, Action onCancel)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_CLOUD_ACTIVE_NAME";

        long lastUploadTime = instance.Sync_LatestSyncTime;        
        if (lastUploadTime > 0)
        {
            config.MessageTid = "TID_SAVE_CLOUD_ACTIVE_DESC";
            DateTime lastUpload = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            lastUpload = lastUpload.AddMilliseconds(lastUploadTime).ToLocalTime();
            string lastUploadStr = lastUpload.ToString("F", LocalizationManager.SharedInstance.Culture);
            config.MessageParams = new string[] { lastUploadStr };
        }
        else
        {
            config.MessageTid = "TID_SAVE_CLOUD_SAVE_ACTIVE_DESC";
        }

        config.ConfirmButtonTid = "TID_SAVE_CLOUD_SAVE_SYNC_NOW";
        config.CancelButtonTid = "TID_GEN_CONTINUE";
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }
    #endregion

    #region log
    private static bool LOG_USE_COLOR = false;
    private const string LOG_CHANNEL = "[Persistence] ";    
    private const string LOG_CHANNEL_COLOR = "<color=cyan>" + LOG_CHANNEL;

    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    public static void Log(string msg)
    {
        if (LOG_USE_COLOR)
        {
            msg = LOG_CHANNEL_COLOR + msg + " </color>";
        }
        else
        {
            msg = LOG_CHANNEL + msg;
        }

        ControlPanel.Log(msg);
    }

    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    public static void LogError(string msg)
    {
        ControlPanel.LogError(LOG_CHANNEL + msg);
    }

    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    public static void LogWarning(string msg)
    {
        ControlPanel.LogWarning(LOG_CHANNEL + msg);
    }
	#endregion
}


