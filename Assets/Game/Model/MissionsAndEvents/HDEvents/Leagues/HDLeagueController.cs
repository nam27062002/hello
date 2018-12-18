using System.Collections.Generic;


public class HDLeagueController : HDLiveDataController {
    //---[Attributes]-----------------------------------------------------------
    private HDSeasonData m_season;
    public HDSeasonData season { get { return m_season; } }

    private List<HDLeagueData> m_leagues;
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
        return false;
    }

    public override SimpleJSON.JSONNode SaveData() {
        return null;
    }

    public override void LoadDataFromCache() {
        if (CacheServerManager.SharedInstance.HasKey(m_type)) {
            SimpleJSON.JSONNode json = SimpleJSON.JSONNode.Parse(CacheServerManager.SharedInstance.GetVariable(m_type));

            LoadData(json);
        }
        m_dataLoadedFromCache = true;
    }

    public override void LoadData(SimpleJSON.JSONNode _data) {
        CleanData();

        m_season.LoadData(_data);

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
    }   

    public override void OnLiveDataResponse() {
        m_season.RequestFullData(true);
    }
}
