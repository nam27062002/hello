// Application.cs
// Hungry Dragon
// 
// Created by David Germade on 24/08/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
/// <summary>
/// This class is responsible for handling stuff related to the whole application in a high level. For example if an analytics event has to be sent when the application is paused or resumed
/// you should send that event from here. It also offers a place where to initialize stuff only once regardless the amount of times the flow leads the user to the Loading scene.
/// </summary>
public class ApplicationManager : UbiBCN.SingletonMonoBehaviour<ApplicationManager>, IBroadcastListener
{
    /// <summary>
    /// Time in seconds that will force a cloud save resync if the application has been in background longer than this amount of time
    /// </summary>
    private const long CloudSaveResyncTime = 3;

    /// <summary>
    /// Time in seconds that will force a reauthentication in the social network if the application has been in background longer than this amount of time
    /// </summary>
    private const long SocialNetworkReauthTime = 120;

    private const string GC_ON_START_KEY = "gc_on_start";

    private static bool m_isAlive = true;
    public static bool IsAlive {
        get { return m_isAlive; }
    }

    public enum Mode
    {
    	PLAY,
    	TEST
    };
    private Mode m_appMode = Mode.PLAY;
    public Mode appMode
    {
    	get{ return m_appMode; }
    }    

    /// <summary>
	/// Initialization. This method will be called only once regardless the amount of times the user is led to the Loading scene.
	/// </summary>
	protected void Awake()
    {
		Application.targetFrameRate = 30;

        // Frame rate forced to 30 fps to make the experience in editor as similar to the one on device as possible
#if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
#endif

        m_isAlive = true;

        if (FeatureSettingsManager.IsDebugEnabled)
        {
            DebugSettings.Init();
        }

        Reset();

        FGOL.Plugins.Native.NativeBinding.Instance.DontBackupDirectory(Application.persistentDataPath);                

        // This class needs to know whether or not the user is in the middle of a game
        Messenger.AddListener(MessengerEvents.GAME_COUNTDOWN_STARTED, Game_OnCountdownStarted);
        Broadcaster.AddListener(BroadcastEventType.GAME_PAUSED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
        Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);

        Device_Init();

        GameCenter_Init();

        // [DGR] GAME_VALIDATOR: Not supported yet
        // GameValidator gv = new GameValidator();
        //gv.StartBuildValidation();        
        ExceptionManager.SharedInstance.AddCrashDelegate(new HDExceptionListener());
    }

    protected void Start()
    {
		// Initialize game settings
		GameSettings.Init();

		if (HasArg("-start_test"))
		{	
			// Start Testing game!
			// ControlPanel.instance.ShowMemoryUsage = true;
				// Tell control panel to show memory
			m_appMode = Mode.TEST;
		}

        // Subscribe to external events
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        }

		CancelLocalNotifications();

