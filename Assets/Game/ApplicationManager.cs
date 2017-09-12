﻿// Application.cs
// Hungry Dragon
// 
// Created by David Germade on 24/08/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class is responsible for handling stuff related to the whole application in a high level. For example if an analytics event has to be sent when the application is paused or resumed
/// you should send that event from here. It also offers a place where to initialize stuff only once regardless the amount of times the flow leads the user to the Loading scene.
/// </summary>
public class ApplicationManager : UbiBCN.SingletonMonoBehaviour<ApplicationManager>
{
    /// <summary>
    /// Time in seconds that will force a cloud save resync if the application has been in background longer than this amount of time
    /// </summary>
    private const long CloudSaveResyncTime = 3;

    /// <summary>
    /// Time in seconds that will force a reauthentication in the social network if the application has been in background longer than this amount of time
    /// </summary>
    private const long SocialNetworkReauthTime = 120;

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
        // Frame rate forced to 30 fps to make the experience in editor as similar to the one on device as possible
#if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 30;
#endif

        m_isAlive = true;

        if (FeatureSettingsManager.IsDebugEnabled)
        {
            DebugSettings.Init();
        }

        Reset();

        FGOL.Plugins.Native.NativeBinding.Instance.DontBackupDirectory(Application.persistentDataPath);
        //SocialFacade.Instance.Init();
        GameServicesFacade.Instance.Init();

        SocialManager.Instance.Init();

        // This class needs to know whether or not the user is in the middle of a game
        Messenger.AddListener(GameEvents.GAME_COUNTDOWN_STARTED, Game_OnCountdownStarted);
        Messenger.AddListener<bool>(GameEvents.GAME_PAUSED, Game_OnPaused);
        Messenger.AddListener(GameEvents.GAME_ENDED, Game_OnEnded);        

        Notifications_Init();

		Device_Init();

        GameCenter_Init();

