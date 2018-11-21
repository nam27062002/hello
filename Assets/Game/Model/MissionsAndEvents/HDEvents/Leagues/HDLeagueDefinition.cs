using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HDLeagueDefinition : HDLiveEventDefinition {

    public override void ParseInfo(SimpleJSON.JSONNode _data) {
        base.ParseInfo(_data);

        Debug.LogWarning("[HDLeagueDefinition] ParseInfo");
    }
}
