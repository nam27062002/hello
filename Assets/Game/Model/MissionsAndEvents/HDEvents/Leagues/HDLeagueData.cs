using System;
using System.Collections.Generic;

public class HDLeagueData {
    //---[Basic Data]-----------------------------------------------------------
    private readonly string m_sku;
    private readonly string m_icon;
    private readonly string m_name;
    private readonly string m_description;


    //---[Extended Data]--------------------------------------------------------
    private float m_demoteScale;
    private float m_promoteScale;

    private List<HDLiveData.Reward> m_rewards;
    private HDLeagueLeaderboard m_leaderboard;

    private HDLiveData.State m_liveDataState;
    public HDLiveData.State liveDataState { get { return m_liveDataState; } }


    //---[Construction Methods]-------------------------------------------------
    public HDLeagueData(DefinitionNode _def) {
        //Load basic data from definition
        m_sku = _def.sku;
        //...
        //
    
        m_demoteScale = 0f;
        m_promoteScale = 0f;

        m_rewards = new List<HDLiveData.Reward>();
        m_leaderboard = new HDLeagueLeaderboard(m_sku);

        m_liveDataState = HDLiveData.State.EMPTY;
    }

    public void LoadData(SimpleJSON.JSONNode _data) {
        if (m_sku.Equals(_data["sku"])) {
            m_demoteScale = _data["demoteScale"].AsFloat;
            m_promoteScale = _data["promoteScale"].AsFloat;

            SimpleJSON.JSONArray rewards = _data["rewars"].AsArray;

            for (int r = 0; r < rewards.Count; ++r) {
                HDLiveData.Reward reward = new HDLiveData.Reward();
                reward.ParseJson(_data, HDTrackingManager.EEconomyGroup.REWARD_LEAGUE, m_sku);
                m_rewards.Add(reward);
            }

            m_liveDataState = HDLiveData.State.VALID;
        } else {
            m_liveDataState = HDLiveData.State.ERROR;
        }
    }


    //---[Query Methods]--------------------------------------------------------
    public string name          { get { return m_name; } }
    public string sku           { get { return m_sku; } }
    public string icon          { get { return m_icon; } }
    public string description   { get { return m_description; } }

    public float demoteScale    { get { return m_demoteScale; } }
    public float promoteScale   { get { return m_promoteScale; } }

    public HDLeagueLeaderboard leaderboard { get { return m_leaderboard; } }

    public Metagame.Reward GetReward(int _i) { return m_rewards[_i].reward; }
}
