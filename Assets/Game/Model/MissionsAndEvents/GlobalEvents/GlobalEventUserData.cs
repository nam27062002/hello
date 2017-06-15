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
	public string eventId = "";	// Event id
	public string userId = "";	// User id
	public float score = 0f;	// Contribution of the player to the event
	public int position = -1;	// -1 if he hasn't participated to the event

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public GlobalEventUserData() {
		Init("", "", 0f, -1);
	}

	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	public GlobalEventUserData(string _eventId, string _userId, float _score, int _position) {
		Init(_eventId, _userId, _score, _position);
	}

	/// <summary>
	/// Copy constructor.
	/// </summary>
	/// <param name="_data">Source data object.</param>
	public GlobalEventUserData(GlobalEventUserData _data) {
		Init(_data.eventId, _data.userId, _data.score, _data.position);
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
	private void Init(string _eventId, string _userId, float _score, int _position) {
		eventId = _eventId;
		userId = _userId;
		score = _score;
		position = _position;
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
		eventId = _data["eventId"];
		userId = _data["userId"];
		score = _data["score"].AsFloat;
		position = _data["position"].AsInt;
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() {
		// Create a new json object for this event
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		data.Add("eventId", eventId);
		data.Add("userId", userId);
		data.Add("score", score.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		data.Add("position", position.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
		return data;
	}
}