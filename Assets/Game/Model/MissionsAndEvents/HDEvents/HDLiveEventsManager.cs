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

    public enum ComunicationErrorCodes
    {
    	NET_ERROR,
    	RESPONSE_NOT_VALID,
    	OTHER_ERROR,
    	NO_RESPONSE,

		LDATA_NOT_FOUND, //(601,200),
    	ENTRANCE_FREE_INVALID, //(602,200),
    	ENTRANCE_AMOUNT_NOT_VALID, // (603,200),
    	ENTRANCE_TYPE_NOT_VALID, // (604,200),
		IS_NOT_A_VALID_TOURNAMENT,// 605,200) -> haces register en un tournament que no es tu tournament
    	IS_NOT_A_TOURNAMENT, //(606,200),
    	EVENT_NOT_FOUND, //(607,200),
    	EVENT_IS_NOT_VALID, // (608,200),
    	EVENT_IS_DISABLED, //(609,200),
    	UNEXPECTED_ERROR, //(610,200),
    	INCONSISTENT_TOURNAMENT_DATA, //(611,200);
		ELO_NOT_FOUND, // (612,200);
		TOURNAMENT_IS_OVER, //(613,200);
		GAMEMODE_NOT_EXISTS, //(614,200)
		EMPTY_REQUIRED_PARAMETERS, //(615,200)

		MATCHMAKING_ERROR, //(617,200),
		QUEST_IS_OVER, //(618,200),
    	IS_NOT_A_QUEST, //(619,200);
        EVENT_STILL_ACTIVE,//(620,200),
        NOTHING_PENDING,//(621,200),
        EVENT_TTL_EXPIRED,//(622,200);

        NO_ERROR
    };

    public HDTournamentManager m_tournament = new HDTournamentManager();
    public HDQuestManager m_quest = new HDQuestManager();
	public HDPassiveEventManager m_passive = new HDPassiveEventManager();
    public HDLeagueManager m_league = new HDLeagueManager();

        // Avoid using dictionaries when possible
    private List<string> m_types;
    private List<HDLiveEventManager> m_managers;
	protected long m_lastMyEventsRequestTimestamp = 0;

    public const long CACHE_TIMEOUT_MS = 1000 * 60 * 60 * 24 * 7;	// 7 days timeout

	private long m_myEventsRequestMinTim = 1000 * 60 * 5;	// 5 min

    public bool m_cacheInfo = false;
