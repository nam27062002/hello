// DisguisePowerTooltip.cs
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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a disguise power tooltip.
/// </summary>
public class PowerTooltip : MonoBehaviour {
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
	[SerializeField] private Image m_powerIcon = null;
	public Image powerIcon {
		get { return m_powerIcon; }
	}

	[SerializeField] private GameObject m_lockInfo = null;

	// Data
	private DefinitionNode m_powerDef = null;
	public DefinitionNode powerDef {
		get { return m_powerDef; }
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
		Debug.Assert(m_powerIcon != null, "Required field!");
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this button with the data from the given definition.
	/// </summary>
	/// <param name="_powerDef">Power definition.</param>
	public void InitFromDefinition(DefinitionNode _powerDef, PowerIcon.Mode _mode) {
		// Ignore if given definition is not valid
		if(_powerDef == null) return;

		// Save definition
		m_powerDef = _powerDef;

		// Power icon
		if(m_powerIcon != null) {
			// Load from resources
			m_powerIcon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + _powerDef.GetAsString("icon"));
			m_powerIcon.color = Color.white;
		}

		// Name and description
		// Name
		if(m_nameText != null) {
			m_nameText.Localize(_powerDef.Get("tidName"));
		}

		// Desc
		if(m_descriptionText != null) {
			m_descriptionText.text = DragonPowerUp.GetDescription(_powerDef, false, _mode == PowerIcon.Mode.PET);   // Custom formatting depending on powerup type, already localized
		}
	}

	public void InitFromDefinition(IModifierDefinition _modDef) {
		if (_modDef == null) return;

		// Power icon
		if(m_powerIcon != null) {
			// Load from resources
			m_powerIcon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + _modDef.GetIconRelativePath());
			m_powerIcon.color = Color.white;
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

	/// <summary>
	/// Sets the lock state of the power.
	/// </summary>
	/// <param name="_locked">Whether the power is locked or not.</param>
	public void SetLocked(bool _locked) {
		// Lock info
		if(m_lockInfo != null) m_lockInfo.SetActive(_locked);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}