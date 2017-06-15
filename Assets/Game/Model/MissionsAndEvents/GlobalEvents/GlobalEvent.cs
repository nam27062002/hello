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

	[Serializable]
	public class Reward {
		public string type = "";
		public float amount = 0f;
	};

	[Serializable]
	public class Step {
		public float targetValue = 0f;
		public Reward reward = null;
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	private string m_id = "";
	public string id {
		get { return m_id; }
	}

	private DefinitionNode m_typeDef = null;
	public DefinitionNode typeDef {
		get { return m_typeDef; }
	}

	// Objective - used to track the player's contribution to this event PER GAME
	private GlobalEventObjective m_objective = null;
	public GlobalEventObjective objective { 
		get { return m_objective; }
	}

	// Milestones and global progress
	private List<Step> m_steps = new List<Step>();	// Sorted from lower to higher step
	public List<Step> steps { 
		get { return m_steps; }
	}

	private float m_currentValue = 0f;
	public float currentValue {
		get { return m_currentValue; }
	}

	public float progress {
		get { return Mathf.Clamp01(m_currentValue/m_steps.Last().targetValue); }
	}

	// Rewards
	private float m_topContributorsPercentage = 0.1f;
	private Reward m_topContributorsReward = null;

	// Timestamps
	private DateTime m_teasingStartTimestamp = new DateTime();
	private DateTime m_startTimestamp = new DateTime();
	private DateTime m_endTimestamp = new DateTime();

	public TimeSpan remainingTime {	// Dynamic, depending on current state
		get { 
			switch(m_state) {
				case State.TEASING:	return m_startTimestamp - m_teasingStartTimestamp;	break;
				case State.ACTIVE:	return m_endTimestamp - m_startTimestamp;			break;
				default:			return new TimeSpan();								break;
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
	/// Load event data from a JSON.
	/// </summary>
	/// <param name="_data">JSON data.</param>
	public void FromJson(SimpleJSON.JSONClass _data) {

	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}