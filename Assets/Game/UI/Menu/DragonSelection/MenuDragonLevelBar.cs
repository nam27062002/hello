// MenuDragonLevelBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
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
/// Simple controller for a dragon level bar in the menu.
/// </summary>
public class MenuDragonLevelBar : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[SerializeField] private Slider m_levelBar;
	[SerializeField] private Text m_levelText;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		DebugUtils.Assert(m_levelBar != null, "Required field!");
		DebugUtils.Assert(m_levelText != null, "Required field!");
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener(MenuDragonSelector.EVENT_DRAGON_CHANGED, Refresh);
		
		// Do a first refresh
		Refresh();
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MenuDragonSelector.EVENT_DRAGON_CHANGED, Refresh);
	}

	/// <summary>
	/// Refresh with data from currently selected dragon
	/// </summary>
	public void Refresh() {
		// Bar value
		m_levelBar.minValue = 0;
		m_levelBar.maxValue = DragonData.NUM_LEVELS;
		m_levelBar.value = DragonManager.currentDragonData.progression.level + 1;	// [1..N] bar should never be empty and should be filled when we're at level 9
			
		// Text
		m_levelText.text = String.Format("Lvl {0}",
		                                StringUtils.FormatNumber(m_levelBar.value, 0));
	}
}