        // [DGR] GAME_VALIDATOR: Not supported yet
        // GameValidator gv = new GameValidator();
        //gv.StartBuildValidation();        
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
        Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, Debug_OnLevelReset);
        Messenger.AddListener(GameEvents.GAME_ENDED, Debug_OnLevelReset);

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

        m_isAlive = false;
    }

    protected override void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");

        // Tracking session has to be finished when the application is closed
        HDTrackingManager.Instance.Notify_ApplicationEnd();

        //PersistenceManager.Save();

        PersistenceFacade.instance.Destroy();
        Device_Destroy();
        
        m_isAlive = false;
        Messenger.Broadcast(GameEvents.APPLICATION_QUIT);

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
    }

    public bool NeedsToRestartFlow { get; set; }    

    protected void Update()
    {
        // To Debug
        if (FeatureSettingsManager.IsDebugEnabled)
        {
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
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                //GameSessionManager.RemoveKeys();
                //PersistencePrefs.Clear();
            }
        }

        PersistenceFacade.instance.Update();
        HDTrackingManager.Instance.Update();        

		#if UNITY_EDITOR
		GameServerManager.SharedInstance.Update();
		#endif

        if (NeedsToRestartFlow)
        {
            NeedsToRestartFlow = false;
            
            // The user is sent to the initial loading again
            FlowManager.Restart();
        }        

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
    }

    private long LastPauseTime { get; set; }

    public void OnApplicationPause(bool pause)
    {
        Debug.Log("OnApplicationPause " + pause);

        // We need to notify the tracking manager before saving the progress so that any data stored by the tracking manager will be saved too
        if (pause)
        {
            HDTrackingManager.Instance.Notify_ApplicationPaused();
        }
        else
        {
            HDTrackingManager.Instance.Notify_ApplicationResumed();
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
    public Vector2 Device_Resolution { get; private set; }

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
                Messenger.AddListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Device_OnOrientationSettingsChanged);
            }
        }
	}

	private void Device_Destroy() 
	{
		if (FeatureSettingsManager.IsVerticalOrientationEnabled) 
		{
			Messenger.RemoveListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Device_OnOrientationSettingsChanged);
		}
	}

	private void Device_CalculateOrientation() 
	{
		ScreenOrientation screenOrientation = Screen.orientation;
		bool verticalOrientationIsAllowed = Prefs.GetBoolPlayer(DebugSettings.VERTICAL_ORIENTATION, false);
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
        Device_Resolution = new Vector2(Screen.width, Screen.height);
        Device_Orientation = Input.deviceOrientation;

        while (IsAlive)
        {
            // Check for a Resolution Change
            if (Device_Resolution.x != Screen.width || Device_Resolution.y != Screen.height)
            {
                Device_Resolution = new Vector2(Screen.width, Screen.height);
                Messenger.Broadcast<Vector2>(GameEvents.DEVICE_RESOLUTION_CHANGED, Device_Resolution);
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
                        Messenger.Broadcast<DeviceOrientation>(GameEvents.DEVICE_ORIENTATION_CHANGED, Device_Orientation);
                    }
                    break;
            }

            yield return new WaitForSeconds(DEVICE_NEXT_UPDATE);
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

    #region
    private void Notifications_Init()
    {        
        NotificationsManager.SharedInstance.Initialise();

        // [DGR] TODO: icons has to be created and located in the right folder
#if UNITY_ANDROID
        NotificationsManager.SharedInstance.SetNotificationIcons ("", "push_notifications", 0xFFFF0000); 
#endif
		int strLanguageSku = PlayerPrefs.GetInt(PopupSettings.KEY_SETTINGS_NOTIFICATIONS, 1);
        NotificationsManager.SharedInstance.SetNotificationsEnabled( strLanguageSku > 0 );
    }
    #endregion

    #region game_center
    // This region is responsible for handling login to the platform (game center or google play)

    private class GameCenterListener : GameCenterManager.GameCenterListenerBase
    {
        public override void onAuthenticationFinished()
        {
            Debug.Log("GameCenterDelegate onAuthenticationFinished");

            GameCenterManager.SharedInstance.RequestUserToken(); // Async process

			Messenger.Broadcast(EngineEvents.GOOGLE_PLAY_STATE_UPDATE);
        }

        public override void onAuthenticationFailed()
        {
            Debug.Log("GameCenterDelegate onAuthenticationFailed");
        }

        public override void onAuthenticationCancelled()
        {
            Debug.Log("GameCenterDelegate onAuthenticationCancelled");
        }

        public override void onUnauthenticated()
        {
            Debug.Log("GameCenterDelegate onUnauthenticated");
			Messenger.Broadcast(EngineEvents.GOOGLE_PLAY_STATE_UPDATE);
        }

        public override void onGetToken(JSONNode kTokenDataJSON)
        {
            Debug.Log("GameCenterDelegate onGetToken: " + kTokenDataJSON.ToString() + 
                " userID = " + GameCenterManager.SharedInstance.GetUserId() + 
                " userName = " + GameCenterManager.SharedInstance.GetUserName());
        }

        public override void onNotAuthenticatedException()
        {
            Debug.Log("GameCenterDelegate onNotAuthenticatedException");
        }

        public override void onGetAchievementsInfo(Dictionary<string, GameCenterManager.GameCenterAchievement> kAchievementsInfo)
        {
            Debug.Log("GameCenterListener: onGetAchievementsInfo");

            foreach (KeyValuePair<string, GameCenterManager.GameCenterAchievement> kEntry in kAchievementsInfo)
            {
                GameCenterManager.GameCenterAchievement kAchievement = (GameCenterManager.GameCenterAchievement)kEntry.Value;

                Debug.Log("-----------------------------------------\nachievement: " + kEntry.Key + "\ndesc: " + kAchievement.m_strDescription + "\npercent: " + kAchievement.m_fPercentComplete + "\nunlocked: " + kAchievement.m_iIsUnlocked + "\ncurrent: " + kAchievement.m_iCurrentAmount + "\namount: " + kAchievement.m_iTotalAmount);
            }
        }
        public override void onGetLeaderboardScore(string strLeaderboardSKU, int iScore, int iRank)
        {
            Debug.Log("GameCenterListener: onGetLeaderboardScore " + strLeaderboardSKU + " : " + iScore + " , " + iRank);
        }
    }
    private GameCenterListener m_gameCenterListener = null;

    private void GameCenter_Init()
    {
        m_gameCenterListener = new GameCenterListener();

        GameCenterManager.GameCenterItemData[] achievementsData = null;
        GameCenterManager.GameCenterItemData[] leaderboardsData = null;
        GameCenterManager.SharedInstance.AddGameCenterListener(m_gameCenterListener);
        GameCenterManager.SharedInstance.Initialise(ref achievementsData, ref leaderboardsData);
    }

    public void GameCenter_Login()
    {
		GameCenterManager.SharedInstance.AuthenticateLocalPlayer();
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
        FeatureSettingsManager.instance.SetupCurrentFeatureSettings(def.ToJSON(), null);

        // The client is notified that some quality settings might have changed
        Messenger.Broadcast(GameEvents.CP_QUALITY_CHANGED);
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

    private bool Debug_IsDrunkOn { get; set; }

    public void Debug_TestToggleDrunk()
    {
        Debug_IsDrunkOn = !Debug_IsDrunkOn;
        Messenger.Broadcast<bool>(GameEvents.DRUNK_TOGGLED, Debug_IsDrunkOn);
    }

    private bool Debug_IsFrameColorOn { get; set; }

    public void Debug_TestToggleFrameColor()
    {
        Debug_IsFrameColorOn = !Debug_IsFrameColorOn;
        Messenger.Broadcast<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, Debug_IsFrameColorOn, DragonBreathBehaviour.Type.Mega);
    }

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
        NotificationsManager.SharedInstance.ScheduleNotification("sku.not.01", "A ver que pasa...", "Action", 5);
    }

    private void Debug_OnLevelReset()
    {
        m_debugParticles = null;
        m_debugParticlesVisibility = true;
    }

    private void Debug_OnSendPlayTest()
    {
        if (FeatureSettingsManager.instance.IsMiniTrackingEnabled)
        {
            MiniTrackingEngine.SendTrackingFile(false,
			(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) => 
            {
				if (_error == null)
                {
                    Debug.Log("Play test tracking sent successfully");
                }
                else
                {
                    Debug.Log("Error when sending play test tracking");
                }
            });
        }
    }

    private void Debug_TestPlayerProgress()
    {
        Debug.Log("player progress = " + UsersManager.currentUser.GetPlayerProgress());
    }

    private void Debug_TestPersistenceSave()
    {
        PersistenceFacade.instance.Save_Request();
    }
    #endregion
}

