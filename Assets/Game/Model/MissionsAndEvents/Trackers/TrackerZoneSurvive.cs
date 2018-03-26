// TrackerSurviveTime.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for survival time.
/// </summary>
public class TrackerZoneSurvive : TrackerBase {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	bool m_inside = false;
	private List<string> m_targetSkus = null;
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerZoneSurvive(List<string> _targetSkus) {
		// Subscribe to external events
		m_targetSkus = _targetSkus;

		Messenger.AddListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);
		Messenger.AddListener<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, OnZone);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerZoneSurvive() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);
		Messenger.RemoveListener<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, OnZone);
		// Call parent
		base.Clear();
	}

	/// <summary>
	/// Localizes and formats a value according to this tracker's type
	/// (i.e. "52", "500 meters", "10 minutes").
	/// </summary>
	/// <returns>The localized and formatted value for this tracker's type.</returns>
	/// <param name="_value">Value to be formatted.</param>
	override public string FormatValue(float _value) {
		// Format value as time
		// [AOC] Different formats for global events!
		TimeUtils.EFormat format = TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES;
		if(m_mode == Mode.GLOBAL_EVENT) {
			format = TimeUtils.EFormat.WORDS_WITHOUT_0_VALUES;
		}
		return TimeUtils.FormatTime(_value, format, 3, TimeUtils.EPrecision.DAYS);
	}

	/// <summary>
	/// Round a value according to specific rules defined for every tracker type.
	/// Typically used for target values.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="_targetValue">The original value to be rounded.</param>
	override public float RoundTargetValue(float _targetValue) {
		// Time value, round it to 10s multiple
		_targetValue = MathUtils.Snap(_targetValue, 10f);
		return base.RoundTargetValue(_targetValue);	// Apply default rounding as well
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnGameUpdated() {
		// We'll receive this event only while the game is actually running, so no need to check anythin
		if ( m_inside ){
			currentValue += Time.deltaTime;
		}
	}

	private void OnZone(bool toggle, ZoneTrigger zone){
		if (toggle){
			if ( m_targetSkus.Contains( zone.m_zoneId ) )
				m_inside = true;
		}else{
			if ( m_targetSkus.Contains( zone.m_zoneId ) )
				m_inside = false;
		}
	}
}