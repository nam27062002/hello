// GlobalEvent.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

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
/// Single global event object.
/// </summary>
[Serializable]
public partial class GlobalEvent {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum State {
		INIT,
		TEASING,
		ACTIVE,
		FINISHED,
		REWARD_COLLECTED,

		COUNT
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	private int m_id = -1;
	public int id {
		get { return m_id; }
	}

	private string m_name = "";
	public string name {
		get { return m_name; }
	}

	// Objective - used to track the player's contribution to this event PER GAME
	private GlobalEventObjective m_objective = null;
	public GlobalEventObjective objective { 
		get { return m_objective; }
	}

	// Milestones and global progress
	private List<RewardSlot> m_rewardSlots = new List<RewardSlot>();	// Sorted from lower to higher step
	public List<RewardSlot> rewardSlots { 
		get { return m_rewardSlots; }
	}

	private float m_currentValue = 0f;
	public float currentValue {
		get { return m_currentValue; }
	}

	public float targetValue {
		get { return m_objective != null ? m_objective.targetValue : 1f; }
	}

	public float progress {
		get { return Mathf.Clamp01(m_currentValue/targetValue); }
	}

	// Rewards
	private RewardSlot m_topContributorsRewardSlot = null; // Percentage is differently used here!
	public RewardSlot topContributorsRewardSlot {
		get { return m_topContributorsRewardSlot; }
	}

	private int m_rewardLevel = -1; // how many rewards will get this user?
	public int rewardLevel { get { return m_rewardLevel; } }

	private bool m_topContributor = false; // if top contributor
	public bool topContributor { get { return m_topContributor; } }

	// Bonuses
	private string m_bonusDragonSku = "";
	public string bonusDragonSku {
		get { return m_bonusDragonSku; }
	}

	// Timestamps
	private DateTime m_teasingStartTimestamp = new DateTime();
	public DateTime teasingStartTimestamp {
		get { return m_teasingStartTimestamp; }
	}

	private DateTime m_startTimestamp = new DateTime();
	public DateTime startTimestamp {
		get { return m_startTimestamp; }
	}

	private DateTime m_endTimestamp = new DateTime();
	public DateTime endTimestamp {
		get { return m_endTimestamp; }
	}

	public TimeSpan remainingTime {	// Dynamic, depending on current state
		get { 
			DateTime now = GameServerManager.SharedInstance.GetEstimatedServerTime();
			switch(m_state) {
				case State.TEASING:	return m_startTimestamp - now;		break;
				case State.ACTIVE:	return m_endTimestamp - now;		break;
				default:			return new TimeSpan();				break;
			}
		}
	}

	// State
	private State m_state = State.INIT;
	public State state {
		get { return m_state; }
		set { SetState(value); }
	}

	public bool isTeasing {
		get { return m_state == State.TEASING; }
	}

	public bool isActive {
		get { return m_state == State.ACTIVE; }
	}

	public bool isFinished {
		get { return m_state == State.FINISHED; }
	}

	public bool isRewardAvailable {
		get { return isFinished && m_rewardLevel > -1; }	// By checking the reward level, we make sure that the rewards have been received from server!
	}

	// Contribution
	private List<GlobalEventUserData> m_leaderboard = new List<GlobalEventUserData>();	// Sorted
	public List<GlobalEventUserData> leaderboard {
		get { return m_leaderboard; }
	}

