// MissionObjective.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Parent class for all mission objective types.
/// </summary>
[Serializable]
public class MissionObjective {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Type of implemented objectives, mainly used to fill missions content
	// Add here any new objective implementation
	public enum Type {
		SCORE
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[NonSerialized] // [AOC] Avoid recursive serialization!!
	protected Mission m_parentMission = null;	// Only missions can create objectives, so this should never be null
	public Mission parentMission { get { return m_parentMission; }}

	// Objective tracking
	[SerializeField] protected int m_currentValue = 0;
	public int currentValue {
		get { return m_currentValue; }
		set { SetValue(value); }	// Use internal method, which will check for objective completion
	}

	// Delegates
	public delegate void OnObjectiveCompleteDelegate();
	public OnObjectiveCompleteDelegate OnObjectiveComplete = delegate() { };	// Default initialization to avoid null reference when invoking. Add as many listeners as you want to this specific event by using the += syntax

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Progress tracking
	public bool isCompleted { get { return currentValue == parentMission.def.targetValue; }}
	public float progress { get { return (float)currentValue/(float)parentMission.def.targetValue; }}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Protected constructor, only to be used by heirs.
	/// </summary>
	/// <param name="_parentMission">The mission this objective belongs to.</param>
	protected MissionObjective(Mission _parentMission) {
		// Store parent mission ref
		m_parentMission = _parentMission;

		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Leave the objective ready for garbage collection.
	/// </summary>
	public virtual void Clear() {
		// Clear external references
		m_parentMission = null;

		// Just in case, make sure delegate loses all its references
		OnObjectiveComplete = null;

		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Set the current value and check for objective completion.
	/// Will be ignored if objective is already completed.
	/// </summary>
	/// <param name="_newValue">The new value to be set.</param>
	private void SetValue(int _newValue) {
		// Skip if already completed
		if(isCompleted) return;

		// Store new value
		m_currentValue = Mathf.Min(_newValue, parentMission.def.targetValue);

		// Check completion
		if(isCompleted) {
			// Invoke delegate
			OnObjectiveComplete();
		}
	}

	//------------------------------------------------------------------//
	// OVERRIDE CANDIDATES												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets the description of this objective.
	/// Override to customize text in specific objectives.
	/// </summary>
	public virtual string GetDescription() {
		// Default
		return Localization.Localize(parentMission.def.tidDesc);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	public void OnGameStarted() {
		// We only care if we're a single-run objective
		if(parentMission.def.singleRun) {
			// Reset counter
			m_currentValue = 0;
		}
	}

	/// <summary>
	/// A new game has just ended.
	/// </summary>
	public void OnGameEnded() {
		// We only care if we're a single-run objective
		// Don't reset if objective was completed
		if(parentMission.def.singleRun && !isCompleted) {
			// Reset counter
			m_currentValue = 0;
		}
	}

	//------------------------------------------------------------------//
	// FACTORY															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Create and initialize a new objective based on the given mission.
	/// Should only be called by Mission class.
	/// </summary>
	/// <returns>A new objective. <c>null</c> if the type was not recognized.</returns>
	/// <param name="_parentMission">The mission where the new objective will belong.</param>
	public static MissionObjective Create(Mission _parentMission) {
		// Create a new objective based on mission type
		switch(_parentMission.def.type) {
			case Type.SCORE: return new MissionObjectiveScore(_parentMission); break;
		}

		// Unrecoginzed mission type, aborting
		return null;
	}
}