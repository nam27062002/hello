// Chest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/10/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
/// Single Chest logic object.
/// </summary>
[Serializable]
public class Chest {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum State {
		INIT,			// Init state
		NOT_COLLECTED,	// Chest spawned but not yet collected
		PENDING_REWARD,	// Chest has been found, pending reward
		COLLECTED,		// Chest reward has been collected
		SHOWROOM		// Chest for display only, state is not relevant
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	private string m_spawnPointID = "";
	public string spawnPointID {
		get { return m_spawnPointID; }
		set { m_spawnPointID = value; }
	}

	// Logic
	private State m_state = State.INIT;
	public State state { 
		get { return m_state; }
	}

	public bool collected {
		get { return m_state == State.PENDING_REWARD || m_state == State.COLLECTED; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// Private, use factory methods to create new Chests.
	/// </summary>
	public Chest() {
		// Nothing to do
	}

	/// <summary>
	/// Change the Chest's state.
	/// Should only be called from the ChestManager.
	/// </summary>
	/// <param name="_newState">The state to go to.</param>
	public void ChangeState(State _newState) {
		// [AOC] TODO!! Check state change restrictions.
		// Perform actions before leaving a state
		switch(m_state) {
			default: {
				// Nothing to do for now
			} break;
		}

		// Change state
		State oldState = m_state;
		m_state = _newState;

		// Perform actions upon entering a new state
		switch(m_state) {
			default: {
				// Nothing to do for now
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load state from a json object.
	/// </summary>
	/// <param name="_data">The json loaded from persistence.</param>
	public void Load(SimpleJSON.JSONNode _data) {
		// State
		m_state = (State)_data["state"].AsInt;

		// Spawn point ID
		m_spawnPointID = _data["spawnPointID"];
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() {
		// Create new object, initialize and return it
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// State
		data.Add("state", ((int)m_state).ToString());

		// Spawn point ID
		data.Add("spawnPointID", m_spawnPointID);

		// Done!
		return data;
	}
}