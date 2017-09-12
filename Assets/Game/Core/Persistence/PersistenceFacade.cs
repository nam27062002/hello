using UnityEngine;
using System;
public class PersistenceFacade : UbiBCN.SingletonMonoBehaviour<PersistenceFacade>
{		 
    private PersistenceFacadeConfig Config { get; set; }   

    public void Init()
    {
        PersistenceFacadeConfigDebug.EUserCaseId userCaseId = PersistenceFacadeConfigDebug.EUserCaseId.Production;
        //userCaseId = PersistenceFacadeConfigDebug.EUserCaseId.Error_Local_Save_DiskSpace;
        if (FeatureSettingsManager.IsDebugEnabled && userCaseId != PersistenceFacadeConfigDebug.EUserCaseId.Production)
        {
            Config = new PersistenceFacadeConfigDebug(userCaseId);
        }
        else
        {
            Config = new PersistenceFacadeConfig();
        }

        Sync_IsFromLaunchApplicationDone = false;
        Sync_Init();
        Local_Init();
        Save_Init();

        GameServerManager.SharedInstance.Configure();
        SocialPlatformManager.SharedInstance.Init();
    }
    
    protected void Update()
    {
        PersistencePrefs.Update();

        if (Sync_Syncer != null)
        {
            Sync_Syncer.Update();
        }

        Save_Update();
	}

	#region sync
	private PersistenceSyncer Sync_Syncer { get; set; }

	public PersistenceData Sync_LocalData { get; set; }
	public PersistenceData Sync_CloudData { get; set; }

	public bool Sync_AreDataInSync { get; set; }

    public bool Sync_IsCloudSaveEnabled
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

    private bool Sync_IsLoadingCloudFromLaunch { get; set; }

	private void Sync_Init()
	{
        Sync_IsLoadingCloudFromLaunch = false;
        Sync_AreDataInSync = false;

        if (Sync_Syncer == null)
        {
            Sync_Syncer = new PersistenceSyncer();
        }
        else
        {
            Sync_Syncer.Reset(true);
        }

        string dataName = PersistencePrefs.ActiveProfileName;
        Sync_LocalData = new PersistenceData(dataName);
        Sync_CloudData = new PersistenceData(dataName);        
    }

    public bool Sync_IsFromLaunchApplicationDone { get; set; }    

	public void Sync_FromLaunchApplication(Action<PersistenceStates.ESyncResult> onDone)
	{
        // Local load is done first in order to make sure the application is launched as quick as possible
        PersistenceSyncOpFactory factory = Config.SyncFromLaunchFactory;
		PersistenceSyncOp localOp = factory.GetLoadLocalOp(Sync_LocalData);
        PersistenceSyncOp syncOp = factory.GetSyncOp(localOp, null, false);

        Action<PersistenceStates.ESyncResult> onLocalLoad = delegate (PersistenceStates.ESyncResult result)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("FROM LAUNCH: Local loaded");
            }

            Sync_IsFromLaunchApplicationDone = true;
            Sync_IsLoadingCloudFromLaunch = true;
            if (onDone != null)
            {
                onDone(result);
            }