	private int m_totalPlayers = 0;
	public int totalPlayers {
		get { return m_totalPlayers; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public GlobalEvent() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~GlobalEvent() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Add a contribution to the event.
	/// Current value as well as local leaderboard will be updated.
	/// </summary>
	/// <param name="_value">Value to be added.</param>
	public void AddContribution(int _value) {
		// Ignore if event is not active
		if(!isActive) return;

		// Update current value
		m_currentValue += _value;
	}

	/// <summary>
	/// Check whether the given user data still belongs to the cached leaderboard.
	/// Leaderboard is updated according to it.
	/// </summary>
	/// <param name="_data">User data to be checked.</param>
	public void RefreshLeaderboardPosition(GlobalEventUserData _data) {
		// Check whether the player is in the leaderboard
		bool sort = false;
		if ( m_leaderboard.Count <= _data.position || _data.position < 0){
			// if I'm not in the leaderboard -> check if I have to be
			int index = leaderboard.Count - 1;

			if ( index >= 0 && m_leaderboard[ index ].score <= _data.score ){	
				m_leaderboard.Add(new GlobalEventUserData(_data));	// Make a copy!
				sort = true;
			}
			else if ( _data.score > 0 && _data.position < 0 )
			{
				_data.position = UnityEngine.Random.Range( m_leaderboard.Count, m_leaderboard.Count * 3);
			}
		}else{
			// Update Score
			m_leaderboard[ _data.position ].score = _data.score;
			sort = true;
		}

		// Sort the leaderboard
		if ( sort ){
			m_leaderboard.Sort((_a, _b) => -(_a.score.CompareTo(_b.score)));	// Using float's CompareTo directly. Reverse sign since we want to sort from bigger to lower.

			// Update positions
			for(int i = 0; i < m_leaderboard.Count; ++i) {
				m_leaderboard[i].position = i;
				if(m_leaderboard[i].userID == _data.userID) {
					_data.position = i;
				}
			}
		}

		// Keep leaderboard size controlled
		if(m_leaderboard.Count > 100) {
			m_leaderboard.RemoveRange(100, m_leaderboard.Count - 100);
		}
	}

	//--------------------------------------------------------------------------------------------------------------
	public void CollectAllRewards() {
		if (m_state == State.FINISHED) {
			// Temporary, apply rewards
			for (int i = 0; i < m_rewardLevel; i++) {
				CollectReward(i);
			}

			FinishRewardCollection();
		}
	}

	public void CollectReward(int _index) {
		if (m_state == State.FINISHED) {
			if (_index < m_rewardSlots.Count) {
				m_rewardSlots[_index].reward.Collect();
			} else {
				m_topContributorsRewardSlot.reward.Collect();
			}
		}
	}

	public void FinishRewardCollection() {
		GlobalEventUserData globalEventUserData = UsersManager.currentUser.GetGlobalEventData(m_id);
		globalEventUserData.rewardCollected = true;

		// Track global event Reward
		HDTrackingManager.Instance.Notify_GlobalEventReward(id, objective.typeDef.sku, m_rewardLevel, globalEventUserData.score, m_topContributor);
		m_state = State.REWARD_COLLECTED;
	}
	//--------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Initialize event from a JSON definition.
	/// </summary>
	/// <param name="_data">JSON data.</param>
	public void InitFromJson(SimpleJSON.JSONNode _data) {
		// Event ID
		m_id = _data["id"].AsInt;
		m_name = _data["name"];

		// Event objective
		// Ditch any existing objective, we don't care
		m_objective = new GlobalEventObjective(this, _data["goal"]);

		// Timestamps
		m_teasingStartTimestamp = TimeUtils.TimestampToDate(_data["teaserTimestamp"].AsLong);
		m_startTimestamp = TimeUtils.TimestampToDate(_data["startTimestamp"].AsLong);
		m_endTimestamp = TimeUtils.TimestampToDate(_data["endTimestamp"].AsLong);

		// Infer event's state from timestamps
		UpdateState();

		// Rewards
		m_rewardSlots.Clear();
		SimpleJSON.JSONArray rewardsDataArray = _data["rewards"].AsArray;
		for(int i = 0; i < rewardsDataArray.Count; ++i) {
			m_rewardSlots.Add(new RewardSlot(rewardsDataArray[i]));
			m_rewardSlots[i].targetAmount = m_rewardSlots[i].targetPercentage * targetValue;
		}

		// Make sure steps are sorted by percentile
		m_rewardSlots.Sort((RewardSlot _a, RewardSlot _b) => { return _a.targetPercentage.CompareTo(_b.targetPercentage); });

		// Special reward for the top X% contributors
		m_topContributorsRewardSlot = new RewardSlot(_data["topReward"]);

		// Bonuses
		m_bonusDragonSku = _data["bonusDragon"];

		// Reset state vars
		m_currentValue = 0f;
		m_leaderboard.Clear();

		GlobalEventUserData playerEventData = UsersManager.currentUser.GetGlobalEventData(m_id);
		playerEventData.endTimestamp = _data["endTimestamp"].AsLong;
	}


	/// <summary>
	/// Update the event's current state from a JSON object.
	/// Corresponds to the GetState server call.
	/// </summary>
	/// <param name="_data">Data.</param>
	public void UpdateFromJson(SimpleJSON.JSONNode _data) {
		// Current value
		m_currentValue = _data["globalScore"].AsFloat;

		// Update event state (just in case, has nothing to do with given json)
		UpdateState();
	}

	/// <summary>
	/// Update event's leaderboard from a JSON object.
	/// Correspnds to the GetLeaderboard server call.
	/// </summary>
	/// <param name="_data">Data.</param>
	public void UpdateLeaderboardFromJson(SimpleJSON.JSONNode _data) {
		/*
		 EXPECTED JSON:
		 {
		  l: [
		    {
		      name: "randomName72",
		      pic: "randomPicUrl72",
		      score: 1926
		    },
		    ...{
		      name: "randomName469",
		      pic: "randomPicUrl469",
		      score: 1338
		    },
		    {
		      name: "randomName205",
		      pic: "randomPicUrl205",
		      score: 1334
		    },
		    {
		      name: "randomName474",
		      pic: "randomPicUrl474",
		      score: 1333
		    }
		  ],
		  n: 500,
		  u: {
		    name: "randomName1",
		    pic: "randomPicUrl1",
		    score: 774
		  }
		}
		*/

		// Parse leaderboard
		SimpleJSON.JSONArray leaderboardData = _data["l"].AsArray;
		int numEntries = leaderboardData.Count;
		for(int i = 0; i < numEntries; ++i) {
			// Reuse existing entries, create a new one if needed
			if(i >= m_leaderboard.Count) {
				m_leaderboard.Add(new GlobalEventUserData());
			}

			// Update leaderboard entry
			m_leaderboard[i].Reset();
			m_leaderboard[i].Load(leaderboardData[i]);
			m_leaderboard[i].position = i;
		}

		// Remove unused entries from the leaderboard
		if(m_leaderboard.Count > numEntries) {
			m_leaderboard.RemoveRange(numEntries, m_leaderboard.Count - numEntries);
		}

		// Store total amount of players
		m_totalPlayers = _data["n"].AsInt;
	}

	/// <summary>
	/// Update the user's reward level for this event with new data from the server.
	/// </summary>
	/// <param name="_data">Data.</param>
	public void UpdateRewardLevelFromJson(SimpleJSON.JSONNode _data) {
		m_rewardLevel = _data["r"].AsInt;
		if (_data.ContainsKey("top")) {
			m_topContributor = _data["top"].AsBool;
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Set the state of the event.
	/// Nothing happens if given state is the same as current one.
	/// </summary>
	/// <param name="_newState">New event state.</param>
	private void SetState(State _newState) {
		// Ignore if same state
		if(_newState == m_state) return;

		// Store new state
		m_state = _newState;
	}

	/// <summary>
	/// Infer event's state based on current timestamp.
	/// </summary>
	public void UpdateState() {
		if ( m_state != State.REWARD_COLLECTED ){
			// Use server time to prevent cheating!
			SetState(GetStateForTimestamp(GameServerManager.SharedInstance.GetEstimatedServerTime()));
		}
	}

	/// <summary>
	/// Given a specific timestamp, figure out to which state it corresponds based
	/// on event deadlines.
	/// </summary>
	/// <returns>The state of the event at the given timestamp.</returns>
	/// <param name="_timestamp">Timestamp to be checked, UTC.</param>
	private State GetStateForTimestamp(DateTime _timestamp) {
		// Finished?
		if(_timestamp >= m_endTimestamp) return State.FINISHED;

		// Active?
		if(_timestamp >= m_startTimestamp) return State.ACTIVE;

		// Teasing?
		if(_timestamp < m_startTimestamp) return State.TEASING;

		// Not even teasing
		return State.INIT;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}