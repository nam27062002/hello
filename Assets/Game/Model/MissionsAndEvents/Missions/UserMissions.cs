
//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
public class UserMissions : IUserMissions {
    
    public UserMissions() {
        m_defMissionCategory = DefinitionsCategory.MISSIONS;
        m_defTypesCategory = DefinitionsCategory.MISSION_TYPES;
    }

    public override void UpdateRewards() {
        for (int i = 0; i < m_missions.Length; i++) {
            Mission mission = m_missions[i];
            if (mission != null) {
                if (mission.reward != null) {
                    mission.reward.bonusPercentage = MissionManager.powerUpSCMultiplier;
                    mission.updated = true;
                }
            }
        }
    }

    //------------------------------------------------------------------//
    // INTERNAL METHODS                                                 //
    //------------------------------------------------------------------//
    /// <summary>
    /// Unlock missions based on owned dragons.
    /// Deprecated!
    /// </summary>
    public override void UnlockByDragonsNumber() {
        for(int i = 0; i < m_missions.Length; i++) {
            // Is the mission locked?
            if(m_missions[i].state == Mission.State.LOCKED) {
                // Do we have enough dragons?
                if(UsersManager.currentUser.GetNumOwnedDragons() >= MissionManager.GetDragonsToUnlock(SceneController.Mode.DEFAULT, (Mission.Difficulty) i)) {
                    m_missions[i].ChangeState(Mission.State.ACTIVE);
                }
            }
        }
    }

    /// <summary>
    /// Create the FTUXP missions and mark tutorial step as completed
    /// </summary>
    private void GenerateTutorialMissions() {
        // One for every difficulty!
        for (int i = 0; i < (int)Mission.Difficulty.COUNT; i++) {
            m_missions[i] = GenerateNewMission((Mission.Difficulty)i, "ftux" + (i + 1).ToString()); // Force sku!
        }

        // Mark tutorial as completed!
        UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.FIRST_MISSIONS_GENERATED);
    }

    protected override DragonTier GetMaxTierUnlocked() {
        if (DragonManager.biggestOwnedDragon != null) {
            return DragonManager.biggestOwnedDragon.tier;
        }

        return DragonTier.TIER_0;
    }

    protected override bool IsMissionLocked(Mission.Difficulty _difficulty) {
        return UsersManager.currentUser.GetNumOwnedDragons() < MissionManager.GetDragonsToUnlock(SceneController.Mode.DEFAULT, _difficulty);
    }

    protected override DefinitionNode GetDragonModifierDef() {
        return DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.MISSION_MODIFIERS, "dragon_sku", DragonManager.biggestOwnedDragon.def.sku);
    }

    protected override DefinitionNode GetForcedDragonModifierDef(string _sku) {
        return DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.MISSION_MODIFIERS, "dragon_sku", _sku);
    }

    protected override float ComputeRemovePCCostModifier() {
        return DragonManager.GetDragonsByLockState(IDragonData.LockState.OWNED).Count;
    }

	protected override Metagame.Reward BuildReward(Mission.Difficulty _difficulty, DefinitionNode _dragonModifierDef) {
		long amount = MissionManager.GetMaxRewardPerDifficulty(SceneController.Mode.DEFAULT, _difficulty);
		amount = Metagame.RewardSoftCurrency.ScaleByMaxDragonOwned(amount);	// Scale the reward based on max owned dragon
        
		Metagame.Reward reward = new Metagame.RewardSoftCurrency(amount, Metagame.Reward.Rarity.COMMON, HDTrackingManager.EEconomyGroup.REWARD_MISSION, "");
        reward.bonusPercentage = MissionManager.powerUpSCMultiplier;

        return reward;
    }

    //------------------------------------------------------------------------//
    // PERSISTENCE                                                            //
    //------------------------------------------------------------------------//

    public override void Load(SimpleJSON.JSONNode _data) {
        // Load missions!
        // Override if tutorial step was not completed
        if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_MISSIONS_GENERATED)) {
            // Create special missions
            GenerateTutorialMissions();
        } else {
            base.Load(_data);
        }
    }

    public override SimpleJSON.JSONNode Save() {
        // If tutorial step was not completed, generate tutorial missions if not already done!
        if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_MISSIONS_GENERATED)) {
            // Create special missions
            GenerateTutorialMissions();
        }

        return base.Save();
    }
}