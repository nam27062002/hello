// CPBoolPref.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Widget to set/get a boolean value stored in Prefs (i.e. cheats).
/// </summary>
public class CPBoolPref : CPPrefBase {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private Toggle m_toggle;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Check requirements
		DebugUtils.Assert(m_toggle != null, "Required component!");

		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Refresh value.
	/// </summary>
	override public void Refresh() {
		base.Refresh();
		m_toggle.isOn = Prefs.GetBoolPlayer(id);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The toggle has changed.
	/// </summary>
	public void OnValueChanged() {
		Prefs.SetBoolPlayer(id, m_toggle.isOn);
	}
}