            Action<PersistenceStates.ESyncResult> onCloudLoad = delegate (PersistenceStates.ESyncResult r)
            {
                Sync_IsLoadingCloudFromLaunch = false;

                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    Log("FROM LAUNCH: CLOUD loaded");
                }
            };

            // It needs to load the cloud and merge. We use a dummy local load op since it's already loaded. We just want to provide the syncer with the local data
            localOp = factory.GetLoadLocalOp(Sync_LocalData, false);
            PersistenceSyncOp cloudOp = factory.GetLoadCloudOp(Sync_CloudData, true);
            syncOp = factory.GetSyncOp(localOp, null, false);
            Sync_Perform(PersistenceSyncer.EPurpose.SyncFromLaunch, localOp, cloudOp, syncOp, onCloudLoad);
        };

        Sync_Perform(PersistenceSyncer.EPurpose.SyncFromLaunch, localOp, null, syncOp, onLocalLoad);       
    }

	public void Sync_FromSettings(Action<PersistenceStates.ESyncResult> onDone)
	{
        PersistenceSyncOpFactory factory = Config.SyncFromSettingsFactory;

        // We need to save the current progress so the user won't lose it during the process
        PersistenceSyncOp localOp = factory.GetSaveLocalOp(Sync_LocalData, false);
		PersistenceSyncOp cloudOp = factory.GetLoadCloudOp(Sync_CloudData, false);
		PersistenceSyncOp syncOp = factory.GetSyncOp(localOp, cloudOp, false);
		Sync_Perform(PersistenceSyncer.EPurpose.SyncFromSettings, localOp, cloudOp, syncOp, onDone);
	}

	private void Sync_Perform(PersistenceSyncer.EPurpose purpose, PersistenceSyncOp localOp, PersistenceSyncOp cloudOp, PersistenceSyncOp syncOp, Action<PersistenceStates.ESyncResult> onDone)
	{
		Action<PersistenceStates.ESyncResult> onSyncDone = delegate(PersistenceStates.ESyncResult result)
		{
            // Syncs are considered in sync if all ops have been successful
            Sync_AreDataInSync = (localOp != null && localOp.Result == PersistenceStates.ESyncResult.Success &&
                                  cloudOp != null && cloudOp.Result == PersistenceStates.ESyncResult.Success &&
                                  syncOp != null && syncOp.Result == PersistenceStates.ESyncResult.Success);


            if (FeatureSettingsManager.IsDebugEnabled)
            {
                string msg = "Sync DONE ";
                if (localOp != null)
                {
                    msg += " localOp.Result = " + localOp.Result;
                }

                if (cloudOp != null)
                {
                    msg += " cloudOp.Result = " + cloudOp.Result;
                }

                if (syncOp != null)
                {
                    msg += " syncOp.Result = " + syncOp.Result;
                }

                msg += " AreDataInSync = " + Sync_AreDataInSync;

                Log(msg);
            }

            Action<PersistenceStates.ESyncResult> onComplete = delegate(PersistenceStates.ESyncResult onCompleteResult)
            {
                Messenger.Broadcast(GameEvents.PERSISTENCE_SYNC_DONE);

                if (onDone != null)
                {
                    onDone(onCompleteResult);
                }
            };

            if (result == PersistenceStates.ESyncResult.Success && Local_UserProfile.SocialState == UserProfile.ESocialState.LoggedIn &&
                (purpose == PersistenceSyncer.EPurpose.SyncFromLaunch || purpose == PersistenceSyncer.EPurpose.SyncFromSettings))
            {
                Sync_OnFirstLoginComplete(onComplete);
            }            
            else 
			{
                onComplete(result);               
			}                                                
		};

		Sync_Syncer.Sync(purpose, localOp, cloudOp, syncOp, onSyncDone);
	}

    public bool Sync_IsSyncing()
    {
        return Sync_Syncer.IsSyncing();
    }

    private void Sync_OnFirstLoginComplete(Action<PersistenceStates.ESyncResult> onDone)
    {       
        Action onLoginCompleteDone = delegate ()
        {
            Action onSaveDone = delegate ()
            {
                if (onDone != null)
                {
                    onDone(PersistenceStates.ESyncResult.Success);                    
                }
            };

            // Gives the reward
            int rewardAmount = Rules_GetPCAmountToIncentivizeSocial();
            Local_UserProfile.EarnCurrency(UserProfile.Currency.HARD, (ulong)rewardAmount, false, HDTrackingManager.EEconomyGroup.INCENTIVISE_SOCIAL_LOGIN);

            // Mark it as already rewarded
            Local_UserProfile.SocialState = UserProfile.ESocialState.LoggedInAndInventivised;

            // It needs to save the persistence            
            Save_Perform(onSaveDone);            
        };

        Popups_OpenLoginComplete(Rules_GetPCAmountToIncentivizeSocial(), onLoginCompleteDone);        
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
    private float Save_TimeToPerform { get; set; }

    private bool Save_IsPerforming { get; set; }

    private void Save_Init()
    {
        Save_TimeToPerform = -1;
        Save_IsPerforming = false;
    }

	public void Save_Request(bool immediate=false)
	{        
		if (immediate)
		{
            Save_Perform();
		}
        else
        {
            Save_TimeToPerform = 0f;
        }
	}

    /// <summary>
    /// Performs save game. Only local save supported so far
    /// </summary>    
	private void Save_Perform(Action onDone = null)
	{
        if (FeatureSettingsManager.IsDebugEnabled)
        {        
            Log("SAVE Trying to save");            
        }

        Save_TimeToPerform = -1;

        // Since the cloud load is not blocker because we want the application to start as soon as the local persistence has been loaded then, taking into condiseration that loading the 
        // cloud persistence can take long, it could happen that a Save is requested. In this case we just save the local persistence in order to avoid any losses
        if (Sync_IsLoadingCloudFromLaunch)
        {
            // It's saved only if the local persistence is not locked
            if (!Sync_Syncer.IsLocalLocked())
            {
                Sync_LocalData.Save();
                Save_OnPerformed(true, onDone);
            }
            else
            {
                Save_OnPerformed(false, onDone);
            }
        }
        else
        {        
            PersistenceSyncOpFactory factory = Config.SaveFactory;
            PersistenceSyncOp localOp = factory.GetSaveLocalOp(Sync_LocalData, false);
            PersistenceSyncOp cloudOp = null;//factory.GetLoadCloudOp(Sync_CloudData, false, true);
            PersistenceSyncOp syncOp = factory.GetSyncOp(localOp, cloudOp, false);

            Action<PersistenceStates.ESyncResult> onSaveDone = delegate (PersistenceStates.ESyncResult result)
            {
                Save_OnPerformed(result == PersistenceStates.ESyncResult.Success, onDone);
            };

            Sync_Perform(PersistenceSyncer.EPurpose.Save, localOp, cloudOp, syncOp, onSaveDone);
        }        
	}

    private void Save_OnPerformed(bool success, Action onDone)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            if (success)
            {
                Log("SAVE completed successfully");
            }
            else
            {
                LogWarning("SAVE couldn't be completed successfully");
            }
        }

        // If success then 
        Save_TimeToPerform = (success) ? -1 : 30f;

        if (onDone != null)
        {
            onDone();
        }
    }

    private void Save_Update()
    {
        if (Save_TimeToPerform > -1)
        {
            Save_TimeToPerform -= Time.deltaTime;
            if (Save_TimeToPerform < 0)
            {                
                Save_Perform();
            }
        }
    }
	#endregion

	#region local
	public UserProfile Local_UserProfile { get; set; }

	private void Local_Init()
	{
        Local_UserProfile = UsersManager.currentUser;
        Sync_LocalData.Systems_RegisterSystem(Local_UserProfile);        

        TrackingPersistenceSystem trackingSystem = HDTrackingManager.Instance.TrackingPersistenceSystem;
        if (trackingSystem != null)
        {
            Sync_LocalData.Systems_RegisterSystem(trackingSystem);            
        }        
	}

    /// <summary>
    /// Resets the current local persistence to the default one. This method should be called only for DEBUG purposes.
    /// </summary>
    public void Local_ResetToDefault()
    {
        if (Sync_LocalData != null)
        {
            SimpleJSON.JSONClass defaultPersistence = PersistenceUtils.GetDefaultDataFromProfile();
            Sync_LocalData.LoadFromString(defaultPersistence.ToString());

            // Saves the new local persistence. We don't need to use any syncer because this method is called only for debug purposes
            Sync_LocalData.Save();
        }
    }
	#endregion

	#region social
	public bool Social_IsLoggedIn()
	{
		return Config.Social_IsLoggedIn();
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
    // This region is responsible for opening the related to persistence popups    
    private static bool Popups_IsInited { get; set; }

    private static void Popups_Init()
    {
        if (!Popups_IsInited)
        {
            Messenger.AddListener<PopupController>(EngineEvents.POPUP_CLOSED, Popups_OnPopupClosed);
            Popups_IsInited = true;
        }
    }

    private static void Popups_Destroy()
    {
        Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_CLOSED, Popups_OnPopupClosed);
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
        if (!Popups_IsLoadingPopupOpen())
        {
            Popups_LoadingPopup = PopupManager.PopupLoading_Open();
        }
    }

    public static void Popups_CloseLoadingPopup()
    {
        if (Popups_IsLoadingPopupOpen())
        {
            Popups_LoadingPopup.Close(true);
        }
    }

    private static void Popups_OnPopupClosed(PopupController popup)
    {
        if (popup == Popups_LoadingPopup)
        {
            Popups_LoadingPopup = null;
        }
    }

    /// <summary>
    /// This popup is shown when the local save is corrupted when the game was going to continue locally
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/20%29Local+save+corrupted
    /// </summary>
    /// <param name="cloudEver">Whether or not the user has synced with server</param>    
    public static void Popups_OpenLoadSaveCorruptedError(bool cloudEver, Action onConfirm)
	{        
		PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = (cloudEver) ? "TID_SAVE_ERROR_LOCAL_CORRUPTED_OFFLINE_DESC" : "TID_SAVE_ERROR_LOCAL_CORRUPTED_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);        

        //Popup popup = FlowController.Instance.OpenPopup("Local save corrupted. Reset?");
		//popup.AddButton("Ok", onConfirm, true);
	}

	/// <summary>
    /// This popup is shown when the access to the local save file is not authorized by the device
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/18%29No+access+to+local+data
    /// </summary>    
    public static void Popups_OpenLocalLoadPermissionError(Action onConfirm)
    {		
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOAD_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOAD_FAILED_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
        
		//Popup popup = FlowController.Instance.OpenPopup("No permission to load local persistence. Try again?");
		//popup.AddButton("Retry", onConfirm, true);
    }

	/// <summary>
    /// This popup is shown when starting the game if there's no free disk space to store the local save
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/27%29No+disk+space
    /// </summary>    
    public static void Popups_OpenLocalSaveDiskOutOfSpaceError(Action onConfirm)
    {		
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_DISABLED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_DISABLED_SPACE_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
     
		//Popup popup = FlowController.Instance.OpenPopup("No space to save local persistence. Try again?");
		//popup.AddButton("Retry", onConfirm, true);
    }

    /// <summary>
    /// This popup is shown when starting the game if there's no access to disk to store the local save.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/28%29No+disk+access
    /// </summary>    
    public static void Popups_OpenLocalSavePermissionError(Action onConfirm)
    {		
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_DISABLED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_DISABLED_ACCESS_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
     
		//Popup popup = FlowController.Instance.OpenPopup("No permission to save local persistence. Try again?");
		//popup.AddButton("Retry", onConfirm, true);
    }


    /// <summary>
    /// This popup is shown when there's no internet connection
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/29%29No+internet+connection
    /// </summary>    
    public static void Popups_OpenErrorConnection(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_ERROR_CONNECTION_NAME";
        config.MessageTid = "TID_SOCIAL_ERROR_CONNECTION_DESC";
        config.MessageParams = new string[] { SocialPlatformManager.SharedInstance.GetPlatformName() };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
        
		//Popup popup = FlowController.Instance.OpenPopup("No connection.");
		//popup.AddButton("Ok", onConfirm, true);
    }

    public static void Popups_OpenLoginComplete(int rewardAmount, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_LOGIN_COMPLETE_NAME";
        config.MessageTid = "TID_SOCIAL_LOGIN_COMPLETE_DESC";
        config.MessageParams = new string[] { rewardAmount + "" };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// This popup is shown when the user clicks on logout button on settings popup
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/12%29Logout
    /// </summary>   
    public static void Popups_OpenLogoutWarning(bool cloudSaveEnabled, Action onConfirm, Action onCancel)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = cloudSaveEnabled ? "TID_SAVE_WARN_CLOUD_LOGOUT_NAME" : "TID_SOCIAL_WARNING_LOGOUT_TITLE";
        config.MessageTid = cloudSaveEnabled ? "TID_SAVE_WARN_CLOUD_LOGOUT_DESC" : "TID_SOCIAL_WARNING_LOGOUT_DESC";
        config.MessageParams = new string[] { SocialPlatformManager.SharedInstance.GetPlatformName() };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        PopupManager.PopupMessage_Open(config);
    }
    #endregion

    #region log
    private static bool LOG_USE_COLOR = true;
    private const string LOG_CHANNEL = "[Persistence]:";    
    private const string LOG_CHANNEL_COLOR = "<color=cyan>" + LOG_CHANNEL;

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

        Debug.Log(msg);
    }

    public static void LogError(string msg)
    {
        Debug.LogError(LOG_CHANNEL + msg);
    }

	public static void LogWarning(string msg)
    {
        Debug.LogWarning(LOG_CHANNEL + msg);
    }
	#endregion
}


