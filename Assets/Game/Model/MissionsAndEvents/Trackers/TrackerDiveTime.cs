// TrackerDiveTime.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for diving time.
/// </summary>
public class TrackerDiveTime : TrackerBaseTime {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerDiveTime() {
		// Subscribe to external events
		Messenger.AddListener<bool>(MessengerEvents.UNDERWATER_TOGGLED, OnUnderwaterToggled);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerDiveTime() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool>(MessengerEvents.UNDERWATER_TOGGLED, OnUnderwaterToggled);

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
		// Time value, round it to 10s multiple
		_targetValue = MathUtils.Snap(_targetValue, 10);
		return base.RoundTargetValue(_targetValue);	// Apply default rounding as well
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The dragon has entered/exit water.
	/// </summary>
	/// <param name="_activated">Whether the dragon has entered or exited the water.</param>
	private void OnUnderwaterToggled(bool _activated) {
		// Update internal flag
		m_updateTime = _activated;
	}
}