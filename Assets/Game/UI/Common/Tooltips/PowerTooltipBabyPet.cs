// DisguisePowerTooltipBabyPet.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using Calety.Customiser.Api;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the Baby Dragon Pets power tooltip.
/// </summary>
public class PowerTooltipBabyPet : UITooltip
{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar class to store UI components needed to display a single power.
	/// </summary>
	[Serializable]
	private class PowerGroup {
		[SerializeField] private PowerIcon m_icon = null;
		[SerializeField] private TextMeshProUGUI m_powerNameText = null;
		[SerializeField] private TextMeshProUGUI m_powerDescText = null;

		[NonSerialized] public DefinitionNode powerDef = null;

		/// <summary>
		/// Initialize with a given power.
		/// </summary>
		/// <param name="_powerSku">Sku of the power to be used for initialization.</param>
		public void InitWithPower(string _powerSku) {
			// Get power definition and use Definition initializer
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _powerSku);
			InitWithPower(def);
		}

		/// <summary>
		/// Initialize with a given power.
		/// </summary>
		/// <param name="_powerDef">Definition of the power to be used for initialization.</param>
		public void InitWithPower(DefinitionNode _powerDef) {
			// Store power def
			powerDef = _powerDef;

			// Nothing to do if null
			if(powerDef == null) return;

			// Power icon
			if(m_icon != null) {
				m_icon.InitFromDefinition(powerDef, false, true, PowerIcon.Mode.PET);
			}

			// Power name
			if(m_powerNameText != null) {
				m_powerNameText.text = LocalizationManager.SharedInstance.Localize(powerDef.Get("tidName"));
			}

			// Power description
			if(m_powerDescText != null) {
				m_powerDescText.text = DragonPowerUp.GetDescription(powerDef, false, true);   // Custom formatting depending on powerup type, already localized
			}
		}
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private PowerGroup m_mainPower = null;
	[SerializeField] private PowerGroup m_collectionPower = null;
	[SerializeField] private PowerGroup m_familyPower = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_collectionCounterText = null;

	// Data
	private DefinitionNode m_petDef = null;

	// For performance, cache invariable values in static variables
	private static int s_babyPetsTotalCount = -1;
	private static int BABY_PETS_TOTAL_COUNT {
		get {
			// Static variable needs initialization?
			if(s_babyPetsTotalCount < 0) {
				// Get all baby pets definitions
				List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.PETS, "category", "baby");

				// Count only those that are not hidden
				s_babyPetsTotalCount = 0;	// Reset counter
				int count = defs.Count;
				for(int i = 0; i < count; ++i) {
					if(!defs[i].GetAsBool("hidden")) {
						s_babyPetsTotalCount++;
					}
				}
			}
			return s_babyPetsTotalCount;
		}
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	new private void Awake() {
		// Start hidden
        if(animator != null) animator.ForceHide(false);
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this tooltip with the data from the given definition.
	/// </summary>
	/// <param name="_babyPetDef">Baby pet definition.</param>
	public void InitFromDefinition(DefinitionNode _babyPetDef) {
		// Ignore if given definition is not valid
		if(_babyPetDef == null) return;

		// Save definition
		m_petDef = _babyPetDef;

		// Init main power
		if(m_mainPower != null) {
			m_mainPower.InitWithPower(m_petDef.GetAsString("powerup"));
		}

		// Init collection bonus power
		if(m_collectionPower != null) {
			// Not using powerups definitions table, so gather definition first
			DefinitionNode collectionPowerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.BABY_DRAGONS_SHARED_POWER, m_petDef.GetAsString("sharedPower"));
			m_collectionPower.InitWithPower(collectionPowerDef);
		}

		// Init collection counter
		if(m_collectionCounterText != null) {
			LocalizationManager.SharedInstance.Localize(
				"TID_FRACTION",
				StringUtils.FormatNumber(UsersManager.currentUser.petCollection.unlockedPetsCount),
				StringUtils.FormatNumber(BABY_PETS_TOTAL_COUNT)
			);
		}

		// Init family bonus power

		// Power icon
		/*if(m_powerIcon != null) {
            // Load from resources
            m_powerIcon.InitFromDefinition(_powerDef,false,false);
		}

		// Name and description
		// Name
		if(m_titleText != null) {
            string title = LocalizationManager.SharedInstance.Localize(_powerDef.Get("tidName"));
            Debug.Log ("set Title: " + title);
            m_titleText.text = title;
		}

		// Desc
		if(m_messageText != null) {
            m_messageText.text = DragonPowerUp.GetDescription(_powerDef, false, _mode == PowerIcon.Mode.PET);   // Custom formatting depending on powerup type, already localized
		}*/
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}