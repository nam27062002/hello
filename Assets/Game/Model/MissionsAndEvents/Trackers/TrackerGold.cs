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
public class TrackerGold : TrackerBase {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerGold() {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerGold() {
		
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
		// Round it to 10 multiple
		_targetValue = MathUtils.Snap(_targetValue, 10);
		return base.RoundTargetValue(_targetValue);	// Apply default rounding as well
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
		// We only care about gold rewards
		if(_reward.coins > 0) {
			currentValue += _reward.coins;
		}
	}
}