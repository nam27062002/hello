using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSpecialMissions : UserMissions {

    public UserSpecialMissions() {
        m_defMissionCategory = DefinitionsCategory.MISSIONS; //MISSIONS_SPECIAL??
        m_defTypesCategory = DefinitionsCategory.MISSION_TYPES;
    }

    public override void UpdateRewards() {
        for (int i = 0; i < m_missions.Length; i++) {
            Mission mission = m_missions[i];
            if (mission != null) {
                if (mission.reward != null) {
                    mission.reward.bonusPercentage = MissionManager.powerUpGFMultiplier;
                    mission.updated = true;
                }
            }
        }
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

    protected override float ComputeRemovePCCostModifier() {
        return 1f;
    }

    protected override Metagame.Reward BuildReward(Mission.Difficulty _difficulty) {
        long amount = (long)_difficulty + 1;
        Metagame.Reward reward = new Metagame.RewardGoldenFragments(amount, Metagame.Reward.Rarity.COMMON, HDTrackingManager.EEconomyGroup.REWARD_MISSION, "");
        reward.bonusPercentage = MissionManager.powerUpGFMultiplier;

        return reward;
    }

    //nothing to do here
    public override void UnlockByDragonsNumber() { }
	
}
