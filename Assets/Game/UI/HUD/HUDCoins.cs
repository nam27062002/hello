﻿// HUDCoins.cs
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
public class HUDCoins : HudWidget {
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
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}    

    //------------------------------------------------------------------//
    // INTERNAL UTILS													//
    //------------------------------------------------------------------//      
    protected override string GetValueAsString() {
		return UIConstants.IconString(GetValue(), UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);
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

