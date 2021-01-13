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
using System.Globalization;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Data structure representing a boosted daily reward.
/// Just like a regular daily reward, but when loading, uses another table in content
/// </summary>
[Serializable]
public class BoostedDailyReward : DailyReward {
	
	//------------------------------------------------------------------------//
	// OVERRIDES PARENT		    											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Constructor from json data.
	/// </summary>
	/// <param name="_data">Data to be parsed.</param>
	public override void LoadData(SimpleJSON.JSONNode _data) {
		// Reset any existing data
		Reset();

		//Debug.Log(Colors.cyan.Tag("LOADING DAILY REWARD DATA\n") + new JsonFormatter().PrettyPrint(_data.ToString()));

		// Def (we're only saving the sku)
		if(_data.ContainsKey("sourceSku")) {
			sourceDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.BOOSTED_DAILY_REWARDS, _data["sourceSku"]);
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

		// Customization ID
		if(_data.ContainsKey("customizationID")) {
			customizationID = _data["customizationID"];
		} else {
			// Retro-compatibility: if not generated, do it now
			customizationID = GenerateCustomizationID();
		}

		// State
		if(_data.ContainsKey("collected")) collected = PersistenceUtils.SafeParse<bool>(_data["collected"]);
		if(_data.ContainsKey("doubled")) multiplied = PersistenceUtils.SafeParse<bool>(_data["doubled"]);
	}

	


}