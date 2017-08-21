// CollectiblesManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/08/2017.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global manager for collectible items in the game.
/// </summary>
public class CollectiblesManager : UbiBCN.SingletonMonoBehaviour<CollectiblesManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private List<CollectibleChest> m_chests = new List<CollectibleChest>();
	public static List<CollectibleChest> chests {
		get { return instance.m_chests; }
	}

	private CollectibleEgg m_egg = null;
	public static CollectibleEgg egg {
		get { return instance.m_egg; }
	}

	private CollectibleKey m_key = null;
	public static CollectibleKey key {
		get { return instance.m_key; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		
	}

	//------------------------------------------------------------------//
	// PUBLIC STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// To be called at the start of the game.
	/// Pick all the collectibles in the level and select target ones.
	/// </summary>
	public static void OnLevelLoaded() {
		// Init all collectible types!
		instance.InitLevelEggs();
		instance.InitLevelChests();
		instance.InitLevelKeys();
	}

	/// <summary>
	/// To be called at the end of the game.
	/// Clear selected collectibles.
	/// </summary>
	public static void Clear() {
		instance.m_egg = null;
		instance.m_chests.Clear();
		instance.m_key = null;
	}

	public static void Process() {
		// Process egg
		if(instance.m_egg != null && instance.m_egg.collected) {
			// Add a new egg to the inventory
			EggManager.AddEggToInventory(Egg.CreateFromSku(Egg.SKU_STANDARD_EGG));
		}

		// Process chests
		ChestManager.ProcessChests();

		// Process keys
		if(instance.m_key != null && instance.m_key.collected) {
			// Just add a new key to the user profile
			UsersManager.currentUser.EarnCurrency(UserProfile.Currency.KEYS, 1, false, HDTrackingManager.EEconomyGroup.REWARD_RUN);
		}

		// Save persistence
		PersistenceManager.Save();

		// Clear manager
		Clear();
	}

	/// <summary>
	/// Given a world position, find the closest active chest.
	/// </summary>
	/// <returns>The closest active chest.</returns>
	/// <param name="_pos">Position.</param>
	public static CollectibleChest GetClosestActiveChest(Vector3 _pos) {
		float minDistance = float.MaxValue;
		CollectibleChest chest = null;
		int size = chests.Count;
		for( int i = 0; i<size; ++i )
		{
			if (!chests[i].chestData.collected)
			{
				float dist = (_pos - chests[i].transform.position).sqrMagnitude;
				if ( dist < minDistance )
				{
					minDistance = dist;
					chest = chests[i];
				}
			}
		}
		return chest;
	}

	//------------------------------------------------------------------//
	// INITIALIZER METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Select a random egg from the current level and define it as the active egg.
	/// All other eggs in the level will be removed.
	/// A level must be loaded beforehand, otherwise nothing will happen.
	/// To be called at the start of the game.
	/// </summary>
	private void InitLevelEggs() {
		// Pick a random egg from the scene
		instance.m_egg = SelectRandomCollectible<CollectibleEgg>(CollectibleEgg.TAG, true, TutorialStep.FIRST_RUN);
	}

	/// <summary>
	/// Pick all the collectible chests in the level and initialize with the current chest data.
	/// To be called at the start of the game.
	/// </summary>
	private void InitLevelChests() {
		// Clear previously selected chests (if any)
		instance.m_chests.Clear();

		// Get all the chests in the scene
		GameObject[] chestSpawners = GameObject.FindGameObjectsWithTag(CollectibleChest.TAG);	// Finding by tag is much faster than finding by type
		if(chestSpawners.Length > 0) {
			// Special case: chests are disabled during the very first run!
			if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN)) {
				for(int i = 0; i < chestSpawners.Length; i++) {
					GameObject.Destroy(chestSpawners[i]);
				}
				return;
			}

			// Aux vars
			List<Chest> pendingChests = new List<Chest>(ChestManager.dailyChests);
			List<CollectibleChest> validSpawners = new List<CollectibleChest>();
			List<CollectibleChest> toRemove = new List<CollectibleChest>();
			DragonTier currentTier = DragonManager.IsReady() ? DragonManager.currentDragon.tier : DragonTier.TIER_0;
			CollectibleChest spawner = null;
			bool spawnerUsed = false;

			// Iterate all spawn points and initialize them
			for(int i = 0; i < chestSpawners.Length; i++) {
				// Reset aux vars
				spawnerUsed = false;
				spawner = chestSpawners[i].GetComponent<CollectibleChest>();
				if(spawner == null) continue;

				// Does this spawn point match one of the chests?
				for(int j = 0; j < pendingChests.Count && !spawnerUsed; j++) {
					if(spawner.name == pendingChests[j].spawnPointID) {
						// Yes!! Use it
						spawner.Initialize(pendingChests[j]);
						chests.Add(spawner);
						pendingChests.RemoveAt(j);
						spawnerUsed = true;
						break;
					}
				}

				// Skip to next spawner if used
				if(spawnerUsed) continue;

				// Check whether it's a valid spawner (filter by dragon tier)
				if(spawner.requiredTier <= currentTier) {
					validSpawners.Add(spawner);
				} else {
					toRemove.Add(spawner);
				}
			}

			// If we have any pending chest, assing a new spawner to it!
			for(int i = 0; i < pendingChests.Count; i++) {
				// Pick a random one from the valid spawners list
				spawner = validSpawners.GetRandomValue();

				// Update chest
				// If chest was in the INIT state, change to NOT_COLLECTED and assign the new spawner to it
				if(pendingChests[i].state == Chest.State.INIT) {
					pendingChests[i].ChangeState(Chest.State.NOT_COLLECTED);
				}
				pendingChests[i].spawnPointID = spawner.name;

				// Initialize spawner
				spawner.Initialize(pendingChests[i]);
				chests.Add(spawner);
				validSpawners.Remove(spawner);
			}

			// Finally remove the unused spawners from the scene
			toRemove.AddRange(validSpawners);
			for(int i = 0; i < toRemove.Count; i++) {
				GameObject.Destroy(toRemove[i].gameObject);
				toRemove[i] = null;
			}
		}
	}

	/// <summary>
	/// Select a random key from the current level and define it as the active key.
	/// All other keys in the level will be removed.
	/// A level must be loaded beforehand, otherwise nothing will happen.
	/// To be called at the start of the game.
	/// </summary>
	private void InitLevelKeys() {
		// Pick a random key from the scene
		instance.m_key = SelectRandomCollectible<CollectibleKey>(CollectibleKey.TAG, true, TutorialStep.FIRST_RUN);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Select a random collectible of the requested type from the current level.
	/// All other collectible of the same type in the level will be destroyed.
	/// A level must be loaded beforehand, otherwise nothing will happen.
	/// To be called at the start of the game.
	/// </summary>
	/// <returns>The selected random collectible. <c>null</c> if no collectible could be found with the given parameters.</returns>
	/// <param name="_tag">Tag filter.</param>
	/// <param name="_filterTier">Check tier?</param>
	/// <param name="_tutorialMask">Steps to be completed before actually select a collectible.</param>
	/// <typeparam name="T">Type of collectible to be picked.</typeparam>
	private T SelectRandomCollectible<T>(string _tag, bool _filterTier, TutorialStep _tutorialMask = TutorialStep.INIT) where T : Collectible {
		// Get all the collectibles in the scene with the requested tag 
		T selectedCollectible = null;
		GameObject[] allCollectibles = GameObject.FindGameObjectsWithTag(_tag);
		if(allCollectibles.Length > 0) {
			// Filter by dragon tier? (blockers, etc.)
			List<GameObject> filteredCollectibles = new List<GameObject>();
			if(_filterTier) {
				DragonTier currentTier = DragonManager.IsReady() ? DragonManager.currentDragon.tier : DragonTier.TIER_0;
				filteredCollectibles = allCollectibles.Where((GameObject _go) => { return _go.GetComponent<T>().requiredTier <= currentTier; }).ToList();
			} else {
				filteredCollectibles = allCollectibles.Select((GameObject _go) => { return _go; }).ToList();
			}

			// Min tutorial step required?
			GameObject selectedObj = null;
			if(UsersManager.currentUser.IsTutorialStepCompleted(_tutorialMask)) {
				// Grab a random one from the list
				selectedObj = filteredCollectibles.GetRandomValue();
				selectedCollectible = selectedObj.GetComponent<T>();
			}

			// Remove the rest of collectibles from the scene
			for(int i = 0; i < allCollectibles.Length; i++) {
				// Skip if selected one
				if(allCollectibles[i] == selectedObj) continue;

				// Delete from scene otherwise
				GameObject.Destroy(allCollectibles[i]);
				allCollectibles[i] = null;
			}
		}

		// Done!
		return selectedCollectible;
	}
}