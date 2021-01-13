// ModifierTooltip.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 10/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a modifier 
/// </summary>
public class ModifierTooltip : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Localizer m_nameText = null;
	public Localizer nameText {
		get { return m_nameText; }
	}

	[SerializeField] private TMPro.TextMeshProUGUI m_descriptionText = null;
	public TMPro.TextMeshProUGUI descriptionText {
		get { return m_descriptionText; }
	}

	[Space]
	[SerializeField] private Image m_modifierIcon = null;
	public Image powerIcon {
		get { return m_modifierIcon; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_nameText != null, "Required field!");
		Debug.Assert(m_descriptionText != null, "Required field!");
		Debug.Assert(m_modifierIcon != null, "Required field!");
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	

	public void InitFromDefinition(IModifierDefinition _modDef) {
		if (_modDef == null) return;

		// Power icon
		if(m_modifierIcon != null) {
			// Load from resources
			m_modifierIcon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + _modDef.GetIconRelativePath());
			m_modifierIcon.color = Color.white;
		}

		// Name and description
		// Name
		if(m_nameText != null) {
			// Already localized by the modifier definition
			m_nameText.Set(_modDef.GetName());
		}

		// Desc
		if(m_descriptionText != null) {
			m_descriptionText.text = _modDef.GetDescription();
		}
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}