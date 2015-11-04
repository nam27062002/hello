// ScoreManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Aux class to represent the score multipliers.
/// </summary>
[Serializable]
public class ScoreMultiplier {
	public float multiplier = 1;
	public int requiredKillStreak = 1;	// Eat, burn and destroy count
	public float duration = 20f;	// Seconds to keep the streak alive
	public List<string> feedbackMessages = new List<string>();
}

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
	// Score multiplier
	[SerializeField] private ScoreMultiplier[] m_scoreMultipliers;
	private int m_scoreMultiplierStreak = 0;	// Amount of consecutive eaten/burnt/destroyed entities without taking damage

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

	// Score multiplier
	private int m_currentScoreMultiplier = 0;
	public static ScoreMultiplier currentScoreMultiplier {
		get { return instance.m_scoreMultipliers[instance.m_currentScoreMultiplier]; }
	}
	public static ScoreMultiplier defaultScoreMultiplier {
		get { return instance.m_scoreMultipliers[0]; }
	}

	// Time to end the current killing streak
	private float m_scoreMultiplierTimer = -1;
	public static float scoreMultiplierTimer {
		get { return instance.m_scoreMultiplierTimer; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// The manager has been enabled.
	/// </summary>
	public void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnKill);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnKill);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnKill);
		Messenger.AddListener<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
	}

	/// <summary>
	/// The manager has been disabled.
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnKill);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnKill);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnKill);
		Messenger.RemoveListener<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Update score multiplier (won't be called if we're in the first multiplier)
		if(m_scoreMultiplierTimer > 0) {
			// Update timer
			m_scoreMultiplierTimer -= Time.deltaTime;
			
			// If timer has ended, end multiplier streak
			if(m_scoreMultiplierTimer <= 0) {
				SetScoreMultiplier(0);
			}
		}
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
		instance.SetScoreMultiplier(0);
	}

	/// <summary>
	/// Adds the current rewards to the user profile. To be called at the end of 
	/// the game, for example.
	/// </summary>
	public static void ApplyRewardsToProfile() {
		// Just do it :)
		UserProfile.AddCoins(instance.m_coins);
		UserProfile.AddPC(instance.m_pc);
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
		// Apply multiplier
		_reward.score = (int)(_reward.score * currentScoreMultiplier.multiplier);
		instance.m_score += _reward.score;

		// Coins
		instance.m_coins += _reward.coins;

		// PC
		instance.m_pc += _reward.pc;

		// XP
		InstanceManager.player.data.progression.AddXp(_reward.xp, true);

		// Global notification (i.e. to show feedback)
		Messenger.Broadcast<Reward, Transform>(GameEvents.REWARD_APPLIED, _reward, _entity);
	}

	/// <summary>
	/// Define the new score multiplier.
	/// </summary>
	/// <param name="_multiplierIdx">The index of the new multiplier in the SCORE_MULTIPLIERS array.</param>
	private void SetScoreMultiplier(int _multiplierIdx) {
		// Make sure given index is valid
		if(_multiplierIdx < 0 || _multiplierIdx >= m_scoreMultipliers.Length) return;
		
		// Reset everything when going to 0
		if(_multiplierIdx == 0) {
			m_scoreMultiplierTimer = -1;
			m_scoreMultiplierStreak = 0;
		}

		// Store new multiplier value
		ScoreMultiplier old = m_scoreMultipliers[m_currentScoreMultiplier];
		m_currentScoreMultiplier = _multiplierIdx;

		// Dispatch game event (only if actually changing)
		if(old != currentScoreMultiplier) {
			Messenger.Broadcast<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, old, currentScoreMultiplier);
		}
	}
	
	/// <summary>
	/// Update the score multiplier with a new kill (eat/burn/destroy).
	/// </summary>
	private void UpdateScoreMultiplier() {
		// Update current streak
		m_scoreMultiplierStreak++;
		
		// Reset timer
		m_scoreMultiplierTimer = currentScoreMultiplier.duration;
		
		// Check if we've reached next threshold
		if(m_currentScoreMultiplier < m_scoreMultipliers.Length - 1 
		&& m_scoreMultiplierStreak >= m_scoreMultipliers[m_currentScoreMultiplier + 1].requiredKillStreak) {
			// Yes!! Change current multiplier
			SetScoreMultiplier(m_currentScoreMultiplier + 1);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An entity has been eaten, burned or killed. If any of these events requires 
	/// a specific implementation, just create a custom callback for it.
	/// </summary>
	/// <param name="_entity">The entity that has been killed.</param>
	/// <param name="_reward">The reward linked to this event.</param>
	private void OnKill(Transform _entity, Reward _reward) {
		// Add the reward
		ApplyReward(_reward, _entity);

		// Update multiplier
		UpdateScoreMultiplier();
	}

	/// <summary>
	/// The player has received damage.
	/// </summary>
	/// <param name="_amount">The amount of damage received.</param>
	/// <param name="_source">The source of the damage.</param>
	private void OnDamageReceived(float _amount, Transform _source) {
		// Break current streak
		SetScoreMultiplier(0);
	}
}