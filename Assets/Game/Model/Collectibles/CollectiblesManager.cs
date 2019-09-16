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
public class CollectiblesManager : Singleton<CollectiblesManager> {
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

	private bool m_key = false;



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

		Messenger.AddListener(MessengerEvents.TICKET_COLLECTED, instance.OnCollectKey);	
	}

	/// <summary>
	/// To be called at the end of the game.
	/// Clear selected collectibles.
	/// </summary>
	public static void Clear() {
		instance.m_egg = null;
		instance.m_chests.Clear();
		instance.m_key = false;

		Messenger.RemoveListener(MessengerEvents.TICKET_COLLECTED, instance.OnCollectKey);
	}

	public static void Process() {
		// Process egg
		if(instance.m_egg != null && instance.m_egg.collected) {
			// Add a new egg to the inventory
			EggManager.AddEggToInventory(Egg.CreateFromSku(Egg.SKU_STANDARD_EGG));

			// Mark tutorial as completed
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.FIRST_EGG_COLLECTED, true);
		}

		// Process chests
		ChestManager.ProcessChests();

		// Process keys
		if (instance.m_key) {
			// Just add a new key to the user profile
			UsersManager.currentUser.EarnCurrency(UserProfile.Currency.KEYS, 1, false, HDTrackingManager.EEconomyGroup.REWARD_RUN);
		}

        // Save persistence
        PersistenceFacade.instance.Save_Request();

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
		// Eggs disabled during FTUX and Tournament
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_EGGS_AT_RUN || SceneController.mode == SceneController.Mode.TOURNAMENT) {
			// Don't pick any egg, call the SelectCollectible method with an invalid name to hide all eggs on scene
			instance.m_egg = SelectCollectible<CollectibleEgg>(CollectibleEgg.TAG, string.Empty);
		}

		// Player hasn't collected any egg yet, force a specific one
		else if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_EGG_COLLECTED)) {
			// Pick a specific egg
			instance.m_egg = SelectCollectible<CollectibleEgg>(CollectibleEgg.TAG, CollectibleEgg.FIRST_EGG_NAME);
		} 

		// Normal case: pick a random egg
		else {
			// Pick a random egg from the scene
			instance.m_egg = SelectRandomCollectible<CollectibleEgg>(CollectibleEgg.TAG, true);
		}
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
            // Special case: chests are disabled during FTUX, TOURNAMENT and SPECIAL DRAGONS
			if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_CHESTS_AT_RUN || SceneController.mode == SceneController.Mode.TOURNAMENT) {
				for(int i = 0; i < chestSpawners.Length; i++) {
					GameObject.Destroy(chestSpawners[i]);
				}
				return;
			}

			// Aux vars
			List<Chest> pendingChests = new List<Chest>(ChestManager.dailyChests);
			List<CollectibleChest> validSpawners = new List<CollectibleChest>();
			List<CollectibleChest> toRemove = new List<CollectibleChest>();
			DragonTier maxOwnedTier = DragonManager.IsReady() ? DragonManager.biggestOwnedDragon.tier : DragonTier.TIER_0;
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
				//if(spawner.requiredTier <= currentTier) {
				if (maxOwnedTier >= spawner.requiredTier && maxOwnedTier <= spawner.maxTier) {
					validSpawners.Add(spawner);
				} else {
					toRemove.Add(spawner);
				}
			}

			// If we have any pending chest, assign a new spawner to it!
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
			// Unless cheating!
			if(!DebugSettings.showAllCollectibles) {
				toRemove.AddRange(validSpawners);
				for(int i = 0; i < toRemove.Count; i++) {
					GameObject.Destroy(toRemove[i].gameObject);
					toRemove[i] = null;
				}
			}
		}
	}


	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	private void OnCollectKey() {
		m_key = true;
	}

	/// <summary>
	/// Select a random collectible of the requested type from the current level.
	/// All other collectible of the same type in the level will be destroyed.
	/// A level must be loaded beforehand, otherwise nothing will happen.
	/// To be called at the start of the game.
	/// </summary>
	/// <returns>The selected random collectible. <c>null</c> if no collectible could be found with the given parameters.</returns>
	/// <param name="_tag">Tag filter.</param>
	/// <param name="_filterTier">Check tier?</param>
	/// <typeparam name="T">Type of collectible to be picked.</typeparam>
	private T SelectRandomCollectible<T>(string _tag, bool _filterTier) where T : Collectible {
		// Get all the collectibles in the scene with the requested tag 
		GameObject[] allCollectibles = GameObject.FindGameObjectsWithTag(_tag);

		// Select our target collectible based on input parameters
		T selectedCollectible = null;
		if(allCollectibles.Length > 0) {
			// Filter by dragon tier? (blockers, etc.)
			List<GameObject> filteredCollectibles = new List<GameObject>();
			if(_filterTier) {
				DragonTier currentTier = DragonManager.IsReady() ? DragonManager.CurrentDragon.tier : DragonTier.TIER_0;
				filteredCollectibles = allCollectibles.Where((GameObject _go) => { 
					T c = _go.GetComponent<T>();
					return currentTier >= c.requiredTier && currentTier <= c.maxTier;
				}).ToList();
			} else {
				filteredCollectibles = allCollectibles.ToList();// allCollectibles.Select((GameObject _go) => { return _go; }).ToList();
			}

			// Grab a random one from the list
			GameObject selectedObj = null;
			if(filteredCollectibles.Count > 0) {
				selectedObj = filteredCollectibles.GetRandomValue();
				selectedCollectible = selectedObj.GetComponent<T>();
			}

			// Remove the rest of collectibles from the scene
			ClearCollectibles(ref allCollectibles, selectedObj);
		}

		// Done!
		return selectedCollectible;
	}

	/// <summary>
	/// Select a specific collectible instance of the requested type from the current level.
	/// All other collectible of the same type in the level will be destroyed.
	/// A level must be loaded beforehand, otherwise nothing will happen.
	/// To be called at the start of the game.
	/// </summary>
	/// <returns>The selected collectible. <c>null</c> if no collectible could be found with the given parameters.</returns>
	/// <param name="_tag">Tag filter.</param>
	/// <param name="_instanceName">Name of the collectible instance to be selected.</param>
	/// <typeparam name="T">Type of collectible to be picked.</typeparam>
	private T SelectCollectible<T>(string _tag, string _instanceName) where T : Collectible {
		// Get all the collectibles in the scene with the requested tag 
		GameObject[] allCollectibles = GameObject.FindGameObjectsWithTag(_tag);

		// Select our target collectible based on input parameters
		T selectedCollectible = null;
		if(allCollectibles.Length > 0) {
			// Find the one with the given name
			GameObject selectedObj = null;
			for(int i = 0; i < allCollectibles.Length; ++i) {
				// First one matching the name
				if(allCollectibles[i].name == _instanceName) {
					selectedObj = allCollectibles[i];
					break;
				}
			}

			// Get component of the requested type
			if(selectedObj != null) {
				selectedCollectible = selectedObj.GetComponent<T>();
			}

			// Remove the rest of collectibles from the scene
			ClearCollectibles(ref allCollectibles, selectedObj);
		}

		// Done!
		return selectedCollectible;
	}

	/// <summary>
	/// Removes all the given collectibles from the scene, except the selected one.
	/// </summary>
	/// <param name="_allCollectibles">Collectibles to be removed from the scene.</param>
	/// <param name="_selectedCollectible">Selected collectible (will be spared). Can be <c>null</c> (all objects will be removed).</param>
	private void ClearCollectibles(ref GameObject[] _allCollectibles, GameObject _selectedCollectible) {
		// Skip for debugging
		if(DebugSettings.showAllCollectibles) return;

		// Iterate collectibles to be removed
		for(int i = 0; i < _allCollectibles.Length; i++) {
			// Skip if selected one
			if(_allCollectibles[i] == _selectedCollectible) continue;

			// Delete from scene otherwise
			GameObject.Destroy(_allCollectibles[i]);
			_allCollectibles[i] = null;
		}
	}
}