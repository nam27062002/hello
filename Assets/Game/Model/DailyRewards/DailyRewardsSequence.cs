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
	private int m_totalRewardIdx;
	public int totalRewardIdx {	// The total index of the NEXT reward to be collected, that is, counting all collected rewards in all sequences
		get { return m_totalRewardIdx; }
	}

	public int rewardIdx {      // The index within the sequence [0..SEQUENCE_SIZE - 1] of the NEXT reward to be collected
		get { return m_totalRewardIdx % SEQUENCE_SIZE; }
	}

	// Other vars
	private DateTime m_nextCollectionTimestamp;	// UTC, time at which the cooldown has expired and the player can collect the next reward
	
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
		m_totalRewardIdx = 0;
		m_nextCollectionTimestamp = DateTime.MinValue;	// Already expired so first reward can be collected
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
		return GameServerManager.SharedInstance.GetEstimatedServerTime() >= m_nextCollectionTimestamp;
	}

	/// <summary>
	/// Collect next reward!
	/// No checks performed.
	/// If the collected reward is the last one from the sequene, a new sequence will be generated.
	/// </summary>
	/// <param name="_doubled">Has the reward been doubled?</param>
	public void CollectNextReward(bool _doubled) {
		// Aux vars
		DailyReward reward = GetNextReward();

		// Do it!
		reward.Collect(_doubled);

		// Advance index!
		m_totalRewardIdx++;

		// Do we need to generate a new sequence?
		if(rewardIdx >= SEQUENCE_SIZE) {
			Generate();
		}

		// Reset timestamp to 00:00 of local time (but using server timezone!)
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
		TimeSpan toMidnight = DateTime.Today.AddDays(1) - DateTime.Now; // Local
		m_nextCollectionTimestamp = serverTime + toMidnight;   // Local 00:00 in server timezone

		// [AOC] Testing purposes
		//m_nextCollectionTimestamp = serverTime.AddSeconds(30);

		// Program local notification
		// [AOC] Actually don't! It will be programmed when the application goes on pause (ApplicationManager class)
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Generate a new sequence of rewards.
	/// </summary>
	public void Generate() {
		// Gather all defined reward definitions
		List<DefinitionNode> rewardDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.DAILY_REWARDS);

		// Store them in a dictionary using their "day" property as key
		Dictionary<int, DefinitionNode> rewardDefsByDay = new Dictionary<int, DefinitionNode>();
		for(int i = 0; i < rewardDefs.Count; ++i) {
			rewardDefsByDay[rewardDefs[i].GetAsInt("day")] = rewardDefs[i];
		}

		// Generate rewards for the next SEQUENCE_SIZE days (totalRewardIdx to totalRewardIdx + SEQUENCE_SIZE)
		for(int i = 0; i < SEQUENCE_SIZE; ++i) {
			// Find out closest reward corresponding to the current total reward index for this day
			// With this, day 14 will use def for day 14, but day 13 will reuse the def for day 6 instead :)
			int targetDay = m_totalRewardIdx + i + 1;
			while(targetDay >= 0 && !rewardDefsByDay.ContainsKey(targetDay)) {
				targetDay -= SEQUENCE_SIZE;
			}

			// This should never happen (as we should have definitions for at least days 1 to SEQUENCE_SIZE), but just in case
			if(targetDay < 0) {
				Debug.LogError(Color.red.Tag("ERROR! Couldn't find a reward for slot " + i + " (day " + (m_totalRewardIdx + i + 1) + ")"));
				targetDay = 1;	// If we don't have a reward for day 1, something is really wrong xDD
			}

			// Initialize reward with the definition for the target day
			m_rewards[i].InitFromDef(rewardDefsByDay[targetDay]);
		}
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

		// Current reward index
		if(_data.ContainsKey("totalRewardIdx")) {
			m_totalRewardIdx = _data["totalRewardIdx"];
		}

		// Collect timestamp
		if(_data.ContainsKey("nextCollectionTimestamp")) {
			m_nextCollectionTimestamp = DateTime.Parse(_data["nextCollectionTimestamp"], PersistenceFacade.JSON_FORMATTING_CULTURE);
		}
	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json.</returns>
	public SimpleJSON.JSONClass SaveData() {
		// Create a new json data object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Rewards
		SimpleJSON.JSONArray rewardsData = new SimpleJSON.JSONArray();
		for(int i = 0; i < m_rewards.Length; ++i) {
			// Even if the reward is not valid, add an entry all the same to keep the array's consistency
			if(m_rewards[i] != null) {
				rewardsData.Add(m_rewards[i].SaveData());
			} else {
				rewardsData.Add(new SimpleJSON.JSONClass());
			}
		}
		data.Add("rewards", rewardsData);

		// Current reward index
		data.Add("totalRewardIdx", m_totalRewardIdx);

		// Collect timestamp
		data.Add("nextCollectionTimestamp", m_nextCollectionTimestamp.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));

		// Done!
		return data;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}