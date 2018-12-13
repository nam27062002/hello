using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class HDLeagueController : HDLiveDataController {
    //---[Classes and Enums]----------------------------------------------------
    public enum State {
        NONE = 0,
        NOT_JOINED,
        JOINED,
        CLOSED,
        PENDING_REWARDS,
        FINALIZED
    }


    //---[Attributes]-----------------------------------------------------------
    private int m_liveDataCode;

    private List<HDLeagueData> m_leagues;
    private HDLeagueData m_currentLeague;
    private HDLeagueData m_nextLeague;

    private State m_state;


    //---[Generic Methods]------------------------------------------------------
    /// <summary>
    /// Default constructor.
    /// </summary>
    public HDLeagueController() {
        m_type = "league";
        m_liveDataCode = -1;

        m_leagues = new List<HDLeagueData>();

        m_state = State.NONE;
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

            LoadData(json);
        }
        m_dataLoadedFromCache = true;
    }

    public override void LoadData(JSONNode _data) {
        /*
            "sku" : "iron_league",
            "nextSku" : "silver_league",
            "status" : "joined"
            */           

        CreateLeagues(_data["sku"], _data.GetSafe("nextSku", ""));

        int status = _data["status"];

    }   

    public override void OnLiveDataResponse() {
        if (m_currentLeague != null) {
            // request the full data
            m_currentLeague.BuildExtendData();
        }
    }
   
    private void CreateLeagues(string _currentLeague, string _nextLeague) {
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
