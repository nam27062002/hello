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
		// Get new dragon's data from the dragon manager
		DragonData data = DragonManager.GetDragonData(_id);

		// Bar value
		m_levelBar.minValue = 0;
		m_levelBar.maxValue = DragonData.NUM_LEVELS;
		m_levelBar.value = data.progression.level + 1;	// [1..N] bar should never be empty and should be filled when we're at level 9
			
		// Text
		m_levelText.text = String.Format("Lvl {0}",
		                                StringUtils.FormatNumber(m_levelBar.value, 0));
	}
}
