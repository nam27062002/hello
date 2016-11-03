// CPFloatPref.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
public class CPFloatPref : CPPrefBase {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed setup
	[Space]
	[SerializeField] private float m_defaultValue = 0f;
	[SerializeField] private Range m_range = new Range(0f, 1f);
	[SerializeField] private Slider m_slider = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Check requirements
		DebugUtils.Assert(m_slider != null, "Required component!");

		// Initialize slider
		m_slider.minValue = m_range.min;
		m_slider.maxValue = m_range.max;
		m_slider.onValueChanged.AddListener(OnValueChanged);

		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_slider.onValueChanged.RemoveListener(OnValueChanged);
	}

	/// <summary>
	/// Refresh value.
	/// </summary>
	override public void Refresh() {
		base.Refresh();
		m_slider.value = Prefs.GetFloatPlayer(id, m_defaultValue);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The toggle has changed.
	/// </summary>
	public void OnValueChanged(float _value) {
		Prefs.SetFloatPlayer(id, _value);
		Messenger.Broadcast<string, float>(GameEvents.CP_FLOAT_CHANGED, id, _value);
		Messenger.Broadcast<string>(GameEvents.CP_PREF_CHANGED, id);
	}
}