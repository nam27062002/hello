﻿using System;

public class HDSeasonData {
    //---[Classes and Enums]----------------------------------------------------

    public enum State {
        NONE = 0,
        TEASING,
        NOT_JOINED,
        JOINED,
        CLOSED,
        PENDING_REWARDS,
        FINALIZING,
        FINALIZED
    }



    //---[Basic Data]-----------------------------------------------------------

    private int m_code;

    private DateTime m_startDate;
    private DateTime m_closeDate;
    private DateTime m_endDate;

    private long m_score;
    public long score { get { return m_score; } }

    public HDLeagueData currentLeague { get; set; }
    public HDLeagueData nextLeague { get; set; }

    private int m_rewardIndex;

    public State state { get; private set; }

    public HDLiveData.State liveDataState { get; private set; }
    public HDLiveDataManager.ComunicationErrorCodes liveDataError { get; private set; }

    public HDLiveData.State scoreDataState { get; private set; }
    public HDLiveDataManager.ComunicationErrorCodes scoreDataError { get; private set; }

    public HDLiveData.State rewardDataState { get; private set; }
    public HDLiveDataManager.ComunicationErrorCodes rewardDataError { get; private set; }

    public HDLiveData.State finalizeDataState { get; private set; }
    public HDLiveDataManager.ComunicationErrorCodes finalizeDataError { get; private set; }



    //---[Methods]--------------------------------------------------------------

    public HDSeasonData() {
        Clean();
    }

    public void Clean() {
        m_code = -1;

        m_startDate = new DateTime(1970, 1, 1);
        m_closeDate = new DateTime(1970, 1, 1);
        m_endDate = new DateTime(1970, 1, 1);

        m_score = 0;

        currentLeague = null;
        nextLeague = null;

        m_rewardIndex = -1;

        state = State.NONE;

        liveDataState = HDLiveData.State.EMPTY;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;

        scoreDataState = HDLiveData.State.EMPTY;
        scoreDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;

        rewardDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        rewardDataState = HDLiveData.State.EMPTY;

        finalizeDataState = HDLiveData.State.EMPTY;
        finalizeDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }



    //---[Initialization]-------------------------------------------------------

    public void LoadData(SimpleJSON.JSONNode _data) {
        int status = _data["status"];
        switch (status) {
            case 0: state = State.NOT_JOINED; break;
            case 1: state = State.JOINED; break;
            case 2: state = State.PENDING_REWARDS; break;
            case 6: state = State.CLOSED; break;
            //....
        }

        LoadDates(_data);

        liveDataState = HDLiveData.State.PARTIAL;
    }

    public void RequestFullData(bool _fetchLeaderboard) {
        GameServerManager.SharedInstance.HDLeagues_GetSeason(_fetchLeaderboard, OnRequestFullData);

        liveDataState = HDLiveData.State.WAITING_RESPONSE;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    private void OnRequestFullData(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Season Full Data", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            // parse Json
            LoadFullData(responseJson.AsArray);
        } else {

            liveDataState = HDLiveData.State.ERROR;
        }

        liveDataError = outErr;
    }

    private void LoadFullData(SimpleJSON.JSONNode _data) {
        m_code = _data["id"];

        LoadDates(_data);

        currentLeague.LoadData(_data["league"]);

        if (_data.ContainsKey("leaderboard")) {
            currentLeague.leaderboard.LoadData(_data["leaderboard"].AsArray);
        }

        liveDataState = HDLiveData.State.VALID;
    }

    private void LoadDates(SimpleJSON.JSONNode _data) {
        m_startDate = TimeUtils.TimestampToDate(_data["startDate"].AsLong);
        m_closeDate = TimeUtils.TimestampToDate(_data["closeDate"].AsLong);
        m_endDate = TimeUtils.TimestampToDate(_data["endDate"].AsLong);

        if (timeToStart.TotalSeconds > 0f) {
            state = State.TEASING;
        } else {
            if (timeToEnd.TotalSeconds < 0f) {
                if (state < State.FINALIZED)
                    state = State.FINALIZED;
            } else {
                if (timeToClose.TotalSeconds < 0f) {
                    if (state < State.CLOSED)
                        state = State.CLOSED;
                }
            }
        }
    }



    //---[Score]----------------------------------------------------------------

    public void SentScore(long _score) {
        if (state >= State.NOT_JOINED && state < State.CLOSED) {
            GameServerManager.SharedInstance.HDLeagues_SetScore(_score, OnSetScore);

            scoreDataState = HDLiveData.State.WAITING_RESPONSE;
            scoreDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        }
    }

    private void OnSetScore(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Season Set Score", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            if (state < State.CLOSED) {
                state = State.JOINED;
            }
            scoreDataState = HDLiveData.State.VALID;
        } else {
            scoreDataState = HDLiveData.State.ERROR;
        }

        scoreDataError = outErr;
    }



    //---[Rewards]--------------------------------------------------------------

    public void RequestMyRewards() {
        if (state == State.PENDING_REWARDS) {
            GameServerManager.SharedInstance.HDLeagues_GetMyRewards(OnRequestMyRewards);
           
            rewardDataState = HDLiveData.State.WAITING_RESPONSE;
            rewardDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        }
    }

    private void OnRequestMyRewards(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Season Rewards", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            // parse Json
            m_rewardIndex = responseJson["index"].AsInt;

            rewardDataState = HDLiveData.State.VALID;
        } else {
            m_rewardIndex = -1;
            rewardDataState = HDLiveData.State.ERROR;
        }

        rewardDataError = outErr;
    }



    //---[Finalize my Season]---------------------------------------------------

    public void RequestFinalize() {
        GameServerManager.SharedInstance.HDLeagues_FinishMySeason(OnRequestFinalize);
        state = State.FINALIZING;

        finalizeDataState = HDLiveData.State.WAITING_RESPONSE;
        finalizeDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    private void OnRequestFinalize(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Season Finalize", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            state = State.FINALIZED;
            finalizeDataState = HDLiveData.State.VALID;
        } else {

            finalizeDataState = HDLiveData.State.ERROR;
        }

        finalizeDataError = outErr;
    }



    //---[Query Methods]--------------------------------------------------------

    public TimeSpan timeToStart { get { return m_startDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }
    public TimeSpan timeToClose { get { return m_closeDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }
    public TimeSpan timeToEnd   { get { return m_endDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }

    public Metagame.Reward reward {
        get {
            Metagame.Reward r = null;

            if (state == State.PENDING_REWARDS && rewardDataState == HDLiveData.State.VALID) {
                r = currentLeague.GetReward(m_rewardIndex);
            }

            return r;
        }
    }
}
