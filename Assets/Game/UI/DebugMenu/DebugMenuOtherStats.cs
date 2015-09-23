// DebugMenuOtherStats.cs
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
/// Simple controller to display misc dragon stats in the debug menu.
/// </summary>
public class DebugMenuOtherStats : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	public Text m_healthText;
	public Text m_scaleText;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

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
	/// Refresh with data from currently selected dragon.
	/// </summary>
	public void Refresh() {
		// Health
		m_healthText.text = String.Format("{0}", StringUtils.FormatNumber(DragonManager.currentDragonData.maxHealth, 0));

		// Scale
		m_scaleText.text = String.Format("{0}", StringUtils.FormatNumber(DragonManager.currentDragonData.scale, 0));
	}
}