        StartCoroutine(Device_Update());
    }

	private bool HasArg(string _argName) 
	{
		string[] args = PlatformUtils.Instance.GetCommandLineArgs();
		if ( args != null )
		{
			for(int i = 0; i < args.Length; i++) {
				if(args[i] == _argName) {
					return true;
				}
			}
		}
		return false;
	}

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Messenger.RemoveListener(MessengerEvents.GAME_COUNTDOWN_STARTED, Game_OnCountdownStarted);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_PAUSED, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
        Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);

        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        }

        m_isAlive = false;

        GameServerManager.SharedInstance.Destroy();
        HDCustomizerManager.instance.Destroy();
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    Debug_OnLevelReset();
            }break;
            case BroadcastEventType.GAME_ENDED:
            {
                    Game_OnEnded();
                    if (FeatureSettingsManager.IsDebugEnabled)
                        Debug_OnLevelReset();
            }break;
            case BroadcastEventType.LANGUAGE_CHANGED:
            {
                    Language_OnChanged();
            }break;
            case BroadcastEventType.GAME_PAUSED:
            {
                Game_OnPaused( (broadcastEventInfo as ToggleParam).value );
            }break;
        }
    }

    protected override void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");

        // Tracking session has to be finished when the application is closed
        HDTrackingManager.Instance.Notify_ApplicationEnd();
        
        //PersistenceManager.Save();

        PersistenceFacade.instance.Destroy();
        Device_Destroy();

        HDAddressablesManager.Instance.Reset();

        m_isAlive = false;
        Messenger.Broadcast(MessengerEvents.APPLICATION_QUIT);

        // This needs to be called once all stuff is done, otherwise this singleton will be marked as destroyed and some other stuff won't
        // be able to access it
        base.OnApplicationQuit();
    }    

    private void Reset()
    {
        LastPauseTime = -1;
        NeedsToRestartFlow = false;        
        Game_IsInGame = false;
        Game_IsPaused = false;
        Debug_IsPaused = false;
        Language_Reset();
    }

    public bool NeedsToRestartFlow { get; set; }    

    protected void Update()
    {        
#if UNITY_EDITOR        
        if (Input.GetKeyDown(KeyCode.A))
        {
            // ---------------------------
            // Test eggs collected
            //Debug_TestEggsCollected();
            // ---------------------------       

            // ---------------------------
            // Test feature settings from server
            //Debug_TestFeatureSettingsFromServer();
            // ---------------------------            

            // ---------------------------
            // Test feature settings
            //Debug_TestFeatureSettingsTypeData();
            // ---------------------------                        

            // ---------------------------
            // Test restart flow
            //Debug_RestartFlow();
            // ---------------------------            

            // ---------------------------
            // Test toggle pause
            //Debug_ToggleIsPaused();
            // ---------------------------

            // ---------------------------
            // Test feature settings
            //Debug_TestToggleSound();
            // ---------------------------     

            // ---------------------------
            // Test drunk effect
            //Debug_TestToggleDrunk();
            // ---------------------------     

            // ---------------------------
            // Test frame color effect
            //Debug_TestToggleFrameColor();
            // ---------------------------    

            // ---------------------------
            // Test quality settings
            //Debug_TestQualitySettings();
            // ---------------------------         

            // ---------------------------
            // Test user profile level
            //Debug_TestUserProfileLevel();
            // ---------------------------

            // ---------------------------
            // Test toggling entities visibility
            //Debug_TestToggleEntitiesVisibility();
            // ---------------------------        

            // ---------------------------
            // Test toggling particles visibility
            //Debug_TestToggleParticlesVisibility();
            // ---------------------------        

            // ---------------------------
            // Test toggling particles culling
            //Debug_TestToggleParticlesCulling();
            // ---------------------------        

            // ---------------------------
            // Test toggling player particles visibility
            //Debug_TestTogglePlayerParticlesVisibility();
            // ---------------------------        

            // ---------------------------
            // Test toggling player particles visibility
            //Debug_TestToggleCustomParticlesCullingEnabled();
            // ---------------------------        

            // ---------------------------
            // Test toggling profiler memory scene
            //Debug_ToggleProfilerMemoryScene();
            // ---------------------------

            // ---------------------------
            // Test toggling profiler load scenes scene
            //Debug_ToggleProfilerLoadScenesScene();
            // ---------------------------

            // ---------------------------
            // Test schedule notification
            //Debug_ScheduleNotification();
            // ---------------------------

            // ---------------------------
            // Test send play test
            //Debug_OnSendPlayTest();
            // ---------------------------

            // ---------------------------
            // Test player's progress
            // Debug_TestPlayerProgress();
            // ---------------------------

            // ---------------------------
            // Test persistence save
            //Debug_TestPersistenceSave();
            // ---------------------------        

            // ---------------------------
            // Test social platform with/without age protection
            //Debug_TestSocialPlatformToggleAgeProtection();
            // ---------------------------        

            // ---------------------------
            // Test CP2 interstitial
            //Debug_TestCP2Interstitial();
            // ---------------------------        

        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            //GameSessionManager.RemoveKeys();
            //PersistencePrefs.Clear();
        }
#endif
		UnityEngine.Profiling.Profiler.BeginSample("ApplicationManager.Update()");

		UnityEngine.Profiling.Profiler.BeginSample("Language.Update()");
        Language_Update();
		UnityEngine.Profiling.Profiler.EndSample();


        UnityEngine.Profiling.Profiler.BeginSample("PersistenceFacade.Update()");
        PersistenceFacade.instance.Update();
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("HDTrackingManager.Update()");
        HDTrackingManager.Instance.Update();
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("HDCustomizerManager.Update()");
        HDCustomizerManager.instance.Update();        
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("GameServerManager.Update()");
        GameServerManager.SharedInstance.Update();
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("HDAddressablesManager.Update()");
        HDAddressablesManager.Instance.Update();
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("GameStoreManager.Update()");
        GameStoreManager.SharedInstance.Update();
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("ChestManager.Update()");
        ChestManager.instance.Update();
		UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("OffersManager.Update()");
        OffersManager.instance.Update();
		UnityEngine.Profiling.Profiler.EndSample();

        #if UNITY_IOS
		UnityEngine.Profiling.Profiler.BeginSample("HDNotificationsManager.Update()");
        HDNotificationsManager.instance.Update();
		UnityEngine.Profiling.Profiler.EndSample();
	
        #endif

        UnityEngine.Profiling.Profiler.BeginSample("TransactionManager.Update()");
        TransactionManager.instance.Update();
        UnityEngine.Profiling.Profiler.EndSample();


        UnityEngine.Profiling.Profiler.BeginSample("BackButtonManager.Update()");
        BackButtonManager.instance.Update();
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("MissionManager.Update()");
        MissionManager.instance.Update();
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("RewardManager.Update()");
        RewardManager.instance.Update();
        UnityEngine.Profiling.Profiler.EndSample(); 

        UnityEngine.Profiling.Profiler.BeginSample("GameSceneManager.Update()");
        GameSceneManager.instance.Update();
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("EggManager.Update()");
        EggManager.instance.Update();
        UnityEngine.Profiling.Profiler.EndSample();


        if (NeedsToRestartFlow)
        {
            NeedsToRestartFlow = false;
            
            // The user is sent to the initial loading again
            FlowManager.Restart();
        }        

        UnityEngine.Profiling.Profiler.BeginSample("Debug.Update()");
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            // Boss camera effect cheat to be able to enable/disable anywhere. We want to be able to check the impact in performance of the effect so we want to have
            // time to close the console before the effect starts playing
            if (Debug_TimeToEnableBossCameraEffect > 0f)
            {
                Debug_TimeToEnableBossCameraEffect -= Time.deltaTime;
                if (Debug_TimeToEnableBossCameraEffect <= 0f)
                {
                    GameCamera gameCamera = InstanceManager.gameCamera;
                    if (gameCamera != null && Debug_BossCameraAffector != null)
                    {
                        bool enabled = !Debug_BossCameraAffector.enabled;
                        Debug_BossCameraAffector.enabled = enabled;

                        if (enabled)
                        {
                            InstanceManager.gameCamera.NotifyBoss(Debug_BossCameraAffector);
                        }
                        else
                        {
                            InstanceManager.gameCamera.RemoveBoss(Debug_BossCameraAffector);
                        }
                    }

                    Debug_BossCameraAffector = null;
                    Debug_TimeToEnableBossCameraEffect = 0f;
                }
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();


        UnityEngine.Profiling.Profiler.EndSample();
    }

    private long LastPauseTime { get; set; }

    // It has to be an IEnumerator to increase unsent events chances of being sent
    public IEnumerator OnApplicationPause(bool pause)
    {                
        Debug.Log("OnApplicationPause " + pause);

        // Unsent events shouldn't be stored when the game is getting paused because the procedure might take longer than the time that the OS concedes and if the procedure
        // doesn't finish then events can get lost (HDK-1897)
        HDTrackingManager.Instance.SaveOfflineUnsentEventsEnabled = !pause;

        GameSettings.OnApplicationPause(pause);

        // We need to notify the tracking manager before saving the progress so that any data stored by the tracking manager will be saved too
        if (pause)
        {
        	if ( GameAds.isInstanceCreated && GameAds.instance.IsWaitingToPlayAnAd())
            {
                if ( GameAds.instance.GetAdType() != AdProvider.AdType.Interstitial )
        		    GameAds.instance.StopWaitingToPlayAnAd();
            }

           HDTrackingManager.Instance.Notify_ApplicationPaused();
           ScheduleLocalNotifications();
        }
        else
        {
            HDTrackingManager.Instance.Notify_ApplicationResumed();
            CancelLocalNotifications();
            
            if ( GameCenterManager.SharedInstance.CheckIfInitialised() )
            {
                bool auth = GameCenterManager.SharedInstance.CheckIfAuthenticated();
            }
            
        }

        // If the persistences are not being synced then we need to make sure the local progress will be stored when going to pause
        if (!PersistenceFacade.instance.Sync_IsSyncing)
        {            
            bool allowGameRestart = true;
            if ((FlowManager.IsInGameScene() && !Game_IsInGame) || Game_IsPaused)
            {
                allowGameRestart = false;
            }

            if (pause)
            {
                if (allowGameRestart)
                {
                    LastPauseTime = Globals.GetUnixTimestamp();
                }
                else
                {
                    LastPauseTime = -1;
                }

                // [DGR] NOTIF Not supported yet
                //NotificationManager.Instance.ScheduleReEngagementNotifications();

                PersistenceFacade.instance.Save_Request(true);
            }
            else
            {
                if (allowGameRestart)
                {
                    // [DGR] NOTIF Not supported yet           
                    //NotificationManager.Instance.CheckNotifications(delegate ()
                    {
                        /*
                        if (SocialManager.Instance.IsUser(SocialFacade.Network.Default))
                        {
                            if (LastPauseTime != -1)
                            {
                                long currentTime = Globals.GetUnixTimestamp();
                                long timePaused = currentTime - LastPauseTime;
                                if (timePaused >= CloudSaveResyncTime && SaveFacade.Instance.cloudSaveEnabled)
                                {
                                    SaveFacade.Instance.OnAppResume();
                                }
                                else if (timePaused >= SocialNetworkReauthTime)
                                {
                                    SocialManager.Instance.OnAppResume(true);
                                }
                                else
                                {
                                    SocialManager.Instance.OnAppResume(false);
                                }

                                LastPauseTime = -1;
                            }
                            else
                            {
                                SocialManager.Instance.OnAppResume(false);
                            }
                        }
                        */
                    }
                    // [DGR] NOTIF Not supported yet           
                    //);
                    // DGR PUSH not supported yet
                    /*
#if ENABLE_PUSHWOOSH
                                        PushNotificationFacade.Instance.ClearNotifications();
#endif
                    */
                }
            }

            // [DGR] AUDIO not supported yet
            /*
            if (Game_IsInGame)
            {
                // if playing a level we need to pause the audio. it doesn't matter if the app will lose or get the focus, in any case we need to stop the audio.
                // PLEASE NOTE: we don't want to unpause the audio because that will be the dismission of the pause popup that will take care of it.
                if (pause)
                {
                    AudioManager.PauseInGameAudio(true);
                }
            }
            */

            // [DGR] STORE not supported yet
            /*
            if (m_storeManager != null)
            {
                m_storeManager.OnApplicationPause(pause);
            }
            */            
        }

        return null;     
    }        

    private void ScheduleLocalNotifications()
    {
		if ( UsersManager.currentUser != null )
        {
        	// Mission notifications
			bool waiting = false;
			double seconds = 0;

            UserMissions userMissions = UsersManager.currentUser.userMissions;
            if (userMissions != null)
            {
                for (Mission.Difficulty i = Mission.Difficulty.EASY; i < Mission.Difficulty.COUNT; i++)
                {
                    if (userMissions.ExistsMission(i))
                    {
                        Mission m = userMissions.GetMission(i);
                        if (m.state == Mission.State.COOLDOWN)
                        {
                            waiting = true;
                            if (m.cooldownRemaining.TotalSeconds > seconds)
                                seconds = m.cooldownRemaining.TotalSeconds;
                        }
                    }
                }
            }

			if ( waiting && seconds > 0)
			{
                HDNotificationsManager.instance.ScheduleNewMissionsNotification((int)seconds);
			}
			
			// Chests notification
			Chest[] chests = UsersManager.currentUser.dailyChests;
			if (chests != null) 
			{
				int max = chests.Length;
				bool missingChests = false;
				for (int i = 0; i < max && !missingChests; i++) 
				{
					if (chests[i] != null && chests[i].state == Chest.State.COLLECTED)
						missingChests = true;
				}
				if (missingChests) 
				{
                    int moreSeconds = 9 * 60 * 60;  // 9 AM
                    int timeToNotification = (int)ChestManager.timeToReset.TotalSeconds + moreSeconds;
                    if ( timeToNotification > 0) {
					    HDNotificationsManager.instance.ScheduleNewChestsNotification (timeToNotification);
                    }
				}
			}

			// Daily reward notification
            if (UsersManager.currentUser.dailyRewards.CanCollectNextReward())
            {
                // reward pending
            }
            else
            {
                // time to reward
                System.DateTime midnight = UsersManager.currentUser.dailyRewards.nextCollectionTimestamp;
                double secondsToMidnight = (midnight - System.DateTime.Now).TotalSeconds;
                int moreSeconds = 9 * 60 * 60;  // 9 AM
                int timeToNotification = (int)secondsToMidnight + moreSeconds;
                if ( timeToNotification > 0 )
                {
                    HDNotificationsManager.instance.ScheduleNewDailyReward (timeToNotification);
                }
            }

			// Free Offer
			// Only when on cooldown
			if(OffersManager.isFreeOfferOnCooldown) {
				// Avoid notification during night
				DateTime endTimeLocal = DateTime.Now.Add(OffersManager.freeOfferRemainingCooldown);
				endTimeLocal = HDNotificationsManager.AvoidSilentHours(endTimeLocal);
				int remainingSeconds = (int)(endTimeLocal - DateTime.Now).TotalSeconds;
				HDNotificationsManager.instance.ScheduleNewFreeOffer(remainingSeconds);
			}
            DefinitionNode gameSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
            int minutesToReengage = gameSettingsDef.GetAsInt("notificationComeBackTimer", 2000);
            int secondsToReengage = minutesToReengage * 60;
            if ( secondsToReengage > 0 )
            {
                System.DateTime dateTime = System.DateTime.Now.AddSeconds( secondsToReengage );
                if ( dateTime.Hour >= 22 || dateTime.Hour <= 9 )
                {
                    // Adjust to avoid midnight timmings
                    System.DateTime fixedDateTime = dateTime;
                    fixedDateTime = fixedDateTime.AddHours( 11 );   // forbidden 11 hours, from 22:00 to 9:00
                    fixedDateTime = fixedDateTime.AddHours( 9 - fixedDateTime.Hour );   // Remove excess
                    secondsToReengage = (int)(fixedDateTime - System.DateTime.Now).TotalSeconds;
                }
                
                HDNotificationsManager.instance.ScheduleReengagementNotification(secondsToReengage);
            }

            // Schedule Egg Hatching notifications
            if (EggManager.incubatingEgg != null)
            {
                if(EggManager.incubatingEgg.isIncubating) {
                    int secondsToFinish =  (int)EggManager.incubatingEgg.incubationRemaining.TotalSeconds;
                    if ( secondsToFinish > 0 )
                    {   
                        HDNotificationsManager.instance.ScheduleEggHatchedNotification(secondsToFinish);
                    }
                }
            }

			// [AOC] TODO!!
        }
    }

    private void CancelLocalNotifications()
    {
		HDNotificationsManager.instance.CancelNewMissionsNotification();
		HDNotificationsManager.instance.CancelNewChestsNotification();
		HDNotificationsManager.instance.CancelDailyRewardNotification();
		HDNotificationsManager.instance.CancelFreeOfferNotification();
        HDNotificationsManager.instance.CancelReengagementNotification();
        HDNotificationsManager.instance.CancelEggHatchedNotification();
    }

#region game
    private bool Game_IsInGame { get; set; }

    private void Game_OnCountdownStarted()
    {
        Game_IsInGame = true;
    }

    private void Game_OnEnded()
    {
        Game_IsInGame = false;
    }

    private bool Game_IsPaused { get; set; }

    private void Game_OnPaused(bool value)
    {
        Game_IsPaused = value;
    }

    
#endregion

#region device   
    // Time in seconds to wait until the device has to be updated again.
    public const float DEVICE_NEXT_UPDATE = 0.5f;

    /// <summary>
    /// Current resolution
    /// </summary>
    public Vector2Int Device_Resolution = new Vector2Int(0,0);

    /// <summary>
    /// Current device orientation
    /// </summary>
    public DeviceOrientation Device_Orientation { get; private set; }

	private void Device_Init() 
	{
        // When this is enabled the user will be allowed to enable the vertical orientation on the control panel
        if (FeatureSettingsManager.IsVerticalOrientationEnabled)
        {
            Device_CalculateOrientation();

            // When this is enabled the user will be allowed to enable the vertical orientation on the control panel
            if (FeatureSettingsManager.IsVerticalOrientationEnabled)
            {
                Messenger.AddListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, Device_OnOrientationSettingsChanged);
            }
        }
	}

	private void Device_Destroy() 
	{
		if (FeatureSettingsManager.IsVerticalOrientationEnabled) 
		{
			Messenger.RemoveListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, Device_OnOrientationSettingsChanged);
		}
	}

	private void Device_CalculateOrientation() 
	{
		ScreenOrientation screenOrientation = Screen.orientation;
		bool verticalOrientationIsAllowed = DebugSettings.verticalOrientation;
		Screen.orientation = (verticalOrientationIsAllowed) ? ScreenOrientation.AutoRotation : ScreenOrientation.Landscape;
	}
		
	private void Device_OnOrientationSettingsChanged(string _key, bool value) 
	{
		if (_key == DebugSettings.VERTICAL_ORIENTATION) 
		{
			Device_CalculateOrientation();
		}
	}

    private IEnumerator Device_Update()
    {
#if UNITY_EDITOR
        Device_Resolution = new Vector2Int(Screen.width, Screen.height);
#else
        Device_Resolution = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
#endif

        Device_Orientation = Input.deviceOrientation;

        WaitForSeconds wait = new WaitForSeconds(DEVICE_NEXT_UPDATE);

        while (IsAlive)
        {

#if UNITY_EDITOR
            // Check for a Resolution Change
            if (Device_Resolution.x != Screen.width || Device_Resolution.y != Screen.height)
            {
                Device_Resolution.x = Screen.width;
                Device_Resolution.y = Screen.width;
#else
            // Check for a Resolution Change
            if (Device_Resolution.x != Screen.currentResolution.width || Device_Resolution.y != Screen.currentResolution.height)
            {
                Device_Resolution.x = Screen.currentResolution.width;
                Device_Resolution.y = Screen.currentResolution.height;
#endif
                Messenger.Broadcast<Vector2>(MessengerEvents.DEVICE_RESOLUTION_CHANGED, Device_Resolution);
            }

            // Check for an Orientation Change
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.Unknown:            // Ignore
                case DeviceOrientation.FaceUp:            // Ignore
                case DeviceOrientation.FaceDown:        // Ignore
                    break;
                default:
                    if (Device_Orientation != Input.deviceOrientation)
                    {
                        Device_Orientation = Input.deviceOrientation;
                        Messenger.Broadcast<DeviceOrientation>(MessengerEvents.DEVICE_ORIENTATION_CHANGED, Device_Orientation);
                    }
                    break;
            }

            yield return wait;
        }
    }
