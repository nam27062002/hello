// HDTournamentManager.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 17/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// 
/// </summary>
[Serializable]
public class HDTournamentManager : HDLiveEventManager {
	//------------------------------------------------------------------------//
	// CLASSES   															  //
	//------------------------------------------------------------------------//
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public TrackerBase m_tracker = new TrackerBase();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDTournamentManager() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(MessengerEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDTournamentManager() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(MessengerEvents.GAME_ENDED, OnGameEnded);
	}

	public override void BuildData()
    {
		m_data = new HDTournamentData();
    }

	public override void ParseDefinition(SimpleJSON.JSONNode _data)
    {
    	base.ParseDefinition( _data );
		m_tracker.Clear();
		HDTournamentDefinition def = m_data.definition as HDTournamentDefinition;
		m_tracker = TrackerBase.CreateTracker( def.m_goal.m_typeDef.sku, def.m_goal.m_params);
    }


    public void RequestLeaderboard()
    {
        if ( HDLiveEventsManager.TEST_CALLS )
        {
			GameServerManager.ServerResponse response = HDLiveEventsManager.CreateTestResponse("tournament_leaderboard.json");
            LeaderboardResponse(null, response);
        }
        else
        {
            HDLiveEventData data = GetEventData();
            GameServerManager.SharedInstance.HDEvents_GetLeaderboard(data.m_eventId, LeaderboardResponse);    
        }
    }

	public bool IsLeaderboardReady() {
		return false;
	}

    protected virtual void LeaderboardResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
        if (_error != null)
        {
            // Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
            return;
        }

        if (_response != null && _response["response"] != null)
        {
            SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
            int eventId = responseJson["code"].AsInt;
			HDTournamentData data = m_data as HDTournamentData;
            if (data != null /*&& data.m_eventId == eventId*/ )
            {
            	if ( responseJson.ContainsKey("c") )// Is cheater
            	{
            		AntiCheatsManager.MarkUserAsCheater();
            	}
            	data.ParseLeaderboard( responseJson );
            }
        }
    }

    public void SendScore(int _score)
    {
        if (HDLiveEventsManager.TEST_CALLS)
        {
            GameServerManager.ServerResponse response = HDLiveEventsManager.CreateTestResponse("tournament_set_score.json");
            SetScoreResponse(null, response);
        }
        else
        {
            HDLiveEventData data = GetEventData();
            GameServerManager.SharedInstance.HDEvents_SetScore(data.m_eventId, _score, SetScoreResponse);
        }
    }

    protected virtual void SetScoreResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
    {
        if (_error != null)
        {
            // Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
            return;
        }

        if (_response != null && _response["response"] != null)
        {
            SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
            int eventId = responseJson["code"].AsInt;
            HDLiveEventData data = GetEventData();
            if (data != null && data.m_eventId == eventId)
            {

            }
        }
    }

	public void OnGameStarted(){
		if ( m_tracker != null)
    	{
			m_tracker.SetValue(0, false);
			// Check if we are in tournament mode
			m_tracker.enabled = m_isActive;
    	}
    }
    public void OnGameEnded(){
    	// Save tracker value?
    }

    public string GetToUseDragon()
    {
		string ret;
    	HDTournamentData data = m_data as HDTournamentData;
    	HDTournamentDefinition def = data.definition as HDTournamentDefinition;
    	if ( !string.IsNullOrEmpty( def.m_build.m_dragon) )
			ret = def.m_build.m_dragon;
		else
			ret = UsersManager.currentUser.currentDragon;
		return ret;
    }

    public string GetToUseSkin()
    {
		string ret;
    	HDTournamentData data = m_data as HDTournamentData;
    	HDTournamentDefinition def = data.definition as HDTournamentDefinition;
    	if ( !string.IsNullOrEmpty( def.m_build.m_skin) )
			ret = def.m_build.m_skin;
		else
			ret = UsersManager.currentUser.GetEquipedDisguise(GetToUseDragon());
		return ret;
    }

    public List<string> GetToUsePets()
    {
    	List<string> ret;
		HDTournamentData data = m_data as HDTournamentData;
    	HDTournamentDefinition def = data.definition as HDTournamentDefinition;
    	if ( def.m_build.m_pets.Count > 0 )
    	{
			ret = def.m_build.m_pets;
		}
		else
		{
			ret = UsersManager.currentUser.GetEquipedPets(GetToUseDragon());
		}
		return ret;
    }
 
	// The score obtained in the last run (tracker)
	public long GetRunScore() {
		// TODO!!
		return 654;
	}

	// The overall best score (the one registered in the leaderboard)
	public long GetBestScore() {
		// TODO!!
		return 0;
	}

	// Given a score, format it based on tournament type
	public string FormatScore(long _score) {
		// TODO!!
		// Depends on tournament type, not all scores are numbers! (time)
		return TimeUtils.FormatTime((double)_score, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES, true);	// MM:SS
		//return StringUtils.FormatNumber(_score);
	}

	// Has the last run been a high score?
	public bool IsNewBestScore() {
		// TODO!!
		return false;
	}

	// Has the player reached the minimum requirement to register a valid score?
	// i.e. "Eat 100 birds as fast as possible" but you died before reaching 100 birds)
	public bool IsValidRun() {
		// TODO!!
		return true;
	}
}