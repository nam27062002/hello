﻿/// <summary>
/// This class is responsible to handle any Hungry Dragon related stuff needed for tracking. It uses Calety Tracking support to send tracking events
/// </summary>

using UnityEngine;
public class HDTrackingManager
{
    // Singleton ///////////////////////////////////////////////////////////
    private static HDTrackingManager smInstance = null;

    public static HDTrackingManager Instance
    {
        get
        {
            if (smInstance == null)
            {
                smInstance = new HDTrackingManager();
            }

            return smInstance;
        }
    }
    //////////////////////////////////////////////////////////////////////////

    private enum EState
    {        
        WaitingForSessionStart,  
        SessionStarted,      
        Disabled
    }
    
    private EState State { get; set; }
        
    public TrackingSaveSystem TrackingSaveSystem { get; set; }
    
    private bool IsStartSessionNotified { get; set; }

    private bool IsDNAInitialised { get; set; }

    private HDTrackingManager()
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

    private void InitCaletyManager()
    {        
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("InitCaletyManager");
        }

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

    private void StartSession()
    {
        Log("StartSession");
        State = EState.SessionStarted;

        // Session counter advanced
        TrackingSaveSystem.SessionCount++;

        CheckAndGenerateUserID();

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
            Log("SessionNumber = " + sessionNumber + " trackingID = " + trackingID);

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
            kNewEventToTrack.SetParameterValue("gameVersion", "Release");            
            TrackingManager.SharedInstance.SendEvent(kNewEventToTrack);
        }
    }

    private void TrackEndSessionEvent()
    {
    }

    public void Update()
    {
        switch (State)
        {
            case EState.WaitingForSessionStart:
                // We need to wait for the tracking save system to be ready because it might contain the tracking user id
                if (TrackingSaveSystem != null && TrackingSaveSystem.IsReady && IsStartSessionNotified)
                {                    
                    // No tracking for hackers because their sessions will be misleading
                    if (SaveFacade.Instance.userSaveSystem.isHacker)
                    {
                        State = EState.Disabled;
                    }
                    else
                    {
                        StartSession();
                    }
                }
                break;
        }
    }
    
    #region notify
    public void NotifyStartSession()
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
                case EState.Disabled:
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

    public void NotifyEndSession()
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
    #endregion    

    #region log
    public static void Log(string msg)
    {
        Debug.Log("<color=cyan>[HDTrackingManager] " + msg + " </color>");
    }

    private void LogError(string msg)
    {
        Debug.LogError("[HDTrackingManager] " + msg);
    }
    #endregion
}
