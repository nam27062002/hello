// PowerTooltip_BabyPet.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization for a baby pet power tooltip.
/// </summary>
public class PowerTooltip_BabyPet : IPowerTooltip {
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

	// For performance, cache invariable values in static variables
	private static int s_babyPetsTotalCount = -1;
	private static int BABY_PETS_TOTAL_COUNT {
		get {
			// Static variable needs initialization?
			if(s_babyPetsTotalCount < 0) {
				// Get all baby pets definitions
				List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.PETS, "category", "baby");

				// Count only those that are not hidden
				s_babyPetsTotalCount = 0;   // Reset counter
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
	
	//------------------------------------------------------------------------//
	// ABSTRACT METHODS IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the tooltip. To be implemented by heirs.
	/// At this point, internal data variables have been initialized.
	/// </summary>
	protected override void Init_Internal() {
		// Nothing to do if pet definition is not valid
		if(m_sourceDef == null) return;

		// Init main power
		if(m_mainPower != null) {
			m_mainPower.InitWithPower(m_sourceDef.GetAsString("powerup"), m_sourceDef);
		}

		// Init collection bonus power
		if(m_collectionPower != null) {
			// Not using powerups definitions table, so gather definition first
			DefinitionNode collectionPowerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.BABY_DRAGONS_SHARED_POWER, m_sourceDef.GetAsString("sharedPower"));
			m_collectionPower.InitWithPower(collectionPowerDef, m_sourceDef);
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
			m_familyPower.InitWithPower(m_sourceDef.GetAsString("statPower"), m_sourceDef);

			// Aux vars
			bool familyPowerActive = false;
			string motherDragonSku = m_sourceDef.GetAsString("motherDragonSKU");

			// If family power is not active, or if in preview mode, do some changes
			if(m_displayMode == PowerIcon.DisplayMode.EQUIPPED) {
				// Only need to check if EQUIPPED mode. Check pet's mother dragon against current dragon.
				familyPowerActive = UsersManager.currentUser.CurrentDragon == motherDragonSku;
			}

			// B/W icon
			if(m_familyPowerIconFX != null) {
				// Only in EQUIPPED mode when family power is not active
				if(m_displayMode == PowerIcon.DisplayMode.EQUIPPED && !familyPowerActive) {
					m_familyPowerIconFX.saturation = UIColorFX.SATURATION_MIN;
				} else {
					m_familyPowerIconFX.saturation = UIColorFX.SATURATION_DEFAULT;
				}
			}

			// Different text
			if(m_familyPower.powerDescText != null) {
				// Change text for preview mode or when fanily power is not active
				if(m_displayMode == PowerIcon.DisplayMode.PREVIEW || !familyPowerActive) {
					// Get mother dragon name to display
					DefinitionNode motherDragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, motherDragonSku);
					string motherName = "";
					if(motherDragonDef != null) {
						// Highlight name using power's color
						Color powerHighlightColor = DragonPowerUp.GetColor(m_familyPower.powerDef);
						motherName = motherDragonDef.GetLocalized("tidName");
						motherName = powerHighlightColor.Tag(motherName);
					}

					// Initialize text
					m_familyPower.powerDescText.text = LocalizationManager.SharedInstance.Localize(
						"TID_POWERUP_BABY_FAMILY_BONUS_DESC",
						DragonPowerUp.GetDescription(m_familyPower.powerDef, false, true),
						motherName
					);
				}

				// Gray out power name to emphazise the fact that the power is disabled
				if(m_familyPower.powerNameText != null) {
					// Only in EQUIPPED mode when family power is not active
					if(m_displayMode == PowerIcon.DisplayMode.EQUIPPED && !familyPowerActive) {
						m_familyPower.powerNameText.text = Colors.silver.Tag(m_familyPower.powerNameText.text);
					}
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}