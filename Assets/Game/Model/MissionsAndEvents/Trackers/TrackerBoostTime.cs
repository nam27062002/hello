// TrackerBoostTime.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 25/10/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for boosting time.
/// </summary>
public class TrackerBoostTime : TrackerBase {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	// Internal
	private bool m_boosting = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerBoostTime() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);
		Messenger.AddListener<bool>(MessengerEvents.BOOST_TOGGLED, OnBoostToggled);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBoostTime() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);
		Messenger.RemoveListener<bool>(MessengerEvents.BOOST_TOGGLED, OnBoostToggled);

		// Call parent
		base.Clear();
	}

	/// <summary>
	/// Round a value according to specific rules defined for every tracker type.
	/// Typically used for target values.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="_targetValue">The original value to be rounded.</param>
	override public float RoundTargetValue(float _targetValue) {
		return _targetValue;
	}

	/// <summary>
	/// Gets the progress string, custom formatted based on tracker type.
	/// </summary>
	/// <returns>The progress string properly formatted.</returns>
	/// <param name="_currentValue">Current value to be evaluated.</param>
	/// <param name="_targetValue">Target value to be evaulated.</param>
	/// <param name="_showTarget">Show target value? (i.e. "25/40"). Some types might override this setting if not appliable.</param>
	public override string GetProgressString(float _currentValue, float _targetValue, bool _showTarget = true) {
		//Time trackers will show a percentage as a progress string
		float p = (_currentValue * 100) / _targetValue;
		return StringUtils.FormatNumber(p, 2, 0, false) + "%";
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	private void OnGameStarted() {
		// Reset flag
		m_boosting = false;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnGameUpdated() {
		// We'll receive this event only while the game is actually running, so no need to check anything
		// Is the dragon underwater?
		if(m_boosting) {
			currentValue += Time.deltaTime;
		}
	}

	/// <summary>
	/// The dragon has entered/exit water.
	/// </summary>
	/// <param name="_activated">Whether the dragon has entered or exited the water.</param>
	private void OnBoostToggled(bool _activated) {
		// Update internal flag
		m_boosting = _activated;
	}
}