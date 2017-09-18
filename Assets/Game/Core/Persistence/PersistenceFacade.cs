using System;
using System.Globalization;

public class PersistenceFacade
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
		//userCaseId = PersistenceFacadeConfigDebug.EUserCaseId.Launch_Local_NeverLoggedIn_Cloud_Error_GetPersistence;
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
        SocialPlatformManager.SharedInstance.Init();
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

	#region sync
    public bool Sync_IsSyncing { get; set; }

    private void Sync_Reset()
    {
        Sync_IsSyncing = false;
    }

    public void Sync_FromLaunchApplication(Action onDone)
	{
        Sync_IsSyncing = true;

        Action onLoadDone = delegate()
		{
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("SYNC: Loading  local DONE! " + LocalData.LoadState);

            Action<bool> performSync = delegate(bool isSilent)
			{
				Action<PersistenceStates.ESyncResult> onSyncDone = delegate(PersistenceStates.ESyncResult result)
				{
					if (!Config.LocalDriver.IsLoadedInGame)
					{
						Config.LocalDriver.IsLoadedInGame = true;
					}

					Sync_OnDone(result, onDone);
				};

				Config.CloudDriver.Sync(isSilent, true, onSyncDone);
			};

			// If local persistence is corrupted then we need to offer the chance to override it with cloud persistence
			// if the user has logged in ever.
			if (LocalData.LoadState == PersistenceStates.ELoadState.Corrupted)
			{
				// TODO
				bool logInSocialEver = false;

				Action onReset = delegate()
				{
					Action onResetDone = delegate()
					{
						Sync_FromLaunchApplication(onDone);
					};

					LocalDriver.OverrideWithDefault(onResetDone);
				};

				Action onConnect = null;

				if (logInSocialEver)
				{
					Action<PersistenceStates.ESyncResult> onConnectDone = delegate(PersistenceStates.ESyncResult result)
					{
						if (result == PersistenceStates.ESyncResult.ErrorLogging)
						{
							Sync_FromLaunchApplication(onDone);
						}
						else
						{
                            Config.LocalDriver.IsLoadedInGame = true;
                            Sync_OnDone(result, onDone);
						}
					};

					onConnect = delegate()
					{
						Config.CloudDriver.Sync(false, true, onConnectDone);
					};				
				}                

                // Lets the user know that local persistence is corrupted. User's options:
                // 1)Reset local persistence to the default one
                // 2)Override local persistence with cloud persistence
                Popup_OpenLocalCorrupted(logInSocialEver, onReset, onConnect);				
			}
			else
			{
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("Ready lo load local persistence in game ");

                Config.LocalDriver.IsLoadedInGame = true;

                // Since local is already loaded then we consider the operation done. Sync will happen in background
                if (onDone != null)
                {
                    onDone();
                }

                Action<PersistenceStates.ESyncResult> onSyncDone = delegate (PersistenceStates.ESyncResult result)
                {
                    if (!Config.LocalDriver.IsLoadedInGame)
                    {
                        Config.LocalDriver.IsLoadedInGame = true;
                    }

                    Sync_OnDone(result, null);
                };

                Config.CloudDriver.Sync(true, true, onSyncDone);                
			}			
		};

        if (FeatureSettingsManager.IsDebugEnabled)
            Log("SYNC: Loading local...");

		Config.LocalDriver.Load(onLoadDone);
	}

	public void Sync_FromSettings(Action onDone)
	{
        Sync_IsSyncing = true;

        Action onSaveDone = delegate()
		{		
			Action<PersistenceStates.ESyncResult> onSyncDone = delegate(PersistenceStates.ESyncResult result)
			{
				Sync_OnDone(result, onDone);
			};

			Config.CloudDriver.Sync(false, false, onSyncDone);
		};

		Config.LocalDriver.Save(onSaveDone);
	}

	private void Sync_OnDone(PersistenceStates.ESyncResult result, Action onDone)
	{
        Sync_IsSyncing = false;

        if (result == PersistenceStates.ESyncResult.NeedsToReload)
		{
            if (FeatureSettingsManager.IsDebugEnabled)
                PersistenceFacade.Log("(SYNCER) RELOADS THE APP TO LOAD CLOUD PERSISTENCE");

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
	public void Save_Request(bool immediate=false)
	{
		Config.LocalDriver.Save(null);
	}
	#endregion

	#region local
	public PersistenceLocalDriver LocalDriver { get { return Config.LocalDriver; } }
	public PersistenceData LocalData { get { return Config.LocalDriver.Data; } }

	private void Local_Reset()
	{
		LocalData.Reset();

        LocalDriver.UserProfile = UsersManager.currentUser;
        LocalData.Systems_RegisterSystem(LocalDriver.UserProfile);
        
        TrackingPersistenceSystem trackingSystem = HDTrackingManager.Instance.TrackingPersistenceSystem;
        if (trackingSystem != null)
        {
            LocalData.Systems_RegisterSystem(trackingSystem);
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
		// UNPH		
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
        
        /*
		Popup popup = FlowController.Instance.OpenPopup("No permission to load local persistence. Try again?");
		popup.AddButton("Retry", onConfirm, true);
        */
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
     	
        /*
		Popup popup = FlowController.Instance.OpenPopup("No space to save local persistence. Try again?");
		popup.AddButton("Retry", onConfirm, true);
        */
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
     	
        /*
		Popup popup = FlowController.Instance.OpenPopup("No permission to save local persistence. Try again?");
		popup.AddButton("Retry", onConfirm, true);
        */
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
        
        /*
		Popup popup = FlowController.Instance.OpenPopup("No connection.");
		popup.AddButton("Ok", onConfirm, true);
        */
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
        
        /*
		Popup popup = FlowController.Instance.OpenPopup("Login completed. +15gems");
		popup.AddButton("Ok", onConfirm, true);
        */
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
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
        
        /*
		Popup popup = FlowController.Instance.OpenPopup("Are you sure you want to logout?");
		popup.AddButton("Ok", onConfirm, true);
		popup.AddButton("Cancel", onCancel, true);
        */
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
        
        /*
		Action onUseLocal = delegate() {
			onResolve (PersistenceStates.EConflictResult.Local);
		};

		Action onUseCloud = delegate() {
			onResolve (PersistenceStates.EConflictResult.Cloud);
		};

		Action onDismiss = delegate() {
			onResolve (PersistenceStates.EConflictResult.Dismissed);
		};

		Popup popup = FlowController.Instance.OpenPopup ("Merge: Local: " + local.dragonsOwned + " Cloud: " + cloud.dragonsOwned);
		popup.AddButton ("UseLocal", onUseLocal, true);
		popup.AddButton ("UseCloud", onUseCloud, true);

		if (dismissable)
		{
			popup.AddButton ("Dismiss", onDismiss, true);
		}
        */
    }

    /// <summary>
    /// This popup is shown when the user doesn't choose the recommended option in sync conflict popup.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/29%29Sync+Conflict
    /// </summary>    
    public static void Popups_OpenSyncConfirmation(Action onConfirm)
    {        
		PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_WARN_CLOUD_WRONG_CHOICE_NAME";
        config.MessageTid = "TID_SAVE_WARN_CLOUD_WRONG_CHOICE_DESC";                
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);       
    }

    public static void Popup_OpenMergeConflict(Action onLocal, Action onCloud)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_TITLE";
        config.MessageTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onCloud;
        config.OnCancel = onLocal;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popup_OpenMergeConflictCloudCorrupted(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_TITLE";
        config.MessageTid = "You can't use this facebook account because its cloud save is corrupted.";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;        
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popup_OpenMergeConflictLocalCorrupted(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_TITLE";
        config.MessageTid = "Your local save is corrupted, do you want to override it with the cloud save?";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popup_OpenMergeConflictBothCorrupted(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_TITLE";
        config.MessageTid = "Both saves are corrupted, reset local save?";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popup_OpenErrorWhenSyncing(Action onContinue, Action onRetry)
	{        
        // UNPH: Two buttons instead of three (upload local save to cloud is not an option. Review the text description)
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_INACCESSIBLE_NAME";
        config.MessageTid = "TID_SAVE_ERROR_CLOUD_INACCESSIBLE_DESC";
        config.ConfirmButtonTid = "TID_GEN_CONTINUE";
        config.CancelButtonTid = "TID_GEN_RETRY";
        config.ExtraButtonTid = "TID_GEN_UPLOAD";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onRetry;
        config.OnCancel = onContinue;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);            

        /*
		Popup popup = FlowController.Instance.OpenPopup("Error when syncing. Retry?");
		popup.AddButton("Retry", onRetry, true); 
		popup.AddButton("Continue", onContinue, true);
        */
    }

	public static void Popup_OpenCloudCorrupted(Action onContinue, Action onOverride)
	{
        /*        
		string msg = (canOverride) ? "Cloud corrupted. Override?" : "Cloud corrupted but inaccessible. Continue locally?";
		
		Popup popup = FlowController.Instance.OpenPopup(msg);
		if (canOverride)
		{
			popup.AddButton("Override", onOverride, true);
		}

		popup.AddButton("Continue", onContinue, true);
        */

        // UNPH: Add TIDS and add popup Popups_OpenCloudSaveCorruptedError when the cloud was overridden successfully?
        string msg = "Cloud corrupted. Override?";

        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_CLOUD_CORRUPTED_NAME";
        config.MessageTid = msg;
        config.IsButtonCloseVisible = false;
        config.OnConfirm = onContinue;        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnCancel = onOverride;        

        PopupManager.PopupMessage_Open(config);
    }

	public static void Popup_OpenLocalCorrupted(bool logInSocialEver, Action onReset, Action onOverride)
	{
        // UNPH : Add tids
        string msg = (logInSocialEver) ? "Local corrupted. Log in to override with cloud?" : "Local corrupted. Reset?";
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = msg;
        config.IsButtonCloseVisible = false;
        config.OnConfirm = onReset;

        if (logInSocialEver)
        {
            config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
            config.OnCancel = onOverride;
        }
        else
        {
            config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        }

        PopupManager.PopupMessage_Open(config);

        /*
        string msg = (logInSocialEver) ? "Local corrupted. Log in to override with cloud?" : "Local corrupted. Reset?";        
		Popup popup = FlowController.Instance.OpenPopup(msg);

		if (logInSocialEver)
		{
			popup.AddButton("Override", onOverride, true);
		}

		popup.AddButton("Reset", onReset, true);
        */
    }

    public static void Popup_OpenLocalAndCloudCorrupted(Action onReset)
	{
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_BOTH_SAVE_CORRUPTED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_BOTH_SAVE_CORRUPTED_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.IsButtonCloseVisible = false;
        config.OnConfirm = onReset;
        PopupManager.PopupMessage_Open(config);

        /*
        Popup popup = FlowController.Instance.OpenPopup("Local and cloud are corrupted. Reset them to default?");
		popup.AddButton("Reset", onReset, true);
        */
	}
    #endregion

    #region log
    private static bool LOG_USE_COLOR = true;
    private const string LOG_CHANNEL = "[Persistence] ";    
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


