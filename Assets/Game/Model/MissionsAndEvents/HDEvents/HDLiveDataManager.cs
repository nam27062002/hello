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
using System.Linq;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global singleton manager for global events.
/// </summary>
public class HDLiveDataManager : Singleton<HDLiveDataManager> {


    public enum ErrorCode {
        NONE = 0,

        OFFLINE,
        NOT_INITIALIZED,
        NOT_LOGGED_IN,
        NO_VALID_EVENT,
        EVENT_NOT_ACTIVE
    }

    public enum ComunicationErrorCodes {
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
        USER_IS_NOT_PENDING_REWARDS,//(625,200)

        SEASON_IS_NOT_CLOSED,//(801,200)
        SEASON_NOT_FOUND,// (802)
        LEAGUEDEF_NOT_FOUND,//(803)
        USER_LEAGUE_NOT_FOUND,//(804)
        SEASON_IS_NOT_ACTIVE,//(805)

        NO_ERROR
    };

	private static Dictionary<int, HDLiveDataManager.ComunicationErrorCodes> s_errorCodesDict = new Dictionary<int, ComunicationErrorCodes> {
        {  0, ComunicationErrorCodes.NO_ERROR },
        {  1, ComunicationErrorCodes.NET_ERROR },
        {  2, ComunicationErrorCodes.RESPONSE_NOT_VALID },
        {  3, ComunicationErrorCodes.OTHER_ERROR },
        {  4, ComunicationErrorCodes.NO_RESPONSE },

        { 601, ComunicationErrorCodes.LDATA_NOT_FOUND },
		{ 602, ComunicationErrorCodes.ENTRANCE_FREE_INVALID },
		{ 603, ComunicationErrorCodes.ENTRANCE_AMOUNT_NOT_VALID },
		{ 604, ComunicationErrorCodes.ENTRANCE_TYPE_NOT_VALID },
		{ 605, ComunicationErrorCodes.IS_NOT_A_VALID_TOURNAMENT },
		{ 606, ComunicationErrorCodes.IS_NOT_A_TOURNAMENT },
		{ 607, ComunicationErrorCodes.EVENT_NOT_FOUND },
		{ 608, ComunicationErrorCodes.EVENT_IS_NOT_VALID },
		{ 609, ComunicationErrorCodes.EVENT_IS_DISABLED },
		{ 610, ComunicationErrorCodes.UNEXPECTED_ERROR },
		{ 611, ComunicationErrorCodes.INCONSISTENT_TOURNAMENT_DATA },
		{ 612, ComunicationErrorCodes.ELO_NOT_FOUND },
		{ 613, ComunicationErrorCodes.TOURNAMENT_IS_OVER },
		{ 614, ComunicationErrorCodes.GAMEMODE_NOT_EXISTS },
		{ 615, ComunicationErrorCodes.EMPTY_REQUIRED_PARAMETERS },
		
		{ 617, ComunicationErrorCodes.MATCHMAKING_ERROR },
		{ 618, ComunicationErrorCodes.QUEST_IS_OVER },
		{ 619, ComunicationErrorCodes.IS_NOT_A_QUEST },
		{ 620, ComunicationErrorCodes.EVENT_STILL_ACTIVE },
		{ 621, ComunicationErrorCodes.NOTHING_PENDING },
		{ 622, ComunicationErrorCodes.EVENT_TTL_EXPIRED },
		{ 625, ComunicationErrorCodes.USER_IS_NOT_PENDING_REWARDS },

		{ 801, ComunicationErrorCodes.SEASON_IS_NOT_CLOSED },
		{ 802, ComunicationErrorCodes.SEASON_NOT_FOUND },
		{ 803, ComunicationErrorCodes.LEAGUEDEF_NOT_FOUND },
		{ 804, ComunicationErrorCodes.USER_LEAGUE_NOT_FOUND },
		{ 805, ComunicationErrorCodes.SEASON_IS_NOT_ACTIVE }
	};

    //
    private HDTournamentManager     m_tournament        = new HDTournamentManager();
    private HDQuestManager          m_quest             = new HDQuestManager();
    private HDPassiveEventManager   m_passive           = new HDPassiveEventManager();
    private HDLeagueController      m_league            = new HDLeagueController();
    private HDDiscountEventManager  m_dragonDiscounts   = new HDDiscountEventManager();

