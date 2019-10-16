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
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Objective for a global event.
/// </summary>
[Serializable]
public class GlobalEventObjective : TrackingObjectiveBase, IBroadcastListener {
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

	private string m_icon = "";
	public string icon {
		get { return m_icon; }
	}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentEvent">The event owning this objective.</param>
	/// <param name="_data">The event goal json object used to initialize this objective.</param>
	public GlobalEventObjective(GlobalEvent _parentEvent, SimpleJSON.JSONNode _data) {
		// Check params
		Debug.Assert(_data != null && !_data.IsNull);

		// Store parent event
		m_parentEvent = _parentEvent;

		// Gather type definition
		m_typeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, _data["type"]);


		// Parse parameters (if any)
		List<string> parameters = new List<string>();
		if(_data.ContainsKey("params")) {
			string paramsString = _data["params"];
			string[] splitResult = paramsString.Split(new string[] { ";" }, StringSplitOptions.None);
			parameters.AddRange(splitResult);
		}

        string zone = _data["zone"];

        // Get icon
        m_icon = _data["icon"];

		// Use parent's initializer
		Init(
			TrackerBase.CreateTracker(m_typeDef.sku, parameters, zone),		// Create the tracker based on goal type
			_data["amount"].AsLong,
			m_typeDef,
			_data["tidDesc"]
		);

		// Tell tracker it's being used by a global event
		m_tracker.mode = TrackerBase.Mode.GLOBAL_EVENT;

		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~GlobalEventObjective() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_ENDED:
            {
                OnGameEnded();
            }break;
        }
    }

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
    
	/// <summary>
	/// A new game has started.
	/// </summary>
	public virtual void OnGameStarted() {
		// Reset counter!
		m_tracker.InitValue(0);

		// Disable during FTUX
		this.enabled = (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_QUESTS_AT_RUN);

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