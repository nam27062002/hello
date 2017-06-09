// GlobalEventUserData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/06/2017.
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
/// Data corresponding to a specific user and event. Use it for persistence.
/// </summary>
[Serializable]
public class GlobalEventsUserData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable]
	public class EventData {
		public string id = "";
		public float contribution = 0f;
		public bool rewardsGiven = false;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Events dictionary
	private Dictionary<string, EventData> m_events = new Dictionary<string, EventData>();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public GlobalEventsUserData() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~GlobalEventsUserData() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Clears current events data.
	/// </summary>
	public void Clear() {
		m_events.Clear();
	}

	//------------------------------------------------------------------------//
	// SHORTCUT OPERATORS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shortcut to the events dictionary.
	/// If the user doesn't have data of the requested event, new data will be created.
	/// </summary>
	/// <param name="_eventID">Target event.</param>
	public EventData this[string _eventID] {
		get { 
			// If the user doesn't have data of this event, create a new one
			EventData data = null;
			if(!m_events.TryGetValue(_eventID, out data)) {
				data = new EventData();
				data.id = _eventID;
				m_events[_eventID] = data;
			}
			return data;
		}
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SimpleJSON.JSONNode _data) {
		// [AOC] TODO!! When and how to remove old events?

		// Clear current events data
		Clear();

		// Parse json
		SimpleJSON.JSONArray eventsData = _data.AsArray;
		for(int i = 0; i < eventsData.Count; ++i) {
			// Create a new event with the given data
			EventData newEvent = new EventData();
			newEvent.id = eventsData[i]["id"];
			newEvent.contribution = eventsData[i]["contribution"].AsFloat;
			newEvent.rewardsGiven = eventsData[i]["rewardsGiven"].AsBool;

			// Store it to the events dictionary
			m_events[newEvent.id] = newEvent;
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() {
		// [AOC] TODO!! When and how to remove old events?

		// Iterate all events and store them into a JSON array
		SimpleJSON.JSONArray eventsData = new SimpleJSON.JSONArray();
		foreach(KeyValuePair<string, EventData> kvp in m_events) {
			// Create a new json object for this event
			SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
			data.Add("id", kvp.Value.id);
			data.Add("rewardsGiven", kvp.Value.rewardsGiven.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
			data.Add("contribution", kvp.Value.contribution.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));

			// Add to the array
			eventsData.Add(data);
		}
		return eventsData;
	}
}