#endregion

#region language
    private string m_languageRequested;
    private bool m_languageNeedsToBeUpdated;

    private void Language_Reset()
    {
        m_languageNeedsToBeUpdated = true;
        m_languageRequested = null;
    }

    private void Language_OnChanged()
    {
        m_languageNeedsToBeUpdated = true;
    }

    private void Language_OnSetInServer(FGOL.Server.Error error, GameServerManager.ServerResponse response)
    {
        // It's stored only if the server has stored it successfully
        if (error == null && !string.IsNullOrEmpty(m_languageRequested))
        {
            PersistencePrefs.SetServerLanguage(m_languageRequested);            
        }

        m_languageRequested = null;
    }

    private void Language_Update()
    {
        // We need to way for ContentManager to be ready in order to make sure that current laguange has been loaded in LocalizationManager
        // Checks that language has changed and there's no a request already being processed
        if (ContentManager.m_ready && m_languageNeedsToBeUpdated && m_languageRequested == null)
        {
            m_languageNeedsToBeUpdated = false;

            string currentLanguage = LocalizationManager.SharedInstance.GetCurrentLanguageSKU();
            string serverLanguage = PersistencePrefs.GetServerLanguage();
            if (currentLanguage != serverLanguage)
            {
                DefinitionNode langDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LOCALIZATION, currentLanguage);
                if (langDef != null)
                {
                    string serverCode = langDef.Get("serverCode");
                    if (!string.IsNullOrEmpty(serverCode))
                    {
                        m_languageRequested = currentLanguage;
                        GameServerManager.SharedInstance.SetLanguage(serverCode, Language_OnSetInServer);
                    }
                }
            }
        }
    }
