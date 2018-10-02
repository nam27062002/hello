// GlobalEventReward.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single global event object.
/// </summary>
public partial class GlobalEvent {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reward item and the percentage required to achieve it.
	/// </summary>
	[Serializable]
	public class RewardSlot {		
		//------------------------------------------------------------------------//
		// MEMBERS																  //
		//------------------------------------------------------------------------//
		public Metagame.Reward reward;

		public float targetPercentage = 0f;
		public float targetAmount = 0f;		// Should match target percentage

		//------------------------------------------------------------------------//
		// METHODS																  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Constructor from json data.
		/// </summary>
		/// <param name="_data">Data to be parsed.</param>
		public RewardSlot(SimpleJSON.JSONNode _data) {
			// Reward data
			reward = Metagame.Reward.CreateFromJson(_data, HDTrackingManager.EEconomyGroup.REWARD_GLOBAL_EVENT, GlobalEventManager.currentEvent.name);

			// [AOC] Going to hell!
			// 		 Mini-hack: if reward is gold fragments, tweak its rarity so displayed reward looks cooler
			if(reward.type == Metagame.RewardGoldenFragments.TYPE_CODE) {
				if(reward.amount >= 5) {
					reward.rarity = Metagame.Reward.Rarity.EPIC;
				} else if(reward.amount >= 3) {
                    reward.rarity = Metagame.Reward.Rarity.RARE;
				} else {
					reward.rarity = Metagame.Reward.Rarity.COMMON;
				}
			}

			// Init target percentage
			// Target amount should be initialized from outside, knowing the global target
			targetPercentage = _data["targetPercentage"].AsFloat;
		}
	};
}