// ResultsScreenStepTracking.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepTracking : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		return true;
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Update global stats
		UsersManager.currentUser.gamesPlayed = UsersManager.currentUser.gamesPlayed + 1;
		DragonManager.CurrentDragon.gamesPlayed = DragonManager.CurrentDragon.gamesPlayed + 1;
		
        /*
		if (FeatureSettingsManager.instance.IsMiniTrackingEnabled) {
            // Pre-process chests
            int chestsFound = 0;
            int totalCollectedChests = ChestManager.collectedChests;
            long chestsCoinsReward = 0;
            for(int i = 0; i < ChestManager.dailyChests.Length; i++) {
                if(ChestManager.dailyChests[i].state == Chest.State.PENDING_REWARD) {
                    // Count chest
                    chestsFound++;
                    totalCollectedChests++;
    
                    // Find out reward
                    Chest.RewardData rewardData = ChestManager.GetRewardData(totalCollectedChests);
                    if(rewardData != null && rewardData.type == Chest.RewardType.SC) {
                        chestsCoinsReward += (long)rewardData.amount;
                    }
                }
            }
    
            // Pre-process missions
            bool[] missionCompleted = new bool[(int)Mission.Difficulty.COUNT];
            int [] missionReward = new int[(int)Mission.Difficulty.COUNT];
            for(int i = 0; i < missionCompleted.Length; i++) {
                Mission m = MissionManager.GetMission((Mission.Difficulty)i);
                if(m != null && m.state == Mission.State.ACTIVE && m.objective.isCompleted) {
                    missionCompleted[i] = true;
                    missionReward[i] = (int)m.reward.amount;
                } else {
                    missionCompleted[i] = false;
                    missionReward[i] = 0;
                }
            }
            
			// Get some special data
			int level = 0;
			if(DragonManager.currentDragon.type == IDragonData.Type.CLASSIC) {
				level = (DragonManager.currentDragon as DragonDataClassic).progression.level;
			}
            else if ( DragonManager.currentDragon.type == IDragonData.Type.CLASSIC )
            {
                
            }

			// Do it!
			MiniTrackingEngine.TrackEvent(
				"GAME_ENDED",
				new TrackingParam("run_nb", UsersManager.currentUser.gamesPlayed),
				new TrackingParam("time_played", m_controller.time),
				new TrackingParam("sc_collected", m_controller.coins),
				new TrackingParam("sc_survival_bonus", m_controller.survivalBonus),
				new TrackingParam("sc_mission_1", missionReward[0]),
				new TrackingParam("sc_mission_2", missionReward[1]),
				new TrackingParam("sc_mission_3", missionReward[2]),
				new TrackingParam("sc_chests", chestsCoinsReward),
				new TrackingParam("hc_collected", RewardManager.pc),
				new TrackingParam("death_cause", RewardManager.deathSource),
				new TrackingParam("death_type", RewardManager.deathType),
				new TrackingParam("chests_found", chestsFound),
				new TrackingParam("egg_found", (CollectiblesManager.egg != null && CollectiblesManager.egg.collected)),
				new TrackingParam("score_total", m_controller.score),
				new TrackingParam("highest_multiplier", RewardManager.maxScoreMultiplier),
				new TrackingParam("highest_base_multiplier", RewardManager.maxBaseScoreMultiplier),
				new TrackingParam("hc_revive_used", RewardManager.paidReviveCount),
				new TrackingParam("ad_revive_used", RewardManager.freeReviveCount),
				new TrackingParam("xp_earn", RewardManager.xp),
				new TrackingParam("current_dragon", DragonManager.currentDragon.sku),
				new TrackingParam("current_level", level),
				new TrackingParam("mission1_completed", missionCompleted[0]),
				new TrackingParam("mission2_completed", missionCompleted[1]),
				new TrackingParam("mission3_completed", missionCompleted[2]),
				new TrackingParam("enterWaterAmount", RewardManager.enterWaterAmount),
				new TrackingParam("enterSpaceAmount", RewardManager.enterSpaceAmount)
			);

			// Tracking is sent silently after every round
			MiniTrackingEngine.SendTrackingFile(true, null);
		}
        */

		// Notify we're finished
		OnFinished.Invoke();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
}