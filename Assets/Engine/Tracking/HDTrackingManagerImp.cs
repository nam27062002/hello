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
                Log("Generate User ID = " + TrackingSaveSystem.UserID);
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

        Log("StartSession");
        State = EState.SessionStarted;

        CheckAndGenerateUserID();

        // Session counter advanced
        TrackingSaveSystem.SessionCount++;              

        // Calety needs to be initialized every time a session starts because the session count has changed
        StartCaletySession();

        // Sends the start session event
        TrackStartSessionEvent();        
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

            Log("SessionNumber = " + sessionNumber + " trackingID = " + trackingID + " userId = " + userID + " socialUserID = " + socialUserID);           

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

    private void TrackStartSessionEvent()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("TrackStartSessionEvent");
        }

        // Custom start session
        TrackingManager.TrackingEvent kNewEventToTrack = TrackingManager.SharedInstance.GetNewTrackingEvent("01_START_SESSION");
        if (kNewEventToTrack != null)
        {
            TrackingManager.SharedInstance.SendEvent(kNewEventToTrack);
        }

        // DNA start session
        kNewEventToTrack = TrackingManager.SharedInstance.GetNewTrackingEvent("game.start");
        if (kNewEventToTrack != null)
        {            
            kNewEventToTrack.SetParameterValue("SubVersion", "SoftLaunch");
            TrackAddParamProviderAuthToEvent(kNewEventToTrack);
            TrackAddParamPlayerIDToEvent(kNewEventToTrack);
            TrackAddParamServerAccIDToEvent(kNewEventToTrack);
            TrackingManager.SharedInstance.SendEvent(kNewEventToTrack);
        }
    }    

    private void TrackEndSessionEvent()
    {
    }

    private void TrackAddParamProviderAuthToEvent(TrackingManager.TrackingEvent e)
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

    private void TrackAddParamPlayerIDToEvent(TrackingManager.TrackingEvent e)
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

    private void TrackAddParamServerAccIDToEvent(TrackingManager.TrackingEvent e)
    {
        int value = 0;
        if (TrackingSaveSystem != null)
        {
            value = TrackingSaveSystem.AccountID;
        }

        e.SetParameterValue("InGameId", value);
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
    }

    #region notify
    public override void NotifyStartSession()
    {        
        Log("NotifyStartSession");

        if (State == EState.WaitingForSessionStart)
        {
            IsStartSessionNotified = true;
        }
        else if (FeatureSettingsManager.IsDebugEnabled)
        {
            switch (State)
            {
                case EState.Banned:
                    Log("Tracking session won't be started because the user has been labelled as cheater");
                    break;

                case EState.SessionStarted:
                    LogError("Finish a tracking session before starting another");
                    break;

                default:
                    LogError("A tracking session can't be started when state is " + State);
                    break;
            }
        }        
    }

    public override void NotifyEndSession()
    {       
        Log("NotifyEndSession");

        if (FeatureSettingsManager.IsDebugEnabled)
        {
            switch (State)
            {
                case EState.WaitingForSessionStart:
                    LogError("No tracking session has been started");
                    break;
            }
        }

        IsStartSessionNotified = false;
        State = EState.WaitingForSessionStart;        
    }

    /// <summary>
    /// This method is called when the user starts a round
    /// </summary>
    /// <param name="playerProgress">An int value that sums up the user's progress</param>
    public override void NotifyStartRound(int playerProgress)
    {

    }

    /// <summary>
    /// This method is called when the user finishes a round
    /// </summary>
    /// <param name="playerProgress">An int value that sums up the user's progress</param>
    public override void NotifyEndRound(int playerProgress)
    {

    }
    #endregion        
}

