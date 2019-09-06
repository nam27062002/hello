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
	public const float COUNTDOWN = 2.5f;	// Seconds. This countdown is used as a safety net if the intro animation does not end or does not send the proper event
	public const float MIN_LOADING_TIME = 1f;	// Seconds, to avoid loading screen flickering303

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

    private const bool m_useSyncLoading = false;

    protected ToggleParam m_pauseParam = new ToggleParam();

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
	private int m_pauseStacks = 0;

    // Level loading
    private LevelLoader m_levelLoader = null;

	public float levelActivationProgress {
		get {
			if(state == EStates.LOADING_LEVEL || state == EStates.ACTIVATING_LEVEL) {
                if (m_levelLoader == null) return 1f;	// Shouldn't be null at this state
                float progress = m_levelLoader.GetProgress();
                return Mathf.Min(progress, 1f - Mathf.Max(m_timer / MIN_LOADING_TIME, 0f)); // Either progress or fake timer
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

	// Results
	private ResultsSceneController m_resultsScene = null;
	public ResultsSceneController resultsScene {
		get { return m_resultsScene; }
	}

	// Internal
	private float m_timer = -1;	// Misc use
	private float m_loadingTimer = 0;

    private SwitchAsyncScenes m_switchAsyncScenes = new SwitchAsyncScenes();

    TrackerBoostTime m_boostTimeTracker;
    TrackerMapUsage m_mapUsageTracker;
    TrackerSpecialPowerTime m_specialPowerTimeTracker;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    override protected void Awake()
    {
        // Call parent
        base.Awake();

        m_boostTimeTracker = new TrackerBoostTime();
        m_mapUsageTracker = new TrackerMapUsage();
        m_specialPowerTimeTracker = new TrackerSpecialPowerTime();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Check whether the tutorial popup must be displayed
        if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN))
        {
            // Tracking
            string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupTutorialControls.PATH);
            HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");

            // Open popup
            PopupManager.OpenPopupInstant(PopupTutorialControls.PATH);
        }

        // Load the dragon
        // DEBUG: Special dragon testing
        if (DebugSettings.useSpecialDragon) {
            // Hola soy special SPECIAAAAAAL
            // [AOC] xDDDDDDDD
            string dragon = DebugSettings.Prefs_GetStringPlayer(DebugSettings.SPECIAL_DRAGON_SKU, "dragon_helicopter");
            DragonTier dragonTier = ( DragonTier )DebugSettings.specialDragonTier;
            int powerLevel = DebugSettings.specialDragonPowerLevel;
            int hpBoost = DebugSettings.specialDragonHpBoostLevel;
            int speedBoost = DebugSettings.specialDragonSpeedBoostLevel;
            int energyBoost = DebugSettings.specialDragonEnergyBoostLevel;
            DragonManager.LoadSpecialDragon_DEBUG(dragon, dragonTier, powerLevel, hpBoost, speedBoost, energyBoost);
        } else {
            if (HDLiveDataManager.tournament.isActive) {
                DragonManager.LoadDragon(HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData);
            } else {
                DragonManager.LoadDragon(DragonManager.currentDragon.sku);	// currentDragon Will automatically select between classic and special dragons depending on active mode
            }
        }

		Messenger.AddListener(MessengerEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
        Messenger.AddListener<float>(MessengerEvents.PLAYER_LEAVING_AREA, OnPlayerLeavingArea);
        Messenger.AddListener(MessengerEvents.PLAYER_ENTERING_AREA, OnPlayerEnteringArea);

		ParticleManager.instance.poolLimits = ParticleManager.PoolLimits.LoadedArea;
        PoolManager.instance.poolLimits = PoolManager.PoolLimits.Limited;
            // Audio Toolkit to use this scene as root
        ObjectPoolController.defaultInstantiateSceme = gameObject.scene;
	}


	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Let's play!
		StartGame();
	}

    private void OnSwitchAreaChangeState(LevelLoader.EState prevState, LevelLoader.EState nextState)
    {
        switch (nextState)
        {
            case LevelLoader.EState.LoadingNextAreaScenes:
                PoolManager.PreBuild();
                ParticleManager.PreBuild();
                ParticleManager.Rebuild();
                break;

            case LevelLoader.EState.WaitingToActivateNextAreaScences:
                m_levelLoader.ActivateNextAreaScenes();
                break;

            case LevelLoader.EState.Done:
            	LevelManager.SetArtSceneActive();
                PoolManager.Rebuild();
                Broadcaster.Broadcast(BroadcastEventType.GAME_AREA_ENTER);
                HDTrackingManagerImp.Instance.Notify_StartPerformanceTracker();
                m_switchingArea = false;
                m_levelLoader = null;
                break;
        }
    }

	/// <summary>
	/// Called every frame.
	/// </summary>
	protected override void Update() {
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
            SpawnPlayer(false);

			LevelEditor.LevelTypeSpawners sp = FindObjectOfType<LevelEditor.LevelTypeSpawners>();
			if ( sp != null )
				sp.IntroSpawn(InstanceManager.player.data.def.sku);
		}
		#endif

        if (m_levelLoader != null)
        {
            m_levelLoader.Update();
        }

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

                if (m_timer <= 0f) {
                    if (m_useSyncLoading) {
                        if (m_levelLoader.IsLoadingNextAreaScenes())
                        {
                            ChangeState(EStates.ACTIVATING_LEVEL);
                        }
                    } else {
                        if (m_levelLoader.IsReadyToActivateNextAreaScenes())
                        {
                            ChangeState(EStates.ACTIVATING_LEVEL);
                        }
                    }
                }

			} break;

			// During activation, wait until all scenes have been activated
			case EStates.ACTIVATING_LEVEL: {
				if(m_levelLoader.IsDone()) {
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
                base.Update();

				// Update running time
				if (m_freezeElapsedSeconds <= 0 && !m_switchingArea)
					m_elapsedSeconds += Time.deltaTime;

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
        Scene emptyScene = new Scene();
        ObjectPoolController.defaultInstantiateSceme = emptyScene;

        Messenger.RemoveListener(MessengerEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
        Messenger.RemoveListener<float>(MessengerEvents.PLAYER_LEAVING_AREA, OnPlayerLeavingArea);
        Messenger.RemoveListener(MessengerEvents.PLAYER_ENTERING_AREA, OnPlayerEnteringArea);
	}

    public void OnPlayerLeavingArea(float _estimatedTime)
    {
        m_freezeElapsedSeconds++;
    }
    public void OnPlayerEnteringArea()
    {
        m_freezeElapsedSeconds--;
    }

    public override void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        base.OnBroadcastSignal(eventType, broadcastEventInfo);
    }

	//------------------------------------------------------------------//
	// FLOW CONTROL														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start a new game. All temp game stats will be reset.
	/// </summary>
	public void StartGame() {
		// Make sure multitouch is enabled for boost functionality!
        Input.multiTouchEnabled = true;

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

        GameAds.instance.ReduceRunsToInterstitial();

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

		// Multitouch no longer needed
		Input.multiTouchEnabled = false;

		Screen.sleepTimeout = SleepTimeout.SystemSetting;

		// Change state
		ChangeState(EStates.FINISHED);

		// Dispatch game event
		Broadcaster.Broadcast(BroadcastEventType.GAME_ENDED);

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
                    InstanceManager.timeScaleController.Pause();
					Screen.sleepTimeout = SleepTimeout.SystemSetting;

                    //Stop Performance tracking
                    HDTrackingManagerImp.Instance.Notify_StopPerformanceTracker();
					// Notify the game
                    m_pauseParam.value = true;
                    Broadcaster.Broadcast(BroadcastEventType.GAME_PAUSED, m_pauseParam);
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
                    InstanceManager.timeScaleController.Resume();
					Screen.sleepTimeout = SleepTimeout.NeverSleep;

					// Notify the game
                    m_pauseParam.value = false;
                    Broadcaster.Broadcast(BroadcastEventType.GAME_PAUSED, m_pauseParam);
                    //Start Performance tracking
                    HDTrackingManagerImp.Instance.Notify_StartPerformanceTracker();

                    Input.multiTouchEnabled = true;
                }
            }

			// Update logic flag
			m_paused = (m_pauseStacks > 0);
		}
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
                // level loader is not needed anymore
                m_levelLoader = null;

				// Build Pools
				PoolManager.Build();

                if (HDLiveDataManager.tournament.isActive) {
                    HDTournamentDefinition tournamentDef = HDLiveDataManager.tournament.tournamentData.tournamentDef;
                    progressionOffsetSeconds = tournamentDef.m_goal.m_progressionSeconds;
    			    progressionOffsetXP = tournamentDef.m_goal.m_progressionXP;
                } else {
                    progressionOffsetSeconds = 0f;
    			    progressionOffsetXP = 0;
                }

                // Dispatch game event
                Broadcaster.Broadcast(BroadcastEventType.GAME_LEVEL_LOADED);

				// Enable dragon back and put it in the spawn point
				// Don't make it playable until the countdown ends
                SpawnPlayer(false);

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

                if (HDLiveDataManager.tournament.isActive) {
                    HDTournamentDefinition tournamentDef = HDLiveDataManager.tournament.tournamentData.tournamentDef;
                    if (string.IsNullOrEmpty(tournamentDef.m_goal.m_area)) {
                        m_levelLoader = LevelManager.LoadLevelForDragon(tournamentDef.dragonData.sku);
                    } else {
                        m_levelLoader = LevelManager.LoadLevel(tournamentDef.m_goal.m_area);
                    }
                } else {
                    m_levelLoader = LevelManager.LoadLevelForDragon(DragonManager.currentDragon.sku);
                }

                m_levelLoader.Perform(m_useSyncLoading);

                PoolManager.PreBuild();
                ParticleManager.Clear();
                string bloodOverride = SeasonManager.GetBloodParticlesName();
                if (string.IsNullOrEmpty(bloodOverride)) {
                    ParticleManager.DisableBloodOverride();
                } else {
                    ParticleManager.EnableBloodOverride(bloodOverride);
                }
				ParticleManager.PreBuild();

				// Initialize minimum loading time as well
				m_timer = MIN_LOADING_TIME;
			} break;

			case EStates.ACTIVATING_LEVEL: {
                if (!m_useSyncLoading) {
                    m_levelLoader.ActivateNextAreaScenes();
                }
            } break;

			case EStates.COUNTDOWN: {
				LevelManager.SetArtSceneActive();
				// Start countdown timer
				m_timer = COUNTDOWN;

				// enable spawners
				SpawnerManager.instance.EnableSpawners();
                DecorationSpawnerManager.instance.EnableSpawners();

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

                HDTrackingManager.Instance.Notify_LoadingResultsStart();

                // Show loading screen
                LoadingScreen.Toggle(true, false, true);

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
                    if ((tokens.Length > 1 && tokens[0].CompareTo("SP") == 0) ||
                        (!LevelManager.IsSceneLoaded(scenesToUnload[i]))) {
                        scenesToUnload.RemoveAt(i);
                    }else{
                        i++;
					}
                }

                List<string> scenesToLoad = new List<string>();
                scenesToLoad.Add(ResultsScreenController.NAME);

                List<string> dependencyIdsToUnload = HDAddressablesManager.Instance.GetAssetBundlesGroupDependencyIds((LevelManager.currentArea));
                AddressablesBatchHandle batchHandle = HDAddressablesManager.Instance.GetAddressablesAreaBatchHandle(ResultsScreenController.NAME);
                m_switchAsyncScenes.Perform(scenesToUnload, scenesToLoad, true, dependencyIdsToUnload, batchHandle.DependencyIds, OnResultsSceneLoaded, OnScenesUnloaded);

                HDAddressablesManager.Instance.Ingame_NotifyLevelUnloaded();
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
		LoadingScreen.Toggle(false, false);	// [AOC] Occasionally the screen is not disabled after the fade out animation, locking the rest of the UI interaction. Remove fade animation until a fix is found.
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

        m_resultsScene = FindObjectOfType<ResultsSceneController>();
        if (m_resultsScene != null) {
			m_resultsScene.Show();
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
			Broadcaster.Broadcast(BroadcastEventType.GAME_AREA_EXIT);
			m_switchingArea = true;
			m_nextArea = _nextArea;

			// Disable everything?
			LevelManager.DisableCurrentArea();

            m_levelLoader = LevelManager.SwitchArea(m_nextArea);
            m_levelLoader.Perform(m_useSyncLoading, OnSwitchAreaChangeState);
        }
    }

    #region track
    private void Track_RoundStart() {
        int dragonXp = 0;
        int dragonProgress = 0;
        string dragonSkin = null;
        bool isSpecial = false;
		List<string> pets = null;
        if (InstanceManager.player != null) {
            IDragonData dragonData = InstanceManager.player.data;
			if (dragonData != null) {
                if (dragonData.type == IDragonData.Type.CLASSIC)
                {
    				DragonProgression progression = (dragonData as DragonDataClassic).progression;
                    if (progression != null) {
                        dragonXp = (int)progression.xp;
                    }

                    dragonProgress = UsersManager.currentUser.GetDragonProgress(dragonData);
                }
                else if ( dragonData.type == IDragonData.Type.SPECIAL )
                {
                    isSpecial = true;
                    // TODO
                    DragonDataSpecial specialData = dragonData as DragonDataSpecial;
                    dragonProgress = specialData.GetLevel();
                }
                dragonSkin = dragonData.disguise;
                pets = dragonData.pets;
            }
        }

		m_boostTimeTracker.InitValue(0);
        m_boostTimeTracker.enabled = true;
		m_mapUsageTracker.InitValue(0);
        m_mapUsageTracker.enabled = true;
        m_specialPowerTimeTracker.InitValue(0);
        m_specialPowerTimeTracker.enabled = true;

        if (isSpecial)
        {
            HDLeagueData leagueData = HDLiveDataManager.league.season.currentLeague;
            DragonDataSpecial specialData = InstanceManager.player.data as DragonDataSpecial;
            string powerLevel = "P" + specialData.powerLevel;
            int specialOwned = UsersManager.currentUser.GetNumOwnedSpecialDragons();
            HDTrackingManager.Instance.Notify_LabGameStart(specialData.sku,
                                                            specialData.GetStat(DragonDataSpecial.Stat.HEALTH).level,
                                                            specialData.GetStat(DragonDataSpecial.Stat.SPEED).level,
                                                            specialData.GetStat(DragonDataSpecial.Stat.ENERGY).level,
                                                            powerLevel,
                                                            specialOwned,
                                                            (leagueData != null)? leagueData.sku : ""
                                                            , pets
                                                            );
        }
        else
        {
            HDTrackingManager.Instance.Notify_RoundStart(dragonXp, dragonProgress, dragonSkin, pets);
        }



        // Automatic connection system is disabled during the round in order to ease performance
        GameServerManager.SharedInstance.Connection_SetIsCheckEnabled(false);
    }

    private void Track_RoundEnd() {

        int timePlayed = (int)elapsedSeconds;
        int score = (int)RewardManager.score;

        int dragonXp = 0;
        int dragonProgress = 0;

        bool isSpecial = false;
        if (InstanceManager.player != null) {
            IDragonData dragonData = InstanceManager.player.data;
			if (dragonData != null)
            {
                if (dragonData.type == IDragonData.Type.CLASSIC)
                {
                    DragonProgression progression = (dragonData as DragonDataClassic).progression;
                    if (progression != null) {
                        dragonXp = (int)progression.xp;
                    }

                    dragonProgress = UsersManager.currentUser.GetDragonProgress(dragonData);
                }
                else if (dragonData.type == IDragonData.Type.SPECIAL)
                {
                    isSpecial = true;
                }
            }
        }

        int eggsFound = (CollectiblesManager.egg != null && CollectiblesManager.egg.collected) ? 1 : 0;

        if ( isSpecial )
        {
            DragonDataSpecial dragonDataSpecial = InstanceManager.player.data as DragonDataSpecial;
            int labHp = dragonDataSpecial.GetStat(DragonDataSpecial.Stat.HEALTH).level;
            int labSpeed = dragonDataSpecial.GetStat(DragonDataSpecial.Stat.HEALTH).level;
            int labBoost = dragonDataSpecial.GetStat(DragonDataSpecial.Stat.ENERGY).level;
            string powerLevel = "P" + dragonDataSpecial.powerLevel;
            HDLeagueData leagueData = HDLiveDataManager.league.season.currentLeague;
            string league = (leagueData != null) ? leagueData.sku : "";
            float powerTime = m_specialPowerTimeTracker.currentValue;
            // If special dragon
            HDTrackingManager.Instance.Notify_LabGameEnd(dragonDataSpecial.sku,  labHp, labSpeed, labBoost, powerLevel,
                                timePlayed, score, eggsFound,
                                RewardManager.maxScoreMultiplier, RewardManager.maxBaseScoreMultiplier, RewardManager.furyFireRushAmount, RewardManager.furySuperFireRushAmount,
                                RewardManager.paidReviveCount, RewardManager.freeReviveCount, (int)RewardManager.coins, (int)RewardManager.pc,
                                powerTime, (int)m_mapUsageTracker.currentValue, league);
        }
        else
        {
            // CHESTS
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
        }

        m_boostTimeTracker.enabled = false;
        m_mapUsageTracker.enabled = false;
        m_specialPowerTimeTracker.enabled = false;


        // Automatic connection system is enabled again since performance is not a constraint anymore
        GameServerManager.SharedInstance.Connection_SetIsCheckEnabled(true);
    }

    private void Track_RunEnd(bool _quitGame) {
        DragonPlayer dragonPlayer = InstanceManager.player;
        IDragonData dragonData = null;
        Vector3 deathCoordinates = Vector3.zero;
        if (dragonPlayer != null) {
            dragonData = dragonPlayer.data;
            deathCoordinates = dragonPlayer.transform.position;
        }

        int dragonXp = 0;
        int timePlayed = (int)elapsedSeconds;
        int score = (int)RewardManager.score;
		if (dragonData != null && dragonData.type == IDragonData.Type.CLASSIC) {
			DragonProgression progression = (dragonData as DragonDataClassic).progression;
			if(progression != null) {
				dragonXp = (int)progression.xp;
			}
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
