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
	public static readonly string NAME = "SC_Game";
	public static readonly float COUNTDOWN = 3f;	// Seconds
	public static readonly float MIN_LOADING_TIME = 1f;	// Seconds, to avoid loading screen flickering

	public enum EStates {
		INIT,
		LOADING_LEVEL,
		COUNTDOWN,
		RUNNING,
		FINISHED
	};

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_resultsScreen;

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
	private AsyncOperation[] m_levelLoadingTask = null;
	public float levelLoadingProgress {
		get {
			if(state == EStates.LOADING_LEVEL) {
				if(m_levelLoadingTask == null) return 1f;	// Shouldn't be null at this state
				float progress = 0f;
				for(int i = 0; i < m_levelLoadingTask.Length; i++) progress += m_levelLoadingTask[i].progress;
				return Mathf.Min(progress/m_levelLoadingTask.Length, 1f - Mathf.Max(m_timer/MIN_LOADING_TIME, 0f));	// Either progress or fake timer
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
		DragonManager.LoadDragon(UserProfile.currentDragon);
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
		if(Input.GetKeyDown(KeyCode.P)) {
			PopupManager.OpenPopupInstant(PopupPause.PATH);
		}
		#endif

		// Different actions based on current state
		switch(m_state) {
			// During loading, wait until level is loaded
			case EStates.LOADING_LEVEL: {
				// Update timer
				if(m_timer > 0) {
					m_timer -= Time.deltaTime;
				}

				// Change state only if allowed, otherwise it will be manually done
				if(levelLoadingProgress >= 1) {
					if(m_startWhenLoaded) ChangeState(EStates.COUNTDOWN);
				}
			} break;

			// During countdown, let's just wait
			case EStates.COUNTDOWN: {
				if(m_timer > 0) {
					m_timer -= Time.deltaTime;
					if(m_timer <= 0) {
						ChangeState(EStates.RUNNING);
					}
				}
			} break;
				
			case EStates.RUNNING: {
				// Update running time
				m_elapsedSeconds += Time.deltaTime;
			} break;

			case EStates.FINISHED: {
				// Show the summary popup after some delay
				if(m_timer > 0) {
					m_timer -= Time.deltaTime;
					if(m_timer <= 0) {
						// Disable dragon and entities!
						InstanceManager.player.gameObject.SetActive(false);
						SpawnerManager.instance.DisableSpawners();

						// Enable Results screen and move the camera to that position
						if (m_resultsScreen != null) {
							m_resultsScreen.SetActive(true);
						}
					}
				}
			} break;
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Clear pools
		FirePropagationManager.DestroyInstance();
		PoolManager.Clear(true);

		// Call parent
		base.OnDestroy();
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
		ChangeState(EStates.LOADING_LEVEL);
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
				// Delete loading task
				m_levelLoadingTask = null;

				// Dispatch game event
				Messenger.Broadcast(GameEvents.GAME_LEVEL_LOADED);
				
				// Enable dragon back and put it in the spawn point
				// Don't make it playable until the countdown ends
				InstanceManager.player.playable = false;
				InstanceManager.player.gameObject.SetActive(true);
				// InstanceManager.player.MoveToSpawnPoint();
				InstanceManager.player.StartIntroMovement();

				// Spawn chest
				ChestManager.SelectChest();
				
				// Notify the game
				Messenger.Broadcast(GameEvents.GAME_STARTED);
			} break;

			case EStates.COUNTDOWN: {
				// Notify the game
				Messenger.Broadcast(GameEvents.GAME_COUNTDOWN_ENDED);
			} break;

			case EStates.RUNNING: {
				// Unsubscribe from external events
				Messenger.RemoveListener(GameEvents.PLAYER_DIED, OnPlayerDied);
			} break;
		}
		
		// Actions to perform when entering the new state
		switch(_newState) {
			case EStates.LOADING_LEVEL: {
				// Start loading current level
				m_levelLoadingTask = LevelManager.LoadLevel(UserProfile.currentLevel);

				// Initialize minimum loading time as well
				m_timer = MIN_LOADING_TIME;

				// Check whether the tutorial popup must be displayed
				if(!UserProfile.IsTutorialStepCompleted(TutorialStep.CONTROLS_POPUP)) {
					// Open popup
					PopupManager.OpenPopupInstant(PopupTutorialControls.PATH);
					UserProfile.SetTutorialStepCompleted(TutorialStep.CONTROLS_POPUP);
					PersistenceManager.Save();
				}
			} break;

			case EStates.COUNTDOWN: {
				LevelManager.SetArtSceneActive(UserProfile.currentLevel);
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
		return state > EStates.LOADING_LEVEL;
	}
}