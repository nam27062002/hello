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
/// Parent class for all mission objective types.
/// </summary>
[Serializable]
public class GlobalEventObjective {
	/*//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[NonSerialized] // [AOC] Avoid recursive serialization!!
	protected Mission m_parentMission = null;	// Only missions can create objectives, so this should never be null
	public Mission parentMission { get { return m_parentMission; }}

	// Objective tracking
	// Use float to have more precision while tracking (i.e. time), although target value is int
	[SerializeField] protected float m_currentValue = 0f;
	public float currentValue {
		get { return m_currentValue; }
		set { SetValue(value); }	// Use internal method, which will check for objective completion
	}

	private int m_targetValue = 0;
	public int targetValue { get { return m_targetValue; }}

	// Delegates
	public UnityEvent OnObjectiveComplete = new UnityEvent();

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Progress tracking
	public bool isCompleted { get { return currentValue == (float)targetValue; }}
	public float progress { get { return currentValue/(float)targetValue; }}

	// State
	public bool enabled { get; set; }

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Protected constructor, only to be used by heirs.
	/// </summary>
	/// <param name="_parentMission">The mission this objective belongs to.</param>
	protected GlobalEventObjective(Mission _parentMission) {
		// Store parent mission ref
		m_parentMission = _parentMission;

		// Initialize some values
		m_targetValue = _parentMission.def.GetAsInt("targetValue");

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
		OnObjectiveComplete.RemoveAllListeners();
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
	private void SetValue(float _newValue) {
		// Skip if not enabled
		if(!enabled) return;

		// Skip during first game session (tutorial)
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN)) return;

		// Skip if already completed
		if(isCompleted) return;

		// Store new value
		m_currentValue = Mathf.Min(_newValue, (float)targetValue);

		// Check completion
		if(isCompleted) {
			// Invoke delegate
			OnObjectiveComplete.Invoke();
		}
	}

	/// <summary>
	/// Get the TID for the name of this mission from the definition. If not defined,
	/// the default TID from the mission type definition will be returned instead.
	/// </summary>
	/// <returns>The TID for the name of this mission.</returns>
	protected string GetNameTID() {
		string tid = parentMission.def.GetAsString("tidName");
		if(string.IsNullOrEmpty(tid)) tid = parentMission.typeDef.GetAsString("tidName");
		return tid;
	}

	/// <summary>
	/// Get the TID for the description of this mission from the definition. If not defined,
	/// the default TID from the mission type definition will be returned instead.
	/// </summary>
	/// <returns>The TID for the description of this mission.</returns>
	protected string GetDescriptionTID() {
		string tid = parentMission.def.GetAsString("tidDesc");
		if(string.IsNullOrEmpty(tid)) {
			// Different default tids for single and multi run
			if(parentMission.def.GetAsBool("singleRun")) {
				tid = parentMission.typeDef.GetAsString("tidDescSingleRun");
			} else {
				tid = parentMission.typeDef.GetAsString("tidDescMultiRun");
			}
		}
		return tid;
	}

	//------------------------------------------------------------------//
	// OVERRIDE CANDIDATES												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets the name of this objective localized and properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The name properly formatted.</returns>
	public virtual string GetName() {
		// Default
        return LocalizationManager.SharedInstance.Localize(GetNameTID());
	}

	/// <summary>
	/// Gets the description of this objective localized and properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The description properly formatted.</returns>
	public virtual string GetDescription() {
		// Default
        return LocalizationManager.SharedInstance.Localize(GetDescriptionTID());
	}

	/// <summary>
	/// Gets the current value of this objective properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The current value properly formatted.</returns>
	public virtual string GetCurrentValueFormatted() {
		return StringUtils.FormatNumber(currentValue, 2);
	}

	/// <summary>
	/// Gets the target value of this objective properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The target value properly formatted.</returns>
	public virtual string GetTargetValueFormatted() {
		return StringUtils.FormatNumber(targetValue);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	public virtual void OnGameStarted() {
		// We only care if we're a single-run objective
		if(parentMission.def.GetAsBool("singleRun")) {
			// Reset counter
			m_currentValue = 0;
		}
	}

	/// <summary>
	/// A new game has just ended.
	/// </summary>
	public virtual void OnGameEnded() {
		// We only care if we're a single-run objective
		// Don't reset if objective was completed
		if(parentMission.def.GetAsBool("singleRun") && !isCompleted) {
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
	public static GlobalEventObjective Create(Mission _parentMission) {
		// Create a new objective based on mission type
		switch(_parentMission.def.GetAsString("typeSku")) {
			case "score":			return new GlobalEventObjectiveScore(_parentMission);
			case "gold":			return new GlobalEventObjectiveGold(_parentMission);
			case "survive_time":	return new GlobalEventObjectiveSurviveTime(_parentMission);
			case "kill":			return new GlobalEventObjectiveKill(_parentMission);
			case "burn":			return new GlobalEventObjectiveBurn(_parentMission);
			case "dive":			return new GlobalEventObjectiveDive(_parentMission);
			case "dive_time":		return new GlobalEventObjectiveDiveTime(_parentMission);
			case "fire_rush":		return new GlobalEventObjectiveFireRush(_parentMission);
		}

		// Unrecoginzed mission type, aborting
		return null;
	}*/
}