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
/// Simple controller for a power tooltip.
/// </summary>
public class PowerTooltip : UITooltip
{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References


	[SerializeField] private PowerIcon m_powerIcon = null;
	public PowerIcon powerIcon {
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
	new private void Awake() {
		// Check required fields
		Debug.Assert(m_powerIcon != null, "Required field!");

        // Start hidden
        if(animator != null) animator.ForceHide(false);
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