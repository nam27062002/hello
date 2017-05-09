// TrackerBase.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class for trackers used in missions and global events.
/// </summary>
[Serializable]
public class TrackerBase {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Variables
	[SerializeField] private float m_currentValue = 0f;
	public float currentValue { 
		get { return m_currentValue; }
		set { SetValue(value, true); }
	}

	private bool m_enabled = true;
	public bool enabled {
		get { return m_enabled; }
		set { m_enabled = value; }
	}

	// Events
	public UnityEvent OnValueChanged = new UnityEvent();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerBase() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBase() {

	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATE METHODS											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Localizes and formats the description according to this tracker's type
	/// (i.e. "Eat 52 birds", "Dive 500m", "Survive 10 minutes").
	/// </summary>
	/// <returns>The localized and formatted description for this tracker's type.</returns>
	/// <param name="_tid">Description TID to be formatted.</param>
	/// <param name="_targetValue">Target value.</param>
	public virtual string FormatDescription(string _tid, float _targetValue) {
		// Default: just attach formatted value to the given tid and localize
		return LocalizationManager.SharedInstance.Localize(_tid, FormatValue(_targetValue));
	}

	/// <summary>
	/// Localizes and formats a value according to this tracker's type
	/// (i.e. "52", "500 meters", "10 minutes").
	/// </summary>
	/// <returns>The localized and formatted value for this tracker's type.</returns>
	/// <param name="_value">Value to be formatted.</param>
	public virtual string FormatValue(float _value) {
		// Default: the number as is
		return StringUtils.FormatNumber(_value, 2);
	}

	/// <summary>
	/// Sets a new value and optionally triggers OnValueChanged event.
	/// </summary>
	/// <param name="_newValue">The new value to be set.</param>
	/// <param name="_triggerEvent">Whether to trigger the OnValueChanged event or not.</param>
	public virtual void SetValue(float _newValue, bool _triggerEvent) {
		// Skip if not enabled
		if(!m_enabled) return;

		// Nothing to do if value is already set
		if(m_currentValue == _newValue) return;

		// Store new value
		m_currentValue = _newValue;

		// Trigger event (if required)
		if(_triggerEvent) OnValueChanged.Invoke();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// FACTORY																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create and initialize a new tracker based on the given type.
	/// </summary>
	/// <returns>A new tracker. <c>null</c> if the type was not recognized.</returns>
	/// <param name="_typeSku">The type of tracker to be created. Typically a sku from the MissionTypesDefinitions table.</param>
	/// <param name="_params">Optional parameters to be passed to the tracker. Depend on tracker's type.</param>
	public static TrackerBase CreateTracker(string _typeSku, List<string> _params = null) {
		// Create a new objective based on mission type
		switch(_typeSku) {
			default: return null;
			case "score":			return new TrackerScore();
			case "gold":			return new TrackerGold();
			case "survive_time":	return new TrackerSurviveTime();
			case "kill":			return new TrackerKill(_params);
			case "burn":			return new TrackerBurn(_params);
			case "dive":			return new TrackerDiveDistance();
			case "dive_time":		return new TrackerDiveTime();
			case "fire_rush":		return new TrackerFireRush();

			// Collect is quite special: depending on first parameter, create one of the existing trackers
			case "collect": {
				if(_params.Count < 1) return null;
				switch(_params[0]) {
					case "coins":	return new TrackerGold();
					//case "eggs":	return new TrackerGold();		// [AOC] TODO!!
					//case "chests":	return new TrackerGold();	// [AOC] TODO!!
				}
			} break;
		}

		// Unrecoginzed mission type, aborting
		return null;
	}
}