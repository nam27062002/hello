// DailyRewardsCollection.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Handles a set of daily rewards: generation, collection, persistence, etc.
/// </summary>
public class DailyRewardsSequence {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const int SEQUENCE_SIZE = 7; // [AOC] CHECK!! From content?

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Sequence
	private DailyReward[] m_rewards;
	public DailyReward[] rewards {
		get {
			if(m_rewards == null) Generate();
			return rewards;
		}
	}

	// Rewards indexing
	public int totalRewardIdx;	// The total index of the NEXT reward to be collected, that is, counting all collected rewards in all sequences
	public int rewardIdx {      // The index within the sequence [0..SEQUENCE_SIZE - 1] of the NEXT reward to be collected
		get { return totalRewardIdx % SEQUENCE_SIZE; }
	}

	// Other vars
	public DateTime nextCollectionTimestamp;	// UTC, time at which the cooldown has expired and the player can collect the next reward
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public DailyRewardsSequence() {
		// Setup initial values
		Reset();
	}

	/// <summary>
	/// Reset to default values.
	/// </summary>
	public void Reset() {
		// Rewards
		m_rewards = new DailyReward[SEQUENCE_SIZE];
		for(int i = 0; i < SEQUENCE_SIZE; ++i) {
			m_rewards[i] = new DailyReward();
		}

		// Other vars
		totalRewardIdx = 0;
		nextCollectionTimestamp = DateTime.MinValue;
	}

	/// <summary>
	/// Obtain the next reward to be collected.
	/// </summary>
	/// <returns>The next reward to be collected. Shouldn't be null.</returns>
	public DailyReward GetNextReward() {
		return rewards[rewardIdx];	// Should always be valid
	}

	/// <summary>
	/// Check whether the player can collect the next reward or not.
	/// </summary>
	/// <returns><c>true</c>, if collect next reward was caned, <c>false</c> otherwise.</returns>
	public bool CanCollectNextReward() {
		// Check timestamps
		return GameServerManager.SharedInstance.GetEstimatedServerTime() >= nextCollectionTimestamp;
	}

	/// <summary>
	/// Collect next reward!
	/// No checks performed.
	/// If the collected reward is the last one from the sequene, a new sequence will be generated.
	/// </summary>
	/// <param name="_doubled">Has the reward been doubled?</param>
	public void CollectNextReward(bool _doubled) {
		// Advance index!
		totalRewardIdx++;

		// Do we need to generate a new sequence?
		if(rewardIdx >= SEQUENCE_SIZE) {
			Generate();
		}

		// Reset timer to the start of the following day, in local time zone
		// [AOC] The fact that we are saving the next collection timestamp will that
		// 		 cheat attempts by advancing the devices clock, although successful,
		//		 are punished by never being able to come back to normal time.
		// [AOC] Small trick to figure out the start of a day, from http://stackoverflow.com/questions/3362959/datetime-now-first-and-last-minutes-of-the-day
		DateTime tomorrow = DateTime.Now.Date.AddDays(1);    // Using local time zone to compute tomorrow's date
		nextCollectionTimestamp = tomorrow.ToUniversalTime();  // Work in UTC (to be able to compare with Server time)

		// [AOC] Testing purposes
		//nextCollectionTimestamp = DateTime.Now.AddSeconds(30).ToUniversalTime();

		// Program local notification
		// [AOC] TODO!!
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Generate a new sequence of rewards.
	/// </summary>
	private void Generate() {
		// Gather all defined reward definitions
		List<DefinitionNode> rewardDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.DAILY_REWARDS);

		// Store them in a dictionary using their "day" property as key
		Dictionary<int, DefinitionNode> rewardDefsByDay = new Dictionary<int, DefinitionNode>();
		for(int i = 0; i < rewardDefs.Count; ++i) {
			rewardDefsByDay[rewardDefs[i].GetAsInt("day")] = rewardDefs[i];
		}

		// 

		/*
		 for(int totalRewardIdx = 0; totalRewardIdx < 31; ++totalRewardIdx) {
                if(totalRewardIdx % SEQUENCE_SIZE == 0) {
                    Console.WriteLine();
                }
                
                int rewardIdx = totalRewardIdx % SEQUENCE_SIZE;
                int day = totalRewardIdx + 1;
                
                // Find out closest reward corresponding to that day of the week
                int targetDay = day;
                while(!rewards.Contains(targetDay)) {
                    targetDay -= 7;
                }
                Console.WriteLine(day + " (" + totalRewardIdx + ")" + " -> " + targetDay + " (" + rewardIdx + ")");
            }
		 */
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Constructor from json data.
	/// </summary>
	/// <param name="_data">Data to be parsed.</param>
	public void LoadData(SimpleJSON.JSONNode _data) {
		// Reset any existing data
		Reset();

		// Rewards
		if(_data.ContainsKey("rewards")) {
			SimpleJSON.JSONArray rewardsData = _data["rewards"].AsArray;
			for(int i = 0; i < rewardsData.Count && i < m_rewards.Length; ++i) {
				m_rewards[i].LoadData(rewardsData[i]);
			}
		}
	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json.</returns>
	public SimpleJSON.JSONClass SaveData() {
		// Reward
		/*SimpleJSON.JSONClass data = reward.ToJson() as SimpleJSON.JSONClass;

		// State
		data.Add("collected", collected);
		data.Add("doubled", doubled);

		return data;*/
		return null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}