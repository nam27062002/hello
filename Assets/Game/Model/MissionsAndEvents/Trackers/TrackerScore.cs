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
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerScore() {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerScore() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
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
		// Round to multiples of 1000
		_targetValue = MathUtils.Snap(_targetValue, 1000f);
		return base.RoundTargetValue(_targetValue);	// Apply default filter
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
			currentValue += _reward.score;
		}
	}
}