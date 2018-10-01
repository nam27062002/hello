
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
                if(UsersManager.currentUser.GetNumOwnedDragons() >= MissionManager.GetDragonsRequiredToUnlockMissionDifficulty((Mission.Difficulty) i)) {
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
            GenerateNewMission((Mission.Difficulty)i, "ftux" + (i + 1).ToString()); // Force sku!
        }

        // Mark tutorial as completed!
        UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.FIRST_MISSIONS_GENERATED);
    }

    protected override bool IsMissionLocked(Mission.Difficulty _difficulty) {
        return UsersManager.currentUser.GetNumOwnedDragons() < MissionManager.GetDragonsRequiredToUnlockMissionDifficulty(_difficulty);
    }

    protected override float ComputeValueModifier(Mission.Difficulty _difficulty, bool _singleRun) {
        float totalModifier = 0f;
        string _dragonModifierSku = DragonManager.biggestOwnedDragon.def.sku;

        // 2.1. Dragon modifier - additive
        DefinitionNode dragonModifierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_MODIFIERS, _dragonModifierSku);  // Matching sku
        if (dragonModifierDef != null) {
            totalModifier += dragonModifierDef.GetAsFloat("quantityModifier");
            Debug.Log("\tDragon Modifier " + dragonModifierDef.GetAsFloat("quantityModifier") + "\n\tTotal modifier: " + totalModifier);
        }

        // 2.2. Difficulty modifier - additive
        DefinitionNode difficultyDef = MissionManager.GetDifficultyDef(_difficulty);
        DefinitionNode difficultyModifierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_MODIFIERS, difficultyDef.sku);
        if (difficultyModifierDef != null) {
            totalModifier += difficultyModifierDef.GetAsFloat("quantityModifier");
            Debug.Log("\tDifficulty Modifier " + difficultyModifierDef.GetAsFloat("quantityModifier") + "\n\tTotal modifier: " + totalModifier);
        }

        // 2.3. Single run modifier - multiplicative
        if (_singleRun) {
            DefinitionNode singleRunModifierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_MODIFIERS, "single_run");
            if (singleRunModifierDef != null) {
                totalModifier *= 1f - singleRunModifierDef.GetAsFloat("quantityModifier");
                Debug.Log("\tSingle Run Modifier " + singleRunModifierDef.GetAsFloat("quantityModifier") + "\n\tTotal modifier: " + totalModifier);
            }
        }

        return totalModifier;
    }

    protected override float ComputeRemovePCCostModifier() {
        return DragonManager.GetDragonsByLockState(IDragonData.LockState.OWNED).Count;
    }

    protected override Metagame.Reward BuildReward(Mission.Difficulty _difficulty) {
        long amount = (long)(MissionManager.maxRewardPerDifficulty[(int)_difficulty] * DragonManager.GetDragonsByLockState(IDragonData.LockState.OWNED).Count);
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