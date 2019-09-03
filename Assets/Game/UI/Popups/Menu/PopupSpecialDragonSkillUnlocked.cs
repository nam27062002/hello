// PopupLabSkillUnlocked.cs
// 
// Created by Alger Ortín Castellví on 08/11/2018.
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
public class PopupSpecialDragonSkillUnlocked : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupSpecialDragonSkillUnlocked";

	//------------------------------------------------------------------------//
	// MEMBERS														 		  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Localizer m_skillNameText = null;
	[SerializeField] private Localizer m_skillDescText = null;
	[SerializeField] private Image m_skillIcon = null;

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given skill definition (from the specialDragonPowerDefinitions table).
	/// </summary>
	public void Init(DefinitionNode _def) {
		// Name
		if(m_skillNameText != null) {
			m_skillNameText.Localize(_def.GetAsString("tidName"));
		}

		// Description
		if(m_skillDescText != null) {
			m_skillDescText.Localize(_def.GetAsString("tidDesc"));
		}

		// Icon
		if(m_skillIcon != null) {
			m_skillIcon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + _def.GetAsString("icon"));
			m_skillIcon.color = Color.white;
		}
	}
}
