// TiltControlToggle.cs
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
public class TiltControlToggle : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Slider m_tiltControlSlider = null;
	[SerializeField] private Button m_calibrateButton = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initial state
		Refresh();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update slider's values from settings.
	/// </summary>
	private void Refresh() {
		// Aux vars
		bool isEnabled = GameSettings.tiltControlEnabled;

		// Set toggle
		if(isEnabled) {
			m_tiltControlSlider.value = m_tiltControlSlider.maxValue;
		} else {
			m_tiltControlSlider.value = m_tiltControlSlider.minValue;
		}

		// Calibrate button
		if(m_calibrateButton != null) {
			m_calibrateButton.interactable = isEnabled;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The tilt control slider has been toggled.
	/// </summary>
	public void OnTiltControlToggleChanged() {
		bool viewIsEnabled = m_tiltControlSlider.value == m_tiltControlSlider.maxValue;
		bool isEnabled = GameSettings.tiltControlEnabled;
		if(isEnabled != viewIsEnabled) {
			GameSettings.tiltControlEnabled = viewIsEnabled;
			Refresh();
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
