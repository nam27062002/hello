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
	private const float STATE_REFRESH_INTERVAL = 5f * 60f;
	private const float LEADERBOARD_REFRESH_INTERVAL = 10f * 60f;
	private const float SERVER_REQUEST_TIMEOUT = 5f;	// Safeguard to avoid spamming the server with requests

	// Error codes for event interaction
	public enum ErrorCode {
		NONE = 0,

		OFFLINE,
		NOT_INITIALIZED,
		NOT_LOGGED_IN,
		NO_VALID_EVENT,
		EVENT_NOT_ACTIVE
	}

	public enum RequestType {
		EVENT_DATA,
		EVENT_STATE,
		EVENT_LEADERBOARD
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
			// [AOC] Disable for now, it's too difficult to control :s
			//		 Manually trigger requests when needed
			/*
			// If data refresh interval has expired, request new data to the server
			// Do it in background, return cached data
			if(instance.m_currentEvent != null) {
				DateTime now = serverTime;
				if(instance.m_stateCheckTimestamp < now) {
					// Request event state
					RequestCurrentEventState();

					// [AOC] Request leaderboard as well?
					if(instance.m_leaderboardCheckTimestamp < now) {
						// [AOC] Don't think so, leaderboard should be requested more carefully
						//RequestCurrentEventLeaderboard();
					}
				}
			}
			*/
			return instance.m_currentEvent;
		}
	}

	// Shortcuts
	private static DateTime serverTime {
		get { return GameServerManager.SharedInstance.GetEstimatedServerTime(); }
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

	public static void TMP_RequestCustomizer() {
		GameServerManager.SharedInstance.GlobalEvent_TMPCustomizer(instance.OnTMPCustomizerResponse);
	}


	private void OnTMPCustomizerResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		bool parsed = false;
		if(_response != null && _response["response"] != null) {
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
			if ( responseJson != null && responseJson.ContainsKey("liveEvents") ){
				SimpleJSON.JSONArray arr = responseJson["liveEvents"].AsArray;
				SimpleJSON.JSONClass liveEvent = arr[0].AsObject;
				int globalEventKey = liveEvent["code"].AsInt;
				if ( globalEventKey >= 0 ){
					GlobalEventUserData globalEventUserData = null;
					if ( user.globalEvents.ContainsKey(globalEventKey) ){
						globalEventUserData = user.globalEvents[globalEventKey];
					}else{
						globalEventUserData = new GlobalEventUserData();
						globalEventUserData.eventID = globalEventKey;
						user.globalEvents.Add(globalEventKey, globalEventUserData);
					}
					globalEventUserData.endTimestamp = liveEvent["end"].AsLong;
				}
				parsed = true;
			}
		}
		if (parsed)
			RequestCurrentEventData();
	}


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
			long minEndTimestamp = long.MaxValue;

			foreach (KeyValuePair<int, GlobalEventUserData> pair in storedEvents) {
				if (pair.Value.rewardCollected) {
					deleteEvents.Add(pair.Key);
				} else {
					if (pair.Value.endTimestamp < minEndTimestamp) {
						currentEventId = pair.Key;
						minEndTimestamp = pair.Value.endTimestamp;
					}
				}
			}

			for (int i = 0; i < deleteEvents.Count; i++) {
				storedEvents.Remove(deleteEvents[i]);
			}
		}

		if (currentEventId >= 0) {
			// We've found an event stored, lets get its data
			Debug.Log("<color=magenta>EVENT DATA</color>");
			GameServerManager.SharedInstance.GlobalEvent_GetEvent(currentEventId, instance.OnCurrentEventResponse);
		} else {
			ClearCurrentEvent();
			Messenger.Broadcast<RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, RequestType.EVENT_DATA);
			Messenger.Broadcast(GameEvents.GLOBAL_EVENT_DATA_UPDATED);
		}
	}

	/// <summary>
	/// Launch an async request to the server asking for current event state.
	/// The state 
	/// Listen to the GameEvents.GLOBAL_EVENT_DATA_UPDATED event to know when the
	/// new data have been received.
	/// </summary>
	public static void RequestCurrentEventState() {
		// We need a valid event
		if(instance.m_currentEvent == null) return;

		// Reset timer
		instance.m_stateCheckTimestamp = serverTime.AddSeconds(SERVER_REQUEST_TIMEOUT);

		// Just do it
		Debug.Log("<color=magenta>EVENT STATE</color>");
		GameServerManager.SharedInstance.GlobalEvent_GetState(instance.m_currentEvent.id, instance.OnEventStateResponse);
	}

	/// <summary>
	/// Requests the current event leaderboard.
	/// If not enough time has passed since the last update, cached data will be returned (unless force flag is active).
	/// </summary>
	/// <param name="_force">Whether to force the request or accept cached data.</param>
	public static void RequestCurrentEventLeaderboard(bool _force) {
		// We need a valid event
		if(instance.m_currentEvent == null) return;

		// If leaderboard refresh interval has expired, request new data to the server
		DateTime now = serverTime;
		if(instance.m_leaderboardCheckTimestamp < now || _force) {
			// Do it
			Debug.Log("<color=magenta>EVENT LEADERBOARD</color>");
			GameServerManager.SharedInstance.GlobalEvent_GetLeaderboard(instance.m_currentEvent.id, instance.OnEventLeaderboardResponse);
		} else {
			// Notify game that leaderboard data is ready
			Messenger.Broadcast<RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, RequestType.EVENT_LEADERBOARD);
			Messenger.Broadcast(GameEvents.GLOBAL_EVENT_LEADERBOARD_UPDATED);
		}
	}

	/// <summary>
	/// Requests the current event rewards.
	/// </summary>
	public static void RequestCurrentEventRewards() {
		if(instance.m_currentEvent == null) return;

		Debug.Log("<color=magenta>EVENT REWARDS</color>");
		GameServerManager.SharedInstance.GlobalEvent_GetRewards(instance.m_currentEvent.id, instance.OnEventRewardResponse);
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
		int contribution = (int)instance.m_currentEvent.objective.currentValue;
		contribution = (int)(_bonusDragonMultiplier * contribution);
		contribution = (int)(_keysMultiplier * contribution);

		// Requets to the server!
		Debug.Log("<color=magenta>REGISTER SCORE</color>");
		GameServerManager.SharedInstance.GlobalEvent_RegisterScore(
			instance.m_currentEvent.id, 
			contribution,
			(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) => {
				// If there was no error, update local cache
				if(_error == null && _response != null && _response.ContainsKey("response")) {
					if ( _response["response"] == null || _response["response"].ToString().ToLower().CompareTo("failed") != 0 ){
						// Add to event's total contribution!
						instance.m_currentEvent.AddContribution(contribution);

						// Add to current user individual contribution in this event
						GlobalEventUserData playerData = user.GetGlobalEventData(instance.m_currentEvent.id);
						playerData.score += contribution;

						// Check if player can join the leaderboard! (or update its position)
						instance.m_currentEvent.RefreshLeaderboardPosition(playerData);
					}
				}

				// Notify game that server response was received
				Debug.Log("<color=purple>REGISTER SCORE</color>");
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

	public static bool Connected() {
		bool ret = false;	
		if ((CPGlobalEventsTest.networkCheck && Application.internetReachability != NetworkReachability.NotReachable) &&
			(CPGlobalEventsTest.loginCheck   && GameSessionManager.SharedInstance.IsLogged())) {
			ret = true;
		}
		return ret;
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
		// if(instance.m_user != _user) 
		{
			instance.m_user = _user;
			if ( Connected() )
			{
				// RequestCurrentEventData();
				TMP_RequestCustomizer();
			}
            
            Messenger.AddListener<bool>(GameEvents.LOGGED, OnLoggedIn);            
        }
	}

	static void OnLoggedIn(bool _isLogged)
	{
        if (_isLogged) {
            // is this the right momment to request the data?
            // RequestCurrentEventData();
            TMP_RequestCustomizer();
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
			// If the ID is different from the stored event, load the new event's data!
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
			if ( responseJson != null )
			{
				// Yes! If no event is created, create one
				if(m_currentEvent == null) {
					m_currentEvent = new GlobalEvent();
				}
				Debug.Log("<color=purple>EVENT DATA</color>\n" + responseJson.ToString(4));
				if(m_currentEvent.id != responseJson["id"]) {
					m_currentEvent.InitFromJson(responseJson);
				}
			}else{
				if(m_currentEvent != null) ClearCurrentEvent();
			}
		} else {
			// No! Clear current event (if any)
			if(m_currentEvent != null) ClearCurrentEvent();
		}

		// Notify game that we have new data concerning the current event
		Messenger.Broadcast<RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, RequestType.EVENT_DATA);
		Messenger.Broadcast(GameEvents.GLOBAL_EVENT_DATA_UPDATED);

		// If we have a valid event, request its state too
		if(m_currentEvent != null) {
			// Get current event's state
			RequestCurrentEventState();
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
			if ( responseJson != null )
			{
				Debug.Log("<color=purple>EVENT STATE</color>\n" + responseJson.ToString(4));
				m_currentEvent.UpdateFromJson(responseJson);

				if (m_currentEvent.isRewardAvailable) {
					GlobalEventManager.RequestCurrentEventRewards();
				}

				GlobalEventUserData currentEventUserData = user.GetGlobalEventData(m_currentEvent.id);
				// Player data
				if ( responseJson.ContainsKey("playerData") ) {
					currentEventUserData.Load(responseJson["playerData"]);
				}else{
					currentEventUserData.Reset();
				}

				// Notify game that we have new data concerning the current event
				Messenger.Broadcast<RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, RequestType.EVENT_STATE);
				Messenger.Broadcast(GameEvents.GLOBAL_EVENT_STATE_UPDATED);
			}
		}
	}

	/// <summary>
	/// Server callback for the event leaderboard request.
	/// </summary>
	/// <param name="_error">Error.</param>
	/// <param name="_response">Response.</param>
	private void OnEventLeaderboardResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		/*
		 EXPECTED RESPONSE:
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
			Debug.Log("<color=purple>EVENT LEADERBOARD</color>\n" + responseJson.ToString(4));
			m_currentEvent.UpdateLeaderboardFromJson(responseJson);

			if (m_currentEvent.isRewardAvailable) {
				GlobalEventManager.RequestCurrentEventRewards();
			}

			GlobalEventUserData currentEventUserData = user.GetGlobalEventData(m_currentEvent.id);
			// Player data
			if ( responseJson.ContainsKey("u") ) {
				currentEventUserData.Load(responseJson["u"]);
			}else{
				currentEventUserData.Reset();
			}

			// Update timestamps
			DateTime now = serverTime;
			instance.m_stateCheckTimestamp = now.AddSeconds(STATE_REFRESH_INTERVAL);
			if(responseJson.ContainsKey("l")) {
				instance.m_leaderboardCheckTimestamp = now.AddSeconds(LEADERBOARD_REFRESH_INTERVAL);
			}

			// Notify game that we have new data concerning the current event
			Messenger.Broadcast<RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, RequestType.EVENT_LEADERBOARD);
			Messenger.Broadcast(GameEvents.GLOBAL_EVENT_LEADERBOARD_UPDATED);
		}
	}

	private void OnEventRewardResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// Error?
		if(_error != null) {
			// [AOC] Do something or just ignore?
			// Probably store somewhere that there was an error so retry timer is reset or smth
			return;
		}

		// Ignore if we don't have a valid event!
		if(m_currentEvent == null) return;

		// Did the server gave us an event?
		if(_response != null && _response["response"] != null) {
			// Validate the event ID?
			// For now let's just assume the given state is for the current event

			// The event will parse the response json by itself
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
			if ( responseJson != null ){
				Debug.Log("<color=purple>EVENT REWARD</color>\n" + responseJson.ToString(4));
				m_currentEvent.UpdateRewardLevelFromJson(responseJson);
			}
		}
	}
}