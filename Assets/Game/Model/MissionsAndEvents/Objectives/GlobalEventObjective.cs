// GlobalEventObjective.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Objective for a global event.
/// </summary>
[Serializable]
public class GlobalEventObjective : TrackingObjectiveBase {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	protected GlobalEvent m_parentEvent = null;
	public GlobalEvent parentEvent {
		get { return m_parentEvent; }
	}

	private DefinitionNode m_goalDef = null;
	public DefinitionNode goalDef {
		get { return m_goalDef; }
	}

	private DefinitionNode m_typeDef = null;
	public DefinitionNode typeDef {
		get { return m_typeDef; }
	}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentEvent">The event owning this objective.</param>
	/// <param name="_goalDef">The event goal definition used to initialize this objective.</param>
	/// <param name="_targetValue">Target value.</param>
	public GlobalEventObjective(GlobalEvent _parentEvent, DefinitionNode _goalDef, float _targetValue) {
		// Check params
		Debug.Assert(_goalDef != null);

		// Store parent event
		m_parentEvent = _parentEvent;

		// Store goal definition
		m_goalDef = _goalDef;

		// Gather type definition
		m_typeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, m_goalDef.Get("type"));

		// Use parent's initializer
		Init(
			TrackerBase.CreateTracker(m_typeDef.sku, m_goalDef.GetAsList<string>("params")),		// Create the tracker based on goal type
			_targetValue,	// [AOC] TODO!! Target value is actually pointless in global events, but we may use it as a cap to detect cheaters
			m_typeDef,
			m_goalDef.Get("titleTID")
		);

		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~GlobalEventObjective() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	public virtual void OnGameStarted() {
		// Reset counter!
		m_tracker.SetValue(0f, false);

		// Disable during first game session (tutorial)
		this.enabled = (UsersManager.currentUser.gamesPlayed > 0);

		// Disable too if parent event is not active!
		this.enabled &= m_parentEvent.isActive;
	}

	/// <summary>
	/// A new game has just ended.
	/// </summary>
	public virtual void OnGameEnded() {
		// Nothing to do for now
	}

	/// <summary>
	/// The tracker's value has changed.
	/// </summary>
	override public void OnValueChanged() {
		// Nothing to do for now
		// Unlike mission objectives, we don't want to stop tracking even if the objective has been completed!
		// [AOC] TODO!! Target value is actually pointless in global events, but we may use it as a cap to detect cheaters
	}
}