#endregion

#region memory_profiler
    private bool m_memoryProfilerIsEnabled = false;
    private bool MemoryProfiler_IsEnabled
    {
        get
        {
            return m_memoryProfilerIsEnabled;
        }

        set
        {
            if (m_memoryProfilerIsEnabled != value)
            {
                m_memoryProfilerIsEnabled = value;
                if (m_memoryProfilerIsEnabled)
                {
                    MemoryProfiler_Enable();
                }
                else
                {
                    MemoryProfiler_Disable();
                }
            }
        }
    }

    private void MemoryProfiler_Enable()
    {
        // Disabled to make sure that all textures in the level are loaded
        FeatureSettingsManager.instance.IsFogOnDemandEnabled = false;
    }

    private void MemoryProfiler_Disable()
    {        
        FeatureSettingsManager.instance.IsFogOnDemandEnabled = true;
    }
#endregion

#region game_center
    // This region is responsible for handling login to the platform (game center or google play)

    private class GameCenterListener : GameCenterManager.GameCenterListenerBase
    {
        public override void onAuthenticationFinished()
        {            
            ControlPanel.Log("onAuthenticationFinished", ControlPanel.ELogChannel.GameCenter);                        

#if UNITY_ANDROID
			// On android if player login we make sure it will try at start again
			CacheServerManager.SharedInstance.DeleteKey(GC_ON_START_KEY, false);
#endif

            GameCenterManager.SharedInstance.RequestUserToken(); // Async process

			Messenger.Broadcast(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE);
        }

        public override void onAuthenticationFailed()
        {            
            ControlPanel.Log("onAuthenticationFailed", ControlPanel.ELogChannel.GameCenter);            
             
			Messenger.Broadcast(MessengerEvents.GOOGLE_PLAY_AUTH_FAILED);
        }

        public override void onAuthenticationCancelled()
        {            
            ControlPanel.Log("onAuthenticationCancelled", ControlPanel.ELogChannel.GameCenter);            

#if UNITY_ANDROID
			// On android if player cancells the authentication we will not ask again
			CacheServerManager.SharedInstance.SetVariable(GC_ON_START_KEY, "false" , false);
#endif
			Messenger.Broadcast(MessengerEvents.GOOGLE_PLAY_AUTH_CANCELLED);
        }

        public override void onUnauthenticated()
        {            
            ControlPanel.Log("onUnauthenticated", ControlPanel.ELogChannel.GameCenter);            

#if UNITY_ANDROID
            // On android if player logs out we will not ask again
            CacheServerManager.SharedInstance.SetVariable(GC_ON_START_KEY, "false", false);
#endif

			Messenger.Broadcast(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE);
        }

        public override void onGetToken(JSONNode kTokenDataJSON)
        {            
            ControlPanel.Log("onGetToken: " + kTokenDataJSON.ToString() +
            " userID = " + GameCenterManager.SharedInstance.GetUserId() +
            " userName = " + GameCenterManager.SharedInstance.GetUserName(), 
            ControlPanel.ELogChannel.GameCenter);            
        }

        public override void onNotAuthenticatedException()
        {            
            ControlPanel.Log("onNotAuthenticatedException", ControlPanel.ELogChannel.GameCenter);            
        }

        public override void onGetAchievementsInfo(Dictionary<string, GameCenterManager.GameCenterAchievement> kAchievementsInfo)
        {            
            ControlPanel.Log("onGetAchievementsInfo", ControlPanel.ELogChannel.GameCenter);            

            foreach (KeyValuePair<string, GameCenterManager.GameCenterAchievement> kEntry in kAchievementsInfo)
            {
                GameCenterManager.GameCenterAchievement kAchievement = (GameCenterManager.GameCenterAchievement)kEntry.Value;

                ControlPanel.Log("-----------------------------------------\nachievement: " + kEntry.Key + "\ndesc: " + kAchievement.m_strDescription + "\npercent: " + kAchievement.m_fPercentComplete + "\nunlocked: " + kAchievement.m_iIsUnlocked + "\ncurrent: " + kAchievement.m_iCurrentAmount + "\namount: " + kAchievement.m_iTotalAmount, ControlPanel.ELogChannel.GameCenter);
            }
        }
        public override void onGetLeaderboardScore(string strLeaderboardSKU, int iScore, int iRank)
        {            
            ControlPanel.Log("onGetLeaderboardScore " + strLeaderboardSKU + " : " + iScore + " , " + iRank, ControlPanel.ELogChannel.GameCenter);            
        }
    }
    private GameCenterListener m_gameCenterListener = null;

    private void GameCenter_Init()
    {
        m_gameCenterListener = new GameCenterListener();

        // Load achievements
		GameCenterManager.GameCenterItemData[] kAchievementsData = null;
		Dictionary<string, DefinitionNode> kAchievementSKUs = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.ACHIEVEMENTS);
		if (kAchievementSKUs != null && kAchievementSKUs.Count > 0)
		{
			kAchievementsData = new GameCenterManager.GameCenterItemData[kAchievementSKUs.Count];
			int iSKUIdx = 0;
			foreach(KeyValuePair<string, DefinitionNode> kEntry in kAchievementSKUs)
			{
				kAchievementsData[iSKUIdx] = new GameCenterManager.GameCenterItemData();
				kAchievementsData[iSKUIdx].m_strSKU = kEntry.Value.Get("sku");
				if (kEntry.Value.Has ("amount"))
				{
					kAchievementsData[iSKUIdx].m_iAmount = kEntry.Value.GetAsInt("amount") / kEntry.Value.GetAsInt("stepSize", 1);			
				}
				else
				{
					kAchievementsData[iSKUIdx].m_iAmount = 1;
				}

				kAchievementsData[iSKUIdx].m_strAppleID = kEntry.Value.Get("appleSku");
				kAchievementsData[iSKUIdx].m_strGoogleID = kEntry.Value.Get("googleSku");
				kAchievementsData[iSKUIdx].m_strAmazonID = kEntry.Value.Get("amazonSku");
				iSKUIdx++;
			}
		}

        GameCenterManager.GameCenterItemData[] leaderboardsData = null;

        // TODO: Load leaderboards

        GameCenterManager.SharedInstance.AddGameCenterListener(m_gameCenterListener);
		GameCenterManager.SharedInstance.Initialise(ref kAchievementsData, ref leaderboardsData);
    }

    public void GameCenter_Login()
    {
#if !UNITY_EDITOR
		GameCenterManager.SharedInstance.AuthenticateLocalPlayer();
#endif
    }

    public void GameCenter_LogOut()
    {
		GameCenterManager.SharedInstance.UnauthenticateLocalPlayer();
    }

    public bool GameCenter_IsAuthenticated()
    {
		return GameCenterManager.SharedInstance.CheckIfAuthenticated();
    }

    public void GameCenter_ShowAchievements()
    {
    	GameCenterManager.SharedInstance.ShowAchievements();
    }

	public void GameCenter_ResetAchievements()
    {
    	GameCenterManager.SharedInstance.ResetAchievements();
    }

    public bool GameCenter_LoginOnStart()
    {
    	bool ret = true;
		if ( CacheServerManager.SharedInstance.HasKey(GC_ON_START_KEY, false) )
    	{
    		ret = false;
    	}
    	return ret;
    }

