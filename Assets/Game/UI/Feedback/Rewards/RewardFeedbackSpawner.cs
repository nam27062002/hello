// RewardFeedbackSpawner.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Spawner in charge of showing all the feedback related ot rewards.
/// </summary>
public class RewardFeedbackSpawner : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_scorePrefab = null;
	[SerializeField] private GameObject m_coinsPrefab = null;
	[SerializeField] private GameObject m_pcPrefab = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		DebugUtils.Assert(m_scorePrefab != null, "Required field!");
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Create the pool
		if(m_scorePrefab != null) PoolManager.CreatePool(m_scorePrefab, this.transform, 15);	// Must be created within the canvas!!
		if(m_coinsPrefab != null) PoolManager.CreatePool(m_coinsPrefab, 5);
		if(m_pcPrefab != null) PoolManager.CreatePool(m_pcPrefab, 3);
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

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

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
		// Show different feedback for different reward types
		// Score
		if(_reward.score > 0) {
			GameObject obj = PoolManager.GetInstance(m_scorePrefab.name);
			RewardScoreFeedbackController scoreFeedback = obj.GetComponent<RewardScoreFeedbackController>();
			Vector3 worldPos = Vector3.zero;
			if(_entity != null) worldPos = _entity.position;
			scoreFeedback.Spawn(_reward.score, worldPos);
		}

		// Coins
		if(_reward.coins > 0) {

		}

		// PC
		if(_reward.pc > 0) {

		}
	}
}