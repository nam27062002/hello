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
using System.Collections;

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
	[SerializeField] private Localizer m_descText;
	
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
		DebugUtils.Assert(m_descText != null, "Required field!");
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		
		// Do a first refresh
		StartCoroutine(Refresh(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon));
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Refresh with data from currently selected dragon
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon</param>
	/// <param name="_delay">Optional delay before refreshing the data</param>
	public IEnumerator Refresh(string _sku, float _delay = -1f) {
		// If there is a delay, respect it
		if(_delay > 0f) {
			yield return new WaitForSeconds(_delay);
		}

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
		m_descText.Localize(data.def.GetAsString("tidDesc"));
	}

	/// <summary>
	/// A new dragon has been selected.
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon.</param>
	private void OnDragonSelected(string _sku) {
		// Refresh after some delay to let the animation finish
		StartCoroutine(Refresh(_sku, 0.25f));
	}
}