#endregion

#region apps
    public enum EApp
    {
        HungryDragon,
        HungrySharkEvo
    };

    public static void Apps_OpenAppInStore(EApp app)
    {
        string appId = Apps_GetAppIdInStore(app);
        if (string.IsNullOrEmpty(appId))
        {                      
            LogError("No appId found for app " + app.ToString());            
        }
        else
        {			
            MiscUtils.OpenAppInStore(appId);
        }
    }

    public static string Apps_GetAppIdInStore(EApp app)
    {
        string returnValue = null;

        switch (app)
        {
            case EApp.HungryDragon:
                returnValue = Apps_GetHDIdInStore();
                break;

            case EApp.HungrySharkEvo:
                returnValue = Apps_GetHSEIdInStore();
                break;
        }

        return returnValue;
    }

    private static string Apps_GetHDIdInStore()
    {
		return Application.identifier;
    }

    private static string Apps_GetHSEIdInStore()
    {
        string returnValue = null;
#if UNITY_IOS
        returnValue = "535500008"; //HSE App Store ID
#elif UNITY_ANDROID
        returnValue = "com.fgol.HungrySharkEvolution"; //HSE Google play ID
#endif

        return returnValue;
    }
#endregion

#region exception    
    private class HDExceptionListener : CyExceptions.ExceptionListener
    {
        /// <summary>
        /// Time in seconds that has to pass between two exceptions report
        /// </summary>
        private const float EXCEPTION_TIME_BETWEEN_REPORTS = 5 * 60f;

        /// <summary>
        /// Timestamp of the latest exception reported. It's stored to prevent tracking from getting spammed when an exception is reported every frame
        /// </summary>
        private float m_exceptionLatestTimestamp = 0f;

        public override void OnUnhandledException(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                float timeSinceLastException = Time.realtimeSinceStartup - m_exceptionLatestTimestamp;
                if (timeSinceLastException >= EXCEPTION_TIME_BETWEEN_REPORTS)
                {
                    m_exceptionLatestTimestamp = Time.realtimeSinceStartup;
                    
                    Log("OnUnhandledException logString = " + logString + " stackTrace = " + stackTrace + " type = " + type.ToString());                    
                 
                    HDTrackingManager.Instance.Notify_Crash((type == LogType.Exception), type.ToString(), logString);
                }
            }
        }
    }    
