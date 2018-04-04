// GameSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main controller for the game scene.
/// </summary>
public class GameSceneController : GameSceneControllerBase {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string NAME = "SC_Game";
	public const float INITIAL_DELAY = 1f;	// Seconds. Initial delay before actually start the loading. Useful to give time to initialize and load assets for the loading screen.
	public const float COUNTDOWN = 3.5f;	// Seconds. This countdown is used as a safety net if the intro animation does not end or does not send the proper event
	public const float MIN_LOADING_TIME = 1f;	// Seconds, to avoid loading screen flickering

	public enum EStates {
		INIT,
		DELAY,
		LOADING_LEVEL,
		ACTIVATING_LEVEL,
		COUNTDOWN,
		RUNNING,
		FINISHED,
        SHOWING_RESULTS
	};

	bool m_switchingArea = false;
	public bool isSwitchingArea { get { return m_switchingArea; } }

	string m_nextArea = "";
	public enum SwitchingAreaSate
	{
		UNLOADING_SCENES,
		LOADING_SCENES,
		ACTIVATING_SCENES
	};
	SwitchingAreaSate m_switchState;
	private List<AsyncOperation> m_switchingAreaTasks;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed	
	[Space]
    [SerializeField] private GameObject m_uiRoot = null;
    public GameObject uiRoot
    {
        get { return m_uiRoot; }
    }

	[SerializeField] private ShowHideAnimator m_gameUIAnimator = null;

	// Countdown
    public float countdown {
		get {
			if(state == EStates.COUNTDOWN) {
				return m_timer;
			} else {
				return 0f;
			}
		}
	}
	
	// Logic state
	private EStates m_state = EStates.INIT;
	public EStates state {
		get { return m_state; }
	}

	// Pause management
	private float m_timeScaleBackup = 1f;	// When going to pause, store timescale to be restored later on
	private int m_pauseStacks = 0;

	// Level loading
	private AsyncOperation[] m_levelLoadingTasks = null;
	public float levelLoadingProgress {
		get {
			if(state == EStates.LOADING_LEVEL) {
				if(m_levelLoadingTasks == null) return 1f;	// Shouldn't be null at this state
				float progress = 0f;
				for(int i = 0; i < m_levelLoadingTasks.Length; i++) {
					// When allowSceneActivation is set to false then progress is stopped at 0.9. The isDone is then maintained at false. When allowSceneActivation is set to true isDone can complete.
					if(m_levelLoadingTasks[i].allowSceneActivation) {
						progress += m_levelLoadingTasks[i].progress;
					} else {
						progress += m_levelLoadingTasks[i].progress/0.9f;
					}
				}
				return Mathf.Min(progress/m_levelLoadingTasks.Length, 1f - Mathf.Max(m_timer/MIN_LOADING_TIME, 0f));	// Either progress or fake timer
			} else if(state > EStates.LOADING_LEVEL) {
				return 1;
			} else {
				return 0;
			}
		}
	}

	public float levelActivationProgress {
		get {
			if(state == EStates.LOADING_LEVEL || state == EStates.ACTIVATING_LEVEL) {
				if(m_levelLoadingTasks == null) return 1f;	// Shouldn't be null at this state
				float progress = 0f;
				for(int i = 0; i < m_levelLoadingTasks.Length; i++) {
					progress += m_levelLoadingTasks[i].progress;
				}
				return Mathf.Min(progress/m_levelLoadingTasks.Length, 1f - Mathf.Max(m_timer/MIN_LOADING_TIME, 0f));	// Either progress or fake timer
			} else if(state > EStates.ACTIVATING_LEVEL) {
				return 1;
			} else {
				return 0;
			}
		}
	}

	// For the tutorial
	private bool m_startWhenLoaded = true;
	public bool startWhenLoaded {
		get { return m_startWhenLoaded; }
		set { m_startWhenLoaded = value; }
	}

	// Internal
	private float m_timer = -1;	// Misc use
	private float m_loadingTimer = 0;

    private SwitchAsyncScenes m_switchAsyncScenes = new SwitchAsyncScenes();

    TrackerBoostTime m_boostTimeTracker;
    TrackerMapUsage m_mapUsageTracker;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

		m_boostTimeTracker = new TrackerBoostTime();
		m_mapUsageTracker = new TrackerMapUsage();

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		// Make sure loading screen is visible
		LoadingScreen.Toggle(true, false);

