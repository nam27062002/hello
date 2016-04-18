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
	[SerializeField] private Localizer m_levelText;
	[SerializeField] private Localizer m_nameText;
	
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
		DebugUtils.Assert(m_nameText != null, "Required field!");
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, Refresh);
		
		// Do a first refresh
		Refresh(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, Refresh);
	}

	/// <summary>
	/// Refresh with data from currently selected dragon
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon</param>
	public void Refresh(string _sku) {
		// Get new dragon's data from the dragon manager
		DragonData data = DragonManager.GetDragonData(_sku);

		// Bar value
		m_levelBar.minValue = 0;
		m_levelBar.maxValue = 1;
		m_levelBar.value = data.progression.progressCurrentLevel;
			
		// Text
		m_levelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber( (float)data.progression.level+1, 0));

		// Dragon Name
		m_nameText.Localize(data.def.GetAsString("tidName"));
	}
}
