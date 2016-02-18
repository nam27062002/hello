// ChestManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global manager of chests.
/// Has its own asset in the Resources/Singletons folder with all the required parameters.
/// </summary>
[CreateAssetMenu]
public class ChestManager : SingletonScriptableObject<ChestManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string TAG = "Chest";

	public enum RewardType {
		COINS,
		PC,
		BOOSTER,	// [AOC] TODO!!
		EGG,		// [AOC] TODO!!

		COUNT
	};

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// To be defined from the inspector
	[SerializeField] private ProbabilitySet m_rewardDropRate = new ProbabilitySet(
		new ProbabilitySet.Element[] {
			new ProbabilitySet.Element("Coins"),
			new ProbabilitySet.Element("PC"),
			new ProbabilitySet.Element("Booster"),
			new ProbabilitySet.Element("Egg")
		}
	);

	[Space(10)]
	[SerializeField] private float m_rewardCoinsFactorA = 140f;
	[SerializeField] private float m_rewardCoinsFactorB = 1f;

	[Space(10)]
	[SerializeField] private float m_rewardPCFactorA = 50f;
	[SerializeField] private float m_rewardPCFactorB = 100f;

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
	protected void OnEnable() {
		
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
		GameObject[] chests = GameObject.FindGameObjectsWithTag(TAG);
		if(chests.Length > 0) {
			// Grab a random one from the list
			// [AOC] CHECK!! We might need to filter by dragon tier (different heights, etc.)
			GameObject chestObj = chests.GetRandomValue();

			// Define it as active chest and remove the rest of chests from the scene
			instance.m_selectedChest = chestObj.GetComponent<Chest>();
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
		instance.m_rewardType = (RewardType)instance.m_rewardDropRate.GetWeightedRandomElementIdx();

		// Now compute amount/sku based on reward type
		switch(instance.m_rewardType) {
			case RewardType.COINS: {
				// [AOC] Formula defined in the chestsRewards table
				float ownedDragons = (float)DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
				ownedDragons = 1;
				float reward = (Mathf.Log(ownedDragons) * instance.m_rewardCoinsFactorA + instance.m_rewardCoinsFactorA * ownedDragons)/instance.m_rewardCoinsFactorB;
				instance.m_rewardAmount = (int)MathUtils.Snap(reward, 100f);
				instance.m_rewardAmount = Mathf.Max(1, instance.m_rewardAmount);	// At least 1 coin
			} break;

			case RewardType.PC: {
				// [AOC] Formula defined in the chestsRewards table
				float ownedDragons = (float)DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
				ownedDragons = 1;
				float reward = (Mathf.Log(ownedDragons) * instance.m_rewardPCFactorA + instance.m_rewardPCFactorA * ownedDragons)/instance.m_rewardPCFactorB;
				instance.m_rewardAmount = (int)MathUtils.Snap(reward, 1f);
				instance.m_rewardAmount = Mathf.Max(1, instance.m_rewardAmount);	// At least 1 pc
			} break;

			case RewardType.BOOSTER: {
				instance.m_rewardAmount = 1;
				instance.m_rewardSku = "TODO!!";
			} break;

			case RewardType.EGG: {
				instance.m_rewardAmount = 1;
				instance.m_rewardSku = "TODO!!";
			} break;
		}
	}

	/// <summary>
	/// Applies the previously generated reward to the player's profile.
	/// </summary>
	public static void ApplyReward() {
		// [AOC] TODO!! Make sure a reward has been generated
		// [AOC] TODO!! Send game event?

		// Depends on reward type
		switch(instance.m_rewardType) {
			case RewardType.COINS: {
				UserProfile.AddCoins(instance.m_rewardAmount);
			} break;

			case RewardType.PC: {
				UserProfile.AddPC(instance.m_rewardAmount);
			} break;

			case RewardType.BOOSTER: {
				// [AOC] TODO!!
			} break;

			case RewardType.EGG: {
				// [AOC] TODO!!
			} break;
		}
	}
}