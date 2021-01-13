// GameOptionsToggleGroup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class can be used to use a slider as a toggle for enabling/disabling the tilt control.
/// </summary>
public class TiltControlToggle : GameSettingsToggle {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Space]
	[SerializeField] private Button m_tiltCalibrateButton = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Call parent
		base.OnEnable();

		// Tilt calibrate button
		if(m_tiltCalibrateButton != null) {
			m_tiltCalibrateButton.interactable = this.toggled;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Slider value changed.
	/// </summary>
	override protected void OnToggleChanged(float _newValue) {
		// Call parent
		base.OnToggleChanged(_newValue);

		// Refresh tilt calibrate button
		if(m_tiltCalibrateButton != null) {
			m_tiltCalibrateButton.interactable = this.toggled;
		}
	}

	/// <summary>
	/// The calibrate button has been pressed.
	/// </summary>
	public void OnCalibrateButton() {
		// Open the calibration popup
		PopupManager.OpenPopupInstant(PopupTiltControl.PATH);
	}
}
