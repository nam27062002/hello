// SliderGradientFill.cs
// 
// Created by Alger Ortín Castellví on 15/06/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Small widget to control the color of a progress bar based on its value 
/// and a given color gradient.
/// </summary>
[RequireComponent(typeof(Slider))]
[ExecuteInEditMode]
public class SliderGradientFill : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private Slider slider = null;
	public Image fillImage = null;
	public Gradient colorGradient = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Start() {
		// Get some references
		slider = GetComponent<Slider>();
		
		// Check required stuff
		DebugUtils.Assert(fillImage != null, "Required member!");
		DebugUtils.Assert(slider != null, "Required member!");
		
		// Subscribe to external events
		slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public void Update() {
		
	}
	
	/// <summary>
	/// Destructor.
	/// </summary>
	public void OnDestroy() {
		// Unsubscribe from external events
		if(slider != null) {
			slider.onValueChanged.RemoveListener(delegate { OnSliderValueChanged(); });
		}
	}
	
	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The value of the slider has changed.
	/// </summary>
	private void OnSliderValueChanged() {
		// Straight conversion
		fillImage.color = colorGradient.Evaluate(slider.normalizedValue);	// [0..1] to Color :)
	}
}
