using System;

public class HDSeasonData {
    //---[Classes and Enums]----------------------------------------------------

    public enum State {
        NONE = 0,
        TEASING,
        NOT_JOINED,         //(0)
        JOINED,             //(1)
        PENDING_REWARDS,    //(2)
        REWARDS_COLLECTED,
        WAITING_NEW_SEASON  //(11)
    }



    //---[Basic Data]-----------------------------------------------------------

    private DateTime m_startDate;
    private DateTime m_closeDate;
    private DateTime m_endDate;

    public HDLeagueData currentLeague { get; set; }		// Can be null
    public HDLeagueData nextLeague { get; set; }		// Can be null

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
        m_startDate = new DateTime(1970, 1, 1);
        m_closeDate = new DateTime(1970, 1, 1);
        m_endDate   = new DateTime(1970, 1, 1);

        currentLeague = null;
        nextLeague = null;

        m_rewardIndex = -1;

        state = State.NONE;

        liveDataState = HDLiveData.State.EMPTY;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;

        scoreDataState = HDLiveData.State.EMPTY;
        scoreDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;

        rewardDataState = HDLiveData.State.EMPTY;
        rewardDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;

        finalizeDataState = HDLiveData.State.EMPTY;
        finalizeDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }



    //---[Initialization]-------------------------------------------------------

    public void RequestData(bool _fetchLeaderboard) {
        __RequestFullData(_fetchLeaderboard);

        liveDataState = HDLiveData.State.WAITING_RESPONSE;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    public void UpdateState() {
        if (timeToStart.TotalSeconds > 0f) {
            state = State.TEASING;
        } else {
            if (timeToEnd.TotalSeconds < 0f) {
                if (state == State.REWARDS_COLLECTED)
                    state = State.WAITING_NEW_SEASON;
            } else {
                if (timeToClose.TotalSeconds < 0f) {
                    if (state < State.JOINED)
                        state = State.WAITING_NEW_SEASON;
                    else if (state < State.PENDING_REWARDS)
                        state = State.PENDING_REWARDS;
                } else {
                    if (state == State.TEASING) {
                        state = State.NOT_JOINED;
                    }
                }
            }
        }
    }

    private void OnRequestData(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Season Data", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            // parse Json
            LoadData(responseJson);
        } else {

            liveDataState = HDLiveData.State.ERROR;
        }

        liveDataError = outErr;
    }

    private void LoadData(SimpleJSON.JSONNode _data) {
        int status = _data["status"].AsInt;
        switch (status) {
            case 0: state = State.NOT_JOINED; break;
            case 1: state = State.JOINED; break;
            case 2: state = State.PENDING_REWARDS; break;
            case 11: state = State.WAITING_NEW_SEASON; break;
        }

        LoadDates(_data);

        currentLeague.LoadData(_data["league"]);

        if (_data.ContainsKey("leaderboard")) {
            currentLeague.leaderboard.LoadData(_data["leaderboard"]);
        }

        liveDataState = HDLiveData.State.VALID;
    }

    private void LoadDates(SimpleJSON.JSONNode _data) {
        m_startDate = TimeUtils.TimestampToDate(_data["startTimestamp"].AsLong);
        m_closeDate = TimeUtils.TimestampToDate(_data["closeTimestamp"].AsLong);
        m_endDate = TimeUtils.TimestampToDate(_data["endTimestamp"].AsLong);

        UpdateState();
    }



    //---[Score]----------------------------------------------------------------

    public void SentScore(long _score) {
        if (state >= State.NOT_JOINED && state < State.PENDING_REWARDS) {
            currentLeague.leaderboard.playerScore = _score;
            __SentScore(_score);

            scoreDataState = HDLiveData.State.WAITING_RESPONSE;
            scoreDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        }
    }

    private void OnSetScore(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Season Set Score", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            if (state < State.JOINED) {
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
            __RequestMyRewards();

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
        __RequestFinalize();

        finalizeDataState = HDLiveData.State.WAITING_RESPONSE;
        finalizeDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    private void OnRequestFinalize(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Season Finalize", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            state = State.WAITING_NEW_SEASON;
            finalizeDataState = HDLiveData.State.VALID;
        } else {
            finalizeDataState = HDLiveData.State.ERROR;
        }

        finalizeDataError = outErr;
    }



    //---[Query Methods]--------------------------------------------------------

    public TimeSpan timeToStart { get { return m_startDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }
    public TimeSpan timeToClose { get { return m_closeDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }
    public TimeSpan timeToEnd   { get { return m_endDate   - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }
	public TimeSpan duration { get { return m_closeDate - m_startDate; }}

    public Metagame.Reward reward {
        get {
            Metagame.Reward r = null;

            if (state == State.PENDING_REWARDS && rewardDataState == HDLiveData.State.VALID) {
                r = currentLeague.GetReward(m_rewardIndex);

                state = State.REWARDS_COLLECTED;
            }

            return r;
        }
    }



    //---[Server Calls]---------------------------------------------------------

    private void __RequestFullData(bool _fetchLeaderboard) {
        if (HDLiveDataManager.TEST_CALLS) {
            ApplicationManager.instance.StartCoroutine(HDLiveDataManager.DelayedCall("league_season_data.json", OnRequestData));
        } else {
            GameServerManager.SharedInstance.HDLeagues_GetSeason(_fetchLeaderboard, OnRequestData);
        }
    }

    private void __SentScore(long _score) {
        if (HDLiveDataManager.TEST_CALLS) {
            OnSetScore(null, HDLiveDataManager.CreateEmptyResponse());
        } else {
            GameServerManager.SharedInstance.HDLeagues_SetScore(_score, OnSetScore);
        }
    }

    private void __RequestMyRewards() {
        if (HDLiveDataManager.TEST_CALLS) {
            ApplicationManager.instance.StartCoroutine(HDLiveDataManager.DelayedCall("league_get_reward_data.json", OnRequestMyRewards));
        } else {
            GameServerManager.SharedInstance.HDLeagues_GetMyRewards(OnRequestMyRewards);
        }
    }

    private void __RequestFinalize() {
        if (HDLiveDataManager.TEST_CALLS) {
            OnSetScore(null, HDLiveDataManager.CreateEmptyResponse());
        } else {
            GameServerManager.SharedInstance.HDLeagues_FinishMySeason(OnRequestFinalize);
        }
    }
}