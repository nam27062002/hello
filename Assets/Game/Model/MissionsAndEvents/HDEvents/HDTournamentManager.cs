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
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDTournamentManager() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDTournamentManager() {

	}

	public override void BuildData()
    {
		m_data = new HDTournamentData();
    }

    public void RequestLeaderboard()
    {
        if ( HDLiveEventsManager.TEST_CALLS )
        {
            GameServerManager.ServerResponse response = HDLiveEventsManager.CreateTestResponse("tournament_leaderbaord.json");
            LeaderboardResponse(null, response);
        }
        else
        {
            HDLiveEventData data = GetEventData();
            GameServerManager.SharedInstance.HDEvents_GetLeaderboard(data.m_eventId, LeaderboardResponse);    
        }
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
            HDLiveEventData data = GetEventData();
            if (data != null && data.m_eventId == eventId)
            {

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
 
}