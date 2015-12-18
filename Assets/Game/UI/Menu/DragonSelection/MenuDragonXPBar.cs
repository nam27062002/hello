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
		Messenger.AddListener<DragonId>(GameEvents.MENU_DRAGON_SELECTED, Refresh);

		// Do a first refresh
		Refresh(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonId>(GameEvents.MENU_DRAGON_SELECTED, Refresh);
	}

	/// <summary>
	/// Refresh with data from currently selected dragon
	/// </summary>
	/// <param name="_id">The id of the selected dragon</param>
	public void Refresh(DragonId _id) {
		// Bar value
		DragonData data = DragonManager.GetDragonData(_id);
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
