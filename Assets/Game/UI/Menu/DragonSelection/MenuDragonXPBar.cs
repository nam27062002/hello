// MenuDragonXPBar.cs
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
/// Simple controller for a xp bar in the debug hud.
/// </summary>
public class MenuDragonXPBar : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private Slider m_bar;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_bar = GetComponentInChildren<Slider>();
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
		DragonData data = DragonManager.currentDragonData;
		Range xpRange = data.progression.xpRange;
		m_bar.minValue = xpRange.min;
		m_bar.maxValue = xpRange.max;
		m_bar.value = data.progression.xp;

		// Special case on last level
		if(data.progression.level == data.progression.lastLevel) {
			m_bar.minValue = xpRange.min - 1;
		}
	}
}
