// HUDScore.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a score counter in the hud.
/// </summary>
public class HUDScore : IHUDCounter {	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//		
	private void Awake() {
		base.Awake();
		if ( SceneController.mode == SceneController.Mode.TOURNAMENT )
		{
			HDTournamentData _data = HDLiveDataManager.tournament.data as HDTournamentData;
			HDTournamentDefinition _def = _data.definition as HDTournamentDefinition;
			if ( ( _def.m_goal.m_mode == HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL && _def.m_goal.m_type == "survive_time" ) || _def.m_goal.m_type == "score")
			{
				gameObject.SetActive(true);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
	}
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
        return StringUtils.FormatNumber(GetValue());
    }

    private long GetValue(){
        return RewardManager.score;
    }

	protected override float GetMinimumAnimInterval() {
		return 0.25f;
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
		// We only care about score rewards
		if(_reward.score > 0) {
            UpdateValue(GetValue(), true);            
        }
	}
}
