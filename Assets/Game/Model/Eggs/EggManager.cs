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
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global manager of eggs.
/// Has its own asset in the Resources/Singletons folder with all the required parameters.
/// </summary>
public class EggManager : SingletonMonoBehaviour<EggManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int INVENTORY_SIZE = 3;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Settings
	[SerializeField] private ProbabilitySet m_rewardDropRate = new ProbabilitySet();

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
			for(int i = 0; i < INVENTORY_SIZE; i++) {
				if(inventory[i] != null && inventory[i].isIncubating) return inventory[i];
			}
			return null;
		}
	}

	// In-game collectible egg
	// Should be initialized at the start of the game by calling the SelectCollectibleEgg() method.
	private CollectibleEgg m_collectibleEgg = null;
	public static CollectibleEgg collectibleEgg { get { return instance.m_collectibleEgg; }}

	// Internal
	UserProfile m_user;

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
		List<DefinitionNode> rewardDefs = new List<DefinitionNode>();
		DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.EGG_REWARDS, ref rewardDefs);
		for(int i = 0; i < rewardDefs.Count; i++) {
			instance.m_rewardDropRate.AddElement(rewardDefs[i].sku);
		}
		for(int i = 0; i < rewardDefs.Count; i++) {
			instance.m_rewardDropRate.SetProbability(i, rewardDefs[i].GetAsFloat("droprate"));
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
		for(int i = 0; i < INVENTORY_SIZE; i++) {
			if(inventory[i] != null && inventory[i].isIncubating) {
				if(DateTime.UtcNow >= inventory[i].incubationEndTimestamp) {
					// Incubation done!
					inventory[i].ChangeState(Egg.State.READY);
				}
			}
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
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
				_newEgg.ChangeState(Egg.State.STORED);

				// Return slot idnex
				return i;
			}
		}

		// No empty slots were found, egg couldn't be added
		return -1;
	}

	/// <summary>
	/// Removes an egg from the the inventory, emptying the slot the egg is in.
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
				// Yes!! Remove the egg and return slot idnex
				inventory[i] = null;
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
		string rewardSku = instance.m_rewardDropRate.GetWeightedRandomElement().label;
		return DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, rewardSku);
	}

	/// <summary>
	/// Select a random egg from the current level and define it as the active egg.
	/// All other eggs in the level will be removed.
	/// A level must be loaded beforehand, otherwise nothing will happen.
	/// To be called at the start of the game.
	/// </summary>
	public static void SelectCollectibleEgg() {
		// Get all the eggs in the scene
		GameObject[] eggs = GameObject.FindGameObjectsWithTag(CollectibleEgg.TAG);
		if(eggs.Length > 0) {
			// [AOC] Filter by dragon tier (blockers, etc.)
			List<GameObject> filteredEggs = new List<GameObject>();
			DragonTier currentTier = DragonManager.IsReady() ? DragonManager.currentDragon.tier : DragonTier.TIER_0;
			for(int i = 0; i < eggs.Length; i++) {
				if(eggs[i].GetComponent<CollectibleEgg>().requiredTier <= currentTier) {
					filteredEggs.Add(eggs[i]);
				}
			}

			// Grab a random one from the list and define it as active egg
			// Don't enable egg during the first run
			GameObject eggObj = null;
			if(UsersManager.currentUser.gamesPlayed > 0) { 
				eggObj = filteredEggs.GetRandomValue();
				instance.m_collectibleEgg = eggObj.GetComponent<CollectibleEgg>();
			}

			// Remove the rest of eggs from the scene
			for(int i = 0; i < eggs.Length; i++) {
				// Skip if selected egg
				if(eggs[i] == eggObj) continue;

				// Delete from scene otherwise
				GameObject.Destroy(eggs[i]);
				eggs[i] = null;
			}
		}
	}

	/// <summary>
	/// Clears the selected collectible egg.
	/// To be called at the end of the game.
	/// </summary>
	public static void ClearCollectibleEgg() {
		// Skip if there is no active egg
		if(instance.m_collectibleEgg == null) return;

		// For now let's just lose the reference to it
		instance.m_collectibleEgg = null;
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