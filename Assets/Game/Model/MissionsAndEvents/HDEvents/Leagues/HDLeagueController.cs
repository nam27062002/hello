﻿using System.Collections.Generic;


public class HDLeagueController : HDLiveDataController {
    //---[Attributes]-----------------------------------------------------------

    private HDSeasonData m_season;	// Never null
    public HDSeasonData season { get { return m_season; } }

    private List<HDLeagueData> m_leagues;	// Never null
    public int leaguesCount { get { return m_leagues.Count; } }
    public HDLeagueData GetLeagueData(int _index) {  return m_leagues[_index]; }



    //---[Generic Methods]------------------------------------------------------

    /// <summary>
    /// Default constructor.
    /// </summary>
    public HDLeagueController() {
        m_type = "league";

        m_season = new HDSeasonData();
        CreateLeagues();

        m_dataLoadedFromCache = false;
    }

    private void CreateLeagues() {
        m_leagues = new List<HDLeagueData>();
        List<DefinitionNode> definitions = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.LEAGUES);

        for (int i = 0; i < definitions.Count; ++i) {
            DefinitionNode definition = definitions[i];
            HDLeagueData league = new HDLeagueData(definition);
            m_leagues.Add(league);
        }
    }

    public override void Activate() {}
    public override void Deactivate() {}
    public override void ApplyDragonMods() {}

    public override void CleanData() {
        m_season.Clean();
     
        for (int i = 0; i < m_leagues.Count; ++i) {
            m_leagues[i].Clean();
        }

        m_dataLoadedFromCache = false;
    }

    public override bool ShouldSaveData() {
        return m_season != null && m_season.state > HDSeasonData.State.TEASING && m_season.state < HDSeasonData.State.WAITING_NEW_SEASON;
    }

    public override SimpleJSON.JSONNode SaveData() {
        SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

        data["sku"] = m_season.currentLeague.sku;
        if (m_season.nextLeague != null) {
            data["nextSku"] = m_season.nextLeague.sku;
        }
        data["status"] = m_season.state.ToString();

        return data;
    }

    public override bool IsFinishPending() {
        bool isFinishPending = m_isFinishPending;

        if (isFinishPending) {
            m_season.RequestFinalize();
            HDLiveDataManager.instance.ForceRequestLeagues();
            m_isFinishPending = false;
        }

        return isFinishPending;
    }

    public override void LoadDataFromCache() {
        if (CacheServerManager.SharedInstance.HasKey(m_type)) {
            SimpleJSON.JSONNode json = SimpleJSON.JSONNode.Parse(CacheServerManager.SharedInstance.GetVariable(m_type));

            LoadData(json);

            if (season.state == HDSeasonData.State.REWARDS_COLLECTED) {
                m_isFinishPending = true;
            }
            m_dataLoadedFromCache = true;
        }
    }

    public override void LoadData(SimpleJSON.JSONNode _data) {
        CleanData();

        m_season.LoadStatus(_data);

        string currentLeague = _data["sku"];
        string nextLeague = _data.GetSafe("nextSku", "");
        
        for (int i = 0; i < m_leagues.Count; ++i) {
            HDLeagueData leagueData = m_leagues[i];

            if (currentLeague.Equals(leagueData.sku)) {
                m_season.currentLeague = leagueData;
            }

            if (nextLeague.Equals(leagueData.sku)) {
                m_season.nextLeague = leagueData;
            }
        }

        m_dataLoadedFromCache = false;
    }

    public override void OnLiveDataResponse() {
        m_season.RequestData(true);

        //Request all leagues
        for (int i = 0; i < m_leagues.Count; ++i) {

            m_leagues[i].WaitForData();
        }

        GameServerManager.SharedInstance.HDLeagues_GetAllLeagues(OnRequestAllLeaguesData);
    }

    private void OnRequestAllLeaguesData(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] All leagues Data", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            SimpleJSON.JSONArray data = responseJson["leagues"].AsArray;
            for (int i = 0; i < data.Count; ++i) {
                int order = data[i]["order"].AsInt;
                m_leagues[order].LoadData(data[i]);
            }
        } else {
            for (int i = 0; i < m_leagues.Count; ++i) {
                m_leagues[i].LoadData(null);
            }
        }
    }
}
