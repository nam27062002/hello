// PopupSpecialDragonInfoUpgrade.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Sub-component of the Special Dragon Info popup.
/// </summary>
public class PopupSpecialDragonInfoUpgrade : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public class Data {
		public int unlockLevel = -1;
		public string description = string.Empty;
		public Sprite icon = null;
		public string leftIconFoot;
		public string rightIconFoot;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[Comment("Shared")]
	[SerializeField] private Localizer m_levelText = null;
	[SerializeField] private Localizer m_descriptionText = null;

	[Comment("For Power Upgrades", 10f)]
	[SerializeField] private Image m_powerIcon = null;
	[SerializeField] private Localizer m_powerName = null;

	[Comment("For Tier Upgrades", 10f)]
	[SerializeField] private TextMeshProUGUI m_petSlotsText;
	[SerializeField] private TextMeshProUGUI m_fireRushMultiplierText;

	// Data
	private int m_unlockLevel = -1;
	public int unlockLevel {
		get { return m_unlockLevel; }
	}

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the info widget as a Power upgrade.
	/// </summary>
	/// <param name="_powerDef">Power definition used for initialization.</param>
	public void InitPowerUpgrade(DefinitionNode _powerDef) {
		// Unlock level
		m_unlockLevel = _powerDef.GetAsInt("upgradeLevelToUnlock");
		if(m_levelText != null) {
			m_levelText.Localize(m_levelText.tid, StringUtils.FormatNumber(m_unlockLevel));
		}

		// Name
		if(m_powerName != null) {
			m_powerName.Localize(_powerDef.GetAsString("tidName"));
		}

		// Description
		if(m_descriptionText != null) {
			// Special powers don't have variables ^_^
			m_descriptionText.Localize(_powerDef.GetAsString("tidDesc"));
		}

		// Icon
		if(m_powerIcon != null) {
			m_powerIcon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + _powerDef.Get("icon"));
		}
	}

	/// <summary>
	/// Initialize the info widget as a Tier upgrade.
	/// </summary>
	/// <param name="_specialTierDef">Special Tier definition used for initialization.</param>
	public void InitTierUpgrade(DefinitionNode _specialTierDef) {
		// Get matching dragon tier def
		DefinitionNode tierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, _specialTierDef.Get("tier"));

		// Unlock level
		m_unlockLevel = _specialTierDef.GetAsInt("upgradeLevelToUnlock");
		if(m_levelText != null) {
			m_levelText.Localize(m_levelText.tid, StringUtils.FormatNumber(m_unlockLevel));
		}

		// Format multipliers
		string numPetsString = "x" + StringUtils.FormatNumber(_specialTierDef.GetAsInt("petsSlotsAvailable"));
		string fireMultiplierString = "x" + StringUtils.FormatNumber(_specialTierDef.GetAsFloat("furyScoreMultiplier", 2), 0);

		// Description
		if(m_descriptionText != null) {
			m_descriptionText.Localize(
				m_descriptionText.tid,
				numPetsString,
				fireMultiplierString
			);
		}

		// Pet slots
		if(m_petSlotsText != null) {
			m_petSlotsText.text = numPetsString;
		}

		// Fire rush multiplier
		if(m_fireRushMultiplierText != null) {
			m_fireRushMultiplierText.text = fireMultiplierString;
		}
	}
}