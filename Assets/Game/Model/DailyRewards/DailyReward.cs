// DailyReward.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Data structure representing a daily reward.
/// </summary>
[Serializable]
public class DailyReward {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Tracking constants
	private const HDTrackingManager.EEconomyGroup ECONOMY_GROUP = HDTrackingManager.EEconomyGroup.REWARD_DAILY;
	private const string DEFAULT_SOURCE = "";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	public DefinitionNode sourceDef = null;
	public List<DefinitionNode> replacementDefs = new List<DefinitionNode>();
	public int order = 0;
	public Metagame.Reward reward = null;

	public bool canBeDoubled {
		get {
			// [AOC] TODO!! Do it by type or by content? Type for now
			if(reward != null) {
				switch(reward.type) {
					// Only currencies for now
					case Metagame.RewardSoftCurrency.TYPE_CODE:
					case Metagame.RewardHardCurrency.TYPE_CODE:
					case Metagame.RewardGoldenFragments.TYPE_CODE: {
						return true;
					}
				}
			}

			return false;
		}
	}

	// State
	public bool collected;
	public bool doubled;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public DailyReward() {
		// Apply initial values
		Reset();
	}

	/// <summary>
	/// Put default values.
	/// </summary>
	public void Reset() {
		// Clear reward
		reward = null;
		sourceDef = null;
		replacementDefs.Clear();
		order = 0;

		// Reset state
		collected = false;
		doubled = false;
	}

	/// <summary>
	/// Initialize data with a given definitions.
	/// Use it when creating a new reward sequence.
	/// We will try to give the reward with the lowest value in the "priority" field from the list. If it's non-consumable and already owned (pet, dragon, skin), it will be replaced by the second one in the list, and so on.
	/// </summary>
	/// <param name="_defs">Collection of definitions from the DAILY_REWARDS category.</param>
	public void InitFromDefs(List<DefinitionNode> _defs) {
		// Clear previous data and put default values
		Reset();

		// Nothing else to do if no valid definitions are given
		if(_defs.Count == 0) return;
		if(_defs[0] == null) return;

		// Sort given definitions by "priority" field
		_defs.Sort(CompareDefsByPriority);

		// Store source and replacement definitions
		sourceDef = _defs[0];
		replacementDefs.AddRange(_defs.GetRange(1, _defs.Count - 1));

		// Create new reward
		reward = CreateRewardFromDef(sourceDef);

		// Special case: If the reward is already owned by the time the sequence 
		// is generated (i.e. Pets), use its replacement instead.
		// This won't be the case if the reward is obtained via other means after the
		// sequence is generated (as designed).
		if(reward != null) {
			if(reward.WillBeReplaced()) {
				reward = reward.replacement;
			}
		}
	}

	/// <summary>
    /// Collect the reward :)
    /// No checks performed.
    /// </summary>
    /// <param name="_doubled">Has the reward been doubled?</param>
    public void Collect(bool _doubled) {
		// Make sure that we actually have a valid reward
		if(reward == null) {
			Debug.LogError("Attempting to collect a NULL daily reward!!");
			return;
		}

		// Double the reward?
		// [AOC] Just in case, don't do it again if it has already been doubled!
		if(_doubled && !this.doubled) {
			reward.bonusPercentage = 100f;
			this.doubled = true;
		}

		// If the reward is already owned by the time of collection (i.e. Dragon
		// has been acquired via other means), define its replacement
		if(reward.CheckReplacement(false)) {
			// Go through all replacement rewards (already sorted by priority) and use the first suitable one
			for(int i = 0; i < replacementDefs.Count; ++i) {
				// Create a reward for this replacement
				Metagame.Reward replacementReward = CreateRewardFromDef(replacementDefs[i]);

				// Is it a valid reward to give as replacement?
				if(replacementReward != null && !replacementReward.CheckReplacement(false)) {
					// Yes! Use it and break the loop
					reward.SetReplacement(replacementReward);
					break;
				}
			}
		}

		// Just push the reward to the rewards queue and marked it as collected
		// [AOC] From this moment on, the rewards are already in the pending rewards list if the flow is interrupted
		UsersManager.currentUser.PushReward(reward);
		this.collected = true;
	}

