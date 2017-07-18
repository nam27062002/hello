/// <summary>
/// This class is responsible to handle any Hungry Dragon related stuff needed for tracking. It uses Calety Tracking support to send tracking events
/// </summary>

using UnityEngine;
public class HDTrackingManagerImp : HDTrackingManager
{    
    private enum EState
    {
        WaitingForSessionStart,
        SessionStarted,
        Banned
    }

    private EState State { get; set; }    

    private bool IsStartSessionNotified { get; set; }

    private bool IsDNAInitialised { get; set; }    

    public HDTrackingManagerImp()
    {
        Reset();
    }

    private void Reset()
    {        
        State = EState.WaitingForSessionStart;
        IsStartSessionNotified = false;
        IsDNAInitialised = false;

        if (TrackingSaveSystem == null)
        {
            TrackingSaveSystem = new TrackingSaveSystem();
        }
        else
        {
            TrackingSaveSystem.Reset();
        }        
    }

    private void CheckAndGenerateUserID()
    {
        if (TrackingSaveSystem != null)
        {
            // Generate Analytics user ID if not already set, it cannot be done in init function as we don't know the user ID at that point
            if (string.IsNullOrEmpty(TrackingSaveSystem.UserID))
            {
                // Generate a GUID so that we can identify users over the course of firing multiple events etc.
                TrackingSaveSystem.UserID = System.Guid.NewGuid().ToString();

                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    Log("Generate User ID = " + TrackingSaveSystem.UserID);
                }
            }
        }
    }   

    private void StartSession()
    {     
        if (!IsDNAInitialised)
        {
            InitDNA();
            IsDNAInitialised = true;
        }

        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("StartSession");
        }

        State = EState.SessionStarted;

        CheckAndGenerateUserID();

        // Session counter advanced
        TrackingSaveSystem.SessionCount++;              

        // Calety needs to be initialized every time a session starts because the session count has changed
        StartCaletySession();

        // Sends the start session event
        Track_StartSessionEvent();        
    }

    private void InitDNA()
    {
        CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
        if (settingsInstance != null)
        {
            UbimobileToolkit.UbiservicesEnvironment kDNAEnvironment = UbimobileToolkit.UbiservicesEnvironment.UAT;
            if (settingsInstance.m_iBuildEnvironmentSelected == (int)CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION)
            {
                kDNAEnvironment = UbimobileToolkit.UbiservicesEnvironment.PROD;
            }

#if UNITY_ANDROID
            DNAManager.SharedInstance.Initialise("12e4048c-5698-4e1e-a1d1-c8c2411b2515", settingsInstance.m_strVersionAndroidGplay, kDNAEnvironment);
#elif UNITY_IOS
			DNAManager.SharedInstance.Initialise ("42cbdf99-63e7-4e80-aae3-d05b9533349e", settingsInstance.m_strVersionIOS, kDNAEnvironment);
#endif            
        }
    }

    private void StartCaletySession()
    {
        CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
        if (settingsInstance != null)
        {
            int sessionNumber = TrackingSaveSystem.SessionCount;
            string trackingID = TrackingSaveSystem.UserID;
            string userID = (Authenticator.Instance.User != null) ? Authenticator.Instance.User.ID : "";
            string socialUserID = SocialFacade.Instance.GetSocialIDFromHighestPrecedenceNetwork();

            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("SessionNumber = " + sessionNumber + " trackingID = " + trackingID + " userId = " + userID + " socialUserID = " + socialUserID);
            }

            TrackingManager.TrackingConfig kTrackingConfig = new TrackingManager.TrackingConfig();
            kTrackingConfig.m_eTrackPlatform = TrackingManager.ETrackPlatform.E_TRACK_PLATFORM_OFFLINE;
            kTrackingConfig.m_strJSONConfigFilePath = "Tracking/TrackingEvents";
            kTrackingConfig.m_strStartSessionEventName = "01_START_SESSION";
            kTrackingConfig.m_strEndSessionEventName = "02_END_SESSION";
            kTrackingConfig.m_strMergeAccountEventName = "MERGE_ACCOUNTS";
            kTrackingConfig.m_strClientVersion = settingsInstance.GetClientBuildVersion();
            kTrackingConfig.m_strTrackingID = trackingID;
            kTrackingConfig.m_iSessionNumber = sessionNumber;
            kTrackingConfig.m_iMaxCachedLoggedDays = 3;

            TrackingManager.SharedInstance.Initialise(ref kTrackingConfig);
        }
    }    

    public override void Update()
    {
        switch (State)
        {
            case EState.WaitingForSessionStart:                
                if (TrackingSaveSystem != null && IsStartSessionNotified)
                {
                    // No tracking for hackers because their sessions will be misleading
                    if (SaveFacade.Instance.userSaveSystem.isHacker)
                    {
                        State = EState.Banned;
                    }
                    else
                    {
                        StartSession();
                    }
                }
                break;
        }

        if (TrackingSaveSystem != null && TrackingSaveSystem.IsDirty)
        {
            TrackingSaveSystem.IsDirty = false;
            SaveFacade.Instance.Save();
        }

        if (Session_AnyRoundsStarted)
        {
            Session_PlayTime += Time.deltaTime;
        }

        if (Debug_IsEnabled)
        {
            Debug_Update();
        }
    }

    #region notify
    public override void Notify_ApplicationStart()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Notify_StartSession");
        }

        if (State == EState.WaitingForSessionStart)
        {
            IsStartSessionNotified = true;
        }        
    }

    public override void Notify_ApplicationEnd()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Notify_ApplicationEnd");
        }

        Notify_SessionEnd(ESeassionEndReason.app_closed);

        IsStartSessionNotified = false;
        State = EState.WaitingForSessionStart;       
    }

    public override void Notify_ApplicationPaused()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Notify_ApplicationPaused");
        }

        Notify_SessionEnd(ESeassionEndReason.no_activity);
    }

    public override void Notify_ApplicationResumed()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Notify_ApplicationResumed");
        }

        // If the dna session had been started then it has to be restarted
        if (Session_AnyRoundsStarted)
        {            
            Track_MobileStartEvent();
        }
    }

    private void Notify_SessionEnd(ESeassionEndReason reason)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            string str = "Notify_SessionEnd reason = " + reason + " sessionPlayTime = " + Session_PlayTime;
            if (TrackingSaveSystem != null)
            {
                str += " totalPlayTime = " + TrackingSaveSystem.TotalPlaytime;
            }

            Log(str);
        }
        
        if (TrackingSaveSystem != null)
        {
            // Current session play time is added up to the total
            int sessionTime = (int)Session_PlayTime;
            TrackingSaveSystem.TotalPlaytime += sessionTime;
        }            

        Track_ApplicationEndEvent(reason.ToString());

        // It needs to be reseted after tracking the event because the end application event needs to send the session play time
        Session_PlayTime = 0f;        
    }

    /// <summary>
    /// Called when the user starts a round
    /// </summary>    
    public override void Notify_RoundStart()
    {
        if (!Session_AnyRoundsStarted)
        {
            Session_AnyRoundsStarted = true;
            Track_MobileStartEvent();
        }
    }    

    /// <summary>
    /// Called when the user completed an in app purchase.
    /// </summary>
    public override void Notify_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType)
    {
        Session_IsPayingSession = true;
        Track_IAPCompleted(storeTransactionID, houstonTransactionID, itemID, promotionType);
    }
    #endregion

    #region track
    private void Track_StartSessionEvent()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_StartSessionEvent");
        }        

        // DNA game.start
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("game.start");
        if (e != null)
        {
            Track_AddParamSubVersion(e);
            Track_AddParamProviderAuth(e);
            Track_AddParamPlayerID(e);
            Track_AddParamServerAccID(e);
            TrackingManager.SharedInstance.SendEvent(e);
        }
    }    

    private void Track_ApplicationEndEvent(string stopCause)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_ApplicationEndEvent " + stopCause);
        }        

        // DNA custom.mobile.stop
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.mobile.stop");
        if (e != null)
        {
            Track_AddParamIsPayingSession(e);
            Track_AddParamPlayerProgress(e);
            Track_AddParamSessionPlaytime(e);
            Track_AddParamStopCause(e, stopCause);
            Track_AddParamTotalPlaytime(e);
            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    private void Track_MobileStartEvent()
    {        
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_MobileStartEvent");
        }

        // DNA custom.mobile.stop
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.mobile.start");
        if (e != null)
        {
            Track_AddParamAbTesting(e);
            Track_AddParamPlayerProgress(e);         
            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    private void Track_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_IAPCompleted");
        }

        // DNA custom.mobile.stop
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.player.iap");
        if (e != null)
        {
            e.SetParameterValue("storeTransactionID", "SoftLaunch");            
            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    // -------------------------------------------------------------
    // Params
    // -------------------------------------------------------------

    private void Track_AddParamSubVersion(TrackingManager.TrackingEvent e)
    {
        // "SoftLaunch" is sent so far. It will be changed wto "HardLaunch" after WWL
        e.SetParameterValue("SubVersion", "SoftLaunch");
    }

    private void Track_AddParamProviderAuth(TrackingManager.TrackingEvent e)
    {
        string value = null;
        if (TrackingSaveSystem != null && !string.IsNullOrEmpty(TrackingSaveSystem.SocialPlatform))
        {
            value = TrackingSaveSystem.SocialPlatform;
        }

        if (string.IsNullOrEmpty(value))
        {
            value = "SilentLogin";
        }

        e.SetParameterValue("providerAuth", value);
    }

    private void Track_AddParamPlayerID(TrackingManager.TrackingEvent e)
    {
        string value = null;
        if (TrackingSaveSystem != null && !string.IsNullOrEmpty(TrackingSaveSystem.SocialID))
        {
            value = TrackingSaveSystem.SocialID;
        }

        if (string.IsNullOrEmpty(value))
        {
            value = "NotDefined";
        }

        e.SetParameterValue("playerID", value);
    }

    private void Track_AddParamServerAccID(TrackingManager.TrackingEvent e)
    {
        int value = 0;
        if (TrackingSaveSystem != null)
        {
            value = TrackingSaveSystem.AccountID;
        }

        e.SetParameterValue("InGameId", value);
    }

    private void Track_AddParamIsPayingSession(TrackingManager.TrackingEvent e)
    {
        int value = (Session_IsPayingSession) ? 1 : 0;        
        e.SetParameterValue("isPayingSession", value);
    }

    private void Track_AddParamAbTesting(TrackingManager.TrackingEvent e)
    {        
        e.SetParameterValue("abtesting", "");
    }

    private void Track_AddParamPlayerProgress(TrackingManager.TrackingEvent e)
    {
        int value = (UsersManager.currentUser != null) ? UsersManager.currentUser.GetPlayerProgress() : 0;        
        e.SetParameterValue("playerProgress", value + "");
    }

    private void Track_AddParamSessionPlaytime(TrackingManager.TrackingEvent e)
    {
        int value = (int)Session_PlayTime;
        e.SetParameterValue("sessionPlaytime", value);
    }

    private void Track_AddParamTotalPlaytime(TrackingManager.TrackingEvent e)
    {
        int value = (TrackingSaveSystem != null) ? TrackingSaveSystem.TotalPlaytime : 0;
        e.SetParameterValue("totalPlaytime", value);
    }

    private void Track_AddParamStopCause(TrackingManager.TrackingEvent e, string stopCause)
    {
        e.SetParameterValue("stopCause", stopCause);
    }
    #endregion

    #region session
    // This region is responsible for storing data generated during the current session. These data don't need to be persisted.

    /// <summary>
    /// Enum with the reason why the session has ended. These names have to match the supported values defined for "stopCause" parameter of
    /// "custom.mobile.stop" event described at https://mdc-web-tomcat17.ubisoft.org/confluence/display/dna/Events+Guidelines+for+Mobile+Games#EventsGuidelinesforMobileGames-custom.mobile.start
    /// </summary>
    private enum ESeassionEndReason
    {
        app_closed,
        no_activity
    }

    /// <summary>
    /// Whether or not the user has paid (actual purchase) during the current session
    /// </summary>
    private bool Session_IsPayingSession { get; set; }

    /// <summary>
    /// Current session duration (in seconds) so far. It has to start being accumulated after the first game round
    /// </summary>
    private float Session_PlayTime { get; set; }

    /// <summary>
    /// This flag states whether or not the user has started any rounds. This is used to start DNA session. DNA session starts when the user 
    /// starts the first round since the application started
    /// </summary>
    private bool Session_AnyRoundsStarted { get; set; }

    private void Session_Reset()
    {
        Session_IsPayingSession = false;
        Session_PlayTime = 0f;
        Session_AnyRoundsStarted = false;
    }
    #endregion

    #region debug
    private const bool Debug_IsEnabled = true;

    private void Debug_Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Notify_RoundStart();
        }        
    }
    #endregion
}

