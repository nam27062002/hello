// EggManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global manager of eggs.
/// Has its own asset in the Resources/Singletons folder with all the required parameters.
/// Slot 0 of the incubator is considered the active egg, which can be incubated, opened, etc.
/// The other slots are just for storage. Inventory should always be filled from 0 to N, and 
/// eggs are automatically shifted when the egg in slot 0 is collected.
/// </summary>
public class EggManager : UbiBCN.SingletonMonoBehaviour<EggManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int INVENTORY_SIZE = 3;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Settings
	// Serialized just for debugging, it's initialized from content
	[SerializeField] private ProbabilitySet m_rewardDropRate = new ProbabilitySet();
	[SerializeField] private List<int> m_goldenEggRequiredFragments = new List<int>();

	// Inventory
	public static Egg[] inventory {
		get { 
			if ( instance.m_user != null )
				return instance.m_user.eggsInventory; 
			return null;
		}
	}

	public static bool isInventoryEmpty {
		get {
			for(int i = 0; i < INVENTORY_SIZE; i++) {
				if(inventory[i] != null) return false;	// Filled slot, inventory not empty
			}
			return true;
		}
	}

	public static bool isInventoryFull {
		get {
			for(int i = 0; i < INVENTORY_SIZE; i++) {
				if(inventory[i] == null) return false;	// Empty slot, inventory not full
			}
			return true;
		}
	}

	// Incubator
	public static Egg incubatingEgg {
		get {
			return inventory[0];	// Always first slot
		}
	}

	// Golden egg managements
	public static int goldenEggRequiredFragments {
		get { 
			// If all eggs are collected, return -1
			if(instance.m_user.goldenEggsCollected < instance.m_goldenEggRequiredFragments.Count) {
				return instance.m_goldenEggRequiredFragments[instance.m_user.goldenEggsCollected]; 
			}
			return -1;
		}
	}

	public static int goldenEggFragments {
		get { return (int)instance.m_user.goldenEggFragments; }
	}

	public static bool goldenEggCompleted {
		get { return !allGoldenEggsCollected && goldenEggFragments >= goldenEggRequiredFragments; }
	}

	public static bool allGoldenEggsCollected {
		get { return instance.m_user.goldenEggsCollected >= instance.m_goldenEggRequiredFragments.Count; }
	}

	// Internal
	UserProfile m_user;

	// Dynamic Probability coeficients
	private List<float> m_probabilities;
	private float m_globalGatchaAdjustmentCoef;
	private float m_formulaCoef1;
	private float m_formulaCoef2;


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		
	}

	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public static void InitFromDefinitions() {
		// Check requirements
		Debug.Assert(ContentManager.ready, "Definitions Manager must be ready before invoking this method.");

		// Initialize reward drop rate table based on definitions
		// Perform a double loop since every time we add a new element the probabilities 
		// are readjusted - therefore we need to first add all the elements and then 
		// define the probabilities for each one
		instance.m_rewardDropRate = new ProbabilitySet();	
		instance.m_probabilities = new List<float>();
		List<DefinitionNode> rewardDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.EGG_REWARDS);
		for(int i = 0; i < rewardDefs.Count; i++) {
			instance.m_rewardDropRate.AddElement(rewardDefs[i].sku);
			instance.m_probabilities.Add(rewardDefs[i].GetAsFloat("droprate"));
		}
		instance.BuildDynamicProbabilities();

		// Restore saved random state from preferences so the distribution is respected
		// Only if we have a state saved!
		if(PlayerPrefs.HasKey("EggManager.RandomState.s0")) {
			// [AOC] GOING TO HELL!! Random.State is a private struct that can't be easily serialized, so use reflection to do so.
			// We know the internal structure of Random.State thanks to unofficial UnityDecompiled repo (https://github.com/MattRix/UnityDecompiled/blob/master/UnityEngine/UnityEngine/Random.cs)
			UnityEngine.Random.State s = instance.m_rewardDropRate.randomState;
			for(int i = 0; i < 4; ++i) {
				FieldInfo prop = s.GetType().GetField("s" + i, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				prop.SetValue(s, PlayerPrefs.GetInt("EggManager.RandomState.s" + i));
				//Debug.Log("<color=magenta>Restoring RandomState.s" + i + ": " + ((int)prop.GetValue(s)).ToString() + "</color>");
			}
			instance.m_rewardDropRate.randomState = s;
		}

		// Initialize required golden egg fragments requirements
		instance.m_goldenEggRequiredFragments.Clear();
		List<DefinitionNode> goldenEggDefinitions = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.GOLDEN_EGGS);
		DefinitionsManager.SharedInstance.SortByProperty(ref goldenEggDefinitions, "order", DefinitionsManager.SortType.NUMERIC);
		for(int i = 0; i < goldenEggDefinitions.Count; i++) {
			instance.m_goldenEggRequiredFragments.Add(goldenEggDefinitions[i].GetAsInt("fragmentsRequired"));
		}
	}

	/// <summary>
	/// Object enabled.
	/// </summary>
	public void OnEnable() {
		// Subscribe to external events
	}

	/// <summary>
	/// Object disabled.
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
	}

	/// <summary>
	/// Monobehaviour update implementation.
	/// </summary>
	public void Update() {
		// Must be initialized
		if(!IsReady()) return;

		// Check for incubator deadline
		if(incubatingEgg != null && incubatingEgg.isIncubating) {
			if(GameServerManager.SharedInstance.GetEstimatedServerTime() >= incubatingEgg.incubationEndTimestamp) {
				// Incubation done!
				incubatingEgg.ChangeState(Egg.State.READY);
			}
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//

	public void BuildDynamicProbabilities() {
		for(int i = 0; i < m_probabilities.Count; i++) {
			// Dynamic probability
			float a = m_globalGatchaAdjustmentCoef - (m_formulaCoef1 * Mathf.Pow(m_user.openEggTriesWithoutRares, m_formulaCoef2));
			float weigth = m_probabilities[i];
			float dp = 100f / Mathf.Pow(weigth, a);

			m_rewardDropRate.SetProbability(i, dp);
		}
	}

	/// <summary>
	/// Add a new egg to the first empty slot in the inventory. 
	/// If the inventory is full, egg won't be added.
	/// </summary>
	/// <returns>The inventory position where the new egg was added. <c>-1</c> if the egg couldn't be added (inventory full).</returns>
	/// <param name="_newEgg">The egg to be added to the inventory.</param>
	public static int AddEggToInventory(Egg _newEgg) {
		for(int i = 0; i < INVENTORY_SIZE; i++) {
			// Is the slot empty?
			if(instance.m_user.eggsInventory[i] == null) {
				// Yes!! Store egg
				instance.m_user.eggsInventory[i] = _newEgg;

				// Set initial state - if the first empty slot is the incubating slot, mark it as "ready for incubation"
				if(i == 0) {
					_newEgg.ChangeState(Egg.State.READY_FOR_INCUBATION);
				} else {
					_newEgg.ChangeState(Egg.State.STORED);
				}

				// Return slot index
				return i;
			}
		}

		// No empty slots were found, egg couldn't be added
		return -1;
	}

	/// <summary>
	/// Removes an egg from the the inventory, shifting all the eggs in the following slots one slot to the left.
	/// Doesn't change the state of the egg.
	/// </summary>
	/// <returns>The inventory slot from where the egg was removed. <c>-1</c> if the egg wasn't found in the inventory.</returns>
	/// <param name="_egg">The egg to be added to the inventory.</param>
	public static int RemoveEggFromInventory(Egg _egg) {
		// Ignore if egg is not valid
		if(_egg == null) return -1;

		// Find the slot the egg is in
		for(int i = 0; i < INVENTORY_SIZE; i++) {
			// Is this the slot?
			if(inventory[i] == _egg) {
				// Yes!! Remove the egg
				inventory[i] = null;

				// Shift the rest of slots to the left
				for(int j = i + 1; j < INVENTORY_SIZE; j++) {
					// Move to the left and clear slot j
					inventory[j-1] = inventory[j];
					inventory[j] = null;
				}

				// If an egg has been moved to the incubator slot, make sure it's ready to incubate
				if(inventory[0] != null && inventory[0].state == Egg.State.STORED) {
					inventory[0].ChangeState(Egg.State.READY_FOR_INCUBATION);
				}

				// Return the slot the egg was in
				return i;
			}
		}

		// The egg wasn't found in the inventory
		return -1;
	}

	/// <summary>
	/// Generate a random reward respecting drop chances.
	/// </summary>
	/// <returns>The definition of the reward to be given, as defined in the EGG_REWARDS definitions category.</returns>
	public static DefinitionNode GenerateReward() {
		// Probabiliy set makes it easy for us!
		// [AOC] Are we testing?
		string rewardSku = "";
		switch(CPGachaTest.rewardChanceMode) {
			case CPGachaTest.RewardChanceMode.DEFAULT: {
				// [AOC] Force common pet during tutorial
				if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_REWARD)) {


					instance.BuildDynamicProbabilities();



					// Get a weighted element
					rewardSku = instance.m_rewardDropRate.GetWeightedRandomElement().label;

					// Save random state to preferences so the distribution is respected
					// [AOC] GOING TO HELL!! Random.State is a private struct that can't be easily serialized, so use reflection to do so.
					// We know the internal structure of Random.State thanks to unofficial UnityDecompiled repo (https://github.com/MattRix/UnityDecompiled/blob/master/UnityEngine/UnityEngine/Random.cs)
					UnityEngine.Random.State s = instance.m_rewardDropRate.randomState;
					for(int i = 0; i < 4; ++i) {
						FieldInfo prop = s.GetType().GetField("s" + i, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
						PlayerPrefs.SetInt("EggManager.RandomState.s" + i, (int)prop.GetValue(s));
						//Debug.Log("<color=magenta>Saving RandomState.s" + i + ": " + ((int)prop.GetValue(s)).ToString() + "</color>");
					}
				} else {
					rewardSku = "pet_common";
				}
			} break;

			case CPGachaTest.RewardChanceMode.COMMON_ONLY: {
				rewardSku = "pet_common";
			} break;

			case CPGachaTest.RewardChanceMode.RARE_ONLY: {
				rewardSku = "pet_rare";
			} break;

			case CPGachaTest.RewardChanceMode.EPIC_ONLY: {
				rewardSku = "pet_epic";
			} break;

			case CPGachaTest.RewardChanceMode.SAME_PROBABILITY: {
				// Exclude special pets!
				do {
					rewardSku = instance.m_rewardDropRate.GetLabel(UnityEngine.Random.Range(0, instance.m_rewardDropRate.numElements));		// Pick one random element without taking probabilities in account
				} while(rewardSku == "pet_special");
			} break;

			case CPGachaTest.RewardChanceMode.FORCED_PET_SKU: {
				// Depends on target pet rarity!
				DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, CPGachaTest.forcedPetSku);
				rewardSku = "pet_" + petDef.GetAsString("rarity");	// [AOC] Super-duper-dirty xD
			} break;
		}
		return DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, rewardSku);
	}


	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	public static bool IsReady()
	{
		return instance.m_user != null;
	}

	public static void SetupUser( UserProfile user)
	{
		instance.m_user = user;
	} 

}