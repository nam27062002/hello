// UIShadersTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[ExecuteInEditMode]
public class UIShadersTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] private Slider m_brightnessSlider = null;
	[SerializeField] private Slider m_saturationSlider = null;
	[SerializeField] private Slider m_contrastSlider = null;
	[Space]
	[SerializeField] private UIColorFX m_colorFX = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		m_brightnessSlider.value = m_colorFX.brightness;
		m_saturationSlider.value = m_colorFX.saturation;
		m_contrastSlider.value = m_colorFX.contrast;
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnValueChanged() {
		m_colorFX.brightness = m_brightnessSlider.value;
		m_colorFX.saturation = m_saturationSlider.value;
		m_colorFX.contrast = m_contrastSlider.value;
	}
}