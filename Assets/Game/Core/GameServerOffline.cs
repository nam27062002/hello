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
public class GameServerOffline : GameServerManagerCalety {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Simulate global event progression
	private Dictionary<string, float> m_eventsValues = new Dictionary<string, float>();

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
		CoroutineManager.DelayedCall(_call, _delay);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void Ping(Action<Error> _callback) {
		// No response
		DelayedCall(() => _callback(null));
	}

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/*	// [AOC] Enable for testing
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_callback">Action called once the server response arrives.</param>
	override public void GetPersistence(Action<Error, Dictionary<string, object>> _callback) {
		// Either load current local persistence of a fake json from disk
		Dictionary<string, object> res = new Dictionary<string, object>();
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
	override public void SetPersistence(string _persistence, Action<Error, Dictionary<string, object>> _callback) {
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
	override public void GlobalEvent_GetCurrent(Action<Error, Dictionary<string, object>> _callback) {
		// Create return dictionary
		Dictionary<string, object> res = new Dictionary<string, object>();

		// Type and target
		SimpleJSON.JSONClass eventData = new SimpleJSON.JSONClass();
		eventData.Add("id", "test_event_0");
		eventData.Add("goal", "eat_birds");
		eventData.Add("targetValue", 1000f.ToString(JSON_FORMAT));

		// Timestamps
		DateTime startTimestamp = DateTime.UtcNow.AddDays(-3.3);		// Started 3.3 days ago
		//DateTime startTimestamp = DateTime.UtcNow.AddDays(2.5);		// Starting in 2.5 days
		//DateTime startTimestamp = DateTime.UtcNow.AddDays(-85);		// Started 85 days ago
		eventData.Add("startTimestamp", startTimestamp.ToString(JSON_FORMAT));
		eventData.Add("teaserTimestamp", startTimestamp.AddDays(-2).ToString(JSON_FORMAT));	// 2 days before start time
		eventData.Add("endTimestamp", startTimestamp.AddDays(7).ToString(JSON_FORMAT));	// Lasts 7 days

		// Rewards
		SimpleJSON.JSONArray rewardsArray = new SimpleJSON.JSONArray();
		rewardsArray.Add(CreateEventRewardData(0.2f, "sc", 500));
		rewardsArray.Add(CreateEventRewardData(0.5f, "sc", 1000));
		rewardsArray.Add(CreateEventRewardData(0.75f, "hc", 100));
		rewardsArray.Add(CreateEventRewardData(1f, "egg", 1));
		eventData.Add("rewards", rewardsArray);

		// Top percentile reward
		eventData.Add("topReward", CreateEventRewardData(0.1f, "pet1", 1));

		// Store response and simulate server delay
		res["response"] = eventData.ToString();
		DelayedCall(() => _callback(null, res));
	}

	/// <summary>
	/// Get the current value and (optionally) the leaderboard for a specific event.
	/// </summary>
	/// <param name="_eventID">The identifier of the event whose state we want.</param>
	/// <param name="_getLeaderboard">Whether to retrieve the leaderboard as well or not (top 100 + player).</param>
	/// <param name="_callback">Callback action.</param>
	override public void GlobalEvent_GetState(string _eventID, bool _getLeaderboard, Action<Error, Dictionary<string, object>> _callback) {
		// Create return dictionary
		Dictionary<string, object> res = new Dictionary<string, object>();

		// Create json
		SimpleJSON.JSONClass eventData = new SimpleJSON.JSONClass();

		// Current value
		// Use a local dictionary to simulate events values progression
		float currentValue = 600f;
		if(!m_eventsValues.TryGetValue(_eventID, out currentValue)) {
			m_eventsValues[_eventID] = currentValue;
		}
		eventData.Add("currentValue", currentValue.ToString(JSON_FORMAT));

		// Leaderboard (if requested)
		if(_getLeaderboard) {
			// Create an array of max 100 random users
			float remainingScore = currentValue;
			float minScore = remainingScore;
			Range scorePerPlayerRange = new Range(remainingScore/100f, remainingScore/10f);	// min 10 players, max 100
			List<SimpleJSON.JSONClass> leaderboard = new List<SimpleJSON.JSONClass>();
			while(remainingScore > 0f) {
				float score = Mathf.Min(remainingScore, scorePerPlayerRange.GetRandom());
				remainingScore -= score;
				minScore = Mathf.Min(minScore, score);

				SimpleJSON.JSONClass leaderboardEntry = new SimpleJSON.JSONClass();
				leaderboardEntry.Add("id", "dummy_player_" + leaderboard.Count);
				leaderboardEntry.Add("score", score.ToString(JSON_FORMAT));
				leaderboard.Add(leaderboardEntry);
			}

			// Add a field with current player's position in the leaderboard
			GlobalEventsUserData.EventData playerEventData = UsersManager.currentUser.globalEvents[_eventID];
			float playerScore = playerEventData == null ? 0f : playerEventData.contribution;
			int playerPos = playerEventData == null ? -1 : UnityEngine.Random.Range(leaderboard.Count, 100000);	// Simulate a random position in the leaderboard
			eventData.Add("playerPosition", playerPos.ToString(JSON_FORMAT));

			// If player is on the leaderboard, add it!
			// We should probably remove the last one, but since it's test code, we don't care enough
			if(playerScore > minScore && playerScore > 0f) {
				SimpleJSON.JSONClass playerData = new SimpleJSON.JSONClass();
				playerData.Add("id", "PLAYER");
				playerData.Add("score", playerScore.ToString(JSON_FORMAT));
				leaderboard.Add(playerData);
			}

			// Sort list by score and store it into a json array
			leaderboard.Sort((_a, _b) => _a["score"].AsInt.CompareTo(_b["score"].AsInt));	// Gotta love delta expressions. Using int's CompareTo directly
			SimpleJSON.JSONArray leaderboardData = new SimpleJSON.JSONArray();
			for(int i = 0; i < leaderboard.Count; i++) {
				leaderboardData.Add(leaderboard[i]);
			}

			// Add leaderboard data to event data
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
	override public void GlobalEvent_RegisterScore(string _eventID, float _score, Action<Error> _callback) {
		// Increase event's current value
		if(!m_eventsValues.ContainsKey(_eventID)) {
			// Invalid event ID, simulate validation error
			DelayedCall(() => _callback(new Error("User can't register score into the event with id " + _eventID, ErrorCodes.ValidationError)));
		} else {
			// Increase event's total score and simulate server delay
			m_eventsValues[_eventID] += _score;
			DelayedCall(() => _callback(null));
		}
	}

	/// <summary>
	/// Reward the player for his contribution to an event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action. Given rewards?</param>
	override public void GlobalEvent_ApplyRewards(string _eventID, Action<Error, Dictionary<string, object>> _callback) {
		// [AOC] TODO!!
		DelayedCall(() => _callback(null, null));
	}
	
	/// <summary>
	/// Create a JSON object simulating an event reward data.
	/// </summary>
	/// <returns>The event reward data json.</returns>
	/// <param name="_percentage">Target percentage. For top percentile reward, percentile of the leaderboard to whom the reward is given.</param>
	/// <param name="_sku">Reward sku.</param>
	/// <param name="_amount">Rewarded amount.</param>
	private SimpleJSON.JSONClass CreateEventRewardData(float _percentage, string _sku, long _amount) {
		SimpleJSON.JSONClass reward = new SimpleJSON.JSONClass();
		reward.Add("targetPercentage", _percentage.ToString(JSON_FORMAT));
		reward.Add("sku", _sku);
		reward.Add("amount", _amount.ToString(JSON_FORMAT));
		return reward;
	}
}