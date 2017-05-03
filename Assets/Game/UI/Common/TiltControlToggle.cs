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
		if(ApplicationManager.instance.Settings_GetTiltControl()) {
			m_tiltControlSlider.value = m_tiltControlSlider.maxValue;
		} else {
			m_tiltControlSlider.value = m_tiltControlSlider.minValue;
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
		bool isEnabled = ApplicationManager.instance.Settings_GetTiltControl();
		if(isEnabled != viewIsEnabled) {
			ApplicationManager.instance.Settings_ToggleTiltControl();
			Refresh();
		}
	}
}
