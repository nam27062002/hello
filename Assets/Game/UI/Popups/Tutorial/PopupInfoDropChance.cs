// PopupInfoDropChance.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Eggs info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupInfoDropChance : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoDropChance";

	[System.Serializable]
	public class RarityInfo {
		public Metagame.Reward.Rarity rarity = Metagame.Reward.Rarity.COMMON;
		public Localizer messageText = null;
		public TextMeshProUGUI chanceText = null;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private RarityInfo[] m_rarityInfos = new RarityInfo[3];

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		InitInfos(m_rarityInfos);
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the given array of info objects with current data from content.
	/// Moved into a static function to avoid duplicating code.
	/// </summary>
	/// <param name="_infos">Infos to be initialized.</param>
	public static void InitInfos(RarityInfo[] _infos) {
		// Apply replacements
		for(int i = 0; i < _infos.Length; ++i) {
			// Apply rarity color from UIConstants
			_infos[i].messageText.Localize(
				_infos[i].messageText.tid, 
				UIConstants.GetRarityColor(_infos[i].rarity).OpenTag()
			);

			// Get rarity drop chance from content
			// Get all egg rewards of the target rarity and add up their probabilities
			List<DefinitionNode> rewardDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(
				DefinitionsCategory.EGG_REWARDS,
				"rarity",
				Metagame.Reward.RarityToSku(_infos[i].rarity)
			);

			float totalProbability = 0f;
			for(int j = 0; j < rewardDefs.Count; ++j) {
				totalProbability += rewardDefs[j].GetAsFloat("droprate", 0f);
			}

			// Format and set text
			_infos[i].chanceText.text = StringUtils.MultiplierToPercentage(totalProbability);
		}
	}
}
