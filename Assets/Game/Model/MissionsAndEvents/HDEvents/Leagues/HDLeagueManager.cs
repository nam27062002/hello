using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class HDLeagueManager : HDLiveDataController {

    private int m_liveDataCode;

    private List<HDLeagueData> m_leagues;
    private HDLeagueData m_currentLeague;
    private HDLeagueData m_nextLeague;

    //------------------------------------------------------------------------//
    // GENERIC METHODS                                                        //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public HDLeagueManager() {
        m_type = "league";
        m_liveDataCode = -1;
    }

    ~HDLeagueManager() {
       
    }


    public override void Activate() {}
    public override void Deactivate() {}
    public override void ApplyDragonMods() {}

    public override void CleanData() {
        m_liveDataCode = -1;

        m_leagues.Clear();
        m_currentLeague = null;
        m_nextLeague = null;

        m_dataLoadedFromCache = false;
    }

    public override bool ShouldSaveData() {
        return false;
    }

    public override JSONNode SaveData() {
        return null;
    }


    public override void LoadDataFromCache() {
        CleanData();
        if (CacheServerManager.SharedInstance.HasKey(m_type)) {
            SimpleJSON.JSONNode json = SimpleJSON.JSONNode.Parse(CacheServerManager.SharedInstance.GetVariable(m_type));

            CreateLeagues("");
        }
        m_dataLoadedFromCache = true;
    }

    public override void LoadData(JSONNode _data) {


        CreateLeagues("");


    }   

    public override void OnLiveDataResponse() {
        // request the full data

    }
   
    private void CreateLeagues(string _currentLeague) {
        List<DefinitionNode> definitions = DefinitionsManager.SharedInstance.GetDefinitionsList("TODO: LEAGUES CAT");
             
        for (int i = 0; i < definitions.Count; ++i) {
            DefinitionNode definition = definitions[i];
            HDLeagueData league = new HDLeagueData(definition);

            m_leagues.Add(league);

            if (_currentLeague.Equals(definition.sku)) {
                m_currentLeague = league;
            }
        }
    }
}