#if UNITY_EDITOR
	public static bool TEST_CALLS {
		get { return DebugSettings.useLiveEventsDebugCalls; }
	}
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
        m_league.m_type = "league";
        //

        m_types = new List<string>(4);
        m_managers = new List<HDLiveEventManager>(4);

        m_types.Add( "tournament" );
        m_managers.Add(m_tournament);

        m_types.Add("quest");
        m_managers.Add(m_quest);

        m_types.Add("passive");
        m_managers.Add(m_passive);

        m_types.Add("league");
        m_managers.Add(m_league);

		Messenger.AddListener<bool>(MessengerEvents.LOGGED, OnLoggedIn);
		Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, SaveEventsToCache);
		Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION,  SaveEventsToCacheWithParams);
		Messenger.AddListener(MessengerEvents.QUEST_SCORE_UPDATED,  SaveEventsToCache);

	}

    ~HDLiveEventsManager()
    {
        Messenger.RemoveListener<bool>(MessengerEvents.LOGGED, OnLoggedIn);
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, SaveEventsToCache);
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION,  SaveEventsToCacheWithParams);
		Messenger.RemoveListener(MessengerEvents.QUEST_SCORE_UPDATED,  SaveEventsToCache);
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

	public void SaveEventsToCacheWithParams(int _eventId, HDLiveEventsManager.ComunicationErrorCodes _err )
	{
		SaveEventsToCache();
	}

	public void SaveEventsToCache()
	{
        int max = m_types.Count;
        for (int i = 0; i < max; i++)
        {
            if (    m_managers[i].EventExists() && 
                    m_managers[i].data.m_state != HDLiveEventData.State.FINALIZED &&
                    m_managers[i].data.m_state != HDLiveEventData.State.REQUIRES_UPDATE
                    )
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
		yield return new WaitForSeconds(0.1f);
		GameServerManager.ServerResponse response = HDLiveEventsManager.CreateTestResponse( _fileName );
		_testResponse(null, response);
	}


	public static SimpleJSON.JSONNode ResponseErrorCheck(FGOL.Server.Error _error, GameServerManager.ServerResponse _response, out HDLiveEventsManager.ComunicationErrorCodes outErr)
	{
		SimpleJSON.JSONNode ret = null;

		if (_error == null)
        {
			outErr = HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR;
			if (_response != null && _response["response"] != null)
        	{
				ret = SimpleJSON.JSONNode.Parse(_response["response"] as string);
				if ( ret != null )
            	{
					if ( ret.ContainsKey("errorCode") )
            		{
            			// Translate error code
						outErr = HDLiveEventsManager.ComunicationErrorCodes.OTHER_ERROR;
						int errorInt = ret["errorCode"];
						switch( errorInt )
						{
							case 601: outErr = HDLiveEventsManager.ComunicationErrorCodes.LDATA_NOT_FOUND;break;
							case 602: outErr = HDLiveEventsManager.ComunicationErrorCodes.ENTRANCE_FREE_INVALID;break;
							case 603: outErr = HDLiveEventsManager.ComunicationErrorCodes.ENTRANCE_AMOUNT_NOT_VALID;break;
							case 604: outErr = HDLiveEventsManager.ComunicationErrorCodes.ENTRANCE_TYPE_NOT_VALID;break;
							case 605: outErr = HDLiveEventsManager.ComunicationErrorCodes.IS_NOT_A_VALID_TOURNAMENT;break;
							case 606: outErr = HDLiveEventsManager.ComunicationErrorCodes.IS_NOT_A_TOURNAMENT;break;
							case 607: outErr = HDLiveEventsManager.ComunicationErrorCodes.EVENT_NOT_FOUND;break;
							case 608: outErr = HDLiveEventsManager.ComunicationErrorCodes.EVENT_IS_NOT_VALID;break;
							case 609: outErr = HDLiveEventsManager.ComunicationErrorCodes.EVENT_IS_DISABLED;break;
							case 610: outErr = HDLiveEventsManager.ComunicationErrorCodes.UNEXPECTED_ERROR;break;
							case 611: outErr = HDLiveEventsManager.ComunicationErrorCodes.INCONSISTENT_TOURNAMENT_DATA;break;
							case 612: outErr = HDLiveEventsManager.ComunicationErrorCodes.ELO_NOT_FOUND;break;
							case 613: outErr = HDLiveEventsManager.ComunicationErrorCodes.TOURNAMENT_IS_OVER;break;
							case 614: outErr = HDLiveEventsManager.ComunicationErrorCodes.GAMEMODE_NOT_EXISTS;break;
							case 615: outErr = HDLiveEventsManager.ComunicationErrorCodes.EMPTY_REQUIRED_PARAMETERS;break;
							// case 616: outErr = HDLiveEventsManager.ComunicationErrorCodes.EMPTY_REQUIRED_PARAMETERS;break;
							case 617: outErr = HDLiveEventsManager.ComunicationErrorCodes.MATCHMAKING_ERROR;break;
							case 618: outErr = HDLiveEventsManager.ComunicationErrorCodes.QUEST_IS_OVER;break;
							case 619: outErr = HDLiveEventsManager.ComunicationErrorCodes.IS_NOT_A_QUEST;break;
                            case 620: outErr = HDLiveEventsManager.ComunicationErrorCodes.EVENT_STILL_ACTIVE;break;
                            case 621: outErr = HDLiveEventsManager.ComunicationErrorCodes.NOTHING_PENDING; break;
                            case 622: outErr = HDLiveEventsManager.ComunicationErrorCodes.EVENT_TTL_EXPIRED; break;
                        }
            		}
            	}
            	else
            	{
					outErr = HDLiveEventsManager.ComunicationErrorCodes.RESPONSE_NOT_VALID;
            	}
        	}
        	else
        	{
				outErr = HDLiveEventsManager.ComunicationErrorCodes.NO_RESPONSE;
        	}
        }
        else
        {
			outErr = HDLiveEventsManager.ComunicationErrorCodes.NET_ERROR;
        }
        return ret;
	}

	public static void ResponseLog( string call, FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
	{
		if (FeatureSettingsManager.IsDebugEnabled)
		{
			if ( _error != null )
			{
				ControlPanel.LogError( "[" + call + "]" + _error.message , ControlPanel.ELogChannel.LiveEvents);
			}

			if ( _response != null )
			{
				ControlPanel.Log( "[" + call + "]" + _response.ToString() , ControlPanel.ELogChannel.LiveEvents);
			}
		}
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
		
		ComunicationErrorCodes outErr = ComunicationErrorCodes.NO_ERROR;
		ResponseLog("GetMyEvents", _error, _response);
		SimpleJSON.JSONNode responseJson = ResponseErrorCheck(_error, _response, out outErr);
		if ( outErr == ComunicationErrorCodes.NO_ERROR )
		{
			int max = m_types.Count;
            for (int i = 0; i < max; i++)
            {
                m_managers[i].CleanData();
                if (responseJson.ContainsKey( m_types[i] ))
                {
                    m_managers[i].OnNewStateInfo(responseJson[ m_types[i] ]);
					if (m_managers[i].data.m_eventId > 0 && (!m_managers[i].HasValidDefinition() || m_cacheInfo))
                    {
                        m_managers[i].RequestDefinition();
                    }
                }
            }
			if ( m_quest.EventExists() && m_quest.ShouldRequestProgress() )
				m_quest.RequestProgress();
			if ( m_tournament.EventExists() && m_tournament.ShouldRequestLeaderboard() )
				m_tournament.RequestLeaderboard();

		}
		else
		{
			int max = m_types.Count;
			for (int i = 0; i < max; i++)
            {
                m_managers[i].CleanData();
            }
		}

		m_cacheInfo = false;

		Messenger.Broadcast(MessengerEvents.LIVE_EVENT_STATES_UPDATED);

	}

	/// <summary>
	/// Treats the errors. Returns true if the error invalidates the event
	/// </summary>
	/// <returns><c>true</c>, if errors was treated, <c>false</c> otherwise.</returns>
	protected bool TreatErrors()
	{
		bool ret = false;

		return ret;
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
		m_tournament.Activate();
		m_passive.Deactivate();
		m_quest.Deactivate();
	}

	public void SwitchToQuest()
	{
		m_tournament.Deactivate();
		m_passive.Activate();
		m_quest.Activate();	
	}
}