// DebugHUDFuryBarController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a fury bar in the debug hud.
/// </summary>
public class DebugHUDFuryBarController : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private Slider m_bar;
	private Text m_valueTxt;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_bar = GetComponentInChildren<Slider>();
		m_valueTxt = gameObject.FindSubObject("TextValue").GetComponent<Text>();
	}

	/// <summary>
	/// Keep values updated
	/// </summary>
	private void Update() {
		// Only if player is alive
		if(InstanceManager.player != null) {
			// Bar value
			m_bar.minValue = 0f;
			m_bar.maxValue = InstanceManager.player.data.maxFury;
			m_bar.value = InstanceManager.player.fury;

			// Text
			m_valueTxt.text = String.Format("{0}/{1}",
			                                StringUtils.FormatNumber(m_bar.value, 0),
			                                StringUtils.FormatNumber(m_bar.maxValue, 0));
		}
	}
}
