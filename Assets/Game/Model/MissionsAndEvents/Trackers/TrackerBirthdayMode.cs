// TrackerKill.cs
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
/// Tracker for killed entities.
/// </summary>
public class TrackerBirthdayMode : TrackerBase {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>	
	public TrackerBirthdayMode() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.ANNIVERSARY_START_BDAY_MODE, OnBirthdayMode);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBirthdayMode() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.ANNIVERSARY_START_BDAY_MODE, OnBirthdayMode);

		// Call parent
		base.Clear();
	}

	/// <summary>
	/// Round a value according to specific rules defined for every tracker type.
	/// Typically used for target values.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="_targetValue">The original value to be rounded.</param>
	override public long RoundTargetValue(long _targetValue) {
		// Round to multiples of 10, except values smaller than 100
		if(_targetValue > 100) {
			_targetValue = MathUtils.Snap(_targetValue, 10);
		}

		// Apply default filter
		return base.RoundTargetValue(_targetValue);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//	
	private void OnBirthdayMode() {
		currentValue++;		
	}
}