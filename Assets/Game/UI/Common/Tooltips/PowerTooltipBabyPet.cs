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
		public PowerIcon icon { get { return m_icon; } }

		[SerializeField] private TextMeshProUGUI m_powerNameText = null;
		public TextMeshProUGUI powerNameText { get { return m_powerNameText; } }

		[SerializeField] private TextMeshProUGUI m_powerDescText = null;
		public TextMeshProUGUI powerDescText { get { return m_powerDescText; } }

		[NonSerialized] public DefinitionNode powerDef = null;

		/// <summary>
		/// Initialize with a given power.
		/// </summary>
		/// <param name="_powerSku">Sku of the power to be used for initialization.</param>
		/// <param name="_petDef">Definition of the pet this power belongs to.</param>
		public void InitWithPower(string _powerSku, DefinitionNode _petDef) {
			// Get power definition and use Definition initializer
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _powerSku);
			InitWithPower(def, _petDef);
		}

		/// <summary>
		/// Initialize with a given power.
		/// </summary>
		/// <param name="_powerDef">Definition of the power to be used for initialization.</param>
		/// <param name="_petDef">Definition of the pet this power belongs to.</param>
		public void InitWithPower(DefinitionNode _powerDef, DefinitionNode _petDef) {
			// Store power def
			powerDef = _powerDef;

			// Nothing to do if null
			if(powerDef == null) return;

			// Power icon
			if(m_icon != null) {
				m_icon.InitFromDefinition(powerDef, _petDef, false, true, PowerIcon.Mode.PET);
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
	[Space]
	[SerializeField] private PowerGroup m_mainPower = null;
	[Space]
	[SerializeField] private PowerGroup m_collectionPower = null;
	[SerializeField] private TextMeshProUGUI m_collectionCounterText = null;
	[Space]
	[SerializeField] private PowerGroup m_familyPower = null;
	[SerializeField] private UIColorFX m_familyPowerIconFX = null;

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
	/// <param name="_displayMode">The display mode for this power.</param>
	public void InitFromDefinition(DefinitionNode _babyPetDef, PowerIcon.DisplayMode _displayMode) {
		// Ignore if given definition is not valid
		if(_babyPetDef == null) return;

		// Save definition
		m_petDef = _babyPetDef;

		// Init main power
		if(m_mainPower != null) {
			m_mainPower.InitWithPower(m_petDef.GetAsString("powerup"), m_petDef);
		}

		// Init collection bonus power
		if(m_collectionPower != null) {
			// Not using powerups definitions table, so gather definition first
			DefinitionNode collectionPowerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.BABY_DRAGONS_SHARED_POWER, m_petDef.GetAsString("sharedPower"));
			m_collectionPower.InitWithPower(collectionPowerDef, m_petDef);
		}

		// Init collection counter
		if(m_collectionCounterText != null) {
			/*m_collectionCounterText.text = LocalizationManager.SharedInstance.Localize(
				"TID_FRACTION",
				StringUtils.FormatNumber(UsersManager.currentUser.petCollection.unlockedPetsCount),
				StringUtils.FormatNumber(BABY_PETS_TOTAL_COUNT)
			);*/
			m_collectionCounterText.text = LocalizationManager.SharedInstance.ReplaceParameters(
				"x%U0",
				StringUtils.FormatNumber(UsersManager.currentUser.petCollection.unlockedBabyPetsCount)
			);
		}

		// Init family bonus power
		if(m_familyPower != null) {
			// Default setup
			m_familyPower.InitWithPower(m_petDef.GetAsString("statPower"), m_petDef);

			// Aux vars
			bool familyPowerActive = false;
			string motherDragonSku = m_petDef.GetAsString("motherDragonSKU");

			// If family power is not active, or if in preview mode, do some changes
			if(_displayMode == PowerIcon.DisplayMode.EQUIPPED) {
				// Only need to check if EQUIPPED mode. Check pet's mother dragon against current dragon.
				familyPowerActive = UsersManager.currentUser.CurrentDragon == motherDragonSku;
			}

			// B/W icon
			if(m_familyPowerIconFX != null) {
				// Only in EQUIPPED mode when family power is not active
				if(_displayMode == PowerIcon.DisplayMode.EQUIPPED && !familyPowerActive) {
					m_familyPowerIconFX.saturation = UIColorFX.SATURATION_MIN;
				} else {
					m_familyPowerIconFX.saturation = UIColorFX.SATURATION_DEFAULT;
				}
			}

			// Different text
			if(m_familyPower.powerDescText != null) {
				// Change text for preview mode or when fanily power is not active
				if(_displayMode == PowerIcon.DisplayMode.PREVIEW || !familyPowerActive) {
					// Get mother dragon definition
					DefinitionNode motherDragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, motherDragonSku);

					// Initialize text
					m_familyPower.powerDescText.text = LocalizationManager.SharedInstance.Localize(
						"TID_POWERUP_BABY_FAMILY_BONUS_DESC",
						//m_familyPower.powerDescText.text,   // Original text, initialized with the InitWithPower() call
						DragonPowerUp.GetDescription(m_familyPower.powerDef, false, true),
						motherDragonDef != null ? motherDragonDef.GetLocalized("tidName") : ""
					);
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}