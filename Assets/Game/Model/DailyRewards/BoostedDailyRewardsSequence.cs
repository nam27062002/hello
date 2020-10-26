// DailyRewardsCollection.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//#define PLAYTEST

#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Boosted seven day login is identical as the regular seven day login but with
/// different rewards, so we only need to change the generation method to read from a
/// different content table.
/// Handles a set of daily rewards: generation, collection, persistence, etc.
/// </summary>
public class BoostedDailyRewardsSequence : DailyRewardsSequence {
	

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Generate a new sequence of rewards.
	/// </summary>
	public void Generate() {
		// Gather all defined reward definitions
		List<DefinitionNode> rewardDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.BOOSTED_DAILY_REWARDS);

		// Store them in a dictionary using their "day" property as key
		// As of 2.8, multiple rewards can be defined for the same day, using them as replacement if the original reward is already owned (dragons, pets, skins)
		Dictionary<int, List<DefinitionNode>> rewardDefsByDay = new Dictionary<int, List<DefinitionNode>>();
		for(int i = 0; i < rewardDefs.Count; ++i) {
			// Skip if reward is not enabled
			if(!rewardDefs[i].GetAsBool("enabled", true)) continue;

			// Put the reward definition in the proper day slot
			int day = rewardDefs[i].GetAsInt("day");
			if(!rewardDefsByDay.ContainsKey(day)) {
				rewardDefsByDay.Add(day, new List<DefinitionNode>());
			}
			rewardDefsByDay[day].Add(rewardDefs[i]);
		}

		// Find the initial sequence day closest to the current reward idx
		int initialSequenceDay = m_totalRewardIdx - (m_totalRewardIdx % SEQUENCE_SIZE);

		// Generate rewards for the next SEQUENCE_SIZE days (initialSequenceDay to initialSequenceDay + SEQUENCE_SIZE)
		for(int i = 0; i < SEQUENCE_SIZE; ++i) {
			// Find out closest reward corresponding to the current total reward index for this day
			// With this, day 14 will use def for day 14, but day 13 will reuse the def for day 6 instead :)
			int targetDay = initialSequenceDay + i + 1;
			while(targetDay >= 0 && !rewardDefsByDay.ContainsKey(targetDay)) {
				targetDay -= SEQUENCE_SIZE;
			}

			// This should never happen (as we should have definitions for at least days 1 to SEQUENCE_SIZE), but just in case
			if(targetDay < 0) {
				Debug.LogError(Color.red.Tag("ERROR! Couldn't find a reward for slot " + i + " (day " + (m_totalRewardIdx + i + 1) + ")"));
				targetDay = 1;	// If we don't have a reward for day 1, something is really wrong xDD
			}

			// Initialize reward with the definition for the target day
			m_rewards[i].InitFromDefs(rewardDefsByDay[targetDay]);
		}
	}
    #region log
    private const string LOG_CHANNEL = "[DailyRewards]";
#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private void Log(string message)
    {
        ControlPanel.Log(LOG_CHANNEL + message, ControlPanel.ELogChannel.DailyRewards);
    }
    #endregion
}