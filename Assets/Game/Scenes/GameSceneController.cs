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
public class GameSceneController : SceneController {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_Game";
	public static readonly float COUNTDOWN = 3f;	// Seconds

	public enum EStates {
		INIT,
		COUNTDOWN,
		RUNNING,
		PAUSED,
		FINISHED
	};

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// [AOC] We want these to be consulted but never set from outside, so don't add a setter
	// Time
	private float m_elapsedSeconds = 0;
	public float elapsedSeconds {
		get { return m_elapsedSeconds; }
	}

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

	public bool paused {
		get { return m_state == EStates.PAUSED; }
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Internal vars
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
	void Start() {
		// Let's play!
		StartGame();
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update() {
		// Different actions based on current state
		switch(m_state) {
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
						// Open popup!
						PopupManager.OpenPopup(PopupSummary.PATH);
					}
				}
			} break;
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
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
		
		// Reset rewards
		RewardManager.Reset();
		
		// [AOC] TODO!! Reset game stats
		
		// Change state
		ChangeState(EStates.COUNTDOWN);
	}
	
	/// <summary>
	/// End the current game. Wont reset the stats so they can be used.
	/// </summary>
	public void EndGame() {
		// Change state
		ChangeState(EStates.FINISHED);
		
		// [AOC] TODO!! Update global stats

		// Save persistence
		PersistenceManager.Save();

		// Dispatch game event
		Messenger.Broadcast(GameEvents.GAME_ENDED);
	}

	/// <summary>
	/// Pause/resume
	/// </summary>
	/// <param name="_bPause">Whether to pause the game or resume it.</param>
	public void PauseGame(bool _bPause) {
		// Only when playing
		if(state == EStates.PAUSED || state == EStates.RUNNING) {
			if(_bPause) {
				ChangeState(EStates.PAUSED);
			} else {
				ChangeState(EStates.RUNNING);
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
			case EStates.RUNNING: {
				// Unsubscribe from external events
				Messenger.RemoveListener(GameEvents.PLAYER_DIED, OnPlayerDied);
			} break;

			case EStates.PAUSED: {
				// Notify the game
				Messenger.Broadcast<bool>(GameEvents.GAME_PAUSED, false);
			} break;
		}
		
		// Actions to perform when entering the new state
		switch(_newState) {
			case EStates.COUNTDOWN: {
				// Start countdown timer
				m_timer = COUNTDOWN;

				// Disable dragon so its HP doesn't go down
				InstanceManager.player.gameObject.SetActive(false);

				// Notify the game
				Messenger.Broadcast(GameEvents.GAME_COUNTDOWN_STARTED);
			} break;
				
			case EStates.RUNNING: {
				// Enable dragon back!
				InstanceManager.player.gameObject.SetActive(true);

				// Subscribe to external events
				Messenger.AddListener(GameEvents.PLAYER_DIED, OnPlayerDied);

				// Notify the game
				if(m_state == EStates.COUNTDOWN) {
					Messenger.Broadcast(GameEvents.GAME_STARTED);
				}
			} break;
				
			case EStates.PAUSED: {
				// Ignore if not running
				if(m_state != EStates.RUNNING) return;

				// Notify the game
				Messenger.Broadcast<bool>(GameEvents.GAME_PAUSED, true);
			} break;
		}
		
		// Store new state
		m_state = _newState;
	}

	/// <summary>
	/// The player has died.
	/// </summary>
	private void OnPlayerDied() {
		// End game
		EndGame();

		// Open summary popup after some delay
		m_timer = 1f;
	}
}

