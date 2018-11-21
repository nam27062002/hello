using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class HDLeagueManager : HDLiveEventManager {
    public class League {
        public string sku;
    }


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES                                                 //
    //------------------------------------------------------------------------//
    protected HDLeagueData m_leagueData;
    protected HDLeagueDefinition m_leagueDefinition;

    private List<League> m_leagues;
    private League m_currentLeague;


    //------------------------------------------------------------------------//
    // GENERIC METHODS                                                        //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public HDLeagueManager() {
        m_type = "league";

        m_leagueData = new HDLeagueData();
        m_leagueDefinition = m_leagueData.definition as HDLeagueDefinition;

        m_data = m_leagueData;
    }

    ~HDLeagueManager() {
        m_data = null;
        m_leagueData = null;
        m_leagueDefinition = null;
    }

    public override void ParseDefinition(JSONNode _data) {
        base.ParseDefinition(_data);

        Debug.LogWarning("[HDLeagueManager] ParseDefinition");
    }
}
