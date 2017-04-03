// GameSceneControllerBase.cs
// Hungry Dragon
// 
// Created by Marc Saña Castellví on 26/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Base class for all scene controllers.
/// Each scene should have an object containing one of these, usually a custom
/// implementation of this class.
/// </summary>
public class GameSceneControllerBase : SceneController {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Time
	protected float m_elapsedSeconds = 0;
	public float elapsedSeconds {
		get { return m_elapsedSeconds; }
	}

	// Map status
	// [AOC] If the map timer runs out during the game, we let the player enjoy the unlocked map for the whole run
	protected bool m_mapUnlocked = false;
	public bool mapUnlocked {
		get { return m_mapUnlocked; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Call parent
		base.Awake();

		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
		Messenger.AddListener(GameEvents.PROFILE_MAP_UNLOCKED, OnMapUnlocked);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Call parent
		base.OnDestroy();

		// Unsubscribe to external events
		Messenger.RemoveListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);
		Messenger.RemoveListener(GameEvents.PROFILE_MAP_UNLOCKED, OnMapUnlocked);
	}

	public virtual bool IsLevelLoaded()
	{
		return true;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The game has started.
	/// </summary>
	protected virtual void OnGameStarted() {
		// Store map state
		if(UsersManager.currentUser != null) {
			m_mapUnlocked = UsersManager.currentUser.mapUnlocked;
		}
	}

	/// <summary>
	/// The game has eneded.
	/// </summary>
	protected virtual void OnGameEnded() {
		
	}

	/// <summary>
	/// Minimap has been upgraded.
	/// </summary>
	private void OnMapUnlocked() {
		// Store new map state
		if(UsersManager.currentUser != null) {
			m_mapUnlocked = UsersManager.currentUser.mapUnlocked;
		}
	}
}

