// GameEntity.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Base data for any game entity: heli, cars, people, birds, fish...
/// </summary>
public class GameEntity : MonoBehaviour {
	#region EXPOSED MEMBERS ---------------------------------------------------------------------------------------------
	public string typeID;

	[Header("Base Stats")]
	public float health = 1;

	[Header("Base Rewards")]
	[SerializeField] private int rewardScore = 0;	// Private, use GetScoreReward() method to compute it
	[SerializeField] private int rewardCoins = 0;
	[Range(0, 1)] public float rewardCoinsProbability = 0.5f;
	public int rewardPC = 0;
	public float rewardHealth = 0;
	public float rewardEnergy = 0;
	[SerializeField] private float rewardFury = 1;	// Use 0 to skip fury reward for this entity.

	[Header("Reward Multipliers")]
	public float scoreBurnedMultiplier = 2f;
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	// Shortcut references to some interesting components
	private EdibleBehaviour mEdibleBehaviour = null;
	private FlamableBehaviour mFlamableBehaviour = null;
	private float mHealth = 1;
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Initialize component shortcuts
		mEdibleBehaviour = gameObject.GetComponent<EdibleBehaviour>();
		mFlamableBehaviour = gameObject.GetComponent<FlamableBehaviour>();
		mHealth = health;
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update() {
		// Nothing to do for now
	}

	/// <summary>
	/// Called right before destroying the object.
	/// </summary>
	void OnDestroy() {
		// Nothing for now
	}
	#endregion

	#region PUBLIC UTILS ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Compute the score given when the entity is eaten/destroyed, taking in account 
	/// current game state.
	/// </summary>
	/// <returns>The score for eating/destroying this entity with the current game state.</returns>
	public long GetScoreReward() {
		// Extra bonus if we're burning :D
		long iScore = rewardScore;
		if(mFlamableBehaviour != null && mFlamableBehaviour.hasBurned) {
			iScore = (long)(iScore * scoreBurnedMultiplier);
		}

		// Apply streak multiplier
		iScore = (long)(iScore * App.Instance.gameLogic.scoreMultiplier);

		// Done! ^_^
		return iScore;
	}

	/// <summary>
	/// Compute the coins reward taking in account the current game state.
	/// </summary>
	/// <returns>The coins rewarded for eating/destroying this entity with the current game state.</returns>
	public long GetCoinsReward() {
		// Always give reward if burning
		if(mFlamableBehaviour != null && mFlamableBehaviour.hasBurned) {
			return rewardCoins;
		}

		// Otherwise just use probability
		else if(UnityEngine.Random.Range(0f, 1f) < rewardCoinsProbability) {
			return rewardCoins;
		}
		return 0;
	}

	/// <summary>
	/// Compute the fury given when the entity is eaten/destroyed, taking in account 
	/// current game state.
	/// </summary>
	/// <returns>The fury for eating/destroying this entity with the current game state.</returns>
	public float GetFuryReward() {
		// Keep it simple for now
		return rewardFury;
	}

	public void RestoreHealth(){

		health = mHealth;
	}
	#endregion
}
#endregion