﻿using System;
using System.Collections.Generic;

public class HDLeagueData {
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

    public HDLeagueLeaderboard leaderboard { set; get; }


    //---[Methods]--------------------------------------------------------------
    public HDLeagueData(DefinitionNode _def) {
        m_sku = _def.sku;

        m_endDate = new DateTime(1970, 1, 1);
        m_rewards = new List<Metagame.Reward>();
        leaderboard = null;
    }

}
