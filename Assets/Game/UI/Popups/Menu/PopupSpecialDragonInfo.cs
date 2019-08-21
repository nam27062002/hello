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

	public class UpgradeData {
		public int unlockLevel = -1;
		public string description = string.Empty;
		public Sprite icon = null;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Special Dragon Stuff")]
	[SerializeField] private PopupSpecialDragonInfoUpgrade[] m_upgrades = new PopupSpecialDragonInfoUpgrade[0];

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

		// Upgrade Infos
		// Gather all the data
		List<UpgradeData> upgradesData = new List<UpgradeData>();

		// Tier upgrades (they only upgrade the pets slots capacity, the tier remains same)
		List<DefinitionNode> specialTierDefs = dragonData.specialTierDefsByOrder;
		for(int i = 0; i < specialTierDefs.Count; ++i) {
			UpgradeData data = new UpgradeData();

			// Get matching dragon tier def
			DefinitionNode tierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, specialTierDefs[i].Get("tier"));

			// Unlock level
			data.unlockLevel = specialTierDefs[i].GetAsInt("upgradeLevelToUnlock");

			// Description
			// Can equip <TID_COLOR_PET>%U0 %U1<TID_END_COLOR> and get a <TID_COLOR_PET>%U2<TID_END_COLOR> multiplier during <TID_COLOR_FIRERUSH><TID_FIRE_RUSH><TID_END_COLOR>
			int numPets = specialTierDefs[i].GetAsInt("petsSlotsAvailable");
            data.description = LocalizationManager.SharedInstance.Localize("TID_SPECIAL_DRAGON_INFO_TIER_DESCRIPTION",
				StringUtils.FormatNumber(numPets),
                (numPets > 1 ? LocalizationManager.SharedInstance.Localize("TID_PET_PLURAL") : LocalizationManager.SharedInstance.Localize("TID_PET")), // Singular/Plural
                "x" + StringUtils.FormatNumber(specialTierDefs[i].GetAsFloat("furyScoreMultiplier", 2), 0)
            );

			// Icon
			data.icon = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, tierDef.GetAsString("icon"));

			// Push to list!
			upgradesData.Add(data);
		}

		// Power upgrades
		List<DefinitionNode> powerDefs = dragonData.specialPowerDefsByOrder;
		for(int i = 0; i < powerDefs.Count; ++i) {
			UpgradeData data = new UpgradeData();

			// Unlock level
			data.unlockLevel = powerDefs[i].GetAsInt("upgradeLevelToUnlock");

			// Description
			data.description = powerDefs[i].GetLocalized("tidDesc");

			// Icon
			data.icon = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + powerDefs[i].Get("icon"));

			// Push to list!
			upgradesData.Add(data);
		}

		// Sort upgrades by unlock level
		upgradesData.Sort(
			(UpgradeData _d1, UpgradeData _d2) => {
				return _d1.unlockLevel.CompareTo(_d2.unlockLevel);
			}
		);

		// Initialize a UI element for each upgrade
		int infoIdx = 0;
		for(int i = 0; i < upgradesData.Count && infoIdx < m_upgrades.Length; ++i) {
			// Skip if unlock level is 0
			if(upgradesData[i].unlockLevel <= 0) continue;

			// Initialize element
			PopupSpecialDragonInfoUpgrade element = m_upgrades[infoIdx];
			infoIdx++;

			// Level
			if(element.levelText != null) {
				element.levelText.text = LocalizationManager.SharedInstance.Localize(
					"TID_LEVEL",
					StringUtils.FormatNumber(upgradesData[i].unlockLevel)
				);
			}

			// Description
			if(element.descriptionText != null) {
				element.descriptionText.text = upgradesData[i].description;
			}

			// Icon
			if(element.icon != null) {
				element.icon.sprite = upgradesData[i].icon;
			}
		}
	}
}