    public static HDTournamentManager      tournament      { get { return instance.m_tournament; } }
    public static HDQuestManager           quest           { get { return instance.m_quest; } }
    public static HDPassiveEventManager    passive         { get { return instance.m_passive; } }
    public static HDLeagueController       league          { get { return instance.m_league; } }
    public static HDDiscountEventManager   dragonDiscounts { get { return instance.m_dragonDiscounts; } }
    //


    // Avoid using dictionaries when possible
    private List<HDLiveDataController> m_managers;
    protected long m_lastMyEventsRequestTimestamp = 0;
    protected long m_lastLeaguesRequestTimestamp = 0;

    public const long CACHE_TIMEOUT_MS = 1000 * 60 * 60 * 24 * 7;   // 7 days timeout

    private long m_myEventsRequestMinTim = 1000 * 60 * 5;   // 5 min


#if UNITY_EDITOR
    public static bool TEST_CALLS {
        // get { return DebugSettings.useLiveEventsDebugCalls; }
        get { return true; }
    }
#else
    // Do not touch!
    public static readonly bool TEST_CALLS = false;
#endif

    public HDLiveDataManager() {
        //
        m_managers = new List<HDLiveDataController>();
        m_managers.Add(m_tournament);
        m_managers.Add(m_quest);
        m_managers.Add(m_passive);
        m_managers.Add(m_dragonDiscounts);
        m_managers.Add(m_league);

        Messenger.AddListener<bool>(MessengerEvents.LOGGED, OnLoggedIn);
        Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, SaveEventsToCache);
        Messenger.AddListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, SaveEventsToCacheWithParams);
        Messenger.AddListener(MessengerEvents.QUEST_SCORE_UPDATED, SaveEventsToCache);
    }

    ~HDLiveDataManager() {
        Messenger.RemoveListener<bool>(MessengerEvents.LOGGED, OnLoggedIn);
        Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, SaveEventsToCache);
        Messenger.RemoveListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, SaveEventsToCacheWithParams);
        Messenger.RemoveListener(MessengerEvents.QUEST_SCORE_UPDATED, SaveEventsToCache);
    }

    void OnLoggedIn(bool _isLogged) {
        if (_isLogged) {
            RequestMyLiveData();
        }
    }

    public void LoadEventsFromCache() {
        long cacheTimestamp = 0;
        if (CacheServerManager.SharedInstance.HasKey("hdliveeventstimestamp")) {
            long.TryParse(CacheServerManager.SharedInstance.GetVariable("hdliveeventstimestamp"), out cacheTimestamp);            
        }

        if (GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - cacheTimestamp < CACHE_TIMEOUT_MS) {
            int max = m_managers.Count;
            for (int i = 0; i < max; ++i) {
                LoadEventFromCache(i);
            }
        } else {
            // Delete cache!
            int max = m_managers.Count;
            for (int i = 0; i < max; ++i) {
                m_managers[i].DeleteCache();
            }
        }
    }

    private void LoadEventFromCache(int _index) {
        m_managers[_index].LoadDataFromCache();
    }

    public void SaveEventsToCacheWithParams(int _eventId, HDLiveDataManager.ComunicationErrorCodes _err) {
        SaveEventsToCache();
    }

    public void SaveEventsToCache() {
        int max = m_managers.Count;
        for (int i = 0; i < max; i++) {
            if (m_managers[i].ShouldSaveData()) {
                CacheServerManager.SharedInstance.SetVariable(m_managers[i].type, m_managers[i].SaveData().ToString());
            } else {
                CacheServerManager.SharedInstance.DeleteKey(m_managers[i].type);
            }
        }

        CacheServerManager.SharedInstance.SetVariable("hdliveeventstimestamp", GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong().ToString());
    }

