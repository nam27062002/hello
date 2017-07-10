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
	private List<Reward> m_rewards = new List<Reward>();	// Sorted from lower to higher step
	public List<Reward> rewards { 
		get { return m_rewards; }
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
	private Reward m_topContributorsReward = null;		// Percentage is differently used here!

	// Timestamps
	private DateTime m_teasingStartTimestamp = new DateTime();
	private DateTime m_startTimestamp = new DateTime();
	private DateTime m_endTimestamp = new DateTime();

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

	public bool isActive {
		get { return m_state == State.ACTIVE; }
	}

	public bool isRewarAvailable {
		get { return m_state == State.FINISHED; }
	}

	// Contribution
	private List<GlobalEventUserData> m_topContributors = new List<GlobalEventUserData>();	// Sorted
	public List<GlobalEventUserData> topContributors {
		get { return m_topContributors; }
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
	/// </summary>
	/// <param name="_value">Value to be added.</param>
	public void AddContribution(float _value) {
		// Ignore if event is not active
		if(!isActive) return;

		// Update current value
		m_currentValue += _value;
	}

	public void ApplyReward() {
		if (m_state == State.FINISHED) {
			// TODO

			// compute reward and store it

			UsersManager.currentUser.GetGlobalEventData(m_id).rewardCollected = true;
			m_state = State.REWARD_COLLECTED;
		}
	}

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
		m_rewards.Clear();
		SimpleJSON.JSONArray rewardsDataArray = _data["rewards"].AsArray;
		for(int i = 0; i < rewardsDataArray.Count; ++i) {
			m_rewards.Add(new Reward(rewardsDataArray[i]));
			m_rewards[i].targetAmount = m_rewards[i].targetPercentage * targetValue;
		}

		// Make sure steps are sorted by percentile
		m_rewards.Sort((Reward _a, Reward _b) => { return _a.targetPercentage.CompareTo(_b.targetPercentage); });

		// Special reward for the top X% contributors
		m_topContributorsReward = new Reward(_data["topReward"]);

		// Reset state vars
		m_currentValue = 0f;
		m_topContributors.Clear();
	}

	/// <summary>
	/// Update the event's current state from a JSON object.
	/// Corresponds to the GetState server call.
	/// </summary>
	/// <param name="_data">Data.</param>
	public void UpdateFromJson(SimpleJSON.JSONNode _data) {
		// Current value
		m_currentValue = _data["currentValue"].AsFloat;

		// Leaderboard (optional)
		if(_data.ContainsKey("leaderboard")) {
			SimpleJSON.JSONArray leaderboardData = _data["leaderboard"].AsArray;
			int numEntries = leaderboardData.Count;
			for(int i = 0; i < numEntries; ++i) {
				// Reuse existing entries, create a new one if needed
				if(i >= m_topContributors.Count) {
					m_topContributors.Add(new GlobalEventUserData());
				}

				// Update leaderboard entry
				m_topContributors[i].Load(leaderboardData[i]);
			}

			// Remove unused entries from the leaderboard
			if(m_topContributors.Count > numEntries) {
				m_topContributors.RemoveRange(numEntries, m_topContributors.Count - numEntries);
			}
		}

		// Update event state (just in case, has nothing to do with given json)
		UpdateState();
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
	private void UpdateState() {
		// Use server time to prevent cheating!
		SetState(GetStateForTimestamp(GameServerManager.SharedInstance.GetEstimatedServerTime()));
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
		if(_timestamp >= m_teasingStartTimestamp) return State.TEASING;

		// Not even teasing
		return State.INIT;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}