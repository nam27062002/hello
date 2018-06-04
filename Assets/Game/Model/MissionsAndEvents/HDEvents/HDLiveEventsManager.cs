// LiveEventManager.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 15/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.


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
public class HDLiveEventsManager : Singleton<HDLiveEventsManager>
{


    public enum ErrorCode
    {
        NONE = 0,

        OFFLINE,
        NOT_INITIALIZED,
        NOT_LOGGED_IN,
        NO_VALID_EVENT,
        EVENT_NOT_ACTIVE
    }

    public HDTournamentManager m_tournament = new HDTournamentManager();
    public HDQuestManager m_quest = new HDQuestManager();
    public HDLiveEventManager m_passive = new HDLiveEventManager();

        // Avoid using dictionaries when possible
    private List<string> m_types;
    private List<HDLiveEventManager> m_managers;
	protected long m_lastMyEventsRequestTimestamp = 0;

    public const long CACHE_TIMEOUT_MS = 1000 * 60 * 60 * 24 * 7;	// 7 days timeout

	private long m_myEventsRequestMinTim = 1000 * 60 * 60 * 5;

    public bool m_cacheInfo = false;
#if UNITY_EDITOR
    public static readonly bool TEST_CALLS = true;
#else
    // Do not touch!
    public static readonly bool TEST_CALLS = false;
#endif

    public HDLiveEventsManager()
	{
        // For testing purposes
        m_tournament.m_type = "tournament";
        m_quest.m_type = "quest";
        m_passive.m_type = "passive";
        //

        m_types = new List<string>(3);
        m_managers = new List<HDLiveEventManager>(3);

        m_types.Add( "tournament" );
        m_managers.Add(m_tournament);

        m_types.Add("quest");
        m_managers.Add(m_quest);

        m_types.Add("passive");
        m_managers.Add(m_passive);


		// Load Cache?
		LoadEventsFromCache();

		// m_passive.Activate();

        Messenger.AddListener<bool>(MessengerEvents.LOGGED, OnLoggedIn);

	}

    ~HDLiveEventsManager()
    {
        Messenger.RemoveListener<bool>(MessengerEvents.LOGGED, OnLoggedIn);
    }

    void OnLoggedIn(bool _isLogged)
    {
        if (_isLogged)
        {
            RequestMyEvents();
        }
    }

	public void LoadEventsFromCache()
	{
		long cacheTimestamp = 0;
		if ( CacheServerManager.SharedInstance.HasKey("hdliveeventstimestamp") )
		{
			cacheTimestamp = long.Parse(CacheServerManager.SharedInstance.GetVariable("hdliveeventstimestamp"));
		}

		if ( GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - cacheTimestamp < CACHE_TIMEOUT_MS)
		{
			m_cacheInfo = true;
	        int max = m_types.Count;
	        for (int i = 0; i < max; ++i)
	        {
	            m_managers[i].CleanData();
	            if (CacheServerManager.SharedInstance.HasKey( m_types[i] ))
	            {
	                SimpleJSON.JSONNode json = SimpleJSON.JSONNode.Parse(CacheServerManager.SharedInstance.GetVariable( m_types[i] ));
	                m_managers[i].OnNewStateInfo(json);
	                m_managers[i].UpdateStateFromTimers();
	            }
	        }
        }
        else
        {
        	// Delete cache!
			m_cacheInfo = false;
	        int max = m_types.Count;
	        for (int i = 0; i < max; ++i)
	        {
	            m_managers[i].CleanData();
	            if (CacheServerManager.SharedInstance.HasKey( m_types[i] ))
	            {
					CacheServerManager.SharedInstance.DeleteKey( m_types[i] );
	            }
	        }
        }
	}

	public void SaveEventsToCache()
	{
        int max = m_types.Count;
        for (int i = 0; i < max; i++)
        {
            if (m_managers[i].EventExists() && m_managers[i].data.m_state != HDLiveEventData.State.FINALIZED )
            {
                CacheServerManager.SharedInstance.SetVariable( m_types[i] , m_managers[i].ToJson().ToString());
            }
            else
            {
                CacheServerManager.SharedInstance.DeleteKey( m_types[i] );
            }
        }

		CacheServerManager.SharedInstance.SetVariable("hdliveeventstimestamp", GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong().ToString());
	}

#if UNITY_EDITOR
    public static GameServerManager.ServerResponse CreateTestResponse(string fileName)
    {
        string path = Directory.GetCurrentDirectory() + "/Assets/HDLiveEventsTest/" + fileName;
        string json = File.ReadAllText(path);
        GameServerManager.ServerResponse response = new GameServerManager.ServerResponse();
        response.Add("response", json);
        return response;
    }

#else
    public static GameServerManager.ServerResponse CreateTestResponse(string fileName)
    {
        return null;
    }
#endif

	protected delegate void TestResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response);
	protected IEnumerator DelayedCall( string _fileName, TestResponse _testResponse )
	{
		yield return new WaitForSeconds(1.0f);
		GameServerManager.ServerResponse response = HDLiveEventsManager.CreateTestResponse( _fileName );
		_testResponse(null, response);
	}


	public bool ShouldRequestMyEvents()
	{
		long diff = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - m_lastMyEventsRequestTimestamp;
		return diff > m_myEventsRequestMinTim;
	}

    public bool RequestMyEvents( bool _force = false )
    {
    	bool ret = false;
    	if ( _force || ShouldRequestMyEvents() )
    	{
			m_lastMyEventsRequestTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
	        if ( TEST_CALLS )
	        {
				ApplicationManager.instance.StartCoroutine( DelayedCall("hd_live_events.json", MyEventsResponse));
	        }
	        else
	        {
	            GameServerManager.SharedInstance.HDEvents_GetMyEvents(instance.MyEventsResponse);    
	        }
	        ret = true;
        }
        return ret;
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
            if ( responseJson != null )
            {
                int max = m_types.Count;
                for (int i = 0; i < max; i++)
                {
                    m_managers[i].CleanData();
                    if (responseJson.ContainsKey( m_types[i] ))
                    {
                        m_managers[i].OnNewStateInfo(responseJson[ m_types[i] ]);
                        if (!m_managers[i].HasValidDefinition() || m_cacheInfo)
                        {
                            m_managers[i].RequestDefinition();
                        }
                    }
                }

            }
			m_cacheInfo = false;

			if ( m_quest.EventExists() && m_quest.ShouldRequestProgress() )
				m_quest.RequestProgress();
			if ( m_tournament.EventExists() && m_tournament.ShouldRequestLeaderboard() )
				m_tournament.RequestLeaderboard();

			Messenger.Broadcast(MessengerEvents.LIVE_EVENT_STATES_UPDATED);
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


	public void ApplyDragonMods()
	{
		for (int i = 0; i < m_managers.Count; i++) {
			if ( m_managers[i].m_isActive )
			{
				m_managers[i].ApplyDragonMods();
			}
		}
	}

	public void SwitchToTournament()
	{
		// if hd live events enabled
		// if (GameSettings.ENABLE_GLOBAL_EVENTS_AT_RUN >= UsersManager.currentUser.gamesPlayed)
		{
			m_tournament.Activate();
		}
		m_passive.Deactivate();
		m_quest.Deactivate();
	}

	public void SwitchToQuest()
	{
		// if hd live events enabled
		m_tournament.Deactivate();
		// if (GameSettings.ENABLE_GLOBAL_EVENTS_AT_RUN >= UsersManager.currentUser.gamesPlayed)
		{
			m_passive.Activate();
			m_quest.Activate();	
		}
	}
}