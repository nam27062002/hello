// Application.cs
// Hungry Dragon
// 
// Created by David Germade on 24/08/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

using UnityEngine;

/// <summary>
/// This class is responsible for handling stuff related to the whole application in a high level. For example if an analytics event has to be sent when the application is paused or resumed
/// you should send that event from here. It also offers a place where to initialize stuff only once regardless the amount of times the flow leads the user to the Loading scene.
/// </summary>
public class ApplicationManager : SingletonMonoBehaviour<ApplicationManager>
{
    /// <summary>
    /// Time in seconds that will force a cloud save resync if the application has been in background longer than this amount of time
    /// </summary>
    private const int CloudSaveResyncTime = 3;

    /// <summary>
    /// Time in seconds that will force a reauthentication in the social network if the application has been in background longer than this amount of time
    /// </summary>
    private const int SocialNetworkReauthTime = 120;

    /// <summary>
	/// Initialization. This method will be called only once regardless the amount of times the user is led to the Loading scene.
	/// </summary>
	protected void Awake()
    {
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
        if (Input.GetKeyDown(KeyCode.A))
        {
            NeedsToRestartFlow = true;           
            //Debug_ToggleIsPaused();
        }     
        
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

    #region debug
    private bool Debug_IsPaused { get; set; }

    private void Debug_ToggleIsPaused()
    {
        Debug_IsPaused = !Debug_IsPaused;
        OnApplicationPause(Debug_IsPaused);
    }
    #endregion
}

