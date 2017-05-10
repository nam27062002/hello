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
public class TrackerSurviveTime : TrackerBase {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerSurviveTime() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_UPDATED, OnGameUpdated);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerSurviveTime() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_UPDATED, OnGameUpdated);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Localizes and formats a value according to this tracker's type
	/// (i.e. "52", "500 meters", "10 minutes").
	/// </summary>
	/// <returns>The localized and formatted value for this tracker's type.</returns>
	/// <param name="_value">Value to be formatted.</param>
	override public string FormatValue(float _value) {
		// Format value as time
		return TimeUtils.FormatTime(_value, TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3, TimeUtils.EPrecision.DAYS);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnGameUpdated() {
		// We'll receive this event only while the game is actually running, so no need to check anythin
		currentValue += Time.deltaTime;
	}
}