#endregion

#region debug
    private bool Debug_IsPaused { get; set; }

    private void Debug_RestartFlow()
    {
        NeedsToRestartFlow = true;
    }

    private void Debug_ToggleIsPaused()
    {
        Debug_IsPaused = !Debug_IsPaused;
        OnApplicationPause(Debug_IsPaused);
    }

    private void Debug_TestToggleSound()
    {
		GameSettings.Set(GameSettings.SOUND_ENABLED, !GameSettings.Get(GameSettings.SOUND_ENABLED));
    }

    private void Debug_TestEggsCollected()
    {
        Debug.Log("eggs collected = " + UsersManager.currentUser.eggsCollected);
    }

    private void Debug_TestFeatureSettingsFromServer()
    {
        // Simulation of quality/get response from server
        string deviceModel = "server";
        FeatureSettingsManager.instance.Device_Model = deviceModel;
        DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.FEATURE_DEVICE_SETTINGS, deviceModel);
        FeatureSettingsManager.instance.SetupCurrentFeatureSettings(def.ToJSON(), null, null);

        // The client is notified that some quality settings might have changed
        Messenger.Broadcast(MessengerEvents.CP_QUALITY_CHANGED);
    }

    private void Debug_TestFeatureSettingsTypeData()
    {
        /*     
        // Int
        string key = FeatureSettings.KEY_INT_TEST;
        int valueAsInt = FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsInt(key);
        Debug.Log(key + " = " + valueAsInt + " as string = " + FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(key));

        // Float
        key = FeatureSettings.KEY_FLOAT_TEST;
        float valueAsFloat = FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsFloat(key);
        Debug.Log(key + " = " + valueAsFloat + " as string = " + FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(key));        

        // Int range
        key = FeatureSettings.KEY_INT_RANGE_TEST;
        valueAsInt = FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsInt(key);
        Debug.Log(key + " = " + valueAsInt + " as string = " + FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(key));

        // String
        key = FeatureSettings.KEY_PROFILE;
        Debug.Log(key + " as string = " + FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(key));                        
        */
    }

    private int m_DebugUserProfileLevel = -1;
    private void Debug_TestUserProfileLevel()
    {
        int currentUserProfileLevel = FeatureSettingsManager.instance.GetUserProfileLevel();
        int currentProfileLevel = FeatureSettingsManager.instance.GetCurrentProfileLevel();
        int maxProfileLevel = FeatureSettingsManager.instance.GetMaxProfileLevelSupported();
        m_DebugUserProfileLevel = (m_DebugUserProfileLevel + 1) % (maxProfileLevel + 1);
        FeatureSettingsManager.Log("before maxProfileLevel = " + maxProfileLevel + " currentUserProfileLevel = " + currentUserProfileLevel + " currentProfileLevel = " + currentProfileLevel + " to set " + m_DebugUserProfileLevel);        
        FeatureSettingsManager.instance.SetUserProfileLevel(m_DebugUserProfileLevel);
        FeatureSettingsManager.Log("after currentUserProfileLevel = " + FeatureSettingsManager.instance.GetUserProfileLevel() + " currentProfileLevel = " + FeatureSettingsManager.instance.GetCurrentProfileLevel());
    }


    private bool Debug_IsFrameColorOn { get; set; }

    public void Debug_TestToggleFrameColor()
    {
        Debug_IsFrameColorOn = !Debug_IsFrameColorOn;
        FuryRushToggled furyRushToggled = new FuryRushToggled();
        furyRushToggled.activated = Debug_IsFrameColorOn;
        furyRushToggled.type = DragonBreathBehaviour.Type.Mega;
        furyRushToggled.color = FireColorSetupManager.FireColorType.RED;
        Broadcaster.Broadcast(BroadcastEventType.FURY_RUSH_TOGGLED, furyRushToggled);
    }

    private bool Debug_IsBakedLightsDisabled { get; set; }
    private List<Light> m_lightList = null;
    private List<MeshRenderer> m_renderers = null;
    private void disableBakedLights(bool value)
    {
        for (int c = 0; c < m_lightList.Count; c++)
        {
            if (m_lightList[c].type != LightType.Directional)
            {
                m_lightList[c].gameObject.SetActive(value);
            }
        }

        for (int c = 0; c < m_renderers.Count; c++)
        {
            m_renderers[c].receiveShadows = value;
            m_renderers[c].shadowCastingMode = value ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    public void Debug_DisableBakedLights(bool value)
    {
        if (m_lightList == null)
        {
            m_lightList = GameObjectExt.FindObjectsOfType<Light>(true);
        }

        if (m_renderers == null)
        {
            m_renderers = GameObjectExt.FindObjectsOfType<MeshRenderer>(true);

        }
        Debug_IsBakedLightsDisabled = !Debug_IsBakedLightsDisabled;
        disableBakedLights(Debug_IsBakedLightsDisabled);
    }

    private bool Debug_IsCollidersDisabled { get; set; }
    private List<MeshCollider> m_CollidersList = null;
    private void disableColliders(bool value)
    {
        for (int c = 0; c < m_CollidersList.Count; c++)
        {
            m_CollidersList[c].gameObject.SetActive(value);
        }
    }

    public void Debug_DisableColliders(bool value)
    {
        if (m_CollidersList == null)
        {
            m_CollidersList = GameObjectExt.FindObjectsOfType<MeshCollider>(true);
        }
        Debug_IsCollidersDisabled = !Debug_IsCollidersDisabled;
        disableColliders(Debug_IsCollidersDisabled);
    }



	//---------------------------------------------------------------------------------------------------
	public void Debug_DisableMeshesAt(float _distance) {
		if (m_renderers == null) {
			m_renderers = GameObjectExt.FindObjectsOfType<MeshRenderer>(true);
		}

		GameCamera camera = Camera.main.GetComponent<GameCamera>();
		if (camera != null) {
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
			for (int i = 0; i < m_renderers.Count; ++i) {
				bool isActive = true;
				if (_distance > 0f) {
					if (!GeometryUtility.TestPlanesAABB(planes, m_renderers[i].bounds)) {					
						float dist = Vector3.Distance(m_renderers[i].bounds.center, camera.position);
						isActive = dist < _distance;
					}
				}
				m_renderers[i].gameObject.SetActive(isActive);
			}
		}
	}
	//---------------------------------------------------------------------------------------------------



    public void Debug_TestQualitySettings()
    {
        FeatureSettingsManager.instance.Debug_Test();
    }

    private float Debug_TimeToEnableBossCameraEffect { get; set; }

    private BossCameraAffector Debug_BossCameraAffector { get; set; }

    public void Debug_OnToggleBossCameraEffect(BossCameraAffector affector)
    {
        if (affector != null)
        {
            GameCamera gameCamera = InstanceManager.gameCamera;
            if (gameCamera != null)
            {
                // If the timer hasn't expired yet then it means that the operation scheduled hasn't been performed yet so we just need to cancel the operation scheduled by setting the timer to 0
                if (Debug_TimeToEnableBossCameraEffect > 0f)
                {
                    Debug_TimeToEnableBossCameraEffect = 0f;
                    Debug_BossCameraAffector = null;
                }
                else
                {
                    Debug_BossCameraAffector = affector;
                    Debug_TimeToEnableBossCameraEffect = 5f;
                }
            }
        }
    }

    private void Debug_TestToggleEntitiesVisibility()
    {
        if (EntityManager.instance != null)
        {
            EntityManager.instance.Debug_EntitiesVisibility = !EntityManager.instance.Debug_EntitiesVisibility;
        }
    }

    private ParticleSystem[] m_debugParticles;
    private bool m_debugParticlesVisibility = true;
    public bool Debug_ParticlesVisibility
    {
        get
        {
            return m_debugParticlesVisibility;
        }

        set
        {
            if (m_debugParticlesVisibility && !value)
            {
                m_debugParticles = GameObject.FindObjectsOfType<ParticleSystem>();
            }

            m_debugParticlesVisibility = value;
                        
            if (m_debugParticles != null)
            {
                int count = m_debugParticles.Length;                
                for (int i = 0; i < count; i++)
                {
                    m_debugParticles[i].gameObject.SetActive(m_debugParticlesVisibility);                    
                }
            }
        }
    }

    private void Debug_TestToggleParticlesVisibility()
    {
        Debug_ParticlesVisibility = !Debug_ParticlesVisibility;
    }

    private enum EDebugParticlesState
    {
        Playing,
        Paused,
        Stopped
    }

    private EDebugParticlesState m_debugParticlesState = EDebugParticlesState.Playing;
    private EDebugParticlesState Debug_ParticlesState
    {
        get
        {
            return m_debugParticlesState;
        }

        set
        {
            m_debugParticlesState = value;
             
            ParticleSystem[] systems = GameObject.FindObjectsOfType<ParticleSystem>();
            if (systems != null)
            {
                int count = systems.Length;
                for (int i = 0; i < count; i++)
                {
                    switch (m_debugParticlesState)
                    {
                        case EDebugParticlesState.Playing:
                            systems[i].Play();
                            break;

                        case EDebugParticlesState.Paused:
                            systems[i].Pause();
                            break;

                        case EDebugParticlesState.Stopped:
                            systems[i].Stop();
                            break;

                    }                   
                }
            }
        }
    }   

    public void Debug_SetParticlesState(int option)
    {
        EDebugParticlesState state = (EDebugParticlesState)option;
        Debug_ParticlesState = state;
    }

    private bool Debug_PlayParticlesCulling { get; set; }    

    private void Debug_TestToggleParticlesCulling()
    {
        Debug_PlayParticlesCulling = !Debug_PlayParticlesCulling;
        CustomParticlesCulling.Manager_SimulateForAll(Debug_PlayParticlesCulling, !Debug_PlayParticlesCulling);        
    }

    private bool m_debug_GroundVisibility = true;
    public bool Debug_GroundVisibility
    {
        get
        {
            return m_debug_GroundVisibility;
        }

        set
        {
            m_debug_GroundVisibility = value;

            GameCamera gameCamera = InstanceManager.gameCamera;
            if (gameCamera != null)
            {
                Camera camera = gameCamera.GetComponent<Camera>();
                if (camera != null)
                {
                    int newMask = camera.cullingMask;
                    int groundVisibleLayer = LayerMask.NameToLayer("GroundVisible");
                    if (m_debug_GroundVisibility)
                    {
                        newMask |= 1 << groundVisibleLayer;
                    }
                    else
                    {
                        newMask &= ~(1 << groundVisibleLayer);
                    }

                    camera.cullingMask = newMask;
                }                
            }            
        }
    }


    private bool m_debug_PlayerParticlesVisibility = true;    
    public bool Debug_PlayerParticlesVisibility
    {
        get
        {
            return m_debug_PlayerParticlesVisibility;
        }

        set
        {
            m_debug_PlayerParticlesVisibility = value;

            DragonPlayer player = InstanceManager.player;
            if (player != null)
            {
                ParticleSystem[] systems = player.GetComponentsInChildren<ParticleSystem>(true);
                if (systems != null)
                {
                    int count = systems.Length;
                    for (int i = 0; i < count; i++)
                    {
                        systems[i].gameObject.SetActive(m_debug_PlayerParticlesVisibility);
                    }
                }
            }
        }
    }

    private void Debug_TestTogglePlayerParticlesVisibility()
    {
        Debug_PlayerParticlesVisibility = !Debug_PlayerParticlesVisibility;
    }

    private void Debug_TestToggleCustomParticlesCullingEnabled()
    {
        CustomParticlesCulling.Manager_IsEnabled = !CustomParticlesCulling.Manager_IsEnabled;
    }

    public void Debug_ToggleProfilerMemoryScene()
    {
        if (GameSceneManager.nextScene == ProfilerMemoryController.NAME)
        {
            FlowManager.GoToMenu();
        }
        else
        {
            FlowManager.GoToProfilerMemoryScene();
        }
    }

    private void Debug_LoadProfilerScenesScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(ProfilerLoadScenesController.NAME);
    }

    public void Debug_ToggleProfilerLoadScenesScene()
    {
        if (GameSceneManager.nextScene == ProfilerLoadScenesController.NAME)
        {
            FlowManager.GoToMenu();
        }
        else
        {
            Debug_LoadProfilerScenesScene();
        }
    }

    public void Debug_ScheduleNotification()
    {
        HDNotificationsManager.instance.ScheduleEggHatchedNotification(5);
        HDNotificationsManager.instance.ScheduleNewMissionsNotification(10);
    }

    private void Debug_OnLevelReset()
    {
        m_debugParticles = null;
        m_debugParticlesVisibility = true;
    }

    private void Debug_TestPlayerProgress()
    {
        Debug.Log("player progress = " + UsersManager.currentUser.GetPlayerProgress());
    }

    private void Debug_TestPersistenceSave()
    {
        PersistenceFacade.instance.Save_Request();
    }

	public void Debug_TestPlayAd() 
	{
		GameAds.instance.ShowRewarded(GameAds.EAdPurpose.UPGRADE_MAP, Debug_OnAdResult);
	}

	private void Debug_OnAdResult(bool success) 
	{
		Debug.Log("OnAdPlayed result = " + success);
	}

    private bool m_debugUseAgeProtection = false;
    private void Debug_TestSocialPlatformToggleAgeProtection()
    {
        m_debugUseAgeProtection = !m_debugUseAgeProtection;        
        NeedsToRestartFlow = true;
    }

    private void Debug_TestCP2Interstitial()
    {
        HDCP2Manager.Instance.PlayInterstitial(false, null);
    }

    private const string LOG_CHANNEL = "[ApplicationManager]";

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private static void Log(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.Log(msg);
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private static void LogWarning(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.LogWarning(msg);
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private static void LogError(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.LogError(msg);
    }
#endregion
}

