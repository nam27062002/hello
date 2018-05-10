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
public class TrackerSurviveTime : TrackerBaseTime {
	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Round a value according to specific rules defined for every tracker type.
	/// Typically used for target values.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="_targetValue">The original value to be rounded.</param>
	public override long RoundTargetValue(long _targetValue) {
		// Time value, round it to 10s multiple
		_targetValue = MathUtils.Snap(_targetValue, 10);
		return base.RoundTargetValue(_targetValue);	// Apply default rounding as well
	}

	protected override void OnGameStarted() {
		base.OnGameStarted();
		m_updateTime = true;
	}
}