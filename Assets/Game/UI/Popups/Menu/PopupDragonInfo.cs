﻿// PopupDragonInfo.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup to display information of a dragon / dragon tier.
/// Specialization of the tier info popup showing some extra info.
/// </summary>
public class PopupDragonInfo : PopupTierPreyInfo {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Dragon Info Extra Elements")]
	[SerializeField] protected Localizer m_dragonNameText = null;
	[SerializeField] protected Localizer m_dragonDescText = null;
	[SerializeField] protected Image m_dragonIcon = null;
	[Space]
	[SerializeField] protected TextMeshProUGUI m_healthText = null;
	[SerializeField] protected TextMeshProUGUI m_energyText = null;
	[SerializeField] protected TextMeshProUGUI m_speedText = null;
	[Space]
	[SerializeField] protected Localizer m_tierInfoText = null;

	// Internal
	protected IDragonData m_dragonData = null;

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initializes and open the info popup with the given dragon data, taking its type in consideration.
	/// </summary>
	/// <param name="_dragonData">The data to be used to choose which popup to open and to initialize it.</param>
	/// <param name="_trackingLocation">Optional, where the popup is being opened from. If empty, no tracking event will be sent.</param>
	/// <returns>A reference to the opened popup.</returns>
	public static PopupDragonInfo OpenPopupForDragon(IDragonData _dragonData, string _trackingLocation = null) {
		// Check params
		if(_dragonData == null) return null;

		// Choose popup type based on dragon type
		string popupPath = string.Empty;
		switch(_dragonData.type) {
			case IDragonData.Type.CLASSIC:	popupPath = PopupClassicDragonInfo.PATH;	break;
			case IDragonData.Type.SPECIAL:	popupPath = PopupSpecialDragonInfo.PATH;	break;
		}

		// Load the chosen popup, initialize and open it
		PopupController popup = PopupManager.LoadPopup(popupPath);
		PopupDragonInfo infoPopup = popup.GetComponent<PopupDragonInfo>();
		infoPopup.Init(_dragonData);
		popup.Open();

		// If defined, send tracking event
		if(!string.IsNullOrEmpty(_trackingLocation)) {
			string popupName = System.IO.Path.GetFileNameWithoutExtension(popupPath);
			HDTrackingManager.Instance.Notify_InfoPopup(popupName, _trackingLocation);
		}

		// Done!
		return infoPopup;
	}

	//------------------------------------------------------------------------//
	// SCROLLING CONTROL													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given dragon info.
	/// </summary>
	/// <param name="_dragonData">Data of the dragon whose info we want to display.</param>
	public void Init(IDragonData _dragonData) {
		// Initialize with currently selected dragon
		m_dragonData = _dragonData;

		// Call parent
		if(m_dragonData != null) {
			base.Init(m_dragonData.tier);
		}
	}

	/// <summary>
	/// Refresh the popup with the info from the currently selected dragon (in the scroller).
	/// </summary>
	protected override void Refresh() {
		// Call parent
		base.Refresh();

		// Only if current data is valid
		if(m_dragonData == null) return;

		// Dragon name
		if(m_dragonNameText != null) {
			m_dragonNameText.Localize(m_dragonData.def.Get("tidName"));
		}

		// Dragon description
		if(m_dragonDescText != null) {
			m_dragonDescText.Localize(m_dragonData.def.Get("tidDesc"));
		}

		// Dragon icon
		if(m_dragonIcon != null) {
            string defaultIcon = IDragonData.GetDefaultDisguise(m_dragonData.def.sku).Get("icon");
            m_dragonIcon.sprite = HDAddressablesManager.Instance.LoadAsset<Sprite>( defaultIcon );
		}

		// HP
		if(m_healthText != null) {
			m_healthText.text = StringUtils.FormatNumber(m_dragonData.maxHealth, 0);
		}

		// Boost
		if(m_energyText != null) {
			m_energyText.text = StringUtils.FormatNumber(m_dragonData.baseEnergy, 0);
		}

		// Speed
		if(m_speedText != null) {
			m_speedText.text = StringUtils.FormatNumber(m_dragonData.maxSpeed * 10f, 0);    // x10 to show nicer numbers
		}

		// Tier description
		if(m_tierInfoText != null) {
			// %U0 dragons can equip <color=%U1>%U2 pets</color> and give a <color=%U1>%U3</color> 
			// multiplier during <color=%U4>Fire Rush</color>
			int numPets = m_dragonData.pets.Count;  // Dragon data has as many slots as defined for this dragon
			m_tierInfoText.Localize("TID_DRAGON_INFO_TIER_DESCRIPTION",
				UIConstants.GetTierTag(m_dragonData.tierDef),
				(numPets > 1 ? LocalizationManager.SharedInstance.Localize("TID_PET_PLURAL") : LocalizationManager.SharedInstance.Localize("TID_PET")), // Singular/Plural
				StringUtils.FormatNumber(numPets),
				"x" + StringUtils.FormatNumber(m_dragonData.def.GetAsFloat("furyScoreMultiplier", 2), 0)
			);
		}
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}