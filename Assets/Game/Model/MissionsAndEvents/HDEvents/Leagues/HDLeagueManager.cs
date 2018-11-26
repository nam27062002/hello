using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class HDLeagueManager : HDLiveDataController {
    public class League {
        public string sku;
    }



    //------------------------------------------------------------------------//
    // GENERIC METHODS                                                        //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public HDLeagueManager() {
        m_type = "league";

       
    }

    ~HDLeagueManager() {
       
    }


    public override void Activate() {
     //  throw new System.NotImplementedException();
    }

    public override void Deactivate() {
      //  throw new System.NotImplementedException();
    }

    public override void ApplyDragonMods() {
     //   throw new System.NotImplementedException();
    }

    public override void CleanData() {
      //  throw new System.NotImplementedException();
    }

    public override bool ShouldSaveData() {
        //  throw new System.NotImplementedException();
        return false;
    }

    public override JSONNode SaveData() {
        //  throw new System.NotImplementedException();
        return null;
    }

    public override void LoadDataFromCache() {
     //   throw new System.NotImplementedException();
    }

    public override void LoadData(JSONNode _data) {
     //   throw new System.NotImplementedException();
    }

    public override void OnLiveDataResponse() {
      //  throw new System.NotImplementedException();
    }
}
