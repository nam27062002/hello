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

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_missionDef">The mission definition used to initialize this objective.</param>
	/// <param name="_typeDef">The mission type definition.</param>
	/// <param name="_targetValue">Target value.</param>
	/// <param name="_singleRun">Is it a single run mission?</param>
	public MissionObjective(DefinitionNode _missionDef, DefinitionNode _typeDef, float _targetValue, bool _singleRun) {
		// Check params
		Debug.Assert(_missionDef != null);

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
		}

		// Disable during first game session (tutorial)
		this.enabled = (UsersManager.currentUser.gamesPlayed > 0);
	}

	/// <summary>
	/// A new game has just ended.
	/// </summary>
	public virtual void OnGameEnded() {
		// If we're a single-run objective, reset counter
		// Unless objective was completed
		if(m_singleRun && !isCompleted) {
			m_tracker.SetValue(0f, false);
		}
	}

	/// <summary>
	/// The tracker's value has changed.
	/// </summary>
	override public void OnValueChanged() {
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