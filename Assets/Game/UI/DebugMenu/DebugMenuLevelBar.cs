// DebugMenuLevelBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/09/2015.
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
/// Simple controller for a dragon level bar in the debug menu.
/// </summary>
public class DebugMenuLevelBar : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private Slider m_bar;
	private Text m_valueText;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_bar = GetComponentInChildren<Slider>();
		m_valueText = gameObject.FindSubObject("TextValue").GetComponent<Text>();
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener(DebugMenuDragonSelector.EVENT_DRAGON_CHANGED, Refresh);
		Messenger.AddListener(DebugMenuSimulate.EVENT_SIMULATION_FINISHED, Refresh);
		
		// Do a first refresh
		Refresh();
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(DebugMenuDragonSelector.EVENT_DRAGON_CHANGED, Refresh);
		Messenger.RemoveListener(DebugMenuSimulate.EVENT_SIMULATION_FINISHED, Refresh);
	}

	/// <summary>
	/// Refresh with data from currently selected dragon
	/// </summary>
	public void Refresh() {
		// Bar value
		m_bar.minValue = 0;
		m_bar.maxValue = DragonData.NUM_LEVELS;
		m_bar.value = DragonManager.currentDragonData.progression.level + 1;	// [1..N] bar should never be empty and should be filled when we're at level 9
			
		// Text
		m_valueText.text = String.Format("Lvl {0}",
		                                StringUtils.FormatNumber(m_bar.value, 0));
	}
}
