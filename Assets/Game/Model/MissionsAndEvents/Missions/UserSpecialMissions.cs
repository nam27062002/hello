using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSpecialMissions : UserMissions {

    public UserSpecialMissions() {
        m_defMissionCategory = DefinitionsCategory.MISSIONS; //MISSIONS_SPECIAL??
        m_defTypesCategory = DefinitionsCategory.MISSION_TYPES;
    }

    //------------------------------------------------------------------//
    // INTERNAL METHODS                                                 //
    //------------------------------------------------------------------//
    protected override bool IsMissionLocked(Mission.Difficulty _difficulty) {
        return false;
    }
    
    protected override float ComputeValueModifier(Mission.Difficulty _difficulty, bool _singleRun) {
        return 1f;
    }

    protected override float ComputeRewardModifier() {
        return 1f;
    }

    protected override float ComputeRemovePCCostModifier() {
        return 1f;
    }

    //nothing to do here
    public override void UnlockByDragonsNumber() { }
	
}
