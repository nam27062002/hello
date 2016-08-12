// CPStringPref.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/11/2015.
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
/// Widget to set/get a string value stored in Prefs (i.e. cheats).
/// </summary>
public class CPStringPref : CPPrefBase {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private InputField m_input;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Check requirements
		DebugUtils.Assert(m_input != null, "Required component!");

		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Refresh value.
	/// </summary>
	override public void Refresh() {
		base.Refresh();
		m_input.text = Prefs.GetStringPlayer(id);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The toggle has changed.
	/// </summary>
	public void OnValueChanged() {
		Prefs.SetStringPlayer(id, m_input.text);
	}
}