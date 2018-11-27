using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class HDLeagueManager : HDLiveDataController {
    public class League {
        public string sku;
    }


    private int m_liveDataCode;


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

    public override void LoadData(JSONNode _data) {}

    public override void OnLiveDataResponse() {}
}
