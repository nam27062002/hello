// GlobalEventManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/03/2017.
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
/// Global singleton manager for global events.
/// </summary>
public class GlobalEventManager : Singleton<GlobalEventManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Refresh intervals: Seconds, if current event data is retrieved and this interval has expired, we will request new data from the server
	private const float STATE_REFRESH_INTERVAL = 1f * 60f;
	private const float LEADERBOARD_REFRESH_INTERVAL = 10f * 60f;

	// Error codes for event interaction
	public enum ErrorCode {
		NONE = 0,

		OFFLINE,
		NOT_INITIALIZED,
		NOT_LOGGED_IN,
		NO_VALID_EVENT,
		EVENT_NOT_ACTIVE
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Current user data
	private UserProfile m_user = null;
	public static UserProfile user {
		get { return instance.m_user; }
	}

	// Current event
	// Will always contain the data to the last relevant event to the user
	// That could be an event whose rewards are pending, a future event or the active event
	// Can also be null if there is no event currently, or if event data could not be retrieved from the server
	private GlobalEvent m_currentEvent = null;
	public static GlobalEvent currentEvent {
		get {
			// If data refresh interval has expired, request new data to the server
			// Do it in background, return cached data
			if(instance.m_currentEvent != null) {
				DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
				if(instance.m_stateCheckTimestamp < serverTime) {
					// Request event state
					// Request leaderboard as well?
					RequestCurrentEventState(instance.m_leaderboardCheckTimestamp < serverTime);
				}
			}
			return instance.m_currentEvent;
		}
	}

	// Internal
	private DateTime m_stateCheckTimestamp = new DateTime();
	private DateTime m_leaderboardCheckTimestamp = new DateTime();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// SINGLETON METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launch an async request to the server asking for current event data.
	/// This will check if there are actually new events for the current user or
	/// if there is no event at all.
	/// Listen to the GameEvents.GLOBAL_EVENT_DATA_UPDATED event to know when the
	/// new data have been received.
	/// In the case a valid event is returned, its state will automatically be retrieved
	/// as well in another async call, so be aware that the event might be triggered twice.
	/// </summary>
	public static void RequestCurrentEventData() {
		// First we have to resolve all the stored events (profile)
		Dictionary<int, GlobalEventUserData> storedEvents = user.globalEvents;
		int currentEventId = -1;

		if (storedEvents.Count > 0) {
			List<int> deleteEvents = new List<int>();

			foreach (KeyValuePair<int, GlobalEventUserData> pair in storedEvents) {
				if (pair.Value.rewardCollected) {
					deleteEvents.Add(pair.Key);
				} else {
					currentEventId = pair.Key;
				}
			}

			for (int i = 0; i < deleteEvents.Count; i++) {
				storedEvents.Remove(deleteEvents[i]);
			}
		}

		if (currentEventId >= 0) {
			// We've found an event stored, lets get its data
			if (m_currentEvent == null) {
				m_currentEvent = new GlobalEvent();
			}

			GameServerManager.SharedInstance.GlobalEvent_GetState(currentEventId, true, instance.OnEventStateResponse);
		} else {
			// Nothing stored so lets ask server for any event
			GameServerManager.SharedInstance.GlobalEvent_GetCurrent(instance.OnCurrentEventResponse);
		}
	}

	/// <summary>
	/// Launch an async request to the server asking for current event state.
	/// The state 
	/// Listen to the GameEvents.GLOBAL_EVENT_DATA_UPDATED event to know when the
	/// new data have been received.
	/// </summary>
	/// <param name="_getLeaderboard">Whether to get the latest leaderboard for the current event or not. Take in consideration that the leaderboard is a lot of data and is cached, so there's no need to update it every time.</param>
	public static void RequestCurrentEventState(bool _getLeaderboard) {
		// We need a valid event
		if(instance.m_currentEvent == null) return;

		// Just do it
		GameServerManager.SharedInstance.GlobalEvent_GetState(instance.m_currentEvent.id, true, instance.OnEventStateResponse);
	}

	/// <summary>
	/// Contribute to current event!
	/// Contribution amount will be taken from current event tracker.
	/// Listen to the GameEvents.GLOBAL_EVENT_SCORE_REGISTERED event to know when the
	/// new data have been received.
	/// </summary>
	/// <returns>Whether the contribution has been applied or not and why.</returns>
	/// <param name="_bonusDragonMultiplier">Apply bonus dragon multiplier?</param>
	/// <param name="_keysMultiplier">Apply keys multiplier? Transaction must have been performed already.</param>
	public static ErrorCode Contribute(float _bonusDragonMultiplier, float _keysMultiplier) {
		// Can the user contribute to the current event?
		ErrorCode err = CanContribute();
		if(err != ErrorCode.NONE) return err;

		// Get contribution amount and apply multipliers
		float contribution = instance.m_currentEvent.objective.currentValue;
		contribution *= _bonusDragonMultiplier;
		contribution *= _keysMultiplier;

		// Requets to the server!
		GameServerManager.SharedInstance.GlobalEvent_RegisterScore(
			instance.m_currentEvent.id, 
			contribution,
			(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) => {
				// If there was no error, update local cache
				if(_error == null) {
					// Add to event's total contribution!
					instance.m_currentEvent.AddContribution(contribution);

					// Add to current user individual contribution in this event
					user.GetGlobalEventData(instance.m_currentEvent.id).score += contribution;

					// [AOC] TODO!! Update leaderboard?

				}

				// Notify game that server response was received
				Messenger.Broadcast<bool>(GameEvents.GLOBAL_EVENT_SCORE_REGISTERED, _error == null);
			}
		);

		// Everything went well!
		return ErrorCode.NONE;
	}

	/// <summary>
	/// Can the user contribute to the current event?
	/// Several conditions will be checked, including internet connectivity, proper setup, current event state, etc.
	/// </summary>
	/// <returns>Whether the user can contribute to the current event and why.</returns>
	public static ErrorCode CanContribute() {
		// Check for debugging overrides
		// We must be online!
		if(CPGlobalEventsTest.networkCheck && Application.internetReachability == NetworkReachability.NotReachable) return ErrorCode.OFFLINE;

		// Manager must be properly setup
		if(!IsReady()) return ErrorCode.NOT_INITIALIZED;

		// User must be logged in!
		// [AOC] CHECK HOW TO DO IT!!
		//if(CPGlobalEventsTest.loginCheck && !SocialManager.Instance.IsLoggedIn(SocialFacade.Network.Default)) return ErrorCode.NOT_LOGGED_IN;
		if(CPGlobalEventsTest.loginCheck && !GameSessionManager.SharedInstance.IsLogged()) return ErrorCode.NOT_LOGGED_IN;

		// We must have a valid event
		if(instance.m_currentEvent == null) return ErrorCode.NO_VALID_EVENT;

		// Event must be active!
		if(!instance.m_currentEvent.isActive) return ErrorCode.EVENT_NOT_ACTIVE;

		// All checks passed!
		return ErrorCode.NONE;
	}

	/// <summary>
	/// Clear any stored data for current event.
	/// </summary>
	public static void ClearCurrentEvent() {
		// This should be enough
		instance.m_currentEvent = null;
	}

	/// <summary>
	/// Tells the manager with which user data he should work.
	/// </summary>
	/// <param name="_user">Profile to work with.</param>
	public static void SetupUser(UserProfile _user) {
		// If it's a new user, request current event to the server for this user
		if(instance.m_user != _user) {
			instance.m_user = _user;
			RequestCurrentEventData();
		}
	}

	/// <summary>
	/// Has a user been loaded into the manager?
	/// </summary>
	public static bool IsReady() {
		return instance.m_user != null;
	}

	//------------------------------------------------------------------------//
	// SERVER COMMUNICATION METHODS											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Server callback for the current event request.
	/// </summary>
	/// <param name="_error">Error.</param>
	/// <param name="_response">Response.</param>
	private void OnCurrentEventResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// Error?
		if(_error != null) {
			// [AOC] Do something or just ignore?
			// Probably store somewhere that there was an error so retry timer is reset or smth
			return;
		}

		// Did the server gave us an event?
		if(_response != null && _response["response"] != null) {
			// Yes! If no event is created, create one
			if(m_currentEvent == null) {
				m_currentEvent = new GlobalEvent();
			}

			// If the ID is different from the stored event, load the new event's data!
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
			Debug.Log("<color=magenta>Received current event data:</color>\n" + responseJson.ToString(4));
			if(m_currentEvent.id != responseJson["id"]) {
				m_currentEvent.InitFromJson(responseJson);
			}
		} else {
			// No! Clear current event (if any)
			if(m_currentEvent != null) ClearCurrentEvent();
		}

		// Notify game that we have new data concerning the current event
		Messenger.Broadcast(GameEvents.GLOBAL_EVENT_DATA_UPDATED);

		// If we have a valid event, request its state too
		if(m_currentEvent != null) {
			// Get current event's state
			RequestCurrentEventState(true);
		}
	}

	/// <summary>
	/// Server callback for the event state request.
	/// </summary>
	/// <param name="_error">Error.</param>
	/// <param name="_response">Response.</param>
	private void OnEventStateResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// Error?
		if(_error != null) {
			// [AOC] Do something or just ignore?
			// Probably store somewhere that there was an error so retry timer is reset or smth
			return;
		}

		// Ignore if we don't have a valid event!
		if(m_currentEvent == null) return;

		// Did the server gave us a response?
		if(_response != null && _response["response"] != null) {
			// Validate the event ID?
			// For now let's just assume the given state is for the current event

			// The event will parse the response json by itself
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
			Debug.Log("<color=purple>Received current event state:</color>\n" + responseJson.ToString(4));
			m_currentEvent.UpdateFromJson(responseJson);

			// Player data
			GlobalEventUserData currentEventUserData = user.GetGlobalEventData(m_currentEvent.id);
			currentEventUserData.Load(responseJson["playerData"]);

			// Update timestamps
			DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
			instance.m_stateCheckTimestamp = serverTime.AddSeconds(STATE_REFRESH_INTERVAL);
			if(responseJson.ContainsKey("leaderboard")) {
				instance.m_leaderboardCheckTimestamp = serverTime.AddSeconds(LEADERBOARD_REFRESH_INTERVAL);
			}

			// Notify game that we have new data concerning the current event
			Messenger.Broadcast(GameEvents.GLOBAL_EVENT_DATA_UPDATED);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}