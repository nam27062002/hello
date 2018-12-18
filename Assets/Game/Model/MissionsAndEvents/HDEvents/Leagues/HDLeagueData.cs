using System;
using System.Collections.Generic;

public class HDLeagueData {
    //---[Basic Data]-----------------------------------------------------------

    private readonly DefinitionNode m_def;
    private readonly string m_sku;
    private readonly string m_icon;
    private readonly string m_name;
    private readonly string m_description;

    private readonly int    m_order;



    //---[Extended Data]--------------------------------------------------------

    private float m_demoteScale;
    private float m_promoteScale;

    private List<HDLiveData.Reward> m_rewards;
    private HDLeagueLeaderboard m_leaderboard;

    public HDLiveData.State liveDataState { get; private set; }
    public HDLiveDataManager.ComunicationErrorCodes liveDataError { get; private set; }



    //---[Construction Methods]-------------------------------------------------

    public HDLeagueData(DefinitionNode _def) {
        m_def = _def;

        //Load basic data from definition
        m_sku = _def.sku;
        m_name = _def.Get("name");
        m_icon = _def.Get("icon");
        m_description = _def.Get("desc");

        m_order = _def.GetAsInt("order");
        //

        m_leaderboard = new HDLeagueLeaderboard(m_sku);
    }

    public void Clean() {
        m_demoteScale = m_def.GetAsFloat("demoteScale");
        m_promoteScale = m_def.GetAsFloat("promoteScale");

        m_rewards = new List<HDLiveData.Reward>();
        m_leaderboard.Clean();

        liveDataState = HDLiveData.State.EMPTY;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    public void LoadData(SimpleJSON.JSONNode _data) {
        if (m_sku.Equals(_data["sku"])) {
            m_demoteScale = _data["demoteScale"].AsFloat;
            m_promoteScale = _data["promoteScale"].AsFloat;

            SimpleJSON.JSONArray rewardsData = _data["rewards"].AsArray;

            for (int r = 0; r < rewardsData.Count; ++r) {
                HDLiveData.Reward reward = new HDLiveData.Reward();
                reward.LoadData(rewardsData[r], HDTrackingManager.EEconomyGroup.REWARD_LEAGUE, m_sku);
                m_rewards.Add(reward);
            }

            liveDataState = HDLiveData.State.VALID;
            liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        } else {
            liveDataState = HDLiveData.State.ERROR;
            liveDataError = HDLiveDataManager.ComunicationErrorCodes.OTHER_ERROR;
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
