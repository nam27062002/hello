// DebugMenuXPBar.cs
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
/// Simple controller for a xp bar in the debug hud.
/// </summary>
public class DebugMenuXPBar : MonoBehaviour {
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
		m_valueText = gameObject.FindTransformRecursive("TextValue").GetComponent<Text>();
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.DEBUG_MENU_DRAGON_SELECTED, Refresh);
		Messenger.AddListener(GameEvents.DEBUG_SIMULATION_FINISHED, Refresh);

		// Do a first refresh
		Refresh();
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.DEBUG_MENU_DRAGON_SELECTED, Refresh);
		Messenger.RemoveListener(GameEvents.DEBUG_SIMULATION_FINISHED, Refresh);
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

		// Text
		m_valueText.text = String.Format("{0}/{1}xp ({2}%)",
		                                StringUtils.FormatNumber(m_bar.value - m_bar.minValue, 2),
		                                StringUtils.FormatNumber(Mathf.Ceil(xpRange.distance), 2),
		                                StringUtils.FormatNumber(data.progression.progressCurrentLevel * 100f, 0));

		// Special case on last level
		if(data.progression.level == data.progression.lastLevel) {
			m_bar.minValue = xpRange.min - 1;

			Range previousLevelRange = data.progression.GetXpRangeForLevel(data.progression.level - 1);
			m_valueText.text = String.Format("{0}/{1}xp ({2}%)",
			                                 StringUtils.FormatNumber(Mathf.Ceil(previousLevelRange.distance), 2),
			                         		 StringUtils.FormatNumber(Mathf.Ceil(previousLevelRange.distance), 2),
			                                 StringUtils.FormatNumber(data.progression.progressCurrentLevel * 100f, 0));
		}
	}
}
