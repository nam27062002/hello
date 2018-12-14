using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class HDLeagueController : HDLiveDataController {
    //---[Attributes]-----------------------------------------------------------
    private HDSeasonData m_season;
    public HDSeasonData season { get { return m_season; } }

    private List<HDLeagueData> m_leagues;


    //---[Generic Methods]------------------------------------------------------
    /// <summary>
    /// Default constructor.
    /// </summary>
    public HDLeagueController() {
        m_type = "league";

        m_season = null;
        m_leagues = new List<HDLeagueData>();
    }

    public override void Activate() {}
    public override void Deactivate() {}
    public override void ApplyDragonMods() {}

    public override void CleanData() {
        m_season = null;
        m_leagues.Clear();

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

            LoadData(json);
        }
        m_dataLoadedFromCache = true;
    }

    public override void LoadData(JSONNode _data) {
        m_season = new HDSeasonData();
        m_season.LoadData(_data);

        CreateLeagues(_data["sku"], _data.GetSafe("nextSku", ""));
    }   

    public override void OnLiveDataResponse() {
        m_season.RequestFullData(true);
    }
   
    private void CreateLeagues(string _currentLeague, string _nextLeague) {
        List<DefinitionNode> definitions = DefinitionsManager.SharedInstance.GetDefinitionsList("TODO: LEAGUES CAT");
             
        for (int i = 0; i < definitions.Count; ++i) {
            DefinitionNode definition = definitions[i];
            HDLeagueData league = new HDLeagueData(definition);

            m_leagues.Add(league);

            if (_currentLeague.Equals(definition.sku)) {
                m_season.currentLeague = league;
            }

            if (_nextLeague.Equals(definition.sku)) {
                m_season.nextLeague = league;
            }
        }
    }
}
