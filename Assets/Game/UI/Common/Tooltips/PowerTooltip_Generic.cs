// PowerTooltip_Generic.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization for a generic power tooltip.
/// </summary>
public class PowerTooltip_Generic : IPowerTooltip {
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

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected override void Awake() {
		// Check required fields
		Debug.Assert(m_powerIcon != null, "Required field!");

		// Let parent do the job
		base.Awake();
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the tooltip. To be implemented by heirs.
	/// At this point, internal data variables have been initialized.
	/// </summary>
	protected override void Init_Internal() {
		// Nothing to do if power definition is not valid
		if(m_powerDef == null) return;

		// Power icon
		if(m_powerIcon != null) {
			// Load from resources
			m_powerIcon.InitFromDefinition(m_powerDef, m_sourceDef, false, false);
		}

		// Name and description
		// Name
		if(m_titleText != null) {
			string title = LocalizationManager.SharedInstance.Localize(m_powerDef.Get("tidName"));
			m_titleText.text = title;
		}

		// Desc
		if(m_messageText != null) {
			m_messageText.text = DragonPowerUp.GetDescription(m_powerDef, false, m_powerMode == PowerIcon.Mode.PET);   // Custom formatting depending on powerup type, already localized
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}