#if UNITY_EDITOR
    public static GameServerManager.ServerResponse CreateTestResponse(string fileName) {
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

    public static GameServerManager.ServerResponse CreateEmptyResponse() {
        GameServerManager.ServerResponse response = new GameServerManager.ServerResponse();
        response.Add("response", "{}");
        return response;
    }

    public delegate void TestResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response);
    public static IEnumerator DelayedCall(string _fileName, TestResponse _testResponse) {
        yield return new WaitForSeconds(0.1f);
        GameServerManager.ServerResponse response = HDLiveDataManager.CreateTestResponse(_fileName);
        _testResponse(null, response);
    }

	public static IEnumerator DelayedGetEventOfTypeCall(string _eventType, TestResponse _testResponse) {
		yield return new WaitForSeconds(0.1f);

		// Load sample json file
		GameServerManager.ServerResponse response = HDLiveDataManager.CreateTestResponse("hd_live_events.json");
		SimpleJSON.JSONClass responseJson = SimpleJSON.JSONNode.Parse(response["response"] as string) as SimpleJSON.JSONClass;

		// Strip other event types
		if(responseJson != null) {
			List<string> keysToDelete = new List<string>();
			ArrayList keys = responseJson.GetKeys();
			for(int i = 0; i < keys.Count; ++i) {
				string key = keys[i] as string;
				if(key != _eventType) keysToDelete.Add(key);
			}

			for(int i = 0; i < keysToDelete.Count; ++i) {
				responseJson.Remove(keysToDelete[i]);	
			}

			response["response"] = responseJson.ToString();
		}

		// Done!
		_testResponse(null, response);
	}


    public static SimpleJSON.JSONNode ResponseErrorCheck(FGOL.Server.Error _error, GameServerManager.ServerResponse _response, out HDLiveDataManager.ComunicationErrorCodes outErr) {
        SimpleJSON.JSONNode ret = null;

        if (_error == null) {
            outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
            if (_response != null && _response["response"] != null) {
                ret = SimpleJSON.JSONNode.Parse(_response["response"] as string);
                if (ret != null) {
                    if (ret.ContainsKey("errorCode")) {
						// Translate error code
						outErr = ErrorCodeIntToEnum(ret["errorCode"].AsInt);
                    }
                } else {
                    outErr = HDLiveDataManager.ComunicationErrorCodes.RESPONSE_NOT_VALID;
                }
            } else {
                outErr = HDLiveDataManager.ComunicationErrorCodes.NO_RESPONSE;
            }
        } else {
            outErr = HDLiveDataManager.ComunicationErrorCodes.NET_ERROR;
        }
        return ret;
    }

    public static void ResponseLog(string call, FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            if (_error != null) {
                ControlPanel.LogError("[" + call + "]" + _error.message, ControlPanel.ELogChannel.LiveData);
            }

            if (_response != null) {
                ControlPanel.Log("[" + call + "]" + _response.ToString(), ControlPanel.ELogChannel.LiveData);
            }
        }
    }



    public bool ShouldRequestMyLiveData() {
        long diff = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - m_lastMyEventsRequestTimestamp;
        return diff > m_myEventsRequestMinTim;
    }

    public bool RequestMyLiveData(bool _force = false) {
        bool ret = false;
        if (_force || ShouldRequestMyLiveData()) {
            m_lastMyEventsRequestTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
            if (TEST_CALLS) {
				ApplicationManager.instance.StartCoroutine(DelayedCall("hd_live_events.json", MyLiveDataResponse));
            } else {
                GameServerManager.SharedInstance.HDEvents_GetMyLiveData(MyLiveDataResponse);
            }
            ret = true;
        }
        return ret;
    }


    public void ForceRequestMyEventType(int _type) {
        if (TEST_CALLS) {
			for(int i = 0; i < m_managers.Count; ++i) {
				if(m_managers[i] is HDLiveEventManager) {
					HDLiveEventManager targetManager = m_managers[i] as HDLiveEventManager;
					if(targetManager.m_numericType == _type) {
						ApplicationManager.instance.StartCoroutine(DelayedGetEventOfTypeCall(targetManager.type, MyLiveDataResponse));
					}
				}
			}
        } else {
            GameServerManager.SharedInstance.HDEvents_GetMyEventOfType(_type, MyLiveDataResponse);
        }
    }

    public void ForceRequestLeagues(bool _force = false) {
        long deltaTime = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - m_lastLeaguesRequestTimestamp;

        if (_force || deltaTime > 1000 * 60 * 0.5f) { // half a minute
            m_lastLeaguesRequestTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();

            if (TEST_CALLS) {
				ApplicationManager.instance.StartCoroutine(DelayedGetEventOfTypeCall(HDLeagueController.TYPE_CODE, MyLiveDataResponse));
            } else {
                GameServerManager.SharedInstance.HDLiveData_GetMyLeagues(MyLiveDataResponse);
            }
        }
    }

    private void MyLiveDataResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {

        ComunicationErrorCodes outErr = ComunicationErrorCodes.NO_ERROR;
        ResponseLog("GetMyEvents", _error, _response);
        SimpleJSON.JSONNode responseJson = ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == ComunicationErrorCodes.NO_ERROR) {
            int max = m_managers.Count;
            for (int i = 0; i < max; i++) {
                bool finishLoadingData = true;
                if (responseJson.ContainsKey(m_managers[i].type)) {
                    // To avoid collecting infinite rewards disabing the network. 
                    // We are going to load the cached data to check if we have
                    // a finish call pending.
                    LoadEventFromCache(i);
                    if (m_managers[i].IsFinishPending()) {
                        finishLoadingData = false;
                    } else { 
                        m_managers[i].LoadData(responseJson[m_managers[i].type]);
                    }
                }

                if (finishLoadingData) {
                    m_managers[i].OnLiveDataResponse();
                }
            }
        } else if (outErr != ComunicationErrorCodes.NET_ERROR) {
            int max = m_managers.Count;
            for (int i = 0; i < max; i++) {
                m_managers[i].CleanData();
            }
        }

        Messenger.Broadcast(MessengerEvents.LIVE_EVENT_STATES_UPDATED);

    }

    /// <summary>
    /// Treats the errors. Returns true if the error invalidates the event
    /// </summary>
    /// <returns><c>true</c>, if errors was treated, <c>false</c> otherwise.</returns>
    protected bool TreatErrors() {
        bool ret = false;

        return ret;
    }

    public bool Connected() {
        bool ret = false;
        if ((CPGlobalEventsTest.networkCheck && DeviceUtilsManager.SharedInstance.internetReachability != NetworkReachability.NotReachable) &&
            (CPGlobalEventsTest.loginCheck && GameSessionManager.SharedInstance.IsLogged())) {
            ret = true;
        }
        return ret;
    }

	public static int ErrorCodeEnumToInt(ComunicationErrorCodes _error) {
		if(s_errorCodesDict.ContainsValue(_error)) {
			// https://stackoverflow.com/questions/2444033/get-dictionary-key-by-value
			return s_errorCodesDict.FirstOrDefault(
				(KeyValuePair<int, ComunicationErrorCodes> _kvp) => {
					return _kvp.Value == _error;
				}
			).Key;
		}
		return -1;
	}

	public static ComunicationErrorCodes ErrorCodeIntToEnum(int _code) {
		if(s_errorCodesDict.ContainsKey(_code)) {
			return s_errorCodesDict[_code];
		}
		return ComunicationErrorCodes.OTHER_ERROR;
	}

    //--------------------------------------------------------------------------

    public void ApplyDragonMods() {
        for (int i = 0; i < m_managers.Count; i++) {
            if (m_managers[i].isActive) {
                m_managers[i].ApplyDragonMods();
            }
        }
    }

    public void SwitchToTournament() {
        m_tournament.Activate();
        m_passive.Deactivate();
        m_quest.Deactivate();
        m_league.Deactivate();
        m_dragonDiscounts.Deactivate();
    }

    public void SwitchToQuest() {
        m_tournament.Deactivate();
        m_league.Deactivate();

        if (UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN)) {
            m_passive.Activate();
            m_quest.Activate();
            m_dragonDiscounts.Activate();
        }
    }

    public void SwitchToLeague() {
        m_tournament.Deactivate();
        m_passive.Deactivate();
        m_quest.Deactivate();
        m_league.Activate();
        m_dragonDiscounts.Activate();
    }

    // You have to activate the events first to allow for Later mods activation
    public void ApplyLaterMods()
    {
        // Every Activate Later Mods already checks if the event is active
        m_tournament.ActivateLaterMods();
        m_passive.ActivateLaterMods();
        m_quest.ActivateLaterMods();
        m_league.ActivateLaterMods();
        m_dragonDiscounts.ActivateLaterMods();
    }

    // Deactivate later mods
    public void RemoveLaterMods()
    {
        // Every Deactivate Later Mods already checks if the event is active
        m_tournament.DeactivateLaterMods();
        m_passive.DeactivateLaterMods();
        m_quest.DeactivateLaterMods();
        m_league.DeactivateLaterMods();
        m_dragonDiscounts.DeactivateLaterMods();
    }
}