		// Check whether the tutorial popup must be displayed
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.CONTROLS_POPUP)
			|| DebugSettings.isPlayTest) {
			// Tracking
			string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupTutorialControls.PATH);
			HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");

			// Open popup
			PopupManager.OpenPopupInstant(PopupTutorialControls.PATH);
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.CONTROLS_POPUP);
		}

		// Load the dragon
		DragonManager.LoadDragon(UsersManager.currentUser.currentDragon);
		Messenger.AddListener(MessengerEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);

		ParticleManager.instance.poolLimits = ParticleManager.PoolLimits.LoadedArea;
	}


	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Let's play!
		StartGame();
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Skip if paused
		if(m_paused) return;        

        // [AOC] Editor utility: open pause popup
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P)) {
			PopupManager.OpenPopupInstant(PopupInGameMap.PATH);
		}
		else if (Input.GetKeyDown(KeyCode.I))
		{
			// Check if in editor!
			bool usingEditor = false;
			InstanceManager.player.StartIntroMovement( usingEditor );
			InstanceManager.gameCamera.StartIntro( usingEditor );
			LevelEditor.LevelTypeSpawners sp = FindObjectOfType<LevelEditor.LevelTypeSpawners>();
			if ( sp != null )
				sp.IntroSpawn(InstanceManager.player.data.def.sku);
		}
		#endif

		// Different actions based on current state
		switch(m_state) {
			case EStates.DELAY: {
				// Just wait for the timer to run off
				if(m_timer > 0) {
					m_timer -= Time.deltaTime;
					if(m_timer <= 0) {
						ChangeState(EStates.LOADING_LEVEL);
					}
				}
			} break;

			// During loading, wait until level is loaded
			case EStates.LOADING_LEVEL: {
				// Update timer
				if(m_timer > 0) {
					m_timer -= Time.deltaTime;
				}

				if(levelLoadingProgress >= 1) {
					ChangeState(EStates.ACTIVATING_LEVEL);
				}
			} break;

			// During activation, wait until all scenes have been activated
			case EStates.ACTIVATING_LEVEL: {
				// All loading tasks must be in the Done state
				bool allDone = true;
				for(int i = 0; i < m_levelLoadingTasks.Length && allDone; i++) {
					allDone &= m_levelLoadingTasks[i].isDone;
				}

				if(allDone) {
					// Change state only if allowed, otherwise it will be manually done
					if(m_startWhenLoaded) ChangeState(EStates.COUNTDOWN);
				}
			} break;

			// During countdown, let's just wait
			case EStates.COUNTDOWN: {
				if(m_timer > 0) {
					m_timer -= Time.deltaTime;
					if(m_timer <= 0) {
						Messenger.Broadcast(MessengerEvents.GAME_COUNTDOWN_ENDED);
						// ChangeState(EStates.RUNNING);
					}
				}
			} break;
				
			case EStates.RUNNING: {
				// Update running time
				m_elapsedSeconds += Time.deltaTime;

				// Dynamic loading
				if ( m_switchingArea )
				{
					switch( m_switchState )
					{
						case SwitchingAreaSate.UNLOADING_SCENES:
						{
							bool done = true;
							if ( m_switchingAreaTasks != null )
							{
								for( int i = 0; i<m_switchingAreaTasks.Count && done; i++ )
								{
									if ( !m_switchingAreaTasks[i].isDone )
									{
										done = false;
									}
								}
							}

							if (done)
							{
                                Resources.UnloadUnusedAssets();
                                System.GC.Collect();
                                                                  
                                m_switchingAreaTasks = LevelManager.LoadArea(m_nextArea);

								ParticleManager.Rebuild();

								if ( m_switchingAreaTasks != null )
								{
									for(int i = 0; i < m_switchingAreaTasks.Count; i++) {
										m_switchingAreaTasks[i].allowSceneActivation = false;
									}
								}
								m_switchState = SwitchingAreaSate.LOADING_SCENES;
							}
						}break;
						case SwitchingAreaSate.LOADING_SCENES:
						{
							bool done = true;
							if ( m_switchingAreaTasks != null )
							{
								for( int i = 0; i<m_switchingAreaTasks.Count && done; i++ )	
								{
									done = m_switchingAreaTasks[i].progress >= 0.9f;
								}
							}

							if ( done )
							{
								if ( m_switchingAreaTasks != null )
								{
									for( int i = 0; i<m_switchingAreaTasks.Count; i++ )	
									{
										m_switchingAreaTasks[i].allowSceneActivation = true;
									}
								}

								m_switchState = SwitchingAreaSate.ACTIVATING_SCENES;

							}
						}break;
						case SwitchingAreaSate.ACTIVATING_SCENES:
						{
							bool done = true;
							if ( m_switchingAreaTasks != null )
							{
								for( int i = 0; i<m_switchingAreaTasks.Count && done; i++ )	
								{
									done = m_switchingAreaTasks[i].isDone;
								}
							}

							if ( done )
							{	
								Messenger.Broadcast(MessengerEvents.GAME_AREA_ENTER);
								PoolManager.Rebuild();
                                HDTrackingManagerImp.Instance.Notify_StartPerformanceTracker();
								m_switchingArea = false;
							}
						}break;
					}
				}

				// Notify listeners
				Messenger.Broadcast(MessengerEvents.GAME_UPDATED);
			} break;

			case EStates.FINISHED: {
				// Show the summary popup after some delay
				if(m_timer > 0) {
					m_timer -= Time.deltaTime;
					if(m_timer <= 0) {						
                        ChangeState(EStates.SHOWING_RESULTS);                        
					}
				}
			} break;

            case EStates.SHOWING_RESULTS: {
                m_switchAsyncScenes.Update();
            } break;
		}
	}       

    /// <summary>
    /// Clears stuff used by the game (RUNNING state)
    /// </summary>
    private void ClearGame() {
        if (ApplicationManager.IsAlive) {
            ParticleManager.Clear();
            PoolManager.Clear(true);
			UIPoolManager.Clear(true);
        }
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
        // Clear the game just in case the user leaves the scene without following the usual flow (through the results screen)
        ClearGame();

        // Call parent
        base.OnDestroy();

        CustomParticlesCulling.Manager_OnDestroy();

        Messenger.RemoveListener(MessengerEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
	}

	//------------------------------------------------------------------//
	// FLOW CONTROL														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start a new game. All temp game stats will be reset.
	/// </summary>
	public void StartGame() {

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Track_RoundStart();

        // Reset timer
        m_elapsedSeconds = 0;

		// Disable dragon until the game actually starts so its HP doesn't go down
		InstanceManager.player.gameObject.SetActive(false);

		// Reset rewards
		RewardManager.Reset();
		
		// [AOC] TODO!! Reset game stats
		
		// Change state
		ChangeState(EStates.DELAY);
	}

    /// <summary>
    /// End the current game. Wont reset the stats so they can be used.
    /// <param name="_quitGame">Whether or not the game is ended because the user has quit.</param>
    public void EndGame(bool _quitGame) {    
        //
        // Tracking
        //
        // If the user has quit then we also need to send the run end event
        if (_quitGame) {
            Track_RunEnd(_quitGame);
        }

        Track_RoundEnd();

        // Make sure game is not paused
        PauseGame(false, true);

		Screen.sleepTimeout = SleepTimeout.SystemSetting;

		// Change state
		ChangeState(EStates.FINISHED);

		// Dispatch game event
		Messenger.Broadcast(MessengerEvents.GAME_ENDED);

		// Open summary screen - override timer after calling this method if you want some delay
		m_timer = 0.0125f;
	}

	/// <summary>
	/// Pause/resume
	/// </summary>
	/// <param name="_pause">Whether to pause the game or resume it.</param>
	/// <param name="_force">Ignore stacks.</param>
	public void PauseGame(bool _pause, bool _force) {
		// Only allowed in specific states
		if(state == EStates.RUNNING || state == EStates.COUNTDOWN) {
			//m_paused = _bPause;
			if(_pause) {
				// If not paused, pause!
				if(!m_paused || _force) {
					// Store current timescale and set it to 0
					// Not if already paused, otherwise resume wont work!
					if(!m_paused) m_timeScaleBackup = Time.timeScale;
					Time.timeScale = 0.0f;
					Screen.sleepTimeout = SleepTimeout.SystemSetting;

                    //Stop Performance tracking 
                    HDTrackingManagerImp.Instance.Notify_StopPerformanceTracker();
					// Notify the game
					Messenger.Broadcast<bool>(MessengerEvents.GAME_PAUSED, true);
				}

				// Increase stack
				m_pauseStacks++;
			} else {
				// Decrease stack (or reset if forcing)
				if(_force) {
					m_pauseStacks = 0;
				} else {
					m_pauseStacks = Mathf.Max(m_pauseStacks - 1, 0);	// At least 0!
				}

				// If empty stack, restore gameplay!
				if(m_pauseStacks == 0) {
					// Restore previous timescale
					Time.timeScale = m_timeScaleBackup;
					Screen.sleepTimeout = SleepTimeout.NeverSleep;

					// Notify the game
					Messenger.Broadcast<bool>(MessengerEvents.GAME_PAUSED, false);
                    //Start Performance tracking 
                    HDTrackingManagerImp.Instance.Notify_StartPerformanceTracker();
                }
            }

			// Update logic flag
			m_paused = (m_pauseStacks > 0);
		}
	}

	/// <summary>
	/// Resets the cached time scale.
	/// </summary>
	public void ResetCachedTimeScale()
	{
		m_timeScaleBackup = 1.0f;
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Changes current logic state. Will be ignored if the state is the same we already are in.
	/// </summary>
	/// <param name="_eNewState">The new state to go to.</param>
	private void ChangeState(EStates _newState) {
		// Ignore if already in this state
		if(m_state == _newState) return;
		
		// Actions to perform when leaving the current state
		switch(m_state) {
			case EStates.LOADING_LEVEL: {
				// Initialize level's map
				InitLevelMap();
			} break;

			case EStates.ACTIVATING_LEVEL: {
				// Delete loading task
				m_levelLoadingTasks = null;

				// Build Pools
				PoolManager.Build();

				// Init game camera
				InstanceManager.gameCamera.Init();

				// Dispatch game event
				Messenger.Broadcast(MessengerEvents.GAME_LEVEL_LOADED);

				// Enable dragon back and put it in the spawn point
				// Don't make it playable until the countdown ends
				InstanceManager.player.playable = false;
				InstanceManager.player.gameObject.SetActive(true);
				// InstanceManager.player.MoveToSpawnPoint();
				InstanceManager.player.StartIntroMovement();

				// Spawn collectibles
				CollectiblesManager.OnLevelLoaded();

				// Wait one frame!
				StartCoroutine( OneFrameAfterActivation() );

                // Notify the game
                Messenger.Broadcast(MessengerEvents.GAME_STARTED);
			} break;

			case EStates.COUNTDOWN: {
				if (UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN) 
					&&  !UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SECOND_RUN)) {
					HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._11_load_is_done);
				}
				// Notify the game
				// Messenger.Broadcast(GameEvents.GAME_COUNTDOWN_ENDED);
			} break;

			case EStates.RUNNING: {
                // Unsubscribe from external events
                Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKO);
                Messenger.RemoveListener(MessengerEvents.PLAYER_DIED, OnPlayerDied);
			} break;
		}
		
		// Actions to perform when entering the new state
		switch(_newState) {
			case EStates.DELAY: {
				// Reset timer
				m_timer = INITIAL_DELAY;

				// Notify loadGameplay start
				HDTrackingManager.Instance.Notify_LoadingGameplayStart();
				m_loadingTimer = Time.time;
			} break;

			case EStates.LOADING_LEVEL: {
				// Start loading current level
				LevelManager.SetCurrentLevel(UsersManager.currentUser.currentLevel);
				
				m_levelLoadingTasks = LevelManager.LoadLevel();
				
				ParticleManager.PreBuild();

				// Initialize minimum loading time as well
				m_timer = MIN_LOADING_TIME;
			} break;

			case EStates.ACTIVATING_LEVEL: {
				// Activate all the scenes
				for(int i = 0; i < m_levelLoadingTasks.Length; i++) {
					m_levelLoadingTasks[i].allowSceneActivation = true;
				}

			} break;

			case EStates.COUNTDOWN: {
				LevelManager.SetArtSceneActive();
				// Start countdown timer
				m_timer = COUNTDOWN;

				// enable spawners
				SpawnerManager.instance.EnableSpawners();

				// Notify the game
				Messenger.Broadcast(MessengerEvents.GAME_COUNTDOWN_STARTED);
                // Begin performance track
                HDTrackingManager.Instance.Notify_StartPerformanceTracker();
            } break;
				
			case EStates.RUNNING: {
                // Subscribe to external events
                Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKO);
                Messenger.AddListener(MessengerEvents.PLAYER_DIED, OnPlayerDied);

				// Make dragon playable!
				InstanceManager.player.playable = true;

				// TODO: Notify loadGameplay end
				HDTrackingManager.Instance.Notify_LoadingGameplayEnd( Time.time - m_loadingTimer );
			} break;

			case EStates.FINISHED: {
				// Disable dragon
				InstanceManager.player.playable = false;

                // The time of the play session that has just finished is accumulated to the total amount of time played by the user so far
                UsersManager.currentUser.timePlayed += (int)m_elapsedSeconds;                    
			} break;

            case EStates.SHOWING_RESULTS: {
				if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN)) {
					HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._04_run_is_done);
				}

                // Stops performance track
                HDTrackingManager.Instance.Notify_StopPerformanceTracker();

                // Show loading screen
                LoadingScreen.Toggle(true, false);

				// Disable dragon and entities!
     			InstanceManager.player.gameObject.SetActive(false);

                // Clear pools to save memory for the results screen
                ClearGame();

                UnityEngine.SceneManagement.Scene thisScene = gameObject.scene;

                // Destroys all game objects in this game object's scene except:
                //    -)this game object because if holds some data that the results screen needs to show
                //    -)The game camera because it has the AudioListener and we don't want Unity to complain about no audio listeners
                //      in the scene as the results screen is loaded
                //    -)uiRoot because it contains the loading screen
                GameObject[] gos = thisScene.GetRootGameObjects();
                if (gos != null) {
                    GameObject mainCameraGO = (mainCamera != null) ? mainCamera.gameObject : null;
                  
                    int count = gos.Length;
                    for (int i = 0; i < count; i++) {                        
                        if (gos[i] != gameObject && gos[i] != mainCameraGO && gos[i] != uiRoot && gos[i] != InstanceManager.player.gameObject ) {
                            Destroy(gos[i]);                                                                                     
                        }
                    }
                }

                // All area scenes currently loaded can be unloaded except the spawner ones since the eggs are stored there and the results
                // screen needs to show whether or not the user has collected any eggs
                List<string> scenesToUnload = LevelManager.GetAllArenaScenesList(LevelManager.currentArea);
                string[] tokens;
                for (int i = 0; i < scenesToUnload.Count;) {
                    tokens = scenesToUnload[i].Split('_');
                    if (tokens.Length > 1 && tokens[0].CompareTo("SP") == 0){
                        scenesToUnload.RemoveAt(i);
                    }else{
                        i++;
					}
                }
                                                     
                List<string> scenesToLoad = new List<string>();
                scenesToLoad.Add(ResultsScreenController.NAME);
                m_switchAsyncScenes.Perform(scenesToUnload, scenesToLoad, true, OnResultsSceneLoaded, OnScenesUnloaded);
            } break;
        }
		
		// Store new state
		m_state = _newState;
	}

	IEnumerator OneFrameAfterActivation()
	{
		yield return null;
		InstanceManager.fogManager.firstTime = true;
		// Hide loading screen
		LoadingScreen.Toggle(false);
	}

	private void OnScenesUnloaded()
	{
		// This scene uiRoot is disabled because the results screen scene's uiRoot is going to be used instead
        if (uiRoot != null) {
            uiRoot.SetActive(false);
        }
		DestroyImmediate( InstanceManager.player.gameObject );
		InstanceManager.player = null;
	}

    private void OnResultsSceneLoaded() {

		Scene scene = SceneManager.GetSceneByName( ResultsScreenController.NAME );
		SceneManager.SetActiveScene(scene);

		// Hide loading screen
		LoadingScreen.Toggle(false, false);

        // We don't need it anymore as the results screen scene, which has its own AudioListener, has been loaded completely
        if (mainCamera != null) {
            Destroy(mainCamera.gameObject);
        }

        ResultsSceneController resultsSceneController = FindObjectOfType<ResultsSceneController>();
        if (resultsSceneController != null) {
            resultsSceneController.Show();
		} else {
			Debug.LogError("<color=red>RESULTS SCENE CONTROLLER NOT FOUND!</color>");
		}
    }

    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
    // The player had died but the user still has the chance to revive
    private void OnPlayerKO(DamageType _damageType, Transform _damageSource) {
        Track_RunEnd(false);
    }

    /// <summary>
    /// The player has died.
    /// </summary>
    private void OnPlayerDied() {
		// End game
		EndGame(false);        

		// Add some delay to the summary popup
		m_timer = 0.5f;
	}

	public override bool IsLevelLoaded()
	{
		return state > EStates.ACTIVATING_LEVEL;
	}

	private void CountDownEnded()
	{
		ChangeState(EStates.RUNNING);
	}


	public void SwitchArea( string _nextArea )
    {
    	if ( LevelManager.currentArea != _nextArea && !m_switchingArea)
    	{
            // ParticleManager.Clear();
            HDTrackingManagerImp.Instance.Notify_StopPerformanceTracker();
			Messenger.Broadcast(MessengerEvents.GAME_AREA_EXIT);
			m_switchingArea = true;
			m_nextArea = _nextArea;
			m_switchState = SwitchingAreaSate.UNLOADING_SCENES;

			// Disable everything?
			LevelManager.DisableCurrentArea();
			m_switchingAreaTasks = LevelManager.UnloadCurrentArea();
		}
    }

    #region track
    private void Track_RoundStart() {
        int dragonXp = 0;
        int dragonProgress = 0;
        string dragonSkin = null;
		List<string> pets = null;
        if (InstanceManager.player != null) {
            DragonData dragonData = InstanceManager.player.data;
            if (dragonData != null) {
                if (dragonData.progression != null) {
                    dragonXp = (int)dragonData.progression.xp;
                }

                dragonProgress = UsersManager.currentUser.GetDragonProgress(dragonData);
                dragonSkin = dragonData.diguise;
				pets = dragonData.pets;
            }
        }

		m_boostTimeTracker.SetValue(0, false);
		m_mapUsageTracker.SetValue(0, false);

		HDTrackingManager.Instance.Notify_RoundStart(dragonXp, dragonProgress, dragonSkin, pets);

        // Automatic connection system is disabled during the round in order to ease performance
        GameServerManager.SharedInstance.Connection_SetIsCheckEnabled(false);
    }

    private void Track_RoundEnd() {
        int dragonXp = 0;
        int timePlayed = (int)elapsedSeconds;
        int score = (int)RewardManager.score;
        int dragonProgress = 0;
        if (InstanceManager.player != null) {
            DragonData dragonData = InstanceManager.player.data;
            if (dragonData != null) {
                if (dragonData.progression != null) {
                    dragonXp = (int)dragonData.progression.xp;
                }

                dragonProgress = UsersManager.currentUser.GetDragonProgress(dragonData);
            }
        }
        
        int eggsFound = (CollectiblesManager.egg != null && CollectiblesManager.egg.collected) ? 1 : 0;

        int chestsFound = 0;
        for (int i = 0; i < ChestManager.dailyChests.Length; i++) {
            if (ChestManager.dailyChests[i].state == Chest.State.PENDING_REWARD) {
                // Count chest
                chestsFound++;
            }
        }

        HDTrackingManager.Instance.Notify_RoundEnd(dragonXp, (int)RewardManager.xp, dragonProgress, timePlayed, score, chestsFound, eggsFound,
            RewardManager.maxScoreMultiplier, RewardManager.maxBaseScoreMultiplier, RewardManager.furyFireRushAmount, RewardManager.furySuperFireRushAmount,
            RewardManager.paidReviveCount, RewardManager.freeReviveCount, (int)RewardManager.coins, (int)RewardManager.pc, m_boostTimeTracker.currentValue, (int)m_mapUsageTracker.currentValue);

        // Automatic connection system is enabled again since performance is not a constraint anymore
        GameServerManager.SharedInstance.Connection_SetIsCheckEnabled(true);
    }

    private void Track_RunEnd(bool _quitGame) {
        DragonPlayer dragonPlayer = InstanceManager.player;
        DragonData dragonData = null;
        Vector3 deathCoordinates = Vector3.zero;
        if (dragonPlayer != null) {
            dragonData = dragonPlayer.data;
            deathCoordinates = dragonPlayer.transform.position;
        }

        int dragonXp = 0;
        int timePlayed = (int)elapsedSeconds;
        int score = (int)RewardManager.score;
        if (dragonData != null && dragonData.progression != null) {
            dragonXp = (int)dragonData.progression.xp;
        }

        string deathType = null;
        string deathSource = null;
        if (_quitGame) {
            deathType = "QUIT";
        }
        else {
            deathType = RewardManager.deathType.ToString();
            deathSource = RewardManager.deathSource;
        }

        HDTrackingManager.Instance.Notify_RunEnd(dragonXp, timePlayed, score, deathType, deathSource, deathCoordinates);
    }
    #endregion
    /*
    // Test to load new areas
	IEnumerator WaitTasksFinished( AsyncOperation[] operations )
	{
		bool done = false;
		while (!done)
		{
			done = true;
			for( int i = 0; i<operations.Length; i++ )	
			{
				done = done && operations[i].progress >= 0.9f;
			}
			if (!done)
			{
				yield return null;
			}
		}
		for( int i = 0; i<operations.Length; i++ )	
		{
			operations[i].allowSceneActivation = true;
		}
		yield return null;

		PoolManager.Rebuild();
		Messenger.Broadcast(GameEvents.GAME_AREA_ENTER);
	}
	*/

}