	//------------------------------------------------------------------------//
	// AUX METHODS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Compare two definitions from the dailyRewardsDefinitions table and determine
	/// which one comes first based on their "priority" field.
	/// </summary>
	/// <param name="_def1">First definition to be compared.</param>
	/// <param name="_def2">Second definition to be compared.</param>
	/// <returns>The result of the comparison (-1, 0, 1).</returns>
	private static int CompareDefsByPriority(DefinitionNode _def1, DefinitionNode _def2) {
		// Directly compare priority field
		int priorityResult = _def1.GetAsFloat("priority", 0).CompareTo(_def2.GetAsFloat("priority", 0));
		return priorityResult;
	}

	/// <summary>
	/// Creates a new Metagame.Reward initialized with the data in the given Definition from dailyRewardsDefinitions table.
	/// </summary>
	/// <param name="_def">Definition from dailyRewardsDefinitions table.</param>
	/// <returns>New reward created from the given definition.</returns>
	private static Metagame.Reward CreateRewardFromDef(DefinitionNode _def) {
		Metagame.Reward.Data rewardData = new Metagame.Reward.Data();
		rewardData.typeCode = _def.GetAsString("type");
		rewardData.amount = _def.GetAsLong("amount");
		rewardData.sku = _def.GetAsString("rewardSku");
		if(rewardData.typeCode == Metagame.RewardSoftCurrency.TYPE_CODE) {
			// Apply SC scaling
			rewardData.amount = ScaleByMaxDragonOwned(rewardData.amount);
		}
		return Metagame.Reward.CreateFromData(rewardData, ECONOMY_GROUP, DEFAULT_SOURCE);
	}

	/// <summary>
	/// Scale currency reward based on player's progression.
	/// </summary>
	/// <param name="_amount"></param>
	/// <returns></returns>
    private static long ScaleByMaxDragonOwned(long _amount)
    {
        DefinitionNode rewardScaleFactorDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.DAILY_REWARD_MODIFIERS, "dragonSku", DragonManager.biggestOwnedDragon.def.sku);
        if (rewardScaleFactorDef != null)
        {
            return Mathf.RoundToInt(((float)_amount) * rewardScaleFactorDef.GetAsFloat("dailyRewardsSCRewardMultiplier"));
        }
        return _amount;
    }

	//------------------------------------------------------------------------//
	// PERSISTENCE METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Constructor from json data.
	/// </summary>
	/// <param name="_data">Data to be parsed.</param>
	public void LoadData(SimpleJSON.JSONNode _data) {
		// Reset any existing data
		Reset();

		//Debug.Log(Colors.cyan.Tag("LOADING DAILY REWARD DATA\n") + new JsonFormatter().PrettyPrint(_data.ToString()));

		// Def (we're only saving the sku)
		if(_data.ContainsKey("sourceSku")) {
			sourceDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DAILY_REWARDS, _data["sourceSku"]);
		} else {
			Debug.Log(Colors.red.Tag("ERROR! Daily Reward doesn't contain sourceSku property!\n" + _data.ToString()));
		}

		// Reward
		reward = Metagame.Reward.CreateFromJson(_data, ECONOMY_GROUP, DEFAULT_SOURCE);

		// Replacements
		if(_data.ContainsKey("replacements")) {
			SimpleJSON.JSONArray replacementsData = _data["replacements"].AsArray;
			for(int i = 0; i < replacementsData.Count; ++i) {
				// Just in case, make sure the stored sku is valid
				DefinitionNode replacementRewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DAILY_REWARDS, replacementsData[i]);
				if(replacementRewardDef != null) {
					replacementDefs.Add(replacementRewardDef);
				}
			}
		}

		// State
		if(_data.ContainsKey("collected")) collected = _data["collected"].AsBool;
		if(_data.ContainsKey("doubled")) doubled = _data["doubled"].AsBool;
	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json.</returns>
	public SimpleJSON.JSONClass SaveData() {
		// Aux vars
		SimpleJSON.JSONClass data = null;

		// Reward
		if(reward != null) {
			data = reward.ToJson() as SimpleJSON.JSONClass;
		} else {
			Debug.Log(Colors.red.Tag("ERROR! Attempting to save a daily reward without a reward being created"));
			data = new SimpleJSON.JSONClass();
		}

		// Def (we're only saving the sku)
		if(sourceDef != null) {
			data.Add("sourceSku", sourceDef.sku);
		}

		// Replacements (only if needed)
		if(replacementDefs.Count > 0) {
			SimpleJSON.JSONArray replacementsData = new SimpleJSON.JSONArray();
			for(int i = 0; i < replacementDefs.Count; ++i) {
				replacementsData.Add(replacementDefs[i].sku);
			}
			data.Add("replacements", replacementsData);
		}

		// State
		data.Add("collected", collected);
		data.Add("doubled", doubled);

		return data;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}