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
		order = 0;

		// Reset state
		collected = false;
		doubled = false;
	}

	/// <summary>
	/// Initialize data with a given definition.
	/// Use it when creating a new reward sequence.
	/// </summary>
	/// <param name="_def">Definition from the DAILY_REWARDS category.</param>
	public void InitFromDef(DefinitionNode _def) {
		// Clear previous data and put default values
		Reset();

		// Store source definition
		sourceDef = _def;

		// Create new reward
		Metagame.Reward.Data rewardData = new Metagame.Reward.Data();
		rewardData.typeCode = _def.GetAsString("type");
		rewardData.amount = _def.GetAsLong("amount");
		rewardData.sku = _def.GetAsString("rewardSku");
		if(rewardData.typeCode == Metagame.RewardSoftCurrency.TYPE_CODE) {
			// Apply SC scaling
			rewardData.amount = ScaleByMaxDragonOwned(rewardData.amount);
		}
		reward = Metagame.Reward.CreateFromData(rewardData, ECONOMY_GROUP, DEFAULT_SOURCE);

		// Special case: If the reward is already owned by the time the sequence 
		// is generated (i.e. Pets), use its replacement instead.
		// This won't be the case if the reward is obtained via other means after the
		// sequence is generated (as designed).
		if(reward.WillBeReplaced()) {
			reward = reward.replacement;
		}
	}

    private long ScaleByMaxDragonOwned(long _amount)
    {
        DefinitionNode rewardScaleFactorDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.DAILY_REWARD_MODIFIERS, "dragonSku", DragonManager.biggestOwnedDragon.def.sku);
        if (rewardScaleFactorDef != null)
        {
            return Mathf.RoundToInt(((float)_amount) * rewardScaleFactorDef.GetAsFloat("dailyRewardsSCRewardMultiplier"));
        }
        return _amount;
    }

    /// <summary>
    /// Collect the reward :)
    /// No checks performed.
    /// </summary>
    /// <param name="_doubled">Has the reward been doubled?</param>
    public void Collect(bool _doubled) {
		// Double the reward?
		// [AOC] Just in case, don't do it again if it has already been doubled!
		if(_doubled && !this.doubled) {
			reward.bonusPercentage = 100f;
			this.doubled = true;
		}

		// Just push the reward to the rewards queue and marked it as collected
		// [AOC] From this moment on, the rewards are already in the pending rewards list if the flow is interrupted
		UsersManager.currentUser.PushReward(reward);
		this.collected = true;
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

		// State
		data.Add("collected", collected);
		data.Add("doubled", doubled);

		return data;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}