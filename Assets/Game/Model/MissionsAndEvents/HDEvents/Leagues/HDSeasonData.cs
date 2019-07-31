using UnityEngine;
using System;
using System.Collections.Generic;

public class HDSeasonData {
	//---[Constants]------------------------------------------------------------
	// Workaround to be able to define a timespan of the Teasing state. We will store
	// in the local device the first date where we receive the data of a season together with 
	// that season's End timestamp.
	// Whenever we receive a season whose Start timestamp is bigger than the stored End timestamp,
	// we will assume it's a different season than the one stored and will overwrite the
	// SEASON_DATA_RECEIVED_TIMESTAMP var.
	private const string DATA_RECEIVED_TIMESTAMP_KEY = "HDSeasonData.DATA_RECEIVED_TIMESTAMP";
	private const string CACHED_END_TIMESTAMP_KEY = "HDSeasonData.CACHED_END_TIMESTAMP";

    public enum State {
        NONE = 0,
        TEASING,
        NOT_JOINED,         //(0)
        JOINED,             //(1)
        WAITING_RESULTS,
        PENDING_REWARDS,    //(2)
        REWARDS_COLLECTED,
        WAITING_NEW_SEASON  //(11)
    }

	public enum Result {
		UNKNOWN = 0,
		PROMOTION,
		DEMOTION,
		NO_CHANGE
	}



    //---[Basic Data]-----------------------------------------------------------

    private DateTime m_startDate;
    private DateTime m_closeDate;
    private DateTime m_resultDate;
    private DateTime m_endDate;

	private DateTime m_dataReceivedDate;
	private DateTime m_cachedEndDate;

    public HDLeagueData currentLeague { get; set; }		// Can be null
    public HDLeagueData nextLeague { get; set; }		// Can be null

    public RangeInt promoteRange { get; set; }
    public RangeInt demoteRange { get; set; }

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
        m_resultDate = new DateTime(1970, 1, 1);
        m_endDate   = new DateTime(1970, 1, 1);
		m_dataReceivedDate = GameServerManager.SharedInstance.GetEstimatedServerTime();
		m_cachedEndDate = new DateTime(1970, 1, 1);

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
        Clean();
        __RequestFullData(_fetchLeaderboard);

        liveDataState = HDLiveData.State.WAITING_RESPONSE;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    public void LoadStatus(SimpleJSON.JSONNode _data) {
        string status = _data["status"];
        switch (status) {
            case "0":
            case "NOT_JOINED":
                state = State.NOT_JOINED; 
                break;

            case "1":
            case "JOINED":
                state = State.JOINED; 
                break;

            case "WAITING_RESULTS":
                state = State.WAITING_RESULTS;
                break;

            case "2":
            case "PENDING_REWARDS":
                if (state < State.REWARDS_COLLECTED) {
                    state = State.PENDING_REWARDS;
                }
                break;

            case "REWARDS_COLLECTED":
                state = State.REWARDS_COLLECTED;
                break;

            case "11":
            case "WAITING_NEW_SEASON":
                state = State.WAITING_NEW_SEASON; 
                break;
        }

        liveDataState = HDLiveData.State.PARTIAL;
    }

