using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class HDLeagueManager : HDLiveDataController {

    private int m_liveDataCode;

    private List<HDLeagueDataBasic> m_leagues;
    private HDLeagueDataFull m_currentLeague;

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


        }
        m_dataLoadedFromCache = true;
    }

    public override void LoadData(JSONNode _data) {

    }   

    public override void OnLiveDataResponse() {
        // request the full data


    }

   
    private void CreateLeagues(string _currentLeague) {

    }
}
