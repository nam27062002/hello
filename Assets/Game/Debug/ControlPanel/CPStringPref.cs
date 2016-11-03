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
	// Exposed value
	[Space]
	[SerializeField] private string m_defaultValue = "";
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

		// Init input
		m_input.onValueChanged.AddListener(OnValueChanged);

		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_input.onValueChanged.RemoveListener(OnValueChanged);
	}

	/// <summary>
	/// Refresh value.
	/// </summary>
	override public void Refresh() {
		base.Refresh();
		m_input.text = Prefs.GetStringPlayer(id, m_defaultValue);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The toggle has changed.
	/// </summary>
	public void OnValueChanged(string _newValue) {
		Prefs.SetStringPlayer(id, _newValue);
		Messenger.Broadcast<string, string>(GameEvents.CP_STRING_CHANGED, id, _newValue);
		Messenger.Broadcast<string>(GameEvents.CP_PREF_CHANGED, id);
	}
}