// ChestManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2016.
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
/// Global manager of chests.
/// </summary>
public class ChestManager : UbiBCN.SingletonMonoBehaviour<ChestManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int NUM_DAILY_CHESTS = 5;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Chests and chest getters
	// All chests, not sorted
	public static Chest[] dailyChests {
		get { return instance.m_user.dailyChests; }
	}

	// Chest sorted by state (INIT -> NOT_COLLECTED -> PENDING_REWARD -> COLLECTED)
	public static List<Chest> sortedChests {
		get {
			List<Chest> sortedChests = new List<Chest>(dailyChests);
			sortedChests.Sort(
				(_ch1, _ch2) => { 
					return _ch1.state.CompareTo(_ch2.state);
				}
			);
			return sortedChests;
		}
	}

	// Collected chests count. Includes chests in the PENDING_REWARD state.
	public static int collectedAndPendingChests {
		// C#'s Linq extensions come in handy!
		get { return dailyChests.Count(_ch => _ch.collected); }
	}

	// Collected chests count. Only chests in the COLLECTED state.
	public static int collectedChests {
		// C#'s Linq extensions come in handy!
		get { return dailyChests.Count(_ch => _ch.state == Chest.State.COLLECTED); }
	}

	// Reset timer
	public static DateTime resetTimestamp {
		get { return instance.m_user.dailyChestsResetTimestamp; }
		private set { instance.m_user.dailyChestsResetTimestamp = value; }
	}

	public static TimeSpan timeToReset {
		get { return resetTimestamp - DateTime.UtcNow; }
	}

	// Internal
	private UserProfile m_user = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	public void Update() {
		// Must be initialized
		if(!IsReady()) return;

		// Check reset timer
		if(DateTime.UtcNow >= resetTimestamp) {
			// Reset!
			Reset();
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Pick all the collectible chests in the level and initialize with the current chest data.
	/// To be called at the start of the game.
	/// </summary>
	public static void OnLevelLoaded() {
		// Get all the chests in the scene
		GameObject[] chestSpawners = GameObject.FindGameObjectsWithTag(CollectibleChest.TAG);	// Finding by tag is much faster than finding by type
		if(chestSpawners.Length > 0) {
			// Special case: chests are disabled during the very first run!
			if(instance.m_user.gamesPlayed <= 0) {
				for(int i = 0; i < chestSpawners.Length; i++) {
					GameObject.Destroy(chestSpawners[i]);
				}
				return;
			}

			// Aux vars
			List<Chest> pendingChests = new List<Chest>(dailyChests);
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
				// If chest was in the INIT state, change to NOT_COLLECTED
				if(pendingChests[i].state == Chest.State.INIT) {
					pendingChests[i].ChangeState(Chest.State.NOT_COLLECTED);
				}
				pendingChests[i].spawnPointID = validSpawners[i].name;

				// Initialize spawner
				spawner.Initialize(pendingChests[i]);
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
	/// Give pending rewards and update chests states and collected chests count.
	/// </summary>
	public static void ProcessChests() {
		// Pre checks
		if(!IsReady()) return;

		// Process all chests pending reward
		Chest chest = null;
		int collectedCount = collectedChests;
		for(int i = 0; i < dailyChests.Length; i++) {
			// If chest is not pending a reward, do nothing
			chest = dailyChests[i];
			if(chest.state != Chest.State.PENDING_REWARD) continue;

			// Mark chest as collected
			chest.ChangeState(Chest.State.COLLECTED);

			// Get reward corresponding to the current amount of collected chests
			collectedCount++;
			DefinitionNode rewardDef = GetRewardDef(collectedCount);
			if(rewardDef == null) continue;	// Do nothing if a reward could not be found

			// Give reward
			switch(rewardDef.Get("type")) {
				case "coins": {
					instance.m_user.AddCoins(rewardDef.GetAsInt("amount"));
				} break;

				case "pc": {
					instance.m_user.AddCoins(rewardDef.GetAsInt("pc"));
				} break;
			}
		}

		// Save persistence
		PersistenceManager.Save();
	}

	/// <summary>
	/// Get the reward definition corresponding to a specific amount of collected chests.
	/// </summary>
	/// <returns>The definition, <c>null</c> if something is wrong.</returns>
	/// <param name="_collectedChests">Amount of collected chests to be considered.</param>
	public static DefinitionNode GetRewardDef(int _collectedChests) {
		// Pretty straightforward, just check the "collectedChests" variable on the chests rewards definitions
		return DefinitionsManager.SharedInstance.GetDefinitionByVariable(
			DefinitionsCategory.CHEST_REWARDS, 
			"collectedChests", 
			_collectedChests.ToString(System.Globalization.CultureInfo.InvariantCulture)
		);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Resets the chests and timer.
	/// Made public for the cheats, shouldn't be called from outside.
	/// </summary>
	public static void Reset() {
		// Pre checks
		if(!IsReady()) return;

		// Reset chests
		for(int i = 0; i < dailyChests.Length; i++) {
			// Should never be null!
			if(dailyChests[i] == null) {
				dailyChests[i] = new Chest(); 
			}
			dailyChests[i].Reset();
		}

		// Reset timer
		resetTimestamp = DateTime.UtcNow.AddHours(24);	// [AOC] HARDCODED!!

		// Notify game
		Messenger.Broadcast(GameEvents.CHESTS_RESET);

		// Save persistence
		PersistenceManager.Save();
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Has the manager been initialized with persistence data?
	/// </summary>
	/// <returns>Whether the manager has been initialized.</returns>
	public static bool IsReady() {
		return instance.m_user != null && PersistenceManager.loadCompleted;
	}

	/// <summary>
	/// Initialize the manager with persistence data.
	/// </summary>
	/// <param name="_user">The persistence data.</param>
	public static void SetupUser(UserProfile _user) {
		instance.m_user = _user;
	} 
}