// HUDCoins.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller to update a textfield with the current amount of coins of the player.
/// </summary>
public class HUDCoins : IHUDCounter {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//		
	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	protected override void OnEnable() {
		// Call parent
		base.OnEnable();

		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	protected override void OnDisable() {
		// Call parent
		base.OnDisable();

		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
	}    

    //------------------------------------------------------------------//
    // INTERNAL UTILS													//
    //------------------------------------------------------------------//      
    protected override string GetValueAsString() {
		return UIConstants.GetIconString(GetValue(), UIConstants.IconType.COINS, UIConstants.IconAlignment.RIGHT);
    }

    private long GetValue() {
        return RewardManager.coins;
    }

    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
    /// <summary>
    /// A reward has been applied, show feedback for it.
    /// </summary>
    /// <param name="_reward">The reward that has been applied.</param>
    /// <param name="_entity">The entity that triggered the reward. Can be null.</param>
    private void OnRewardApplied(Reward _reward, Transform _entity) {
		// We only care about coin rewards
		if(_reward.coins > 0) {
            UpdateValue(GetValue(), true);            
		}
	}
}

