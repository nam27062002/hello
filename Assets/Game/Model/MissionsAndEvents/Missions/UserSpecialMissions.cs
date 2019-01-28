﻿using System.Collections;
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
    protected override DragonTier GetMaxTierUnlocked() {
        return DragonManager.maxSpecialDragonTierUnlocked;
    }

    protected override bool IsMissionLocked(Mission.Difficulty _difficulty) {
        return false;
    }
    
    protected override DefinitionNode GetDragonModifierDef() {
        return DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.MISSION_SPECIAL_MODIFIERS, "tier", IDragonData.TierToSku(DragonManager.maxSpecialDragonTierUnlocked));
    }

    protected override DefinitionNode GetForcedDragonModifierDef(string _sku) {
        return DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.MISSION_SPECIAL_MODIFIERS, "tier", _sku);
    }

    protected override float ComputeRemovePCCostModifier() {
        return (float)DragonManager.maxSpecialDragonTierUnlocked;
    }

	protected override Metagame.Reward BuildReward(Mission.Difficulty _difficulty, DefinitionNode _dragonModifierDef) {
        long amount = (long)MissionManager.GetMaxRewardPerDifficulty(SceneController.Mode.SPECIAL_DRAGONS, _difficulty);
        Metagame.Reward reward = new Metagame.RewardGoldenFragments(amount, Metagame.Reward.Rarity.COMMON, HDTrackingManager.EEconomyGroup.LAB_REWARD_MISSION, "");
        reward.bonusPercentage = MissionManager.powerUpGFMultiplier;

        return reward;
    }

    //nothing to do here
    public override void UnlockByDragonsNumber() { }
	
}
