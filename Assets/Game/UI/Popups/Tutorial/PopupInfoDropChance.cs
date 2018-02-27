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
		// Apply replacements
		for(int i = 0; i < m_rarityInfos.Length; ++i) {
			// Apply rarity color from UIConstants
			m_rarityInfos[i].messageText.Localize(
				m_rarityInfos[i].messageText.tid, 
				UIConstants.GetRarityColor(m_rarityInfos[i].rarity).OpenTag()
			);

			// Get rarity drop chance from content
			// Get all egg rewards of the target rarity and add up their probabilities
			List<DefinitionNode> rewardDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(
				DefinitionsCategory.EGG_REWARDS,
				"rarity",
				Metagame.Reward.RarityToSku(m_rarityInfos[i].rarity)
			);

			float totalProbability = 0f;
			for(int j = 0; j < rewardDefs.Count; ++j) {
				totalProbability += rewardDefs[j].GetAsFloat("droprate", 0f);
			}

			// Format and set text
			m_rarityInfos[i].chanceText.text = StringUtils.MultiplierToPercentage(totalProbability);
		}
	}
}
