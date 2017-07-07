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
public class GlobalEventUserData {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Event data
	public int eventID = -1;			 // Event id
	public string userID = "";			 // User id
	public float score = 0f;			 // Contribution of the player to the event
	public int position = -1;			 // -1 if he hasn't participated to the event
	public bool rewardCollected = false; // User collected the reward 

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public GlobalEventUserData() {
		Init(-1, "", 0f, -1, false);
	}

	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	public GlobalEventUserData(int _eventID, string _userID, float _score, int _position) {
		Init(_eventID, _userID, _score, _position, false);
	}

	/// <summary>
	/// Copy constructor.
	/// </summary>
	/// <param name="_data">Source data object.</param>
	public GlobalEventUserData(GlobalEventUserData _data) {
		Init(_data.eventID, _data.userID, _data.score, _data.position, _data.rewardCollected);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~GlobalEventUserData() {

	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Internal initializer.
	/// </summary>
	/// <param name="_eventId">Event identifier.</param>
	/// <param name="_userId">User identifier.</param>
	/// <param name="_score">Score.</param>
	/// <param name="_position">Position.</param>
	private void Init(int _eventID, string _userID, float _score, int _position, bool _rewardCollected) {
		eventID = _eventID;
		userID = _userID;
		score = _score;
		position = _position;
		rewardCollected = _rewardCollected;
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SimpleJSON.JSONNode _data) {
		// Easy
		if(_data.ContainsKey("eventId")) eventID = _data["eventId"].AsInt;	// Event ID is optional
		userID = _data["userId"];
		score = _data["score"].AsFloat;
		position = _data["position"].AsInt;
		rewardCollected = _data["rewardCollected"].AsBool;
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	/// <param name="_includeEventId">Whether to save the event ID as well.</param>
	public SimpleJSON.JSONNode Save(bool _includeEventID) {
		// Create a new json object for this event
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		if(_includeEventID) data.Add("eventId", eventID.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		data.Add("userId", userID);
		data.Add("score", score.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		data.Add("position", position.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		data.Add("rewardCollected", rewardCollected);
		return data;
	}
}