    public void UpdateState() {
        if (liveDataState == HDLiveData.State.VALID) {
            if (timeToStart.TotalSeconds > 0f) {
                state = State.TEASING;
            } else {
                bool closed = timeToClose.TotalSeconds <= 0f;
                bool results = timeToResuts.TotalSeconds <= 0f;
                bool ended = timeToEnd.TotalSeconds <= 0f;

                switch (state) {
                    case State.TEASING:
                    case State.NOT_JOINED: {
                            if (closed) {
                                nextLeague = currentLeague;
                                state = State.WAITING_NEW_SEASON;
                            }
                        }
                        break;

                    case State.JOINED:
                    case State.WAITING_RESULTS: {
                            if (ended || results) {
                                state = State.PENDING_REWARDS;
                            } else if (closed) {
                                state = State.WAITING_RESULTS;
                            }
                        }
                        break;

                    case State.PENDING_REWARDS: { }
                        break;

                    case State.REWARDS_COLLECTED: {
                            if (ended || results) {
                                state = State.WAITING_NEW_SEASON;
                            }
                        }
                        break;

                    case State.WAITING_NEW_SEASON: { }
                        break;
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
        LoadStatus(_data);
        LoadDates(_data);

        promoteRange = new RangeInt();
        if (_data.ContainsKey("promoteRange")) {
            promoteRange.min = _data["promoteRange"]["lower"].AsInt;
            promoteRange.max = _data["promoteRange"]["upper"].AsInt;
        }

        demoteRange = new RangeInt();
        if (_data.ContainsKey("demoteRange")) {
            demoteRange.min = _data["demoteRange"]["lower"].AsInt;
            demoteRange.max = _data["demoteRange"]["upper"].AsInt;
        }

        currentLeague = HDLiveDataManager.league.GetLeagueData(_data["league"]["order"].AsInt);
        currentLeague.LoadData(_data["league"]);

        if (_data.ContainsKey("nextLeague")) {
            nextLeague = HDLiveDataManager.league.GetLeagueData(_data["nextLeague"]["order"].AsInt);
        }

        if (_data.ContainsKey("leaderboard")) {
            currentLeague.leaderboard.LoadData(_data["leaderboard"]);
        }

        liveDataState = HDLiveData.State.VALID;
    }

    private void LoadDates(SimpleJSON.JSONNode _data) {
        m_startDate = TimeUtils.TimestampToDate(_data["startTimestamp"].AsLong);
        m_closeDate = TimeUtils.TimestampToDate(_data["closeTimestamp"].AsLong);
        m_resultDate = TimeUtils.TimestampToDate(_data["extraTimeTimestamp"].AsLong); 
        m_endDate = TimeUtils.TimestampToDate(_data["endTimestamp"].AsLong);

		// Update cached dates
		m_dataReceivedDate = Prefs.GetDateTimePlayer(DATA_RECEIVED_TIMESTAMP_KEY, m_dataReceivedDate);
		m_cachedEndDate = Prefs.GetDateTimePlayer(CACHED_END_TIMESTAMP_KEY, m_cachedEndDate);

		// Is it a different season from the cached one?
		if(m_cachedEndDate < m_startDate) {		// Season that occurred in the past
			// It's a new season! Update cached vars
			m_dataReceivedDate = GameServerManager.SharedInstance.GetEstimatedServerTime();
			Prefs.SetDateTimePlayer(DATA_RECEIVED_TIMESTAMP_KEY, m_dataReceivedDate);
			Prefs.SetDateTimePlayer(CACHED_END_TIMESTAMP_KEY, m_endDate);
		}

        UpdateState();
    }



    //---[Score]----------------------------------------------------------------

    public void SetScore(long _score, bool _fetchLeaderboard) {
        if (state >= State.NOT_JOINED && state < State.PENDING_REWARDS) {
            currentLeague.leaderboard.playerScore = _score;
            __SetScore(_score, _fetchLeaderboard);

            scoreDataState = HDLiveData.State.WAITING_RESPONSE;
        } else {
            if (_fetchLeaderboard) {
                if (currentLeague != null) {
                    currentLeague.leaderboard.RequestData();
                }
            }
            scoreDataState = HDLiveData.State.VALID;
        }
        scoreDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    private void OnSetScore(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Season Set Score", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            if (state < State.JOINED) {
                state = State.JOINED;
            }

            if (currentLeague != null) {
                currentLeague.leaderboard.LoadData(responseJson["leaderboard"]);
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
        int errorCode = -1;
        String responseStr = "";
        try {
            errorCode = 0;
            HDLiveDataManager.ResponseLog("[Leagues] Season Rewards", _error, _response);

            errorCode = 1;
            HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
            errorCode = 2;
            SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);
            if (responseJson != null) {
                responseStr = responseJson.ToString();
            }

            errorCode = 3;
            if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
                errorCode = 4;
                // parse Json
                m_rewardIndex = responseJson["index"].AsInt;

                errorCode = 5;
                if (responseJson.ContainsKey("rank")) {
                    //TODO: maybe the leaderboard is null?
                    errorCode = 6;
                    currentLeague.leaderboard.playerRank = responseJson["rank"].AsInt;
                }

                errorCode = 7;
                if (responseJson.ContainsKey("nextLeague")) {
                    errorCode = 8;
                    SetNextLeague(responseJson["nextLeague"]["sku"]);
                } else {
                    errorCode = 9;
                    FindNextLeague();
                }
                errorCode = 10;
                HDTrackingManager.Instance.Notify_LabResult(currentLeague.leaderboard.playerRank, currentLeague.sku, nextLeague.sku);

                errorCode = 11;
                rewardDataState = HDLiveData.State.VALID;
            } else {
                m_rewardIndex = -1;
                rewardDataState = HDLiveData.State.ERROR;
            }

            errorCode = 12;
            rewardDataError = outErr;
        } catch(Exception e) {
            throw new System.Exception("HDseasonData.OnRequestMyRewards: " + errorCode + "\n" + responseStr + "\n" + e);
        }
    }

    private void SetNextLeague(string _sku) {
        HDLeagueController leagues = HDLiveDataManager.league;
        int leaguesCount = leagues.leaguesCount;

        for (int i = 0; i < leaguesCount; ++i) {
            HDLeagueData league = leagues.GetLeagueData(i);

            if (league.sku.Equals(_sku)) {
                nextLeague = league;
                break;
            }
        }
    }

    private void FindNextLeague() {
        HDLeagueController leagues = HDLiveDataManager.league;
        int leaguesCount = leagues.leaguesCount;
        int rank = currentLeague.leaderboard.playerRank;

        // default case
        nextLeague = currentLeague;

        if (promoteRange.min <= rank && rank < promoteRange.max) {
            if (currentLeague.order < leaguesCount - 1) {
                nextLeague = leagues.GetLeagueData(currentLeague.order + 1);
            }
        } else if (demoteRange.min <= rank && rank < demoteRange.max) {
            if (currentLeague.order > 0) {
                nextLeague = leagues.GetLeagueData(currentLeague.order - 1);
            }
        }
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
            HDLiveDataManager.instance.SaveEventsToCache();

            finalizeDataState = HDLiveData.State.VALID;
        } else {
            finalizeDataState = HDLiveData.State.ERROR;
        }

        finalizeDataError = outErr;
    }



    //---[Query Methods]--------------------------------------------------------

    public TimeSpan timeToStart             { get { return m_startDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }
    public TimeSpan timeToClose             { get { return m_closeDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }
    public TimeSpan timeToResuts            { get { return m_resultDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }
    public TimeSpan timeToEnd               { get { return m_endDate   - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }
	public TimeSpan duration                { get { return m_closeDate - m_startDate; } }
    public TimeSpan durationWaitResults     { get { return m_resultDate - m_closeDate; } }
    public TimeSpan durationWaitNewSeason   { get { return m_endDate - m_closeDate; } }
	public TimeSpan durationTeasing			{ get { return m_startDate - m_dataReceivedDate; } }

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

	public Result result {
		get {
			if(nextLeague == null) {
				return Result.UNKNOWN;
			} else {
				if(nextLeague > currentLeague) {
					return Result.PROMOTION;
				} else if(nextLeague < currentLeague) {
					return Result.DEMOTION;
				} else {
					return Result.NO_CHANGE;
				}
			}
		}
	}

	public bool IsRunning() {
        return (state >= State.NOT_JOINED && state < State.PENDING_REWARDS);
    }



    //---[Server Calls]---------------------------------------------------------

    private void __RequestFullData(bool _fetchLeaderboard) {
        if (HDLiveDataManager.TEST_CALLS) {
            ApplicationManager.instance.StartCoroutine(HDLiveDataManager.DelayedCall("league_season_data.json", OnRequestData));
        } else {
            GameServerManager.SharedInstance.HDLeagues_GetSeason(_fetchLeaderboard, OnRequestData);
        }
    }

    private void __SetScore(long _score, bool _fetchLeaderboard) {
        if (HDLiveDataManager.TEST_CALLS) {
            OnSetScore(null, HDLiveDataManager.CreateEmptyResponse());
        } else {
            IDragonData dragonData = DragonManager.CurrentDragon;
            SimpleJSON.JSONClass build = new SimpleJSON.JSONClass();
            {
                build.Add("dragon", dragonData.sku);
                build.Add("skin", UsersManager.currentUser.GetEquipedDisguise(dragonData.sku));

                List<string> equipedPets = UsersManager.currentUser.GetEquipedPets(dragonData.sku);
                if (equipedPets.Count > 0) {
                    SimpleJSON.JSONArray pets = new SimpleJSON.JSONArray();
                    {
                        int max = equipedPets.Count;
                        for (int i = 0; i < max; i++) {
                            pets.Add(equipedPets[i]);
                        }
                    }
                    build.Add("pets", pets);
                }

                if (dragonData is DragonDataClassic) {
                    DragonDataClassic classicData = dragonData as DragonDataClassic;
                    build.Add("level", classicData.progression.level);
                } else {
                    DragonDataSpecial specialData = dragonData as DragonDataSpecial;
                    build.Add("level", specialData.Level);

                    SimpleJSON.JSONClass stats = new SimpleJSON.JSONClass();
                    {
                        stats.Add("health", specialData.GetStat(DragonDataSpecial.Stat.HEALTH).level);
                        stats.Add("speed",  specialData.GetStat(DragonDataSpecial.Stat.SPEED).level);
                        stats.Add("energy", specialData.GetStat(DragonDataSpecial.Stat.ENERGY).level);
                    }
                    build.Add("stats", stats);
                }
            }

            GameServerManager.SharedInstance.HDLeagues_SetScore(_score, build, _fetchLeaderboard, OnSetScore);
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
            OnRequestFinalize(null, HDLiveDataManager.CreateEmptyResponse());
        } else {
            GameServerManager.SharedInstance.HDLeagues_FinishMySeason(OnRequestFinalize);
        }
    }
}