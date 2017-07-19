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
using System.Linq;
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

	// Simulate users database
	public struct FakeUserSocialInfo {
		public string id;
		public string name;
		public string pictureUrl;
	}
	private Dictionary<string, FakeUserSocialInfo> m_socialInfoDatabase = new Dictionary<string, FakeUserSocialInfo>();
	private Dictionary<string, FakeUserSocialInfo> m_socialInfoDatabasePool = new Dictionary<string, FakeUserSocialInfo>();	// To avoid repeating a lot, we will use a copy of the database when generating random profiles and removing created profiles from it. Once empty, we will fill it again with a new copy of the original database.

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

		// Load fake users social info
		TextAsset jsonFile = Resources.Load<TextAsset>("Debug/debug_sample_friends_list");
		if(jsonFile != null) {
			// Clear dictionary
			m_socialInfoDatabase = new Dictionary<string, FakeUserSocialInfo>();

			// Parse json
			SimpleJSON.JSONNode jsonData = SimpleJSON.JSON.Parse(jsonFile.text);
			SimpleJSON.JSONArray friendsDataArray = jsonData["data"].AsArray;
			foreach(SimpleJSON.JSONNode friendData in friendsDataArray) {
				/*{
		            "picture":{
		                "data":{
		                    "height":200,
		                    "is_silhouette":false,
		                    "url":"https://scontent.xx.fbcdn.net/v/t1.0-1/p200x200/13886387_10100280084427592_3186940560560269419_n.jpg?oh=a9ab49109a28166c2a3409afecdb269e&oe=5A0404C0",
		                    "width":200
		                }
		            },
		            "name":"Owen Jose Nibbler Convey",
		            "id":"223102040"
		        }*/
				FakeUserSocialInfo newSocialInfo = new FakeUserSocialInfo();
				newSocialInfo.id = friendData["id"];
				newSocialInfo.name = friendData["name"];
				newSocialInfo.pictureUrl = friendData["picture"]["data"]["url"];
				m_socialInfoDatabase.Add(newSocialInfo.id, newSocialInfo);
			}
		}
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

	private DateTime CreateStartDate(CPGlobalEventsTest.EventStateTest _targetState) {
		DateTime startTimestamp = new DateTime();
		switch(_targetState) {
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
		return startTimestamp;
	}

	private string CreateTeaseTimestamp(DateTime _startDate) {
		return TimeUtils.DateToTimestamp(_startDate.AddDays(-2)).ToString(JSON_FORMAT);	// 2 days before start time
	}

	private string CreateStartTimestamp(DateTime _startDate) {
		return TimeUtils.DateToTimestamp(_startDate).ToString(JSON_FORMAT);
	}

	private string CreateEndTimestamp(DateTime _startDate) {
		return TimeUtils.DateToTimestamp(_startDate.AddDays(7)).ToString(JSON_FORMAT); // Lasts 7 days
	}

	//------------------------------------------------------------------------//
	// GLOBAL EVENTS														  //
	//------------------------------------------------------------------------//
	override public void GlobalEvent_TMPCustomizer(ServerCallback _callback) {
		ServerResponse res = new ServerResponse();

		// Check debug settings
		CPGlobalEventsTest.EventStateTest targetState = CPGlobalEventsTest.eventState;

		// Simulate no event?
		if(targetState == CPGlobalEventsTest.EventStateTest.NO_EVENT) {
			res["response"] = null;
		} else {
			DateTime startDate = CreateStartDate(targetState);

			SimpleJSON.JSONClass eventData = new SimpleJSON.JSONClass(); {
				SimpleJSON.JSONArray liveEvents = new SimpleJSON.JSONArray(); {
					SimpleJSON.JSONClass currentLiveEvent = new SimpleJSON.JSONClass(); {
						currentLiveEvent.Add("code", CPGlobalEventsTest.eventCode);
						currentLiveEvent.Add("name", "test_event_0");
						currentLiveEvent.Add("start", CreateStartTimestamp(startDate));
						currentLiveEvent.Add("end", CreateEndTimestamp(startDate));
					}
					liveEvents.Add(currentLiveEvent);
				}
				eventData.Add("liveEvents", liveEvents);
			}
			res["response"] = eventData.ToString();
		}

		// Simulate server delay
		DelayedCall(() => _callback(null, res));
	}

	override public void GlobalEvent_GetEvent( int _eventID, ServerCallback _callback) {
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
			eventData.Add("id", CPGlobalEventsTest.eventCode);
			eventData.Add("name", "test_event_0");

			// Goal definition
			SimpleJSON.JSONClass goalData = new SimpleJSON.JSONClass();
			goalData.Add("type", "kill");
			goalData.Add("params", "Canary01_Flock;Canary02_Flock;Canary03_Flock;Canary04_Flock");
			goalData.Add("icon", "icon_canary");
			goalData.Add("tidDesc", "TID_EVENT_EAT_BIRDS");
			goalData.Add("amount", 100000f.ToString(JSON_FORMAT));
			eventData.Add("goal", goalData);

			// Timestamps
			// By tuning the start timestamp in relation to the current time we can simulate the different states of the event
			DateTime startDate = CreateStartDate(targetState);
			eventData.Add("startTimestamp", CreateStartTimestamp(startDate));
			eventData.Add("teaserTimestamp", CreateTeaseTimestamp(startDate));
			eventData.Add("endTimestamp", CreateEndTimestamp(startDate));		

			// Rewards
			SimpleJSON.JSONArray rewardsArray = new SimpleJSON.JSONArray();
			rewardsArray.Add(CreateEventRewardData(0.2f, Metagame.RewardSoftCurrency.Code, "", 500));
			rewardsArray.Add(CreateEventRewardData(0.5f, Metagame.RewardSoftCurrency.Code, "", 1000));
			rewardsArray.Add(CreateEventRewardData(0.75f, Metagame.RewardHardCurrency.Code, "", 100));
			rewardsArray.Add(CreateEventRewardData(1f, Metagame.RewardEgg.Code, "egg_premium", -1));
			eventData.Add("rewards", rewardsArray);

			// Top percentile reward
			eventData.Add("topReward", CreateEventRewardData(0.1f, "pet", "pet_24", -1));

			// Bonuses
			eventData.Add("bonusDragon", "dragon_reptile");

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
	override public void GlobalEvent_GetState(int _eventID, ServerCallback _callback) {
		// Create return dictionary
		ServerResponse res = new ServerResponse();

		// Create json
		SimpleJSON.JSONClass eventData = new SimpleJSON.JSONClass();

		// Current value
		// Use a local dictionary to simulate events values progression
		float currentValue = 0f;
		if(!m_eventValues.TryGetValue(_eventID, out currentValue)) {
			currentValue = 60000f;	// Default initial value
			m_eventValues[_eventID] = currentValue;
		}
		eventData.Add("currentValue", currentValue.ToString(JSON_FORMAT));

		// Current player data
		GlobalEventUserData playerEventData = UsersManager.currentUser.GetGlobalEventData(_eventID);

		// Add player data to the json
		eventData.Add("playerData", playerEventData.Save(true));

		// Store response and simulate server delay
		res["response"] = eventData.ToString();
		DelayedCall(() => _callback(null, res));
	}

	/// <summary>
	/// Get leaderboard.
	/// </summary>
	/// <param name="_eventID">The identifier of the event whose leaderboard we want.</param>
	/// <param name="_callback">Callback action.</param>
	override public void GlobalEvent_GetLeaderboard(int _eventID, ServerCallback _callback) {
		// Create return dictionary
		ServerResponse res = new ServerResponse();

		// Create json
		SimpleJSON.JSONClass eventData = new SimpleJSON.JSONClass();

		// Current value
		// Use a local dictionary to simulate events values progression
		float currentValue = 0f;
		if(!m_eventValues.TryGetValue(_eventID, out currentValue)) {
			currentValue = 60000f;	// Default initial value
			m_eventValues[_eventID] = currentValue;
		}

		// Current player data
		GlobalEventUserData playerEventData = UsersManager.currentUser.GetGlobalEventData(_eventID);

		// Make sure there is one entry in the social database with this user ID!
		FakeUserSocialInfo playerSocialData = GetFakeSocialInfo(playerEventData.userID);	// This will create a new entry if needed, since we're using the player's ID

		// If there is no leaderboard for this event, create one with random values
		List<GlobalEventUserData> leaderboard = null;
		if(!m_eventLeaderboards.TryGetValue(_eventID, out leaderboard)) {
			// Create an array of max 100 random users
			// Pick random social data for each, but without repeating!
			// Remove current player from the pool
			m_socialInfoDatabasePool.Remove(playerSocialData.id);

			// Select leaderboard size
			float totalContributors = UnityEngine.Random.Range(50f, 150f);
			switch(CPGlobalEventsTest.leaderboardSize) {
				case CPGlobalEventsTest.LeaderboardSize.SIZE_0:		totalContributors = 0;		break;
				case CPGlobalEventsTest.LeaderboardSize.SIZE_5:		totalContributors = 5;		break;
				case CPGlobalEventsTest.LeaderboardSize.SIZE_10:	totalContributors = 10;		break;
				case CPGlobalEventsTest.LeaderboardSize.SIZE_25:	totalContributors = 25;		break;
				case CPGlobalEventsTest.LeaderboardSize.SIZE_50:	totalContributors = 50;		break;
				case CPGlobalEventsTest.LeaderboardSize.SIZE_75:	totalContributors = 75;		break;
				case CPGlobalEventsTest.LeaderboardSize.SIZE_100:	totalContributors = 100;	break;
			}

			// Restriction: at least 1 per player!
			float minScorePerPlayer = 1f;
			totalContributors = Mathf.Min(totalContributors, currentValue/minScorePerPlayer);

			// Do it!
			// Distribute the total score of the event among the total amount of players
			float remainingScore = currentValue;
			int remainingContributors = (int)totalContributors;
			leaderboard = new List<GlobalEventUserData>(100);
			while(remainingContributors > 0 && remainingScore > 0f && leaderboard.Count <= 100) {
				// Compute new score for this user
				float score = 0f;
				if(remainingContributors == 1) {
					// Last contributor? Put all the remaining score
					score = remainingScore;
				} else {
					// Leave enough score for the rest of contributors!
					score = UnityEngine.Random.Range(
						minScorePerPlayer, 
						//remainingScore - remainingContributors * minScorePerPlayer
						Mathf.Min(remainingScore - remainingContributors * minScorePerPlayer, currentValue * 0.25f)	// Try to distribute score more evenly by limiting the max reward
					);
				}
				remainingScore -= score;
				remainingContributors--;

				// Pick a random social info and consume it
				FakeUserSocialInfo socialInfo = GetFakeSocialInfo();

				// Create user data
				GlobalEventUserData leaderboardEntry = new GlobalEventUserData(
					_eventID,
					socialInfo.id,
					score,
					-1,	// Position will be initialized afterwards when the leaderboard is sorted
					0
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
		float minScore = leaderboard.Count > 0 ? leaderboard.Last().score : 0f;
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
		// Create a json array with every entry in the leaderboard
		SimpleJSON.JSONArray leaderboardData = new SimpleJSON.JSONArray();
		for(int i = 0; i < leaderboard.Count; i++) {
			leaderboardData.Add(leaderboard[i].Save(false));
		}
		eventData.Add("leaderboard", leaderboardData);

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
			// Increase event's total score
			m_eventValues[_eventID] += _score;

			// Update leaderboard cache!
			string userID = UsersManager.currentUser.userId;
			List<GlobalEventUserData> leaderboard = null;
			if(m_eventLeaderboards.TryGetValue(_eventID, out leaderboard)) {
				// Check whether the player is in the leaderboard
				int idx = leaderboard.FindIndex((_a) => _a.userID == userID);
				bool wasOnTheLeaderboard = (idx >= 0);
				bool shouldBeOnTheLeaderboard = _score > 0;

				// Cases:
				// a) Player on the leaderboard
				if(wasOnTheLeaderboard) {
					// Should stay?
					if(shouldBeOnTheLeaderboard) {
						// Update score
						leaderboard[idx].score += _score;
					} else {
						// Remove from the leaderboard
						leaderboard.RemoveAt(idx);
					}
				}

				// b) Player not on the leaderboard
				else {
					// Should enter?
					if(shouldBeOnTheLeaderboard) {
						leaderboard.Add(new GlobalEventUserData(_eventID, userID, _score, -1, 0));
					} else {
						// Nothing to do
					}
				}

				// Sort the leaderboard
				leaderboard.Sort((_a, _b) => -(_a.score.CompareTo(_b.score)));	// Using float's CompareTo directly. Reverse sign since we want to sort from bigger to lower.

				// Update positions
				for(int i = 0; i < leaderboard.Count; ++i) {
					leaderboard[i].position = i;
					if(leaderboard[i].userID == userID) {
						leaderboard[i].position = i;
					}
				}

				// Keep leaderboard size controlled
				if(leaderboard.Count > 100) {
					leaderboard.RemoveRange(100, leaderboard.Count - 100);
				}
			}

			// Simulate server delay
			DelayedCall(() => _callback(null, null));
		}
	}

	/// <summary>
	/// Reward the player for his contribution to an event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action. Given rewards?</param>
	override public void GlobalEvent_GetRewards(int _eventID, ServerCallback _callback) {		
		/*
		{
			r: [
				"SC:100",
				"SC:200"
			],
			top: "SC:50"
		}
*/
		ServerResponse res = new ServerResponse();

		// Check debug settings
		CPGlobalEventsTest.EventStateTest targetState = CPGlobalEventsTest.eventState;

		// Simulate no event?
		if(targetState == CPGlobalEventsTest.EventStateTest.NO_EVENT) {
			res["response"] = null;
		} else {
			SimpleJSON.JSONClass eventData = new SimpleJSON.JSONClass(); {
				SimpleJSON.JSONArray r = new SimpleJSON.JSONArray(); {					
					r.Add(Metagame.RewardSoftCurrency.Code, 500);
					r.Add(Metagame.RewardSoftCurrency.Code, 1000);
					r.Add(Metagame.RewardHardCurrency.Code, 100);
				}
				SimpleJSON.JSONClass top = new SimpleJSON.JSONClass(); {
					top.Add(Metagame.RewardEgg.Code, "egg_premium");
				}
				eventData.Add("r", r);
				eventData.Add("top", top);
			}
			res["response"] = eventData.ToString();
		}

		DelayedCall(() => _callback(null, res));
	}
	
	/// <summary>
	/// Auxiliar method to create a JSON object simulating an event reward data.
	/// </summary>
	/// <returns>The event reward data json.</returns>
	/// <param name="_percentage">Target percentage. For top percentile reward, percentile of the leaderboard to whom the reward is given.</param>
	/// <param name="_type">Type of reward to be given.</param>
	/// <param name="_sku">Reward sku (optional).</param>
	/// <param name="_amount">Rewarded amount (optional).</param>
	private SimpleJSON.JSONClass CreateEventRewardData(float _percentage, string _type, string _sku, long _amount) {
		SimpleJSON.JSONClass reward = new SimpleJSON.JSONClass();
		reward.Add("targetPercentage", _percentage.ToString(JSON_FORMAT));
		reward.Add("type", _type);
		if(!string.IsNullOrEmpty(_sku)) reward.Add("sku", _sku);
		if(_amount > 0f) reward.Add("amount", _amount.ToString(JSON_FORMAT));
		return reward;
	}

	/// <summary>
	/// Given a user, get fake social info linked to him :P
	/// If no info is found in the database, a new one will be created using random values.
	/// </summary>
	/// <returns>The fake social info.</returns>
	/// <param name="_userID">User ID.</param>
	public FakeUserSocialInfo GetFakeSocialInfo(string _userID = "") {
		FakeUserSocialInfo socialInfo;
		if(string.IsNullOrEmpty(_userID) || !m_socialInfoDatabase.TryGetValue(_userID, out socialInfo)) {
			// Info not found!
			// Create a new fake profile picking a random entry from the generation pool
			//Debug.Log("<color=red>Social info for id </color><color=white>" + _userID + "</color><color=red> not found!</color>");

			// If the pool is empty, fill it with a new copy of the source database!
			if(m_socialInfoDatabasePool.Count == 0) {
				m_socialInfoDatabasePool = new Dictionary<string, FakeUserSocialInfo>(m_socialInfoDatabase);
			}

			// Pick a random id from the pool
			List<string> keys = Enumerable.ToList(m_socialInfoDatabasePool.Keys);
			string id = keys[UnityEngine.Random.Range(0, keys.Count)];

			// Grab his social info and remove it from the pool
			socialInfo = m_socialInfoDatabasePool[id];
			m_socialInfoDatabasePool.Remove(id);

			// If we requested a specific id, overwrite pool's id
			if(!string.IsNullOrEmpty(_userID)) {
				socialInfo.id = _userID;
			}

			// Add new social info to the database if not there
			if(!m_socialInfoDatabase.ContainsKey(socialInfo.id)) {
				m_socialInfoDatabase.Add(socialInfo.id, socialInfo);
			}
		}

		return socialInfo;
	}

	/// <summary>
	/// Clears the fake generated event leaderboard.
	/// </summary>
	/// <param name="_eventID">Event identifier.</param>
	public void ClearEventLeaderboard(int _eventID) {
		if(m_eventLeaderboards.ContainsKey(_eventID)) {
			m_eventLeaderboards.Remove(_eventID);
		}
	}
}