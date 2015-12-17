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

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller to update a textfield with the current amount of coins of the player.
/// </summary>
public class HUDCoins : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private Text m_text = null;
	[SerializeField] private Animator m_anim = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required references
		DebugUtils.Assert(m_text != null, "Required component!");
		m_text.text = "";
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		UpdateText();
	}
	
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
	/// <summary>
	/// Updates the displayed coins.
	/// </summary>
	private void UpdateText() {
		// Do it!
		m_text.text = StringUtils.FormatNumber(RewardManager.coins);
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
			UpdateText();
			if(m_anim != null) m_anim.SetTrigger("start");
		}
	}
}

