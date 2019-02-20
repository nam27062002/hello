// DailyRewardsCollection.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//#define PLAYTEST

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
	public const int SEQUENCE_SIZE = 7;

#if PLAYTEST
	public const float PLAYTEST_COOLDOWN_DURATION = 3 * 60f;    // Seconds
#endif

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Sequence
	private DailyReward[] m_rewards;
	public DailyReward[] rewards {
		get {
			if(!ValidateRewards()) Generate();	// Just in case
			return m_rewards;
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
	public DateTime nextCollectionTimestamp {
		get { return m_nextCollectionTimestamp; }
	}

	public TimeSpan timeToCollection {
		get {
			// Avoid negative values
			TimeSpan remainingTime = m_nextCollectionTimestamp - GameServerManager.SharedInstance.GetEstimatedServerTime();
			if(remainingTime.TotalSeconds < 0) {
				return new TimeSpan(0);
			} else {
				return remainingTime;
			}
		}
	} 

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
		m_nextCollectionTimestamp = DateTime.MinValue;  // Already expired so first reward can be collected
	}

	/// <summary>
	/// Obtain the next reward to be collected.
	/// </summary>
	/// <returns>The next reward to be collected. Shouldn't be null.</returns>
	public DailyReward GetNextReward() {
		return m_rewards[rewardIdx];	// Should always be valid
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

		// Tracking (before advancing the indexes!)
		// Special case when the reward is a pet replaced by gf
		string sourceType = reward.sourceDef.Get("type");
		string trackingType = reward.reward.type;
		string trackingSku = reward.reward.sku;
		if(sourceType == Metagame.RewardPet.TYPE_CODE && reward.reward.type != sourceType) {
			trackingType = sourceType + "," + reward.reward.type;
			trackingSku = reward.sourceDef.Get("rewardSku") + "," + reward.reward.type;
		}
		HDTrackingManager.Instance.Notify_DailyReward(
			rewardIdx,
			m_totalRewardIdx,
			trackingType,
			_doubled ? reward.reward.amount * 2 : reward.reward.amount,
			trackingSku,
			_doubled
		);

		// Do it!
		reward.Collect(_doubled);

		// Do we need to generate a new sequence?
		// [AOC] Before increasing the total index!! Otherwise rewardIdx will give 0
		bool newSequenceNeeded = rewardIdx + 1 >= SEQUENCE_SIZE;

		// Advance index!
		m_totalRewardIdx++;

		// Generate the new sequence if needed (AFTER increasing the index)
		if(newSequenceNeeded) {
			Generate();
		}

		// Reset timestamp to 00:00 of local time (but using server timezone!)
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
#if PLAYTEST
		m_nextCollectionTimestamp = serverTime.AddSeconds(PLAYTEST_COOLDOWN_DURATION);
#else
		TimeSpan toMidnight = DateTime.Today.AddDays(1) - DateTime.Now; // Local
		m_nextCollectionTimestamp = serverTime + toMidnight;   // Local 00:00 in server timezone
#endif

		// Program local notification
		// [AOC] Actually don't! It will be programmed when the application goes on pause (ApplicationManager class)
#if PLAYTEST
		// [AOC] DEBUG!! We'll do it just for the playtest
		HDNotificationsManager.instance.ScheduleDailyRewardNotification();
#endif

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

	/// <summary>
	/// Make sure the rewards are valid.
	/// </summary>
	/// <returns>Whether all the rewards in the sequence are valid. Returns <c>false</c> if at least one reward is not valid.</returns>
	private bool ValidateRewards() {
		// Check array
		if(m_rewards == null) {
			return false;
		}

		// Check sequence size
		if(m_rewards.Length != SEQUENCE_SIZE) {
			return false;
		}

		// Check rewards
		for(int i = 0; i < SEQUENCE_SIZE; ++i) {
			// Check Daily Reward object
			if(m_rewards[i] == null) {
				return false;
			}

			// Check Metagame Reward object
			if(m_rewards[i].reward == null) {
				return false;
			}
		}

		// Everything ok!
		return true;
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

		// Current reward index
		if(_data.ContainsKey("totalRewardIdx")) {
			m_totalRewardIdx = _data["totalRewardIdx"];
		}

		// Collect timestamp
		if(_data.ContainsKey("nextCollectionTimestamp")) {
			m_nextCollectionTimestamp = DateTime.Parse(_data["nextCollectionTimestamp"], PersistenceFacade.JSON_FORMATTING_CULTURE);
		}

		// Rewards
		if(_data.ContainsKey("rewards")) {
			SimpleJSON.JSONArray rewardsData = _data["rewards"].AsArray;
			for(int i = 0; i < rewardsData.Count && i < m_rewards.Length; ++i) {
				if(m_rewards[i] != null) {
					m_rewards[i].LoadData(rewardsData[i]);
				}
			}
		}

		// Make sure rewards are valid, generate a new sequence otherwise
		if(!ValidateRewards()) {
			Generate();
		}
	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json. Can be null if sequence has never been generated.</returns>
	public SimpleJSON.JSONClass SaveData() {
		// If sequence is not valid, don't save
		if(!ValidateRewards()) {
			return null;
		}

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
	// DEBUG																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Manually force a specific total reward index.
	/// </summary>
	/// <param name="_value">New index.</param>
	public void DEBUG_SetTotalRewardIdx(int _value) {
		// Do we need to generate a new sequence?
		bool needNewSequence = Mathf.Abs(m_totalRewardIdx - _value) > SEQUENCE_SIZE;

		// Advance index!
		m_totalRewardIdx = _value;

		// Do we need to generate a new sequence?
		if(needNewSequence) {
			Generate();
		}
	}

	/// <summary>
	/// Skip the cooldown timer.
	/// </summary>
	public void DEBUG_SkipCooldownTimer() {
		// Nothing to do if not in cooldown
		if(CanCollectNextReward()) return;

		// Set collection timestamp to current time
		m_nextCollectionTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTime();
	}
}