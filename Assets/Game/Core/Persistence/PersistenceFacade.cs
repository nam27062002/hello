#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;

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
        Sync_Update();
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
    
    private float Sync_AutoImplicitLoginAt { get; set; }
    
    private void Sync_Reset()
    {
        Sync_IsSyncing = false;
        Sync_ResetAutoImplicitLogin();
    }    

    public void Sync_FromLaunchApplication(Action onDone)
    {
        Sync_IsSyncing = true;

        Action onSyncFromLaunchDone = delegate ()
        {            
            Sync_ScheduleAutoImplicitLogin();                        

            if (onDone != null)
            {
                onDone();
            }
        };

        Action onLoadDone = delegate ()
        {
            Log("SYNC: Loading  local DONE! " + LocalData.LoadState);

            // Retrieves the latest social platform that the user logged in to
            SocialPlatformManager socialPlatformManager = SocialPlatformManager.SharedInstance;
            SocialUtils.EPlatform platformId = socialPlatformManager.CurrentPlatform_GetId();//SocialUtils.KeyToEPlatform(LocalDriver.Prefs_SocialPlatformKey);
            bool isPlatformSupported = socialPlatformManager.IsPlatformIdSupported(platformId);
            bool isImplicit = socialPlatformManager.IsImplicit(platformId);

            // If local persistence is corrupted then we'll try to override it with cloud persistence if the user has ever logged in the social network
            if (LocalData.LoadState == PersistenceStates.ELoadState.Corrupted)
            {
                bool logInSocialEver = isPlatformSupported &&
                                       (isImplicit || !string.IsNullOrEmpty(LocalDriver.Prefs_SocialId));

                Action onReset = delegate ()
                {
                    Action onResetDone = delegate ()
                    {
                        Sync_FromLaunchApplication(onSyncFromLaunchDone);
                    };

                    LocalDriver.OverrideWithDefault(onResetDone);
                };

                Action onRetry = delegate ()
                {
                    Sync_FromLaunchApplication(onSyncFromLaunchDone);
                };

                if (logInSocialEver)
                {
                    Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onConnectDone = delegate (PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail)
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
                            Sync_OnDone(result, onSyncFromLaunchDone);
                        }
                    };

                    // Logs in to the latest platform known
                    Config.CloudDriver.Sync(platformId, PersistenceCloudDriver.ESyncMode.Lite, PersistenceCloudDriver.EErrorMode.OnlyMergeConflict, true, onConnectDone);
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
                if (onSyncFromLaunchDone != null)
                {
                    onSyncFromLaunchDone();
                }

                Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onSyncDone = delegate (PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail)
                {
                    Sync_OnDone(result, null);
                };

                PersistenceCloudDriver.ESyncMode mode = Sync_GetMode(platformId);                

#if UNITY_EDITOR
                ApplicationManager.instance.PersistenceTester.OnSyncModeAtLaunch(mode);
#endif
                if (mode == PersistenceCloudDriver.ESyncMode.None)
                {
                    onSyncDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.NoLogInSocial);
                }
                else
                { 
                    Config.CloudDriver.Sync(platformId, mode, PersistenceCloudDriver.EErrorMode.OnlyMergeConflict, true, onSyncDone);
                }
            }
        };

        Log("SYNC: Loading local...");

        Config.LocalDriver.Load(onLoadDone);
    }

    public void Sync_FromSettings(SocialUtils.EPlatform platformId, PersistenceCloudDriver.ESyncMode mode, Action onDone)
    {
        // Check if there's already a sync being performed since only one can be performed simultaneously. this Typically happens when the sync from launching the app wasn't over yet when the game loaded so
        // the user could click on manual sync
        if (Sync_IsSyncing)
        {
            // A popup is shown so the user gets some feedback
            Popup_SyncAlreadyOn(platformId, onDone);
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

                Config.CloudDriver.Sync(platformId, mode, PersistenceCloudDriver.EErrorMode.Verbose, false, onSyncDone);
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

                // Uses the same social platform that is currently in usage since the user can not change social platforms
                // when reconnecting
                SocialUtils.EPlatform platform = SocialPlatformManager.SharedInstance.CurrentPlatform_GetId();
                PersistenceCloudDriver.ESyncMode mode = Sync_GetMode(platform);
                PersistenceCloudDriver.EErrorMode errorMode = (SocialPlatformManager.SharedInstance.IsImplicit(platform) && mode == PersistenceCloudDriver.ESyncMode.Full) ? PersistenceCloudDriver.EErrorMode.OnlyMergeConflict : PersistenceCloudDriver.EErrorMode.Silent;
                if (mode == PersistenceCloudDriver.ESyncMode.None)
                {
                    if (onSyncDone != null)
                    {
                        onSyncDone(PersistenceStates.ESyncResult.ErrorLogging, PersistenceStates.ESyncResultDetail.NoLogInSocial);
                    }
                }
                else
                {
                    Config.CloudDriver.Sync(platform, mode, errorMode, false, onSyncDone);
                }
            };

            Config.LocalDriver.Save(onSaveDone);
        }
    }

    private PersistenceCloudDriver.ESyncMode Sync_GetMode(SocialUtils.EPlatform platform)
    {
        PersistenceCloudDriver.ESyncMode returnValue = PersistenceCloudDriver.ESyncMode.None;

        if (FeatureSettingsManager.instance.IsImplicitCloudSaveEnabled())
        {
            if (SocialPlatformManager.SharedInstance.IsPlatformIdSupported(platform))
            {
                if (LocalDriver.Prefs_SocialWasLoggedInWhenQuit ||
                    (SocialPlatformManager.SharedInstance.IsImplicit(platform) && LocalDriver.Prefs_SocialImplicitMergeState == PersistenceCloudDriver.EMergeState.None))
                {
                    returnValue = PersistenceCloudDriver.ESyncMode.Full;
                }
                else if (LocalDriver.HasEverExplicitlyLoggedIn() || LocalDriver.Prefs_SocialImplicitMergeState == PersistenceCloudDriver.EMergeState.Ok)
                {
                    returnValue = PersistenceCloudDriver.ESyncMode.Lite;
                }
            }
        }
        else
        {
            returnValue = PersistenceCloudDriver.ESyncMode.Full;
        }

        return returnValue;
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

    private void Sync_ResetAutoImplicitLogin()
    {
        Sync_AutoImplicitLoginAt = -1;
    }

    private void Sync_ScheduleAutoImplicitLogin()
    {
        if (Sync_NeedsToAutoImplicitLogin())
        {
            Sync_AutoImplicitLoginAt = Time.timeSinceLevelLoad + 10f;
        }
        else
        {
            Sync_ResetAutoImplicitLogin();
        }
    }

    private bool Sync_NeedsToAutoImplicitLogin()
    {
        return FeatureSettingsManager.instance.IsImplicitCloudSaveEnabled() && LocalDriver.Prefs_SocialImplicitMergeState == PersistenceCloudDriver.EMergeState.None && LocalDriver.HasEverExplicitlyLoggedIn();
    }

    private void Sync_Update()
    {
        if (!Sync_IsSyncing && Sync_AutoImplicitLoginAt > -1 && Time.timeSinceLevelLoad >= Sync_AutoImplicitLoginAt)
        {
            if (Sync_NeedsToAutoImplicitLogin())
            {
                Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onSyncDone = delegate (PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail)
                {
                    Sync_ScheduleAutoImplicitLogin();
                    Sync_OnDone(result, null);
                };

                Sync_ResetAutoImplicitLogin();
                Sync_IsSyncing = true;                
                CloudDriver.Sync(SocialUtils.EPlatform.DNA, PersistenceCloudDriver.ESyncMode.Full, PersistenceCloudDriver.EErrorMode.OnlyMergeConflict, false, onSyncDone);
            }
            else
            {
                Sync_ResetAutoImplicitLogin();
            }
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

    public void Save_Request(bool immediate = false)
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
    private static int SYNC_GENERIC_ERROR_CODE_SYNC_ALREADY_PERFORMING   = 5;
    private static int SYNC_GENERIC_ERROR_CODE_SYNC_UNEXPECTED_FLOW      = 6;


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
        switch (eventType)
        {
            case BroadcastEventType.POPUP_CLOSED:
                {
                    PopupManagementInfo info = (PopupManagementInfo)broadcastEventInfo;
                    Popups_OnPopupClosed(info.popupController);
                } break;
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
    public static void Popups_OpenErrorConnection(SocialUtils.EPlatform platformId, Action onConfirm)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_ERROR_CONNECTION_NAME";
        config.MessageTid = "TID_SOCIAL_ERROR_CONNECTION_DESC";
        config.MessageParams = new string[] { SocialPlatformManager.SharedInstance.GetPlatformName(platformId) };

        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
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
        config.MessageParams = new string[] { SocialPlatformManager.SharedInstance.CurrentPlatform_GetName() };
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

    public static void Popup_OpenMergeConflict(SocialUtils.EPlatform platformId, Action onLocal, Action onCloud)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_TITLE";
        config.MessageTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_DESC";
        string platformName = SocialPlatformManager.SharedInstance.GetPlatformName(platformId);
        config.MessageParams = new string[] { platformName, platformName };
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onCloud;
        config.OnCancel = onLocal;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }

    public static void Popup_OpenSyncGenericError(SocialUtils.EPlatform platformId, int errorCode, Action onConfirm)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_SYNC_FAILED_NAME";

        // A different message is shown for this error code to address HDK-2489
        if (errorCode == SYNC_GENERIC_ERROR_CODE_SYNC_ALREADY_PERFORMING)
        {
            config.MessageTid = "TID_SOCIAL_LOGIN_ERROR";
            string platformName = SocialPlatformManager.SharedInstance.GetPlatformName(platformId);
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

    public static void Popup_OpenMergeConflictCloudCorrupted(SocialUtils.EPlatform platformId, Action onConfirm)
    {
        // Alternative: "You can't use this facebook account because its cloud save is corrupted."
        Popup_OpenSyncGenericError(platformId, SYNC_GENERIC_ERROR_CODE_MERGE_CLOUD_SAVE_CORRUPTED, onConfirm);
    }

    public static void Popup_OpenMergeConflictLocalCorrupted(SocialUtils.EPlatform platformId, Action onConfirm)
    {
        // Local save corrupted when syncing
        // Alternative: "Your local save is corrupted, do you want to override it with the cloud save?"
        Popup_OpenSyncGenericError(platformId, SYNC_GENERIC_ERROR_CODE_MERGE_LOCAL_SAVE_CORRUPTED, onConfirm);
    }

    public static void Popup_OpenMergeConflictBothCorrupted(SocialUtils.EPlatform platformId, Action onConfirm)
    {
        // Alternative "Both saves are corrupted, reset local save?"
        Popup_OpenSyncGenericError(platformId, SYNC_GENERIC_ERROR_CODE_MERGE_BOTH_SAVES_CORRUPTED, onConfirm);
    }

    /// <summary>
    /// Called when there was an attempt to sync while a sync is already being performed
    /// </summary>
    /// <param name="onConfirm"></param>
    public static void Popup_SyncAlreadyOn(SocialUtils.EPlatform platformId, Action onConfirm)
    {
        Popup_OpenSyncGenericError(platformId, SYNC_GENERIC_ERROR_CODE_SYNC_ALREADY_PERFORMING, onConfirm);
    }

    public static void Popup_OpenMergeWithADifferentAccount(SocialUtils.EPlatform platformId, Action onConfirm, Action onCancel)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_PROFILE_CONFLICT_MERGE_CHOOSE_TITLE";
        config.MessageTid = "TID_SAVE_WARN_CLOUD_SWITCH_DESC";
        string platformName = SocialPlatformManager.SharedInstance.GetPlatformName(platformId);
        config.MessageParams = new string[] { platformName };
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = onConfirm;
        config.OnCancel = onCancel;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }

    private static SocialUtils.EPlatform m_implicitMergePlatform;
    private static PopupController m_implicitMergeConflictPopupController = null;
    private static Action m_implicitMergeConflictPopupOnRestore;
    private static Action<bool> m_implicitMergeConflictPopupOnKeep;
    private static bool m_implicitMergeConflictPopupNeedsToPopRequest;

    private static void Popup_ImplicitMergeConflictReset()
    {
        m_implicitMergePlatform = SocialUtils.EPlatform.None;
        m_implicitMergeConflictPopupController = null;
        m_implicitMergeConflictPopupOnRestore = null;
        m_implicitMergeConflictPopupOnKeep = null;
        m_implicitMergeConflictPopupNeedsToPopRequest = false;
    }

    public static void Popup_OpenErrorWhenForcingLocalProgressInImplicitMergeConflict(Action _onRestore, Action _onKeep)
    {
        // Check params
        Debug.Assert(_onRestore != null && _onKeep != null, "Both _onRestore and _onKeep callbacks must be defined!");

        // Initialize popup
        IPopupMessage.Config config = IPopupMessage.GetConfig();

        // Button setup
        config.IsButtonCloseVisible = false;
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.ConfirmAndExtra;
        config.OnConfirm = _onRestore;
        config.OnExtra = _onKeep;
        config.BackButtonStrategy = IPopupMessage.Config.EBackButtonStratety.PerformExtra;
        config.HighlightButton = IPopupMessage.Config.EHighlightButton.Confirm;
        
        // Texts setup        
        config.TitleTid = "TID_DNA_MERGE_ERROR_TITLE";   // Something went wrong!
        config.MessageTid = "TID_DNA_MERGE_ERROR_MESSAGE"; // Current progress couldn't be saved into our servers.\n\nDo you want to keep your current progres (Cloud Save disabled) or go back to your previous progress (Cloud Save enabled)?\n\nCloud Save can still be activated in the Game Settings at any time.
        config.ConfirmButtonTid = "TID_DNA_MERGE_ERROR_BUTTON_1"; // Recover previous progress
        config.ExtraButtonTid = "TID_DNA_MERGE_ERROR_BUTTON_2"; // Keep current progress
                
        // Open popup!
        // It's stored so it can be closed later on if an extra popup (no connection) needs to be prompted on the top of this one
        m_implicitMergeConflictPopupController = PopupManager.PopupMessage_Open(config);
    }

    private static void Popup_OnKeepingAtImplicitMergeConflict()
    {
        PersistenceCloudDriver cloudDriver = instance.CloudDriver;
        
        Action<PersistenceStates.ESyncResult, PersistenceStates.ESyncResultDetail> onSyncDone = delegate (PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail detail)
        {            
            bool finishFlow = true;

            //result = PersistenceStates.ESyncResult.ErrorSyncing;
            switch (result)
            {
                case PersistenceStates.ESyncResult.ErrorLogging:                    
                    switch (detail)
                    {
                        case PersistenceStates.ESyncResultDetail.NoConnection:
                            // This case is treated automatically
                            finishFlow = false;
                            break;

                        case PersistenceStates.ESyncResultDetail.NoLogInSocial:
                            finishFlow = false;

                            if (m_implicitMergeConflictPopupController != null)
                            {
                                m_implicitMergeConflictPopupController.Close(true);
                                m_implicitMergeConflictPopupController = null;
                            }

                            Action onKeep = delegate ()
                            {
                                // User chooses to keep local progress anyway 
                                Popup_ImplicitMergeConflictOnKeep(false);                                                               
                            };                           

                            // Show popup notifying that there's been an irrecoverable error on server side
                            // User needs to choose between local progress with cloud service disabled and
                            // recovering remote progress with cloud service enabled
                            Popup_OpenErrorWhenForcingLocalProgressInImplicitMergeConflict(Popup_ImplicitConflictOnRestore, onKeep);
                            break;
                    }
                    break;
            }

            if (finishFlow)
            {
                // Show a generic error
                if(result != PersistenceStates.ESyncResult.Ok)
                {
                    Popup_OpenSyncGenericError(m_implicitMergePlatform, SYNC_GENERIC_ERROR_CODE_SYNC_UNEXPECTED_FLOW, null);
                }

                // Close popup
                if (m_implicitMergeConflictPopupController != null)
                {
                    m_implicitMergeConflictPopupController.Close(true);
                    m_implicitMergeConflictPopupController = null;
                }

                Popup_ImplicitMergeConflictOnKeep(true);               
            }
        };

        bool prevValue = m_implicitMergeConflictPopupNeedsToPopRequest;

        m_implicitMergeConflictPopupNeedsToPopRequest = true;

        // For automatic social platforms the user is allowed to override the server Id associated to the platform user Id        
        // Sync is restarted stating that force is allowed
        cloudDriver.Sync(m_implicitMergePlatform, PersistenceCloudDriver.ESyncMode.UpToMerge, PersistenceCloudDriver.EErrorMode.Verbose, false, onSyncDone, true, !prevValue);
    }   

    private static void Popup_ImplicitMergeConflictOnKeep(bool success)
    {
        if (m_implicitMergeConflictPopupNeedsToPopRequest)
        {
            instance.CloudDriver.Sync_PopRequest();
        }

        if (m_implicitMergeConflictPopupOnKeep != null)
        {
            m_implicitMergeConflictPopupOnKeep(success);
        }        

        Popup_ImplicitMergeConflictReset();
    }

    private static void Popup_ImplicitConflictOnRestore()
    {
        if (m_implicitMergeConflictPopupNeedsToPopRequest)
        {
            instance.CloudDriver.Sync_PopRequest();
        }

        if (m_implicitMergeConflictPopupOnRestore != null)
        {
            m_implicitMergeConflictPopupOnRestore();
        }        

        Popup_ImplicitMergeConflictReset();
    }

    public static void Popup_OpenImplicitMergeConflict(SocialUtils.EPlatform _plaftormId, PersistenceComparatorSystem _localProgress,
            PersistenceComparatorSystem _cloudProgress, Action<bool> _onKeep, Action _onRestore) {
        // Check params
        Debug.Assert(_localProgress != null && _cloudProgress != null, "Both _localProgress and _cloudProgress must be defined!");
        Debug.Assert(_onRestore != null && _onKeep != null, "Both _onRestore and _onKeep callbacks must be defined!");

        // Store some data
        m_implicitMergePlatform = _plaftormId;
        m_implicitMergeConflictPopupOnRestore = _onRestore;
        m_implicitMergeConflictPopupOnKeep = _onKeep;

        // Initialize popup
        PopupController popup = PopupManager.LoadPopup(PopupMergeDNA.PATH);
        PopupMergeDNA mergePopup = popup.GetComponent<PopupMergeDNA>();
        mergePopup.Setup(
            _localProgress,
            _cloudProgress, 
            Popup_OnKeepingAtImplicitMergeConflict, 
            Popup_ImplicitConflictOnRestore
        );

        // Open the popup!
        popup.Open();

        // Store the popup so it can be closed later on if an extra popup (no connection) needs to be prompted on the top of this one
        m_implicitMergeConflictPopupController = popup;
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



	public static void Popup_OpenCloudCorrupted(SocialUtils.EPlatform platformId, Action onContinue, Action onOverride)
	{
        // Internal error is shown.
        Popup_OpenSyncGenericError(platformId, SYNC_GENERIC_ERROR_CODE_SYNC_CLOUD_SAVE_CORRUPTED, onContinue);
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

    /// <summary>
    /// This popup is shown when there was a problem connecting to the store
    /// </summary>    
    public static void Popups_OpenStoreErrorConnection(Action onConfirm)
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_ERROR_STORE_CONNECTION_NAME";
        config.MessageTid = "TID_ERROR_STORE_CONNECTION_DESC";
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
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


