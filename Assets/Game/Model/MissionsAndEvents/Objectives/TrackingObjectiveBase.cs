// ObjectiveBase.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/03/2017.
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
/// Parent class for all objective types.
/// </summary>
[Serializable]
public abstract class TrackingObjectiveBase {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Objective tracking
	[SerializeField] protected TrackerBase m_tracker = null;
	public TrackerBase tracker { get { return m_tracker; }}

	// Use float to have more precision while tracking (i.e. time)
	[SerializeField] protected long m_targetValue = 0;
	public long targetValue { get { return m_targetValue; }}
	public long currentValue { 
		get { return m_tracker.currentValue; }
		set { m_tracker.currentValue = value; }
	}

    // Zone
    protected string m_zone = null;
    public string zone { get { return m_zone; } }

    // Recommended dragon
    protected IDragonData m_recommendedDragon = null;
    public IDragonData recommendedDragon { get { return m_recommendedDragon; } }

    // Progress tracking
    public bool isCompleted { 
		get { return m_tracker.currentValue >= targetValue; }
	}

	public float progress { 
		get { return m_targetValue > 0f ? (float)((double)m_tracker.currentValue/(double)targetValue) : 1f; }	// Protect for infinite values!
	}

	// State
	public bool enabled {
		get { return m_tracker.enabled; }
		set { 
            m_tracker.enabled = value; }
	}

	// UI and other data
	[SerializeField] protected string m_tidDesc = "";		// Main objective TID, typically with at least one replacement %U0 for the amount and optional extra replacements for the objectives (i.e. "Eat %U0 %U1", "Swim %U0 meters")
	[SerializeField] protected string m_tidTarget = "";		// Optional TID for the objective's target (i.e. "Birds")

	[SerializeField] protected DefinitionNode m_typeDef = null;
	public DefinitionNode typeDef { get { return m_typeDef; }}

	// Delegates
	public UnityEvent OnObjectiveComplete = new UnityEvent();

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	protected TrackingObjectiveBase() {

	}

	/// <summary>
	/// Initializer, only to be used by heirs.
	/// </summary>
	/// <param name="_tracker">Tracker to be used for this objective.</param>
	/// <param name="_targetValue">Objective's target value.</param>
	/// <param name="_typeDef">Definition of hte objective type.</param>
	/// <param name="_tidDesc">TID corresponding to the objective's description. Typically contains at least one replacement %U0 for the amount and optional extra replacements for the objectives (i.e. "Eat %U0 %U1", "Swim %U0 meters").</param>
	/// <param name="_tidTarget">Optional TID for the objective's target (i.e. "Birds").</param>
	public virtual void Init(TrackerBase _tracker, long _targetValue, DefinitionNode _typeDef, string _tidDesc, string _tidTarget = "") {
		// Check some required parameters
		Debug.Assert(_tracker != null, "Invalid Tracker!");

		// Init tracker
		m_tracker = _tracker;
		m_tracker.OnValueChanged.AddListener(OnValueChanged);

		// Store parameters
		m_targetValue = m_tracker.RoundTargetValue(_targetValue);	// Round target value using each specific tracker rounding rules!
		m_typeDef = _typeDef;
		m_tidDesc = _tidDesc;
		m_tidTarget = _tidTarget;

		// Make sure complete event is valid
		if(OnObjectiveComplete == null) {
			OnObjectiveComplete = new UnityEvent();
		}
	}

	/// <summary>
	/// Leave the objective ready for garbage collection.
	/// </summary>
	public virtual void Clear() {
		// Just in case, make sure delegate loses all its references
		OnObjectiveComplete.RemoveAllListeners();
		OnObjectiveComplete = null;

		// Unsubscribe from events
		if(m_tracker != null) {
			m_tracker.Clear();
			m_tracker = null;	// Lose reference so it gets garbage-collected
		}
	}

	//------------------------------------------------------------------//
	// OVERRIDE CANDIDATES												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets the description of this objective localized and properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The description properly formatted.</returns>
	public virtual string GetDescription() {
		// Use replacements?
		if(string.IsNullOrEmpty(m_tidTarget)) {
			return m_tracker.FormatDescription(m_tidDesc, targetValue);
		} else {
			return m_tracker.FormatDescription(m_tidDesc, targetValue, LocalizationManager.SharedInstance.Localize(m_tidTarget));
		}
	}

	/// <summary>
	/// Gets the current value of this objective properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The current value properly formatted.</returns>
	public virtual string GetCurrentValueFormatted() {
		return m_tracker.FormatValue(m_tracker.currentValue);
	}

	/// <summary>
	/// Gets the target value of this objective properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The target value properly formatted.</returns>
	public virtual string GetTargetValueFormatted() {
		return m_tracker.FormatValue(targetValue);
	}

	/// <summary>
	/// Gets the progress string, custom formatted based on objective type.
	/// </summary>
	/// <returns>The progress string properly formatted.</returns>
	/// <param name="_showTarget">Show target value? (i.e. "25/40"). Some types might override this setting if not appliable.</param>
	public virtual string GetProgressString(bool _showTarget = true) {
		return m_tracker.GetProgressString(m_tracker.currentValue, targetValue, _showTarget);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The tracker's value has changed.
	/// </summary>
	public virtual void OnValueChanged() {
		// For heirs to override
	}
}