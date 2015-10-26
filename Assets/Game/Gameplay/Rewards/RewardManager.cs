// ScoreManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global rewards controller. Keeps current game score, coins earned, etc.
/// Singleton class, access it via its static methods.
/// </summary>
public class RewardManager : SingletonMonoBehaviour<RewardManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//


	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private long m_score = 0;
	public static long score { 
		get { return instance.m_score; }
	}

	private long m_coins = 0;
	public static long coins {
		get { return instance.m_coins; }
	}

	private long m_pc = 0;
	public static long pc {
		get { return instance.m_pc; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// The manager has been enabled.
	/// </summary>
	public void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnReward);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnReward);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnReward);
	}

	/// <summary>
	/// The manager has been disabled.
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnReward);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnReward);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnReward);
	}

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Reset the temp data. To be called, for example, when starting a new game.
	/// </summary>
	public static void Reset() {
		// Score
		instance.m_score = 0;
		instance.m_coins = 0;
		instance.m_pc = 0;

		// Multipliers
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply the given rewards package.
	/// </summary>
	/// <param name="_reward">The rewards to be applied.</param>
	/// <param name="_entity">The entity that has triggered the reward. Can be null.</param>
	private void ApplyReward(Reward _reward, Transform _entity) {
		// Score
		// [AOC] TODO!! Apply multiplier
		instance.m_score += _reward.score;

		// Coins
		instance.m_coins += _reward.coins;

		// PC
		instance.m_pc += _reward.pc;

		// Global notification (i.e. to show feedback)
		Messenger.Broadcast<Reward, Transform>(GameEvents.REWARD_APPLIED, _reward, _entity);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An event requiring a reward has been triggered. Groups the entity eaten, 
	/// burned and destroyed events. If any of them requires a specific 
	/// implementation, just create a custom callback for it.
	/// </summary>
	/// <param name="_entity">The entity that has been eaten.</param>
	/// <param name="_reward">The reward linked to this event.</param>
	private void OnReward(Transform _entity, Reward _reward) {
		ApplyReward(_reward, _entity);
	}
}