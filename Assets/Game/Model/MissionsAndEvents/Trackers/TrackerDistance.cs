// TrackerDistance.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for traveling distance.
/// </summary>
public class TrackerDistance : TrackerBase {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	// Internal
	private Vector3 m_lastPosition = Vector3.zero;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerDistance() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(GameEvents.GAME_UPDATED, OnGameUpdated);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerDistance() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(GameEvents.GAME_UPDATED, OnGameUpdated);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	private void OnGameStarted() {
		// Store initial dragon position
		m_lastPosition = InstanceManager.player.transform.position;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnGameUpdated() {
		// We'll receive this event only while the game is actually running, so no need to check anything else
		currentValue += (InstanceManager.player.transform.position - m_lastPosition).magnitude;
		//Debug.Log("<color=orange>" + (InstanceManager.player.transform.position - m_lastPosition).magnitude + "</color><color=red> (" + currentValue + ")</color>");
		m_lastPosition = InstanceManager.player.transform.position;
	}
}