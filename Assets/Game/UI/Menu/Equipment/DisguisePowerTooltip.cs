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
public class DisguisePowerTooltip : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private Text m_descriptionText = null;
	[Space]
	[SerializeField] private Image m_powerIcon = null;
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
		Debug.Assert(m_lockInfo != null, "Required field!");
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this button with the data from the given definition.
	/// </summary>
	/// <param name="_powerDef">Power definition.</param>
	public void InitFromDefinition(DefinitionNode _powerDef) {
		// Ignore if given definition is not valid
		if(_powerDef == null) return;

		// Save definition
		m_powerDef = _powerDef;

		// Power icon
		if(m_powerIcon != null) {
			// Load power icons spritesheet
			Sprite[] allIcons = Resources.LoadAll<Sprite>("UI/Popups/Disguises/powers/icons_powers");

			// Pick target icon
			string iconName = _powerDef.GetAsString("icon");
			m_powerIcon.sprite = Array.Find<Sprite>(allIcons, (_sprite) => { return _sprite.name == iconName; });
			m_powerIcon.color = Color.white;
		}

		// Name and description
		// Name
		m_nameText.Localize(_powerDef.Get("tidName"));

		// Desc
		m_descriptionText.text = DragonPowerUp.GetDescription(_powerDef, false);	// Custom formatting depending on powerup type, already localized
	}

	/// <summary>
	/// Sets the lock state of the power.
	/// </summary>
	/// <param name="_locked">Whether the power is locked or not.</param>
	public void SetLocked(bool _locked) {
		// Lock info
		m_lockInfo.SetActive(_locked);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}