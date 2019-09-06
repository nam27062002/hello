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
public class ChestManager : Singleton<ChestManager> {
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
		get { 
			if(IsReady()) {
				return instance.m_user.dailyChests;
			}
			return new Chest[0];
		}
	}

	// Chest sorted by state (COLLECTED -> PENDING_REWARD -> NOT_COLLECTED -> INIT)
	public static List<Chest> sortedChests {
		get {
			List<Chest> sortedChests = new List<Chest>(dailyChests);
			sortedChests.Sort(
				(_ch1, _ch2) => { 
					return -_ch1.state.CompareTo(_ch2.state);	// [AOC] Invert sign because we actually want to sort them in the reverse order of the enum
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
		get {
			if(IsReady()) {
				return instance.m_user.dailyChestsResetTimestamp; 
			}
			return DateTime.Now;
		}

		private set { 
			if(IsReady()) {
				instance.m_user.dailyChestsResetTimestamp = value;
			}
		}
	}

	public static TimeSpan timeToReset {
		get { return resetTimestamp - GameServerManager.SharedInstance.GetEstimatedServerTime(); }
	}

	// Internal
	private UserProfile m_user = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	public void Update() {
		// Must be initialized
		if(!IsReady()) return;

		// Check reset timer
		if(GameServerManager.SharedInstance.GetEstimatedServerTime() >= resetTimestamp) {
			// Reset!
			Reset();
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC STATIC METHODS											//
	//------------------------------------------------------------------//
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
			Chest.RewardData rewardData = GetRewardData(collectedCount);
			if(rewardData == null) continue;	// Do nothing if a reward could not be found

			// Give reward
			switch(rewardData.type) {
				case Chest.RewardType.SC: {
					instance.m_user.EarnCurrency(UserProfile.Currency.SOFT, (ulong)rewardData.amount, false, HDTrackingManager.EEconomyGroup.REWARD_CHEST);
				} break;

				case Chest.RewardType.PC: {
					instance.m_user.EarnCurrency(UserProfile.Currency.HARD, (ulong)rewardData.amount, false, HDTrackingManager.EEconomyGroup.REWARD_CHEST);
				} break;

                case Chest.RewardType.GF: {
                    instance.m_user.EarnCurrency(UserProfile.Currency.GOLDEN_FRAGMENTS, (ulong)rewardData.amount, false, HDTrackingManager.EEconomyGroup.REWARD_CHEST);
                } break;
			}
		}

        // Save persistence
        PersistenceFacade.instance.Save_Request(false);

        // Notify game
        Messenger.Broadcast(MessengerEvents.CHESTS_PROCESSED);
	}

	/// <summary>
	/// Get the reward data corresponding to a specific amount of collected chests.
	/// </summary>
	/// <returns>The reward data, <c>null</c> if something went wrong.</returns>
	/// <param name="_collectedChests">Amount of collected chests to be considered [1..N].</param>
	public static Chest.RewardData GetRewardData(int _collectedChests) {
		// Get definition by checking the "collectedChests" variable on the chests rewards definitions
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinitionByVariable(
			DefinitionsCategory.CHEST_REWARDS, 
			"collectedChests", 
			_collectedChests.ToString(System.Globalization.CultureInfo.InvariantCulture)
		);

		// Check for errors
		if(def == null) return null;

		// Create and initialize the data object with the definition and return
		return new Chest.RewardData(def);
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

		// Reset timestamp to 00:00 of local time (but using server timezone!)
		TimeSpan toMidnight = DateTime.Today.AddDays(1) - DateTime.Now;	// Local
		resetTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime() + toMidnight;	// Local 00:00 in server timezone

		// Notify game
		Messenger.Broadcast(MessengerEvents.CHESTS_RESET);

        // Save persistence
        PersistenceFacade.instance.Save_Request(false);
    }

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Has the manager been initialized with persistence data?
	/// </summary>
	/// <returns>Whether the manager has been initialized.</returns>
	public static bool IsReady() {
		return instance.m_user != null && PersistenceFacade.instance.LocalDriver.IsLoadedInGame;
	}

	/// <summary>
	/// Initialize the manager with persistence data.
	/// </summary>
	/// <param name="_user">The persistence data.</param>
	public static void SetupUser(UserProfile _user) {
		instance.m_user = _user;
	} 
}