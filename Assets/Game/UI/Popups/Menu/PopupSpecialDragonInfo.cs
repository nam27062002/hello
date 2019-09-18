// PopupSpecialDragonInfo.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// Popup to display information of a special dragon.
/// </summary>
public class PopupSpecialDragonInfo : PopupDragonInfo {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	new public const string PATH = "UI/Popups/Menu/PF_PopupSpecialDragonInfo";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Special Dragon Stuff")]
	[SerializeField] private TextMeshProUGUI m_dragonIconLevelText = null;
	[SerializeField] private SpecialDragonBar m_specialDragonLevelBar = null;
	[SerializeField] private PopupSpecialDragonInfoUpgrade[] m_powerUpgrades = new PopupSpecialDragonInfoUpgrade[0];
	[SerializeField] private PopupSpecialDragonInfoUpgrade[] m_tierUpgrades = new PopupSpecialDragonInfoUpgrade[0];

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the popup with the info from the currently selected dragon (in the scroller).
	/// </summary>
	protected override void Refresh() {
		// Call parent
		base.Refresh();

		// Aux vars
		DragonDataSpecial dragonData = m_dragonData as DragonDataSpecial;
		Debug.Assert(dragonData != null, "ONLY FOR SPECIAL DRAGONS!");

		// More aux vars
		int i = 0;
		int j = 0;
		int powerUpgradesCount = m_powerUpgrades.Length;
		int tierUpgradesCount = m_tierUpgrades.Length;

		// Level text
		if(m_dragonIconLevelText != null) {
			m_dragonIconLevelText.text = StringUtils.FormatNumber(dragonData.Level);
		}

		// Initialize level bar
		m_specialDragonLevelBar.BuildFromDragonData(dragonData);

		// Upgrade Infos
		// Power upgrades
		List<DefinitionNode> powerDefs = dragonData.specialPowerDefsByOrder;
		for(i = 0, j = 0; i < powerDefs.Count && j < powerUpgradesCount; ++i, ++j) {
			m_powerUpgrades[j].gameObject.SetActive(true);
			m_powerUpgrades[j].InitPowerUpgrade(powerDefs[i]);
		}

		// Hide remaining widgets
		for(; j < powerUpgradesCount; ++i) {
			m_powerUpgrades[j].gameObject.SetActive(false);
		}

		// Tier upgrades
		// Special case! Skip first tier, since is the default one
		List<DefinitionNode> specialTierDefs = dragonData.specialTierDefsByOrder;
		for(i = 1, j = 0; i < specialTierDefs.Count && j < tierUpgradesCount; ++i, ++j) {
			m_tierUpgrades[j].gameObject.SetActive(true);
			m_tierUpgrades[j].InitTierUpgrade(specialTierDefs[i]);
		}

		// Hide remaining widgets
		for(; j < tierUpgradesCount; ++j) {
			m_tierUpgrades[j].gameObject.SetActive(false);
		}

		// Put widgets into proper X position to match their unlock level
		RepositionUpgrades();
	}

	/// <summary>
	/// Put upgrade info widgets into position.
	/// </summary>
	private void RepositionUpgrades() {
		// Power upgrades
		for(int i = 0; i < m_powerUpgrades.Length; ++i) {
			// Get matching bar element
			SpecialDragonBarElement barElement = m_specialDragonLevelBar.GetElementAtLevel(m_powerUpgrades[i].unlockLevel - 1); // 0-indexed

			// Apply the same X position
			// Use global position since they are at diferent hierarchy levels!
			m_powerUpgrades[i].transform.SetPosX(barElement.transform.position.x);
		}

		// Tier upgrades
		for(int i = 0; i < m_tierUpgrades.Length; ++i) {
			// Get matching bar element
			SpecialDragonBarElement barElement = m_specialDragonLevelBar.GetElementAtLevel(m_tierUpgrades[i].unlockLevel - 1);  // 0-indexed
			if(barElement == null) {
				Debug.Log(Color.red.Tag("NO BAR ELEMENT COULD BE FOUND FOR LEVEL " + m_tierUpgrades[i].unlockLevel));
				break;
			}

			// Apply the same X position
			// Use global position since they are at diferent hierarchy levels!
			m_tierUpgrades[i].transform.SetPosX(barElement.transform.position.x);
		}
	}
}