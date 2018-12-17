using System;
using System.Collections.Generic;
using UnityEngine;

public class HDSeasonData {
    //---[Classes and Enums]----------------------------------------------------
    public enum State {
        NONE = 0,
        NOT_JOINED,
        JOINED,
        CLOSED,
        PENDING_REWARDS,
        FINALIZED
    }


    //---[Basic Data]-----------------------------------------------------------
    private int m_code;

    private DateTime m_startDate;
    public TimeSpan timeToStart { get { return m_startDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }

    private DateTime m_closeDate;
    public TimeSpan timeToClose { get { return m_closeDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }

    private DateTime m_endDate;
    public TimeSpan timeToEnd { get { return m_endDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }

    public HDLeagueData currentLeague { get; set; }
    public HDLeagueData nextLeague { get; set; }

    private State m_state;

    private HDLiveData.State m_liveDataState;
    public HDLiveData.State liveDataState { get { return m_liveDataState; } }



    //---[Methods]--------------------------------------------------------------
    public HDSeasonData() {
        m_code = -1;

        m_startDate = new DateTime(1970, 1, 1);
        m_closeDate = new DateTime(1970, 1, 1);
        m_endDate = new DateTime(1970, 1, 1);

        currentLeague = null;
        nextLeague = null;

        m_state = State.NONE;
        m_liveDataState = HDLiveData.State.EMPTY;
    }

    public void LoadData(SimpleJSON.JSONNode _data) {
        int status = _data["status"];
        switch (status) {
            case 0: m_state = State.NOT_JOINED; break;
            case 1: m_state = State.JOINED; break;
            case 2: m_state = State.PENDING_REWARDS; break;
            case 6: m_state = State.CLOSED; break;
            //....
        }

        m_liveDataState = HDLiveData.State.PARTIAL;
    }

    public void RequestFullData(bool _fetchLeaderboard) {
        GameServerManager.SharedInstance.HDLeagues_GetSeason(_fetchLeaderboard, OnRequestFullData);

        m_liveDataState = HDLiveData.State.WAITING_RESPONSE;
    }

    private void OnRequestFullData(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Season", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            // parse Json
            LoadFullData(responseJson.AsArray);
        } else {

            m_liveDataState = HDLiveData.State.ERROR;
        }
    }

    private void LoadFullData(SimpleJSON.JSONNode _data) {
        m_code = _data["id"];

        m_startDate = TimeUtils.TimestampToDate(_data["startDate"].AsLong);
        m_closeDate = TimeUtils.TimestampToDate(_data["closeDate"].AsLong);
        m_endDate = TimeUtils.TimestampToDate(_data["endDate"].AsLong);

        currentLeague.LoadData(_data["league"]);

        if (_data.ContainsKey("leaderboard")) {
            currentLeague.leaderboard.LoadData(_data["leaderboard"].AsArray);
        }

        m_liveDataState = HDLiveData.State.VALID;
    }
}
