// ChestManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global manager of chests.
/// Has its own asset in the Resources/Singletons folder with all the required parameters.
/// </summary>
[CreateAssetMenu]
public class ChestManager : Singleton<ChestManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum RewardType {
		COINS,
		PC,
		BOOSTER,	// [AOC] TODO!!

		COUNT
	};

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Store drop rate probabilities
	private ProbabilitySet m_rewardDropRate = new ProbabilitySet();

	// Internal vars
	private Chest m_selectedChest = null;
	public static Chest selectedChest { get { return instance.m_selectedChest; }}

	// Reward
	private RewardType m_rewardType;
	public static RewardType rewardType { get { return instance.m_rewardType; }}

	private int m_rewardAmount;
	public static int rewardAmount { get { return instance.m_rewardAmount; }}

	private string m_rewardSku;
	public static string rewardSku { get { return instance.m_rewardSku; }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public ChestManager() {
		// Initialize probability set from definitions
		m_rewardDropRate = new ProbabilitySet();
		List<DefinitionNode> rewardDefs = new List<DefinitionNode>();
		DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.CHEST_REWARDS, ref rewardDefs);
		DefinitionsManager.SharedInstance.SortByProperty(ref rewardDefs, "index", DefinitionsManager.SortType.NUMERIC);	// Make sure it matches the enum
		for(int i = 0; i < rewardDefs.Count; i++) {
			m_rewardDropRate.AddElement(rewardDefs[i].sku);
			m_rewardDropRate.SetProbability(i, rewardDefs[i].Get<float>("dropRate"), false);
		}
		m_rewardDropRate.Validate();
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Select a random chest from the current level and define it as the active chest.
	/// All other chests in the level will be removed.
	/// A level must be loaded beforehand, otherwise nothing will happen.
	/// To be called at the start of the game.
	/// </summary>
	public static void SelectChest() {
		// Get all the chests in the scene
		GameObject[] chests = GameObject.FindGameObjectsWithTag(Chest.TAG);
		if(chests.Length > 0) {
			// [AOC] Filter by dragon tier (blockers, etc.)
			List<GameObject> filteredChests = new List<GameObject>();
			DragonTier currentTier = DragonManager.IsReady() ? DragonManager.currentDragon.tier : DragonTier.TIER_0;
			for(int i = 0; i < chests.Length; i++) {
				if(chests[i].GetComponent<Chest>().requiredTier <= currentTier) {
					filteredChests.Add(chests[i]);
				}
			}

			// Grab a random one from the list and define it as active chest
			// Don't enable chest during the first run
			GameObject chestObj = null;
			if(UsersManager.currentUser.gamesPlayed > 0) { 
				chestObj = filteredChests.GetRandomValue();
				instance.m_selectedChest = chestObj.GetComponent<Chest>();
			}

			// Remove the rest of chests from the scene
			for(int i = 0; i < chests.Length; i++) {
				// Skip if selected chest
				if(chests[i] == chestObj) continue;

				// Delete from scene otherwise
				GameObject.Destroy(chests[i]);
				chests[i] = null;
			}
		}
	}

	/// <summary>
	/// Clears the active chest.
	/// To be called at the end of the game.
	/// </summary>
	public static void ClearSelectedChest() {
		// Skip if there is no active chest
		if(instance.m_selectedChest == null) return;

		// For now let's just lose the reference to it
		instance.m_selectedChest = null;
	}

	/// <summary>
	/// Generate a new chest reward using manager's setup.
	/// Reward can be accessed through the manager's properties.
	/// </summary>
	public static void GenerateReward() {
		// First of all, select reward type
		// Special case: force some specific reward for the first chest found
		if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.CHEST_REWARD)) {
			// Tutorial completed, get random reward
			instance.m_rewardType = (RewardType)instance.m_rewardDropRate.GetWeightedRandomElementIdx();
		} else {
			// Tutorial not completed: force some specific reward
			// [AOC] TODO!! Disabled for now, give a random reward
			instance.m_rewardType = (RewardType)instance.m_rewardDropRate.GetWeightedRandomElementIdx();

			// Complete tutorial
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.CHEST_REWARD);
			PersistenceManager.Save();
		}

		// Now compute amount/sku based on reward type
		switch(instance.m_rewardType) {
			case RewardType.COINS: {
				// [AOC] Formula defined in the chestsRewards table
				// A(LN(MaxDragon) +1)/B
				DefinitionNode rewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.CHEST_REWARDS, "coins");
				float A = rewardDef.Get<float>("factorA");
				float B = rewardDef.Get<float>("factorB");
				float ownedDragons = (float)DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
				float maxReward = A * (Mathf.Log(ownedDragons) + 1) / B;
				float minReward = A * (Mathf.Log(1) + 1) / B;
				float reward = Random.Range(minReward, maxReward);
				instance.m_rewardAmount = (int)MathUtils.Snap(reward, 100f);
				instance.m_rewardAmount = Mathf.Max(1, instance.m_rewardAmount);	// At least 1 coin
			} break;

			case RewardType.PC: {
				// [AOC] Formula defined in the chestsRewards table
				// A(LN(MaxDragon) +1)/B
				DefinitionNode rewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.CHEST_REWARDS, "pc");
				float A = rewardDef.Get<float>("factorA");
				float B = rewardDef.Get<float>("factorB");
				float ownedDragons = (float)DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
				float maxReward = A * (Mathf.Log(ownedDragons) + 1) / B;
				float minReward = A * (Mathf.Log(1) + 1) / B;
				float reward = Random.Range(minReward, maxReward);
				instance.m_rewardAmount = (int)MathUtils.Snap(reward, 1f);
				instance.m_rewardAmount = Mathf.Max(1, instance.m_rewardAmount);	// At least 1 pc
			} break;

			case RewardType.BOOSTER: {
				instance.m_rewardAmount = 1;
				instance.m_rewardSku = "TODO!!";
			} break;
		}
	}

	/// <summary>
	/// Only for testing purposes, sets a custom reward and amount.
	/// </summary>
	/// <param name="_type">Reward type.</param>
	/// <param name="_amount">Reward amount.</param>
	/// <param name="_sku">Reward sku.</param>
	public static void SetReward(RewardType _type, int _amount, string _sku) {
		instance.m_rewardType = _type;
		instance.m_rewardAmount = _amount;
		instance.m_rewardSku = _sku;
	}

	/// <summary>
	/// Applies the previously generated reward to the player's profile.
	/// Persistence should be saved afterwards.
	/// </summary>
	public static void ApplyReward() {
		// [AOC] TODO!! Make sure a reward has been generated
		// [AOC] TODO!! Send game event?

		// Depends on reward type
		switch(instance.m_rewardType) {
			case RewardType.COINS: {
				UsersManager.currentUser.AddCoins(instance.m_rewardAmount);
			} break;

			case RewardType.PC: {
				UsersManager.currentUser.AddPC(instance.m_rewardAmount);
			} break;

			case RewardType.BOOSTER: {
				// [AOC] TODO!!
			} break;
		}
	}
}