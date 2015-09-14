// DebugHUDXPBarController.cs
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
/// Simple controller for a xp bar in the debug hud.
/// </summary>
public class DebugHUDXPBarController : MonoBehaviour {
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
			Range xpRange = InstanceManager.player.data.progression.xpRange;
			m_bar.minValue = xpRange.min;
			m_bar.maxValue = xpRange.max;
			m_bar.value = InstanceManager.player.data.progression.xp;

			// Text
			m_valueTxt.text = String.Format("{0}/{1} (lv. {2}/{3})",
			                                StringUtils.FormatNumber(m_bar.value - m_bar.minValue, 0),
			                                StringUtils.FormatNumber(m_bar.maxValue - m_bar.minValue, 0),
			                                StringUtils.FormatNumber(InstanceManager.player.data.progression.level + 1),
			                                StringUtils.FormatNumber(InstanceManager.player.data.progression.lastLevel + 1));
		}
	}
}
