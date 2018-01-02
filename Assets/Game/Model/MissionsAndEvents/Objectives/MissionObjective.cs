﻿// MissionObjective_NEW.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//#define LOG

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
/// Objective for a mission.
/// </summary>
[Serializable]
public class MissionObjective : TrackingObjectiveBase {
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
	public MissionObjective(Mission _parentMission, DefinitionNode _missionDef, DefinitionNode _typeDef, float _targetValue, bool _singleRun) {
		#if LOG
		DebugUtils.Log("<color=green>Creating MissionObjective:</color> " + _parentMission + ", " + _missionDef + ", " + _typeDef + ", " + _targetValue + ", " + _singleRun);
		#endif

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

		// Use parent's initializer
		Init(
			TrackerBase.CreateTracker(_typeDef.sku, _missionDef.GetAsList<string>("params")),		// Create the tracker based on mission type
			_targetValue,
			_typeDef,
			tidDesc,
			_missionDef.Get("tidObjective")	// Does this mission have a custom target TID? (i.e. "Birds", "Archers", etc.)
		);

		// Tell tracker it's being used by a mission
		m_tracker.mode = TrackerBase.Mode.MISSION;

		#if LOG
		DebugUtils.Log("<color=green>Done! </color>" + ToString());
		#endif

		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~MissionObjective() {
		#if LOG
		Debug.Log("<color=red>Destroying MissionObjective</color> " + ToString());
		#endif
	}

	/// <summary>
	/// Leave the objective ready for garbage collection.
	/// </summary>
	override public void Clear() {
		#if LOG
		DebugUtils.Log("<color=orange>Clearing MissionObjective </color>" + ToString());
		#endif

		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);

		// Forget parent mission reference
		m_parentMission = null;

		// Call parent
		base.Clear();
	}

	/// <summary>
	/// String representation of this objective.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="MissionObjective"/>.</returns>
	public string ToString() {
		string str = "";
		if(m_parentMission == null) {
			str += "<color=red>Mission NULL</color>";
		} else {
			if(m_parentMission.def == null) {
				str += "<color=red>Mission Def NULL</color>";
			} else {
				str += m_parentMission.def.sku;
			}
		}

		str += " | ";

		if(m_tracker == null) {
			str += "<color=red>Tracker NULL</color>";
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
			m_tracker.SetValue(0f, false);

			#if LOG
			DebugUtils.Log("<color=red>Resetting mission! </color>" + ToString());
			#endif
		}

		// Disable during FTUX
		this.enabled = (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_MISSIONS_AT_RUN);

		// Disable too if mission is not active
		this.enabled &= m_parentMission.state == Mission.State.ACTIVE;
	}

	/// <summary>
	/// A new game has just ended.
	/// </summary>
	public virtual void OnGameEnded() {
		// If we're a single-run objective, reset counter
		// Unless objective was completed
		if(m_singleRun && !isCompleted) {
			m_tracker.SetValue(0f, false);

			#if LOG
			DebugUtils.Log("<color=red>Resetting mission! </color> " + ToString());
			#endif
		}
	}

	#if LOG
	int m_lastIntValue = 0;
	#endif

	/// <summary>
	/// The tracker's value has changed.
	/// </summary>
	override public void OnValueChanged() {
		#if LOG
		int newIntValue = Mathf.FloorToInt(currentValue);
		if(newIntValue != m_lastIntValue) {
			DebugUtils.Log(ToString() + "<color=yellow>: " + currentValue + "/" + targetValue + "</color>");
			m_lastIntValue = newIntValue;
		}
		#endif

		// Check completion
		if(isCompleted) {
			// Cap value to target value
			m_tracker.SetValue(Mathf.Min(currentValue, (float)targetValue), false);

			// Stop tracking
			m_tracker.enabled = false;

			// Invoke delegate
			OnObjectiveComplete.Invoke();
		}
	}
}