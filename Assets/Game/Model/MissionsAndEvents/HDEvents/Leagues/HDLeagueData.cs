using System;
using System.Collections.Generic;

public class HDLeagueData {
    //---[Classes and Enums]----------------------------------------------------
    public enum State {
        NONE = 0,
        WAITING_RESPONSE,
        SUCCESS,
        ERROR
    }


    //---[Basic Data]-----------------------------------------------------------
    private readonly string m_sku;
    public string sku { get { return m_sku; } }

    private readonly string m_icon;
    public string icon { get { return m_icon; } }

    private readonly string m_name;
    public string name { get { return m_name; } }

    private readonly string m_description;
    public string description { get { return m_description; } }


    //---[Extended Data]--------------------------------------------------------
    private DateTime m_startDate;
    public DateTime startDate { set { m_startDate = value; } }
    public TimeSpan timeToStart { get { return m_startDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }

    private DateTime m_endDate;
    public DateTime endDate { set { m_endDate = value; } }
    public TimeSpan timeToEnd { get { return m_endDate - GameServerManager.SharedInstance.GetEstimatedServerTime(); } }

    private List<Metagame.Reward> m_rewards;
    public void AddReward(Metagame.Reward _reward) { m_rewards.Add(_reward); }

    private HDLeagueLeaderboard m_leaderboard;
    public HDLeagueLeaderboard leaderboard { get { return m_leaderboard; } }

    private State m_state;
    public State state { get { return m_state; } }


    //---[Methods]--------------------------------------------------------------
    public HDLeagueData(DefinitionNode _def) {
        m_sku = _def.sku;

        m_startDate = new DateTime(1970, 1, 1);
        m_endDate = new DateTime(1970, 1, 1);

        m_rewards = new List<Metagame.Reward>();
        m_leaderboard = new HDLeagueLeaderboard(m_sku);

        m_state = State.NONE;
    }

    public void BuildExtendData() {
        // call server for this league definition
        GameServerManager.SharedInstance.HDLeagues_GetLeague(m_sku, OnGetLeagueResponse);

        m_state = State.WAITING_RESPONSE;
    }

    private void OnGetLeagueResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Get League", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            // parse Json
            LoadData(responseJson);

            // build the leader board
            m_leaderboard.RequestLeaderboard();

            m_state = State.SUCCESS;
        } else {

            m_state = State.ERROR;
        }
    }

    private void LoadData(SimpleJSON.JSONNode _data) {

    }
}
