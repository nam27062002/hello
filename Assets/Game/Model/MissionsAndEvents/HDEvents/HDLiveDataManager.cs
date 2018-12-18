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

        NO_ERROR
    };

    public HDTournamentManager m_tournament = new HDTournamentManager();
    public HDQuestManager m_quest = new HDQuestManager();
    public HDPassiveEventManager m_passive = new HDPassiveEventManager();
    public HDLeagueController m_league = new HDLeagueController();
    public HDDiscountEventManager m_dragonDiscounts = new HDDiscountEventManager();

    // Avoid using dictionaries when possible
    private List<HDLiveDataController> m_managers;
    protected long m_lastMyEventsRequestTimestamp = 0;

    public const long CACHE_TIMEOUT_MS = 1000 * 60 * 60 * 24 * 7;   // 7 days timeout

    private long m_myEventsRequestMinTim = 1000 * 60 * 5;   // 5 min


#if UNITY_EDITOR
    public static bool TEST_CALLS {
        get { return DebugSettings.useLiveEventsDebugCalls; }
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
            cacheTimestamp = long.Parse(CacheServerManager.SharedInstance.GetVariable("hdliveeventstimestamp"));
        }

        if (GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - cacheTimestamp < CACHE_TIMEOUT_MS) {
            int max = m_managers.Count;
            for (int i = 0; i < max; ++i) {
                m_managers[i].LoadDataFromCache();
            }
        } else {
            // Delete cache!
            int max = m_managers.Count;
            for (int i = 0; i < max; ++i) {
                m_managers[i].DeleteCache();
            }
        }
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

    protected delegate void TestResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response);
    protected IEnumerator DelayedCall(string _fileName, TestResponse _testResponse) {
        yield return new WaitForSeconds(0.1f);
        GameServerManager.ServerResponse response = HDLiveDataManager.CreateTestResponse(_fileName);
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
                        outErr = HDLiveDataManager.ComunicationErrorCodes.OTHER_ERROR;
                        int errorInt = ret["errorCode"];
                        switch (errorInt) {
                            case 601: outErr = HDLiveDataManager.ComunicationErrorCodes.LDATA_NOT_FOUND; break;
                            case 602: outErr = HDLiveDataManager.ComunicationErrorCodes.ENTRANCE_FREE_INVALID; break;
                            case 603: outErr = HDLiveDataManager.ComunicationErrorCodes.ENTRANCE_AMOUNT_NOT_VALID; break;
                            case 604: outErr = HDLiveDataManager.ComunicationErrorCodes.ENTRANCE_TYPE_NOT_VALID; break;
                            case 605: outErr = HDLiveDataManager.ComunicationErrorCodes.IS_NOT_A_VALID_TOURNAMENT; break;
                            case 606: outErr = HDLiveDataManager.ComunicationErrorCodes.IS_NOT_A_TOURNAMENT; break;
                            case 607: outErr = HDLiveDataManager.ComunicationErrorCodes.EVENT_NOT_FOUND; break;
                            case 608: outErr = HDLiveDataManager.ComunicationErrorCodes.EVENT_IS_NOT_VALID; break;
                            case 609: outErr = HDLiveDataManager.ComunicationErrorCodes.EVENT_IS_DISABLED; break;
                            case 610: outErr = HDLiveDataManager.ComunicationErrorCodes.UNEXPECTED_ERROR; break;
                            case 611: outErr = HDLiveDataManager.ComunicationErrorCodes.INCONSISTENT_TOURNAMENT_DATA; break;
                            case 612: outErr = HDLiveDataManager.ComunicationErrorCodes.ELO_NOT_FOUND; break;
                            case 613: outErr = HDLiveDataManager.ComunicationErrorCodes.TOURNAMENT_IS_OVER; break;
                            case 614: outErr = HDLiveDataManager.ComunicationErrorCodes.GAMEMODE_NOT_EXISTS; break;
                            case 615: outErr = HDLiveDataManager.ComunicationErrorCodes.EMPTY_REQUIRED_PARAMETERS; break;
                            // case 616: outErr = HDLiveDataManager.ComunicationErrorCodes.EMPTY_REQUIRED_PARAMETERS;break;
                            case 617: outErr = HDLiveDataManager.ComunicationErrorCodes.MATCHMAKING_ERROR; break;
                            case 618: outErr = HDLiveDataManager.ComunicationErrorCodes.QUEST_IS_OVER; break;
                            case 619: outErr = HDLiveDataManager.ComunicationErrorCodes.IS_NOT_A_QUEST; break;
                            case 620: outErr = HDLiveDataManager.ComunicationErrorCodes.EVENT_STILL_ACTIVE; break;
                            case 621: outErr = HDLiveDataManager.ComunicationErrorCodes.NOTHING_PENDING; break;
                            case 622: outErr = HDLiveDataManager.ComunicationErrorCodes.EVENT_TTL_EXPIRED; break;
                        }
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
            ApplicationManager.instance.StartCoroutine(DelayedCall("hd_live_events.json", MyLiveDataResponse));
        } else {
            GameServerManager.SharedInstance.HDEvents_GetMyEventOfType(_type, MyLiveDataResponse);
        }
    }

    private void MyLiveDataResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {

        ComunicationErrorCodes outErr = ComunicationErrorCodes.NO_ERROR;
        ResponseLog("GetMyEvents", _error, _response);
        SimpleJSON.JSONNode responseJson = ResponseErrorCheck(_error, _response, out outErr);
        if (outErr == ComunicationErrorCodes.NO_ERROR) {
            int max = m_managers.Count;
            for (int i = 0; i < max; i++) {
                if (responseJson.ContainsKey(m_managers[i].type)) {
                    m_managers[i].LoadData(responseJson[m_managers[i].type]);
                }

                m_managers[i].OnLiveDataResponse();
            }
        } else {
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
        if ((CPGlobalEventsTest.networkCheck && Application.internetReachability != NetworkReachability.NotReachable) &&
            (CPGlobalEventsTest.loginCheck && GameSessionManager.SharedInstance.IsLogged())) {
            ret = true;
        }
        return ret;
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
    }

    public void SwitchToQuest() {
        m_tournament.Deactivate();
        m_passive.Activate();
        m_quest.Activate();
        m_league.Deactivate();
    }

    public void SwitchToLeague() {
        m_tournament.Deactivate();
        m_passive.Deactivate();
        m_quest.Deactivate();
        m_league.Activate();
    }
}