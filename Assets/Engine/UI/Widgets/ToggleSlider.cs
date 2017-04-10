// ToggleSlider.cs
// 
// Created by Alger Ortín Castellví on 07/04/2017.
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
/// Simple script to make a slider behave like an On/Off toggle.
/// 
/// </summary>
public class ToggleSlider : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Slider m_slider = null;
	public Slider slider {
		get { return m_slider; }
	}

	public bool toggled {
		get { return m_slider.value > 0; }
		set { m_slider.value = value ? 1 : 0; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Setup the slider
		m_slider.wholeNumbers = true;
		m_slider.minValue = 0;
		m_slider.maxValue = 1;
		m_slider.value = Mathf.Round(Mathf.Clamp01(m_slider.value));
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Change the value!
	/// </summary>
	public void Toggle() {
		toggled = !toggled;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}