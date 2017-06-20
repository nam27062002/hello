/// <summary>
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
        None,
        SettingUp,
        Running,
        Disabled
    }

    private EState State { get; set; }

    public TrackingSaveSystem TrackingSaveSystem { get; set; }

    /// <summary>
    /// Whether or not Calety Tracking Manager has already been initialised. It should be initialised only once
    /// </summary>
    private bool IsCaletyManagerInitialised { get; set; }

    private HDTrackingManager()
    {
        IsCaletyManagerInitialised = false;
        Reset();
    }

    private void Reset()
    {
        State = EState.None;

        if (TrackingSaveSystem == null)
        {
            TrackingSaveSystem = new TrackingSaveSystem();
        }
    }

    private void OnLoadComplete()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("OnLoadComplete");
        }

        // No tracking for hackers since their sessions are not representative
        if (SaveFacade.Instance.userSaveSystem.isHacker)
        {
            State = EState.Disabled;            
        }
        else 
        {                        
            if (!IsCaletyManagerInitialised)
            {                                
                InitCaletyManager();
            }

            CheckAndGenerateUserID();

            // This is the first event that needs to be sent and it should be sent only once
            TrackStartSessionEvent();

            State = EState.Running;
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
            }        
        }
    }

    public void Init()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Init");
        }

        if (State == EState.None)
        {
            State = EState.SettingUp;

            // We need to wait for the persistence to be loaded since it contains the tracking user id and the tracking session count
            SaveFacade.Instance.OnLoadComplete += OnLoadComplete;
        }       
    }

    private void InitCaletyManager()
    {
        if (!IsCaletyManagerInitialised)
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

                int sessionNumber = TrackingSaveSystem.SessionCount;
                string trackingID = TrackingSaveSystem.UserID;
                Debug.Log("SessionNumber = " + sessionNumber + " trackingID = " + trackingID);

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

            IsCaletyManagerInitialised = true;
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

    private void Log(string msg)
    {        
        Debug.Log("[HDTrackingManager] " + msg);
    }
}
