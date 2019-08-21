// PopupLabTierUnlocked.cs
// 
// Created by Alger Ortín Castellví on 09/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tiers info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupLabTierUnlocked : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupLabTierUnlocked";

	//------------------------------------------------------------------------//
	// MEMBERS														 		  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Localizer m_tierDescText = null;
	[SerializeField] private Image m_tierIcon = null;


	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given special tier definition (from the specialDragonTierDefinitions table).
	/// </summary>
	/// <param name="_tierDef">Definition of the tier.</param>
	/// <param name="_specialTierDef">Definition of the special tier.</param>
	public void Init(DefinitionNode _tierDef, DefinitionNode _specialTierDef) {

		// Description
		if(m_tierDescText != null) {
			// Can equip <TID_COLOR_PET>%U0 %U1<TID_END_COLOR> and get a <TID_COLOR_PET>%U2<TID_END_COLOR> multiplier during <TID_COLOR_FIRERUSH><TID_FIRE_RUSH><TID_END_COLOR>
			int numPets = _specialTierDef.GetAsInt("petsSlotsAvailable");
			m_tierDescText.Localize(
				"TID_SPECIAL_DRAGON_INFO_TIER_DESCRIPTION",
				StringUtils.FormatNumber(numPets),
				(numPets > 1 ? LocalizationManager.SharedInstance.Localize("TID_PET_PLURAL") : LocalizationManager.SharedInstance.Localize("TID_PET")), // Singular/Plural
				"x" + StringUtils.FormatNumber(_specialTierDef.GetAsFloat("furyScoreMultiplier", 2), 0)
			);
		}

		// Icon
		if(m_tierIcon != null) {
			m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, _tierDef.GetAsString("icon"));
			m_tierIcon.color = Color.white;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//


	/// <summary>
	/// The popup has just been closed.
	/// </summary>
	public void OnClosePostAnimation() {
		// [AOC] TODO!! If it's the last tier, show small info popup informing the player that he can still keep upgrading his special dragon stats
	}
}
