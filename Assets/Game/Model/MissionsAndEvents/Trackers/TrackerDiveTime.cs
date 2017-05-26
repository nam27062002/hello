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
public class TrackerDiveTime : TrackerBase {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	// Internal
	private bool m_diving = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerDiveTime() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(GameEvents.GAME_UPDATED, OnGameUpdated);
		Messenger.AddListener<bool>(GameEvents.UNDERWATER_TOGGLED, OnUnderwaterToggled);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerDiveTime() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(GameEvents.GAME_UPDATED, OnGameUpdated);
		Messenger.RemoveListener<bool>(GameEvents.UNDERWATER_TOGGLED, OnUnderwaterToggled);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
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
	/// A new game has started.
	/// </summary>
	private void OnGameStarted() {
		// Reset flag
		m_diving = false;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnGameUpdated() {
		// We'll receive this event only while the game is actually running, so no need to check anything
		// Is the dragon underwater?
		if(m_diving) {
			currentValue += Time.deltaTime;
		}
	}

	/// <summary>
	/// The dragon has entered/exit water.
	/// </summary>
	/// <param name="_activated">Whether the dragon has entered or exited the water.</param>
	private void OnUnderwaterToggled(bool _activated) {
		// Update internal flag
		m_diving = _activated;
	}
}