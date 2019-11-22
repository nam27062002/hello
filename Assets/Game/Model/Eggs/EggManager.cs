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
public class EggManager : Singleton<EggManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int INVENTORY_SIZE = 3;

	private const string PET_COMMON 	= "pet_common";
	private const string PET_RARE 		= "pet_rare";
	private const string PET_EPIC 		= "pet_epic";
	private const string PET_SPECIAL 	= "pet_special";

	private const string RANDOM_STATE_PREFS_KEY = "EggManager.RandomState";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Settings	
	private ProbabilitySet m_rewardDropRate = new ProbabilitySet();
	

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


	private static float[] sm_weightIDs = {1f, 2f, 3f};
	public static void SetWeightIDs(float[] _weightIDs) {
		sm_weightIDs = _weightIDs;
	}

	public static void RestoreWeightIDs() {
		sm_weightIDs = new float[] {1f, 2f, 3f};
	}

	// Internal
	UserProfile m_user;

	// Dynamic Probability coeficients
	private List<float> m_weights;

	// Store default probabilities to display them to the user
	private List<float> m_defaultProbabilities = null;

	private bool  m_dynamicGatchaEnabled = true;
	private float m_coeficientG = 4f;
	private float m_coeficientX = 0.01f;
	private float m_coeficientY = 1.4f;


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public static void InitFromDefinitions() {
		instance.__InitFromDefinitions();
	}

	private void __InitFromDefinitions() {
		// Check requirements
		Debug.Assert(ContentManager.ready, "Definitions Manager must be ready before invoking this method.");

		// Initialize reward drop rate table based on the dynamic gatcha formula
		// We'll retrieve the coeficients of that formula from content and we will
		// recalculate the probabilities on each draw based on the luck of the player.
		m_rewardDropRate = new ProbabilitySet();	
		m_weights = new List<float>();
		m_defaultProbabilities = null;	// Force a recalculation of the default probabilities

		DefinitionNode dynamicGatchaDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DYNAMIC_GATCHA, "dynamicGatcha");
		m_dynamicGatchaEnabled = dynamicGatchaDef.GetAsBool("activated");
		m_coeficientG = dynamicGatchaDef.GetAsFloat("coefficientG");
		m_coeficientX = dynamicGatchaDef.GetAsFloat("coefficientX");
		m_coeficientY = dynamicGatchaDef.GetAsFloat("coefficientY");

		m_rewardDropRate.AddElement(PET_COMMON);
		m_weights.Add(0);
		m_rewardDropRate.AddElement(PET_RARE);
		m_weights.Add(0);
		m_rewardDropRate.AddElement(PET_EPIC);
		m_weights.Add(0);

		__BuildDynamicProbabilities();


		// Restore saved random state from preferences so the distribution is respected
		// Only if we have a state saved!
		if(PlayerPrefs.HasKey(RANDOM_STATE_PREFS_KEY)) {
			UnityEngine.Random.State s = instance.m_rewardDropRate.randomState;
			instance.m_rewardDropRate.randomState = s.Deserialize(PlayerPrefs.GetString(RANDOM_STATE_PREFS_KEY));
		}
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
	/// <summary>
	/// Gets the default probability of getting a specific reward rarity (ignoring
	/// the dynamic algorithm).
	/// </summary>
	/// <returns>The probability. <c>0</c> if given rarity is not in the gacha system.</returns>
	/// <param name="_rarity">Reward rarity.</param>
	public static float GetDefaultProbability(Metagame.Reward.Rarity _rarity) {
		// Make sure they're initialized
		if(instance.m_defaultProbabilities == null) return 0f;

		// Matching initialization order (see InitFromDefintions)
		switch(_rarity) {
			case Metagame.Reward.Rarity.COMMON:	return instance.m_defaultProbabilities[0];
			case Metagame.Reward.Rarity.RARE:	return instance.m_defaultProbabilities[1];
			case Metagame.Reward.Rarity.EPIC:	return instance.m_defaultProbabilities[2];
		}
		return 0f;	// Unsupported rarity, 0 chance of getting it
	}

	/// <summary>
	/// Gets the probability of getting a specific reward rarity using the dynamic
	/// algorithm.
	/// </summary>
	/// <returns>The probability. <c>0</c> if given rarity is not in the gacha system.</returns>
	/// <param name="_rarity">Reward rarity.</param>
	public static float GetDynamicProbability(Metagame.Reward.Rarity _rarity) {
		string rewardId = "";
		switch(_rarity) {
			case Metagame.Reward.Rarity.COMMON:	rewardId = PET_COMMON;	break;
			case Metagame.Reward.Rarity.RARE:	rewardId = PET_RARE;	break;
			case Metagame.Reward.Rarity.EPIC:	rewardId = PET_EPIC;	break;
			default: return 0f;	// Unsupported rarity, 0 chance of getting it
		}

		return instance.m_rewardDropRate.GetProbability(rewardId);
	}

	public static void BuildDynamicProbabilities() {
		instance.__BuildDynamicProbabilities();
	}

	private void __BuildDynamicProbabilities() {
		// If default probabilites are not initialized, do it now!
		bool computeDefaultProbabilites = false;
		float defaultProbabilitiesTotalWeight = 0f;
		if(m_defaultProbabilities == null) {
			computeDefaultProbabilites = true;
			m_defaultProbabilities = new List<float>();
		}

		float weight = 0f;
		float weightTotal = 0f;
		for(int i = 0; i < m_weights.Count; i++) {
			// Dynamic probability
			int triesWithoutRares = 0;
			if (m_dynamicGatchaEnabled && m_user != null) {
				triesWithoutRares = m_user.openEggTriesWithoutRares;
			}

			float a = m_coeficientG - (m_coeficientX * Mathf.Pow(triesWithoutRares, m_coeficientY));
			if (sm_weightIDs[i] > 0) {
				weight = 1f / Mathf.Pow(sm_weightIDs[i], a); // i+1 is the weightID of the formula
			} else {
				weight = 0f;
			}
			m_weights[i] = weight;
			weightTotal += weight;

			// Need to compute default probabilities as well?
			if(computeDefaultProbabilites) {
				// Same formula with 0 tries without rares
				weight = 1f / Mathf.Pow(i + 1, m_coeficientG);	// [AOC] Optimization: Since triesWithoutRares is 0, a == coeficientG.
				m_defaultProbabilities.Add(weight);
				defaultProbabilitiesTotalWeight += weight;
			}
		}

		// Normalize dynamic probabilities
		for(int i = 0; i < m_weights.Count; i++) {
			m_rewardDropRate.SetProbability(i, m_weights[i] / weightTotal, false);
		}

		// Normalize default probabilities
		if(computeDefaultProbabilites) {
			for(int i = 0; i < m_defaultProbabilities.Count; ++i) {
				m_defaultProbabilities[i] = m_defaultProbabilities[i] / defaultProbabilitiesTotalWeight;
			}
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

	public static DefinitionNode GenerateReward(float[] _customWeights) {
		float[] save = sm_weightIDs;
		SetWeightIDs(_customWeights);
		DefinitionNode def = GenerateReward();
		SetWeightIDs(save);
		BuildDynamicProbabilities();
		return def;
	}

	/// <summary>
	/// Given a set of custom weights, compute that set.
	/// </summary>
	/// <returns>The probabilities.</returns>
	/// <param name="_customWeights">Custom set of weights by rarity.</param>
	public static float[] ComputeProbabilities(float[] _customWeights) {
		// Backup weights and set new ones
		float[] save = sm_weightIDs;
		SetWeightIDs(_customWeights);
		BuildDynamicProbabilities();

		// Get the probability for each rarity
		float[] probs = new float[(int)Metagame.Reward.Rarity.COUNT];
		for(int i = 0; i < (int)Metagame.Reward.Rarity.COUNT; ++i) {
			probs[i] = GetDynamicProbability((Metagame.Reward.Rarity)i);
		}

		// Restore previous weights
		SetWeightIDs(save);
		BuildDynamicProbabilities();

		return probs;
	}

	/// <summary>
	/// Given an egg definition, compute the default probabilities (before applying 
	/// the adjustment algorithm) for that egg type.
	/// </summary>
	/// <returns>The probabilities.</returns>
	/// <param name="_eggDef">Egg definition.</param>
	public static float[] ComputeProbabilities(DefinitionNode _eggDef) {
		// Check params
		Debug.Assert(_eggDef != null, "Invalid egg definition!");

		// Create an array with the probabilities for each rarity
		float[] probabilities = new float[(int)Metagame.Reward.Rarity.COUNT];
		if(_eggDef.Has("weightCommon")) {
			// Custom probabilities
			float[] weights = new float[(int)Metagame.Reward.Rarity.COUNT];
			weights[0] = _eggDef.GetAsFloat("weightCommon", 1);
			weights[1] = _eggDef.GetAsFloat("weightRare", 2);
			weights[2] = _eggDef.GetAsFloat("weightEpic", 3);
			probabilities = EggManager.ComputeProbabilities(weights);
		} else {
			// Default probabilities
			for(int i = 0; i < (int)Metagame.Reward.Rarity.COUNT; ++i) {
				probabilities[i] = EggManager.GetDefaultProbability((Metagame.Reward.Rarity)i);
			}
		}

		// Done!
		return probabilities;
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
				instance.__BuildDynamicProbabilities();

				// Get a weighted element
				rewardSku = instance.m_rewardDropRate.GetWeightedRandomElement().label;

				if (rewardSku.Equals(PET_COMMON)) {
					instance.m_user.openEggTriesWithoutRares++;
				} else {
					instance.m_user.openEggTriesWithoutRares = 0;
				}

				// Save random state to preferences so the distribution is respected
				PlayerPrefs.SetString(RANDOM_STATE_PREFS_KEY, instance.m_rewardDropRate.randomState.Serialize());

			} break;

			case CPGachaTest.RewardChanceMode.COMMON_ONLY:	rewardSku = PET_COMMON; break;
			case CPGachaTest.RewardChanceMode.RARE_ONLY:  	rewardSku = PET_RARE;   break;
			case CPGachaTest.RewardChanceMode.EPIC_ONLY: 	rewardSku = PET_EPIC;   break;

			case CPGachaTest.RewardChanceMode.SAME_PROBABILITY: {
				// Exclude special pets!
				do {
					rewardSku = instance.m_rewardDropRate.GetLabel(UnityEngine.Random.Range(0, instance.m_rewardDropRate.numElements));		// Pick one random element without taking probabilities in account
				} while(rewardSku == PET_SPECIAL);
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
	public static bool IsReady() {
		return instance.m_user != null;
	}

	public static void SetupUser(UserProfile user) {
		instance.m_user = user;
	}
}