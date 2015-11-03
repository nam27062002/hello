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
/// [AOC] TODO!! Split score management into a different component
/// [AOC] TODO!! Score multipliers
/// </summary>
public class DragonBirdsSceneController : SceneController {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_World_Scale";

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
	
	// Logic state
	private EStates m_state = EStates.INIT;
	public EStates state {
		get { return m_state; }
	}

	public bool paused {
		get { return m_state == EStates.PAUSED; }
	}
	
	// Reference to player
	private DragonMotion m_player = null;
	public DragonMotion player {
		get { return m_player; }
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
		// [AOC] HACK!! Fool the SceneManager into thinking we're playing the game
		SceneManager.SetCurrentScene(GameSceneController.NAME);

		// Load the dragon
		DragonManager.LoadDragon(UserProfile.currentDragon);

		// Call parent
		base.Awake();
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
				// [AOC] TODO!! Show some feedback
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
						PopupManager.OpenPopupAsync(PopupSummary.PATH);
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

		// Dispatch game event
		Messenger.Broadcast(GameEvents.GAME_STARTED);
		
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
		}
		
		// Actions to perform when entering the new state
		switch(_newState) {
			case EStates.COUNTDOWN: {
				// Start countdown timer
				m_timer = 3f;	// [AOC] TODO!! Do it properly
			} break;
				
			case EStates.RUNNING: {
				// Subscribe to external events
				Messenger.AddListener(GameEvents.PLAYER_DIED, OnPlayerDied);
			} break;
				
			case EStates.PAUSED: {
				// Ignore if not running
				if(m_state != EStates.RUNNING) return;
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

		// Open summary popup after some delay
		m_timer = 1f;
	}

	/// <summary>
	/// Callback for the dragon change button.
	/// </summary>
	/// <param name="_newDragonType">The new dragon to be loaded.</param>
	public void OnChangeDragonType(int _newDragonType) {
		// Store new dragon type into the user profile
		UserProfile.currentDragon = (DragonId)_newDragonType;

		// Reload scene
		Application.LoadLevel(Application.loadedLevel);		// [AOC] Trick to hard-reload current level
	}


	public void OnToggleEat() {
		DragonEatBehaviour b = m_player.GetComponent<DragonEatBehaviour>();
		b.enabled = !b.enabled;
	}

	public void OnToggleBreath() {
		DragonBreathBehaviour b = m_player.GetComponent<DragonBreathBehaviour>();
		b.enabled = !b.enabled;		
	}
}

