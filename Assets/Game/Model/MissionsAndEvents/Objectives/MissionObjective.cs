// MissionObjective_NEW.cs
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
/// Objective for a mission.
/// </summary>
[Serializable]
public class MissionObjective : TrackingObjectiveBase, IBroadcastListener {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	protected bool m_singleRun = false;
	public bool singleRun { get { return m_singleRun; }}

	protected Mission m_parentMission = null;
	public Mission parentMission { get { return m_parentMission; }}

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentMission">The mission owning this objective.</param>
	/// <param name="_missionDef">The mission definition used to initialize this objective.</param>
	/// <param name="_typeDef">The mission type definition.</param>
	/// <param name="_targetValue">Target value.</param>
	/// <param name="_singleRun">Is it a single run mission?</param>
	public MissionObjective(Mission _parentMission, DefinitionNode _missionDef, DefinitionNode _typeDef, long _targetValue, bool _singleRun) {

		// Check params
		DebugUtils.Assert(_missionDef != null, "Mission Def Null!");

		// Store parent mission
		m_parentMission = _parentMission;

		// Single run?
		m_singleRun = _singleRun;

		// Find out description TID
		// Different description for single and multi run
		string tidDesc = string.Empty;
		if(m_singleRun) {
			tidDesc = _typeDef.GetAsString("tidDescSingleRun");
		} else {
			tidDesc = _typeDef.GetAsString("tidDescMultiRun");
		}

		// Check params in not empty before asking for a List
		List<string> _params;
		string paramCheck = _missionDef.GetAsString("params", null);
		if ( !string.IsNullOrEmpty( paramCheck ) ){
			_params = _missionDef.GetAsList<string>("params");
		}else{
			_params = new List<string>();
		}

        // Zone
        m_zone = _missionDef.GetAsString("zone", null);


        // Use parent's initializer
        Init(
			TrackerBase.CreateTracker(_typeDef.sku, _params, m_zone),		// Create the tracker based on mission type
			_targetValue,
			_typeDef,
			tidDesc,
			_missionDef.Get("tidObjective")	// Does this mission have a custom target TID? (i.e. "Birds", "Archers", etc.)
		);

		// Tell tracker it's being used by a mission
		m_tracker.mode = TrackerBase.Mode.MISSION;


		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~MissionObjective() {
	}

	/// <summary>
	/// Leave the objective ready for garbage collection.
	/// </summary>
	override public void Clear() {

		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);

		// Forget parent mission reference
		m_parentMission = null;

		// Call parent
		base.Clear();
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
    

	/// <summary>
	/// String representation of this objective.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="MissionObjective"/>.</returns>
	public string ToString() {
		string str = "";
		if(m_parentMission == null) {
			str += Color.red.Tag("Mission NULL");
		} else {
			if(m_parentMission.def == null) {
				str += Color.red.Tag("Mission Def NULL");
			} else {
				str += m_parentMission.def.sku;
			}
		}

		str += " | ";

		if(m_tracker == null) {
			str += Color.red.Tag("Tracker NULL");
		} else {
			str += m_tracker.GetType().ToString();
		}

		return str;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	public virtual void OnGameStarted() {
		// If we're a single-run objective, reset counter
		if(m_singleRun) {
			m_tracker.InitValue(0);
		}
	}

	/// <summary>
	/// A new game has just ended.
	/// </summary>
	public virtual void OnGameEnded() {
		// If we're a single-run objective, reset counter
		// Unless objective was completed
		if(m_singleRun && !isCompleted) {
			m_tracker.InitValue(0);
		}
	}


	/// <summary>
	/// The tracker's value has changed.
	/// </summary>
	override public void OnValueChanged() {

		// Check completion
		if(isCompleted) {
			// Cap value to target value
			m_tracker.InitValue(Math.Min(currentValue, targetValue));

			// Stop tracking
			m_tracker.enabled = false;

			// Invoke delegate
			OnObjectiveComplete.Invoke();
		}
	}
}