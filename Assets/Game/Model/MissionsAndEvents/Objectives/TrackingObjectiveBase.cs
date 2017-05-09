﻿// ObjectiveBase.cs
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
	[SerializeField] protected float m_targetValue = 0f;
	public float targetValue { get { return m_targetValue; }}
	public float currentValue { 
		get { return m_tracker.currentValue; }
		set { m_tracker.currentValue = value; }
	}

	// Progress tracking
	public bool isCompleted { get { return m_tracker.currentValue >= targetValue; }}
	public float progress { get { return m_tracker.currentValue/targetValue; }}

	// State
	public bool enabled {
		get { return m_tracker.enabled; }
		set { m_tracker.enabled = value; }
	}

	// UI and other data
	[SerializeField] protected string m_tidDesc = "";
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
	/// <param name="_tidDesc">TID corresponding to the objective's description.</param>
	protected virtual void Init(TrackerBase _tracker, float _targetValue, string _tidDesc, DefinitionNode _typeDef) {
		// Check some required parameters
		Debug.Assert(_tracker != null);

		// Init tracker
		m_tracker = _tracker;
		m_tracker.OnValueChanged.AddListener(OnValueChanged);

		// Store parameters
		m_targetValue = _targetValue;
		m_tidDesc = _tidDesc;
		m_typeDef = _typeDef;
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
			m_tracker.OnValueChanged.RemoveListener(OnValueChanged);
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
		// Default
		return m_tracker.FormatDescription(m_tidDesc, targetValue);
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