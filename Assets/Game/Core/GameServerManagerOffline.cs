// GameServerOffline.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

using FGOL.Server;

using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Game server implementation for debugging server calls offline.
/// Inherits from GameServerManagerCalety, so if a method is not overriden, the default
/// behaviour is executed.
/// </summary>
public class GameServerManagerOffline : GameServerManagerCalety {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal vars
	private DateTime m_initTimestamp = new DateTime();

	// Simulate global event progression
	private Dictionary<int, float> m_eventValues = new Dictionary<int, float>();
	private Dictionary<int, List<GlobalEventUserData>> m_eventLeaderboards = new Dictionary<int, List<GlobalEventUserData>>();

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Simple method to delay an action a few seconds (simulating a server response delay).
	/// </summary>
	/// <param name="_call">Action to be called.</param>
	/// <param name="_delay">Delay. If not defined, a random value from the range defined in DebugSettings will be used.</param>
	private void DelayedCall(Action _call, float _delay = -1f) {
		// If delay is not defined, get a random delay from the debug settings
		if(_delay < 0f) {
			_delay = DebugSettings.serverDelayRange.GetRandom();
		}

		// [AOC] Since this is a simple class and can't launch coroutines, use auxiliar manager to do so
		UbiBCN.CoroutineManager.DelayedCall(_call, _delay);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void Ping(ServerCallback _callback) {
		// No response
		DelayedCall(() => _callback(null, null));
	}

	/// <summary>
	/// 
	/// </summary>
	protected override void ExtendedConfigure() {
		// Call parent
		base.ExtendedConfigure();

		// Some extra initialization
		m_initTimestamp = DateTime.UtcNow;
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/*	// [AOC] Enable for testing
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void GetPersistence(ServerCallback _callback) {
		// Either load current local persistence of a fake json from disk
		ServerResponse res = new ServerResponse();
	#if false
		// Current local persistence
		res["response"] = PersistenceManager.LoadToObject().ToString();
	#else
		// Custom test persistence
		SimpleJSON.JSONClass persistenceData = PersistenceManager.LoadToObject("debug_offline_cloud_savegame");
		if(persistenceData == null) {
			// Test persistence not found, load default profile
			persistenceData = PersistenceManager.GetDefaultDataFromProfile();
		}
		res["response"] = persistenceData.ToString();
	#endif

		// Simulate server delay
		DelayedCall(() => _callback(null, res));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void SetPersistence(string _persistence, ServerCallback _callback) {
		// Doesn't need response
		DelayedCall(() => _callback(null, null));
	}
	*/

	//------------------------------------------------------------------------//
	// GLOBAL EVENTS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the current global event for this user from the server.
	/// Current global event can be a past event with pending rewards, an active event,
	/// a future event or no event at all.
	/// </summary>
	/// <param name="_callback">Callback action.</param>
	override public void GlobalEvent_GetCurrent(ServerCallback _callback) {
		// Create return dictionary
		ServerResponse res = new ServerResponse();

		// Check debug settings
		CPGlobalEventsTest.EventStateTest targetState = CPGlobalEventsTest.eventState;

		// Simulate no event?
		if(targetState == CPGlobalEventsTest.EventStateTest.NO_EVENT) {
			res["response"] = null;
		} else {
			// ID
			SimpleJSON.JSONClass eventData = new SimpleJSON.JSONClass();
			eventData.Add("id", 0);
			eventData.Add("name", "test_event_0");

			// Goal definition
			SimpleJSON.JSONClass goalData = new SimpleJSON.JSONClass();
			goalData.Add("type", "kill");
			goalData.Add("params", "Canary01_Flock;Canary02_Flock;Canary03_Flock;Canary04_Flock");
			goalData.Add("icon", "icon_canary");
			goalData.Add("tidDesc", "TID_EVENT_EAT_BIRDS");
			goalData.Add("amount", 1000f.ToString(JSON_FORMAT));
			eventData.Add("goal", goalData);

			// Timestamps
			// By tuning the start timestamp in relation to the current time we can simulate the different states of the event
			DateTime startTimestamp = new DateTime();
			switch(targetState) {
				case CPGlobalEventsTest.EventStateTest.DEFAULT:
				case CPGlobalEventsTest.EventStateTest.ACTIVE: {
					startTimestamp = m_initTimestamp.AddDays(-3.3);		// Started 3.3 days ago
				} break;

				case CPGlobalEventsTest.EventStateTest.TEASING: {
					startTimestamp = m_initTimestamp.AddDays(1.5);		// Starting in 1.5 days
				} break;

				case CPGlobalEventsTest.EventStateTest.FINISHED: {
					startTimestamp = m_initTimestamp.AddDays(-85);		// Started 85 days ago
				} break;
			}

			eventData.Add("startTimestamp", startTimestamp.ToString(JSON_FORMAT));
			eventData.Add("teaserTimestamp", startTimestamp.AddDays(-2).ToString(JSON_FORMAT));	// 2 days before start time
			eventData.Add("endTimestamp", startTimestamp.AddDays(7).ToString(JSON_FORMAT));		// Lasts 7 days

			// Rewards
			SimpleJSON.JSONArray rewardsArray = new SimpleJSON.JSONArray();
			rewardsArray.Add(CreateEventRewardData(0.2f, GlobalEvent.Reward.Type.SC, "", 500));
			rewardsArray.Add(CreateEventRewardData(0.5f, GlobalEvent.Reward.Type.SC, "", 1000));
			rewardsArray.Add(CreateEventRewardData(0.75f, GlobalEvent.Reward.Type.PC, "", 100));
			rewardsArray.Add(CreateEventRewardData(1f, GlobalEvent.Reward.Type.EGG, "egg_premium", -1));
			eventData.Add("rewards", rewardsArray);

			// Top percentile reward
			eventData.Add("topReward", CreateEventRewardData(0.1f, GlobalEvent.Reward.Type.PET, "pet_24", -1));

			// Store response
			res["response"] = eventData.ToString();
		}

		// Simulate server delay
		DelayedCall(() => _callback(null, res));
	}

	/// <summary>
	/// Get the current value and (optionally) the leaderboard for a specific event.
	/// </summary>
	/// <param name="_eventID">The identifier of the event whose state we want.</param>
	/// <param name="_getLeaderboard">Whether to retrieve the leaderboard as well or not (top 100 + player).</param>
	/// <param name="_callback">Callback action.</param>
	override public void GlobalEvent_GetState(int _eventID, bool _getLeaderboard, ServerCallback _callback) {
		// Create return dictionary
		ServerResponse res = new ServerResponse();

		// Create json
		SimpleJSON.JSONClass eventData = new SimpleJSON.JSONClass();

		// Current value
		// Use a local dictionary to simulate events values progression
		float currentValue = 0f;
		if(!m_eventValues.TryGetValue(_eventID, out currentValue)) {
			currentValue = 600f;	// Default initial value
			m_eventValues[_eventID] = currentValue;
		}
		eventData.Add("currentValue", currentValue.ToString(JSON_FORMAT));

		// Current player data
		GlobalEventUserData playerEventData = null;
		if(!UsersManager.currentUser.globalEvents.TryGetValue(_eventID, out playerEventData)) {
			// User has never contributed to this event, create a new, empty, player event data
			playerEventData = new GlobalEventUserData(_eventID, UsersManager.currentUser.userId, 0f, -1);
		}

		// If there is no leaderboard for this event, create one with random values
		List<GlobalEventUserData> leaderboard = null;
		if(!m_eventLeaderboards.TryGetValue(_eventID, out leaderboard)) {
			// Create an array of max 100 random users
			float remainingScore = currentValue;
			Range scorePerPlayerRange = new Range(remainingScore/100f, remainingScore/10f);	// min 10 players, max 100
			leaderboard = new List<GlobalEventUserData>(100);
			while(remainingScore > 0f) {
				// Compute new score for this user
				float score = Mathf.Min(remainingScore, scorePerPlayerRange.GetRandom());
				remainingScore -= score;

				// Create user data
				GlobalEventUserData leaderboardEntry = new GlobalEventUserData(
					_eventID,
					"dummy_player_" + leaderboard.Count,
					score,
					-1	// Position will be initialized afterwards when the leaderboard is sorted
				);

				// Add it to the leaderboard
				leaderboard.Add(leaderboardEntry);
			}

			// Store leaderboard to fake cache data
			m_eventLeaderboards[_eventID] = leaderboard;
		}

		// Sort leaderboard by score
		leaderboard.Sort((_a, _b) => -(_a.score.CompareTo(_b.score)));	// Gotta love delta expressions. Using float's CompareTo directly. Reverse sign since we want to sort from bigger to lower.

		// If player is not on the leaderboard but should be, add it!
		// We should probably remove the last one, but since it's test code, we don't care enough
		int playerIdx = leaderboard.FindIndex((_data) => _data.userID == playerEventData.userID);
		float minScore = leaderboard.Last().score;
		if(playerIdx < 0 && playerEventData.score > minScore && playerEventData.score > 0f) {
			leaderboard.Add(new GlobalEventUserData(playerEventData));	// Create a copy of the data
		} else if(playerIdx >= 0 && leaderboard.Count >= 100 && playerEventData.score < minScore) {
			leaderboard.RemoveAt(playerIdx);	// Remove from the leaderboard
		}

		// Sort leaderboard again with the new data
		leaderboard.Sort((_a, _b) => -(_a.score.CompareTo(_b.score)));	// Gotta love delta expressions. Using float's CompareTo directly. Reverse sign since we want to sort from bigger to lower.

		// Update position for each data
		for(int i = 0; i < leaderboard.Count; i++) {
			leaderboard[i].position = i;

			// If it's the player, update position as well
			if(leaderboard[i].userID == playerEventData.userID) {
				playerEventData.position = i;
			}
		}

		// Add player data to the json
		eventData.Add("playerData", playerEventData.Save(true));

		// Add leaderboard data to the json (if requested)
		if(_getLeaderboard) {
			// Create a json array with every entry in the leaderboard
			SimpleJSON.JSONArray leaderboardData = new SimpleJSON.JSONArray();
			for(int i = 0; i < leaderboard.Count; i++) {
				leaderboardData.Add(leaderboard[i].Save(false));
			}
			eventData.Add("leaderboard", leaderboardData);
		}

		// Store response and simulate server delay
		res["response"] = eventData.ToString();
		DelayedCall(() => _callback(null, res));
	}
	
	/// <summary>
	/// Register a score to a target event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_score">The score to be registered.</param>
	/// <param name="_callback">Callback action.</param>
	override public void GlobalEvent_RegisterScore(int _eventID, float _score, ServerCallback _callback) {
		// Increase event's current value
		if(!m_eventValues.ContainsKey(_eventID)) {
			// Invalid event ID, simulate validation error
			DelayedCall(() => _callback(new Error("User can't register score into the event with id " + _eventID, ErrorCodes.ValidationError), null));
		} else {
			// Increase event's total score and simulate server delay
			m_eventValues[_eventID] += _score;
			DelayedCall(() => _callback(null, null));
		}
	}

	/// <summary>
	/// Reward the player for his contribution to an event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action. Given rewards?</param>
	override public void GlobalEvent_ApplyRewards(int _eventID, ServerCallback _callback) {
		// [AOC] TODO!!
		DelayedCall(() => _callback(null, null));
	}
	
	/// <summary>
	/// Auxiliar method to create a JSON object simulating an event reward data.
	/// </summary>
	/// <returns>The event reward data json.</returns>
	/// <param name="_percentage">Target percentage. For top percentile reward, percentile of the leaderboard to whom the reward is given.</param>
	/// <param name="_type">Type of reward to be given.</param>
	/// <param name="_sku">Reward sku (optional).</param>
	/// <param name="_amount">Rewarded amount (optional).</param>
	private SimpleJSON.JSONClass CreateEventRewardData(float _percentage, GlobalEvent.Reward.Type _type, string _sku, long _amount) {
		SimpleJSON.JSONClass reward = new SimpleJSON.JSONClass();
		reward.Add("targetPercentage", _percentage.ToString(JSON_FORMAT));
		reward.Add("type", GlobalEvent.Reward.TypeToString(_type));
		if(!string.IsNullOrEmpty(_sku)) reward.Add("sku", _sku);
		if(_amount > 0f) reward.Add("amount", _amount.ToString(JSON_FORMAT));
		return reward;
	}
}