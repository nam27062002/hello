// Application.cs
// Hungry Dragon
// 
// Created by David Germade on 24/08/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

using System;
using System.Collections;
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
    private const int CloudSaveResyncTime = 3;

    /// <summary>
    /// Time in seconds that will force a reauthentication in the social network if the application has been in background longer than this amount of time
    /// </summary>
    private const int SocialNetworkReauthTime = 120;    

    private static bool m_isAlive = true;
    public static bool IsAlive { 
    	get{ return m_isAlive; }
    }

    /// <summary>
	/// Initialization. This method will be called only once regardless the amount of times the user is led to the Loading scene.
	/// </summary>
	protected void Awake()
    {                
		m_isAlive = true;

#if !PRODUCTION
        DebugSettings.Init();
#endif
        Setting_Init();

        Reset();
        
        FGOL.Plugins.Native.NativeBinding.Instance.DontBackupDirectory(Application.persistentDataPath);        
        SocialFacade.Instance.Init();
        GameServicesFacade.Instance.Init();

        SocialManager.Instance.Init();

        // This class needs to know whether or not the user is in the middle of a game
        Messenger.AddListener(GameEvents.GAME_COUNTDOWN_STARTED, Game_OnCountdownStarted);
        Messenger.AddListener<bool>(GameEvents.GAME_PAUSED, Game_OnPaused);
        Messenger.AddListener(GameEvents.GAME_ENDED, Game_OnEnded);

        SaveFacade.Instance.OnLoadStarted += OnLoadStarted;
        SaveFacade.Instance.OnLoadComplete += OnLoadComplete;        

        // [DGR] NOTIF: Not supported yet
        //NotificationManager.Instance.Init();        

        // [DGR] GAME_VALIDATOR: Not supported yet
        // GameValidator gv = new GameValidator();
        //gv.StartBuildValidation();        
    }

    protected void Start()
    {
        StartCoroutine(Device_Update());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        m_isAlive = false;
    }

	protected void OnApplicationQuit()
    {
        m_isAlive = false;
        Messenger.Broadcast(GameEvents.APPLICATION_QUIT);
    }


    private void Reset()
    {
        LastPauseTime = -1;
        NeedsToRestartFlow = false;
        SaveLoadIsCompleted = false;
        Game_IsInGame = false;
        Game_IsPaused = false;
        Debug_IsPaused = false;        
    }

    public bool NeedsToRestartFlow { get; set; }

    private bool SaveLoadIsCompleted { get; set; }
    
    protected void Update()
    {        
        // To Debug
        /*if (Input.GetKeyDown(KeyCode.A))
        {
            // Simulation of quality/get response from server
            string deviceModel = "server";
            GameFeatureSettingsManager.instance.Device_Model = deviceModel;
            DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.FEATURE_DEVICE_SETTINGS, deviceModel);
            GameFeatureSettingsManager.instance.SetupCurrentFeatureSettings(def.ToJSON());

            // The client is notified that some quality settings might have changed
            Messenger.Broadcast(GameEvents.CP_QUALITY_CHANGED);
            
            //NeedsToRestartFlow = true;           
            //Debug_ToggleIsPaused();

            //Settings_SetSoundIsEnabled(!Settings_GetSoundIsEnabled(), true);
            //Debug.Log("eggs collected = " + UsersManager.currentUser.eggsCollected);            
        }*/            
        
        if (NeedsToRestartFlow)
        {
            NeedsToRestartFlow = false;
            FlowManager.Restart();
        }        
    }

    private int LastPauseTime { get; set; }

    public void OnApplicationPause(bool pause)
    {
        // If the save stuff done in the first loading is not done then the pause is ignored
        if (SaveLoadIsCompleted)
        {
            int currentTime = Globals.GetUnixTimestamp();
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

                SaveFacade.Instance.Save(null, false);
            }
            else
            {
                if (allowGameRestart)
                {
                    // [DGR] NOTIF Not supported yet           
                    //NotificationManager.Instance.CheckNotifications(delegate ()
                    {
                        if (SocialManager.Instance.IsUser(SocialFacade.Network.Default))
                        {
                            if (LastPauseTime != -1)
                            {
                                int timePaused = currentTime - LastPauseTime;
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

            // [DGR] ANALYTICS not supported yet
            // HSXAnalyticsManager.Instance.OnApplicationPause(pause);
        }
    }

    private void OnLoadStarted()
    {
        SaveLoadIsCompleted = false;
    }

    private void OnLoadComplete()
    {
        SaveLoadIsCompleted = true;
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

    #region settings
    // This region is responsible for managing option settings such as sound

    private const string SETTINGS_SOUND_KEY = "sound";

    private bool m_settingsSoundIsEnabled;

    private void Setting_Init()
    {
        // Sound is disabled by default
        Settings_SetSoundIsEnabled(PlayerPrefs.GetInt(SETTINGS_SOUND_KEY, 0) > 0, false);
    }

    public bool Settings_GetSoundIsEnabled()
    {
        return m_settingsSoundIsEnabled;
    }

    private void Settings_SetSoundIsEnabled(bool value, bool persist)
    {
        m_settingsSoundIsEnabled = value;

        // TODO: To use AudioManager instead
        AudioListener.pause = !m_settingsSoundIsEnabled;

        if (persist)
        {
            int intValue = (m_settingsSoundIsEnabled) ? 1 : 0;
            PlayerPrefs.SetInt(SETTINGS_SOUND_KEY, intValue);
            PlayerPrefs.Save();
        }
    }

    public void Settings_ToggleSoundIsEnabled()
    {
        Settings_SetSoundIsEnabled(!Settings_GetSoundIsEnabled(), true);
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

    #region debug
    private bool Debug_IsPaused { get; set; }

    private void Debug_ToggleIsPaused()
    {
        Debug_IsPaused = !Debug_IsPaused;
        OnApplicationPause(Debug_IsPaused);
    }
    #endregion    
}

