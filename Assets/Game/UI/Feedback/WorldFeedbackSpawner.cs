﻿// WorldFeedbackSpawner.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/10/2015.
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
/// Spawner in charge of showing all the feedback in the world scene
/// </summary>
public class WorldFeedbackSpawner : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[Separator("Feedback Prefabs")]
	[SerializeField] private GameObject m_scoreFeedbackPrefab = null;
	[SerializeField] private GameObject m_coinsFeedbackPrefab = null;
	[SerializeField] private GameObject m_pcFeedbackPrefab = null;
	[SerializeField] private GameObject m_killFeedbackPrefab = null;
	[SerializeField] private GameObject m_flockBonusFeedbackPrefab = null;
	[SerializeField] private GameObject m_escapedFeedbackPrefab = null;

	[Separator("Container References")]
	[SerializeField] private GameObject m_scoreFeedbackContainer = null;
	[SerializeField] private GameObject m_killFeedbackContainer = null;
	[SerializeField] private GameObject m_escapeFeedbackContainer = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Create the pools
		// No more than X simultaneous messages on screen!
		// Use container if defined to keep hierarchy clean

		if(m_scoreFeedbackPrefab != null) {
			// Must be created within the canvas
			Transform parent = this.transform;
			if(m_scoreFeedbackContainer != null) {
				parent = m_scoreFeedbackContainer.transform;
			}
			PoolManager.CreatePool(m_scoreFeedbackPrefab, parent, 15);
		}

		if(m_coinsFeedbackPrefab != null) {
			PoolManager.CreatePool(m_coinsFeedbackPrefab, 5);
		}

		if(m_pcFeedbackPrefab != null) {
			PoolManager.CreatePool(m_pcFeedbackPrefab, 3);
		}

		if(m_killFeedbackPrefab != null) {
			Transform parent = this.transform;
			if(m_killFeedbackContainer != null) {
				parent = m_killFeedbackContainer.transform;
			}
			PoolManager.CreatePool(m_killFeedbackPrefab, parent, 5, false);
		}
			
		if(m_flockBonusFeedbackPrefab != null) { 
			Transform parent = this.transform;
			if(m_scoreFeedbackContainer != null) {
				parent = m_scoreFeedbackContainer.transform;
			}
			PoolManager.CreatePool(m_flockBonusFeedbackPrefab, parent, 2);
		}

		if ( m_escapedFeedbackPrefab != null )
		{
			Transform parent = this.transform;
			if(m_escapeFeedbackContainer != null) {
				parent = m_escapeFeedbackContainer.transform;
			}
			PoolManager.CreatePool(m_escapedFeedbackPrefab, parent, 2, false);
		}
	}

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnEaten);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnBurned);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnDestroyed);
		Messenger.AddListener<Transform, Reward>(GameEvents.FLOCK_EATEN, OnFlockEaten);
		Messenger.AddListener<Transform>(GameEvents.ENTITY_ESCAPED, OnEscaped);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnEaten);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnBurned);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnDestroyed);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.FLOCK_EATEN, OnFlockEaten);
		Messenger.RemoveListener<Transform>(GameEvents.ENTITY_ESCAPED, OnEscaped);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------//
	// INTERNAL															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Spawn a kill message feedback with the given text and entity.
	/// </summary>
	/// <param name="_type">The type of feedback to be displayed.</param>
	/// <param name="_entity">The source of the kill.</param>
	private void SpawnKillFeedback(FeedbackData.Type _type, Transform _entity) {
		// Some checks first
		if(_entity == null) return;

		// Get the feedback data from the source entity
		Entity_Old entity = _entity.GetComponent<Entity_Old>();
		if(entity == null) return;

		// Check that there's actually some text to be spawned
		string text = entity.feedbackData.GetFeedback(_type);
		if(string.IsNullOrEmpty(text)) return;

		// Get an instance from the pool and spawn it!
		GameObject obj = PoolManager.GetInstance(m_killFeedbackPrefab.name);
		if (obj != null) {
			WorldFeedbackController worldFeedback = obj.GetComponent<WorldFeedbackController>();
			worldFeedback.Spawn(text, _entity.position);
		}
	}

	private void SpawnEscapedFeedback( Transform _entity)
	{
		string tid = "TID_FEEDBACK_ESCAPED";
		switch( UnityEngine.Random.Range(0,3))
		{
			case 0: tid = "TID_FEEDBACK_ESCAPED";break;
			case 1: tid = "TID_FEEDBACK_SO_CLOSE";break;
			case 2: tid = "TID_FEEDBACK_GOT_AWAY";break;
		}

		// Get an instance from the pool and spawn it!
		GameObject obj = PoolManager.GetInstance(m_escapedFeedbackPrefab.name);
		if (obj != null) 
		{
			WorldFeedbackController worldFeedback = obj.GetComponent<WorldFeedbackController>();
            worldFeedback.Spawn( LocalizationManager.SharedInstance.Get( tid ) , _entity.position);
		}
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
		// Find out spawn position
		Vector3 worldPos = Vector3.zero;
		if(_entity != null) worldPos = _entity.position;

		// Show different feedback for different reward types
		// Score
		if(_reward.score > 0) {
			// Get a score feedback instance and initialize it with the target reward
			if(m_scoreFeedbackPrefab != null) {
				// Get an instance from the pool
				GameObject obj = PoolManager.GetInstance(m_scoreFeedbackPrefab.name);

				// Initialize score component
				ScoreFeedback scoreFeedback = obj.GetComponent<ScoreFeedback>();
				scoreFeedback.SetScore(_reward.score);

				// Spawn
				WorldFeedbackController controller = obj.GetComponent<WorldFeedbackController>();
				controller.Spawn(worldPos);
			}
		}

		// Coins
		if(m_coinsFeedbackPrefab != null && _reward.coins > 0) {
			GameObject coinsParticle = PoolManager.GetInstance(m_coinsFeedbackPrefab.name);

			if (coinsParticle != null) {
				CoinsFeedbackController coins = coinsParticle.GetComponent<CoinsFeedbackController>();
				coins.Launch(worldPos, _reward.coins);
			}
		}

		// PC
		if(m_pcFeedbackPrefab != null && _reward.pc > 0) {
			// [AOC] TODO!!
		}
	}

	/// <summary>
	/// An entity has been eaten.
	/// </summary>
	/// <param name="_entity">The eaten entity.</param>
	/// <param name="_reward">The reward given. Won't be used.</param>
	private void OnEaten(Transform _entity, Reward _reward) {
		SpawnKillFeedback(FeedbackData.Type.EAT, _entity);
	}

	/// <summary>
	/// An entity has been burned.
	/// </summary>
	/// <param name="_entity">The burned entity.</param>
	/// <param name="_reward">The reward given. Won't be used.</param>
	private void OnBurned(Transform _entity, Reward _reward) {
		SpawnKillFeedback(FeedbackData.Type.BURN, _entity);
	}

	/// <summary>
	/// An entity has been destroyed.
	/// </summary>
	/// <param name="_entity">The destroyed entity.</param>
	/// <param name="_reward">The reward given. Won't be used.</param>
	private void OnDestroyed(Transform _entity, Reward _reward) {
		SpawnKillFeedback(FeedbackData.Type.DESTROY, _entity);
	}
		
	/// <summary>
	/// A full flock has been eaten.
	/// </summary>
	/// <param name="_entity">Entity.</param>
	/// <param name="_reawrd">Reawrd.</param>
	private void OnFlockEaten(Transform _entity, Reward _reward) {		
		// Spawn flock feedback bonus, score will be displayed as any other score feedback
		GameObject flockBonus = PoolManager.GetInstance(m_flockBonusFeedbackPrefab.name);
		if (flockBonus != null) {
            string text = LocalizationManager.SharedInstance.Localize("TID_FEEDBACK_FLOCK_BONUS");
			WorldFeedbackController worldFeedback = flockBonus.GetComponent<WorldFeedbackController>();
			worldFeedback.Spawn(text, _entity.position);
		}
	}


	private void OnEscaped(Transform _entity)
	{
		SpawnEscapedFeedback(_entity);
	}
}