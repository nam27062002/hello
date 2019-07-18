// TrackerBase.cs
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
/// Tracker for score.
/// </summary>
public class TrackerScore : TrackerBase {
	private float m_score;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerScore() {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerScore() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);

		m_score = 0f;

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
		// Round to multiples of 1000
		_targetValue = MathUtils.Snap(_targetValue, 1000);
		return base.RoundTargetValue(_targetValue);	// Apply default filter
	}

	/// <summary>
	/// Sets the initial value for the tracker.
	/// Doesn't perform any check or trigger any event.
	/// Use for initialization/reset/restore persistence.
	/// Use also by heirs to reset any custom vars that needed to be reset.
	/// </summary>
	/// <param name="_initialValue">Initial value.</param>
	override public void InitValue(long _initialValue) {
		// Call parent
		base.InitValue(_initialValue);

		// Reset local vars
		m_score = (float)_initialValue;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A reward has been applied.
	/// </summary>
	/// <param name="_reward">The reward.</param>
	/// <param name="_entity">The source entity, optional.</param>
	private void OnRewardApplied(Reward _reward, Transform _entity) {
		// We only care about score rewards
		if(_reward.score > 0) {
			m_score += _reward.score;
			currentValue = Mathf.FloorToInt(m_score);
		}
	}
}