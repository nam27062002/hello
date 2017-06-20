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
public class GlobalEvent {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum State {
		INIT,
		TEASING,
		ACTIVE,
		FINISHED,

		COUNT
	};

	/// <summary>
	/// Reward item and the percentage required to achieve it.
	/// </summary>
	[Serializable]
	public class Reward {
		public DefinitionNode def = null;
		public float amount = 0f;

		public float targetPercentage = 0f;
		public float targetAmount = 0f;		// Should match target percentage

		/// <summary>
		/// Constructor from json data.
		/// </summary>
		/// <param name="_data">Data to be parsed.</param>
		public Reward(SimpleJSON.JSONNode _data) {
			// Reward data
			def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.GLOBAL_EVENT_REWARDS, _data["sku"]);
			amount = _data["amount"].AsFloat;

			// Init target percentage
			// Target amount should be initialized from outside, knowing the global target
			targetPercentage = _data["targetPercentage"].AsFloat;
		}
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	private string m_id = "";
	public string id {
		get { return m_id; }
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

	private float m_targetValue = 0f;
	public float targetValue {
		get { return m_targetValue; }
	}

	public float progress {
		get { return Mathf.Clamp01(m_currentValue/m_targetValue); }
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
	/// Set the state of the event.
	/// </summary>
	/// <param name="_newState">New event state.</param>
	public void SetState(State _newState) {
		// Store new state
		m_state = _newState;
	}

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

	/// <summary>
	/// Initialize event from a JSON definition.
	/// </summary>
	/// <param name="_data">JSON data.</param>
	public void InitFromJson(SimpleJSON.JSONClass _data) {
		// Event ID
		m_id = _data["eventId"];

		// Target value
		m_targetValue = _data["targetValue"].AsFloat;

		// Event objective
		// Ditch any existing objective, we don't care
		DefinitionNode objectiveDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.GLOBAL_EVENT_OBJECTIVES, _data["goal"]);
		m_objective = new GlobalEventObjective(this, objectiveDef, m_targetValue);	// Target value is actually pointless in global events, but we may use it as a cap to detect cheaters

		// Timestamps
		m_teasingStartTimestamp = DateTime.Parse(_data["teaserTimestamp"], GameServerManager.JSON_FORMAT);
		m_startTimestamp = DateTime.Parse(_data["startTimestamp"], GameServerManager.JSON_FORMAT);
		m_endTimestamp = DateTime.Parse(_data["endTimestamp"], GameServerManager.JSON_FORMAT);

		// Infer event's state from timestamps
		// Use server time to prevent cheating!
		m_state = GetStateForTimestamp(GameServerManager.SharedInstance.GetEstimatedServerTime());

		// Rewards
		m_rewards.Clear();
		SimpleJSON.JSONArray rewardsDataArray = _data["rewards"].AsArray;
		for(int i = 0; i < rewardsDataArray.Count; ++i) {
			m_rewards.Add(new Reward(rewardsDataArray[i]));
			m_rewards[i].targetAmount = m_rewards[i].targetPercentage * m_targetValue;
		}

		// Make sure steps are sorted by percentile
		m_rewards.Sort((Reward _a, Reward _b) => { return _a.targetPercentage.CompareTo(_b.targetPercentage); });

		// Special reward for the top X% contributors
		m_topContributorsReward = new Reward(_data["topReward"]);

		// Reset state vars
		m_currentValue = 0f;
		m_topContributors.Clear();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
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