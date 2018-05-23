// LiveEventManager.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 15/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

#if UNITY_EDITOR
// #define TEST_HD_LIVE_EVENTS
#endif

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global singleton manager for global events.
/// </summary>
public class HDLiveEventsManager : Singleton<HDLiveEventsManager> {


	public enum ErrorCode {
		NONE = 0,

		OFFLINE,
		NOT_INITIALIZED,
		NOT_LOGGED_IN,
		NO_VALID_EVENT,
		EVENT_NOT_ACTIVE
	}

	HDTournamentManager m_tournament = new HDTournamentManager();
	HDLiveEventManager m_quest = new HDLiveEventManager();
	HDLiveEventManager m_passive = new HDLiveEventManager();

	public bool m_cacheInfo = false;

	public HDLiveEventsManager()
	{
        // For testing
        m_tournament.m_type = "tournament";
        m_quest.m_type = "quest";
        m_passive.m_type = "passive";

		// Load Cache?
		LoadEventsFromCache();
	}

	public void LoadEventsFromCache()
	{
		m_cacheInfo = true;
		m_tournament.CleanData();
        if ( CacheServerManager.SharedInstance.HasKey("tournament") )
        {
            SimpleJSON.JSONNode json = SimpleJSON.JSONNode.Parse(CacheServerManager.SharedInstance.GetVariable("tournament"));
            m_tournament.OnNewStateInfo( json );
		}
		
        if (CacheServerManager.SharedInstance.HasKey("passive"))
        {
            SimpleJSON.JSONNode json = SimpleJSON.JSONNode.Parse(CacheServerManager.SharedInstance.GetVariable("passive"));
            m_tournament.OnNewStateInfo(json);
        }

        if (CacheServerManager.SharedInstance.HasKey("quest"))
        {
            SimpleJSON.JSONNode json = SimpleJSON.JSONNode.Parse(CacheServerManager.SharedInstance.GetVariable("quest"));
            m_tournament.OnNewStateInfo(json);
        }
	}

	public void SaveEventsToCache()
	{
		if ( m_tournament.IsActive()){
			CacheServerManager.SharedInstance.SetVariable( "tournament", m_tournament.ToJson().ToString());
		}else{
			CacheServerManager.SharedInstance.DeleteKey("tournament");
		}

		if ( m_passive.IsActive()){
			CacheServerManager.SharedInstance.SetVariable( "passive", m_passive.ToJson().ToString());
		}else{
			CacheServerManager.SharedInstance.DeleteKey("passive");
		}

		if ( m_quest.IsActive()){
			CacheServerManager.SharedInstance.SetVariable( "quest", m_quest.ToJson().ToString());
		}else{
			CacheServerManager.SharedInstance.DeleteKey("quest");
		}
	}

#if UNITY_EDITOR
    public static GameServerManager.ServerResponse CreateTestResponse(string fileName)
    {
        string path = Directory.GetCurrentDirectory() + "/Assets/HDLiveEventsTests/" + fileName;
        string json = File.ReadAllText(path);
        GameServerManager.ServerResponse response = new GameServerManager.ServerResponse();
        response.Add("response", json);
        return response;
    }
#endif

    public void RequestMyEvents()
    {
#if TEST_HD_LIVE_EVENTS
        GameServerManager.ServerResponse response = CreateTestResponse( "hd_live_events.json" );
        MyEventsResponse( null, response );
#else
        GameServerManager.SharedInstance.HDEvents_GetMyEvents(instance.MyEventsResponse);
#endif
    }

	private void MyEventsResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) 
	{
		if ( _error != null ){
			// Messenger.Broadcast(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR);
			return;
		}

		if(_response != null && _response["response"] != null) 
		{
			SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"] as string);
			m_cacheInfo = false;

			m_tournament.CleanData();
			if ( responseJson != null && responseJson.ContainsKey("tournament")){
				m_tournament.OnNewStateInfo(responseJson["tournament"]);	
			}

			m_quest.CleanData();
			if ( responseJson != null && responseJson.ContainsKey("quest")){
				m_quest.OnNewStateInfo(responseJson["tournament"]);	
			}

			m_passive.CleanData();
			if ( responseJson != null && responseJson.ContainsKey("passive")){
				m_quest.OnNewStateInfo(responseJson["passive"]);	
			}
		}
	}

	public bool Connected() {
		bool ret = false;	
		if ((CPGlobalEventsTest.networkCheck && Application.internetReachability != NetworkReachability.NotReachable) &&
			(CPGlobalEventsTest.loginCheck   && GameSessionManager.SharedInstance.IsLogged())) {
			ret = true;
		}
		return ret;
	}
}