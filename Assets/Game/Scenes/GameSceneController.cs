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
		FINISHED
	};

	bool m_switchingArea = false;
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
	[SerializeField] private ResultsSceneController m_resultsScene;
	public ResultsSceneController resultsScene {
		get { return m_resultsScene; }
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
	private bool m_paused = false;
	private float m_timeScaleBackup = 1f;	// When going to pause, store timescale to be restored later on
	public bool paused {
		get { return m_paused; }
	}

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

	// For the tutorial
	private bool m_startWhenLoaded = true;
	public bool startWhenLoaded {
		get { return m_startWhenLoaded; }
		set { m_startWhenLoaded = value; }
	}

	// Internal
	private float m_timer = -1;	// Misc use

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

		// Load the dragon
		DragonManager.LoadDragon(UsersManager.currentUser.currentDragon);
		Messenger.AddListener(GameEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
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

				// Change state only if allowed, otherwise it will be manually done
				if(levelLoadingProgress >= 1) {
					if(m_startWhenLoaded) ChangeState(EStates.ACTIVATING_LEVEL);
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
					if(m_startWhenLoaded) ChangeState(EStates.COUNTDOWN);
				}
			} break;

			// During countdown, let's just wait
			case EStates.COUNTDOWN: {
				if(m_timer > 0) {
					m_timer -= Time.deltaTime;
					if(m_timer <= 0) {
						Messenger.Broadcast(GameEvents.GAME_COUNTDOWN_ENDED);
						// ChangeState(EStates.RUNNING);
					}
				}
			} break;
				
			case EStates.RUNNING: {
				// Update running time
				m_elapsedSeconds += Time.deltaTime;
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
								m_switchingAreaTasks = LevelManager.LoadArea(m_nextArea);
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
								PoolManager.Rebuild();
								Messenger.Broadcast(GameEvents.GAME_AREA_ENTER);
								m_switchingArea = false;
							}
						}break;
					}
				}

			} break;

			case EStates.FINISHED: {
				// Show the summary popup after some delay
				if(m_timer > 0) {
					m_timer -= Time.deltaTime;
					if(m_timer <= 0) {
						// Disable dragon and entities!
						InstanceManager.player.gameObject.SetActive(false);

                        // Clear pools to save memory for the results screen
                        ClearGame();

                        // Enable Results screen and move the camera to that position
                        if (m_resultsScene != null) {
						    m_resultsScene.Show();
					    }
					}
				}
			} break;
		}
	}

    /// <summary>
    /// Clears stuff used by the game (RUNNING state)
    /// </summary>
    private void ClearGame() {        
        PoolManager.Clear(true);        
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

        Messenger.RemoveListener(GameEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
	}

	//------------------------------------------------------------------//
	// FLOW CONTROL														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start a new game. All temp game stats will be reset.
	/// </summary>
	public void StartGame() {
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
	/// </summary>
	public void EndGame() {
		// Make sure game is not paused
		PauseGame(false);

		// Change state
		ChangeState(EStates.FINISHED);

		// Dispatch game event
		Messenger.Broadcast(GameEvents.GAME_ENDED);

		// Open summary screen - override timer after calling this method if you want some delay
		m_timer = 0.0125f;
	}

	/// <summary>
	/// Pause/resume
	/// </summary>
	/// <param name="_bPause">Whether to pause the game or resume it.</param>
	public void PauseGame(bool _bPause) {
		// Only allowed in specific states
		if(state == EStates.RUNNING || state == EStates.COUNTDOWN) {
			m_paused = _bPause;
			if(_bPause) {
				// Store current timescale and set it to 0
				m_timeScaleBackup = Time.timeScale;
				Time.timeScale = 0.0f;

				// Notify the game
				Messenger.Broadcast<bool>(GameEvents.GAME_PAUSED, true);
			} else {
				// Restore previous timescale
				Time.timeScale = m_timeScaleBackup;

				// Notify the game
				Messenger.Broadcast<bool>(GameEvents.GAME_PAUSED, false);
			}
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
				Messenger.Broadcast(GameEvents.GAME_LEVEL_LOADED);

				// Enable dragon back and put it in the spawn point
				// Don't make it playable until the countdown ends
				InstanceManager.player.playable = false;
				InstanceManager.player.gameObject.SetActive(true);
				// InstanceManager.player.MoveToSpawnPoint();
				InstanceManager.player.StartIntroMovement();

				// Spawn collectibles
				ChestManager.OnLevelLoaded();
				EggManager.SelectCollectibleEgg();                

                // Notify the game
                Messenger.Broadcast(GameEvents.GAME_STARTED);
			} break;

			case EStates.COUNTDOWN: {
				// Notify the game
				// Messenger.Broadcast(GameEvents.GAME_COUNTDOWN_ENDED);
			} break;

			case EStates.RUNNING: {
				// Unsubscribe from external events
				Messenger.RemoveListener(GameEvents.PLAYER_DIED, OnPlayerDied);
			} break;
		}
		
		// Actions to perform when entering the new state
		switch(_newState) {
			case EStates.DELAY: {
				// Reset timer
				m_timer = INITIAL_DELAY;
			} break;

			case EStates.LOADING_LEVEL: {
				// Start loading current level
				LevelManager.SetCurrentLevel(UsersManager.currentUser.currentLevel);
				m_levelLoadingTasks = LevelManager.LoadLevel();

				// Initialize minimum loading time as well
				m_timer = MIN_LOADING_TIME;

				// Check whether the tutorial popup must be displayed
				if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.CONTROLS_POPUP)
				|| DebugSettings.isPlayTest) {
					// Open popup
					PopupManager.OpenPopupInstant(PopupTutorialControls.PATH);
					UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.CONTROLS_POPUP);
					PersistenceManager.Save();
				}
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
				Messenger.Broadcast(GameEvents.GAME_COUNTDOWN_STARTED);
			} break;
				
			case EStates.RUNNING: {
				// Subscribe to external events
				Messenger.AddListener(GameEvents.PLAYER_DIED, OnPlayerDied);

				// Make dragon playable!
				InstanceManager.player.playable = true;
			} break;

			case EStates.FINISHED: {
				// Disable dragon
				InstanceManager.player.playable = false;

                // The time of the play session that has just finished is accumulated to the total amount of time played by the user so far
                SaveFacade.Instance.timePlayed += (int)m_elapsedSeconds;
			} break;
		}
		
		// Store new state
		m_state = _newState;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The player has died.
	/// </summary>
	private void OnPlayerDied() {
		// End game
		EndGame();

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
			Messenger.Broadcast(GameEvents.GAME_AREA_EXIT);
			m_switchingArea = true;
			m_nextArea = _nextArea;
			m_switchState = SwitchingAreaSate.UNLOADING_SCENES;
			m_switchingAreaTasks = LevelManager.UnloadCurrentArea();
		}
    }


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