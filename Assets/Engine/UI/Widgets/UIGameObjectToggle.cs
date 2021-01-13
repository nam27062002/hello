// UIGameObjectToggle.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to automatically toggle a GameObject on/off based on a toggle.
/// No need to connect toggle events in the inspector.
/// </summary>
public class UIGameObjectToggle : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
#if UNITY_EDITOR
	public enum DebugMode {
		ON,
		OFF,
		ALL_ON,
		ALL_OFF
	}
#endif

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Toggle m_toggle = null;
	public Toggle toggle {
		get { return m_toggle; }
	}

	[SerializeField] private GameObject[] m_targets = new GameObject[0];
	[SerializeField] private GameObject[] m_inverseTargets = new GameObject[0];

	// Exposed Setup
	[Space]
	[SerializeField] private bool m_initialValue = true;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Apply initial value
		m_toggle.isOn = m_initialValue;
		Apply(m_initialValue);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to toggle events
		m_toggle.onValueChanged.AddListener(OnToggle);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from the toggle events
		m_toggle.onValueChanged.RemoveListener(OnToggle);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply toggle's current value to the targets.
	/// </summary>
	private void Apply(bool _isOn) {
		Apply(_isOn, !_isOn);
	}

	private void Apply(bool _regularTargetsValue, bool _inverseTargetsValue) {
		// Regular targets
		for(int i = 0; i < m_targets.Length; ++i) {
			if(m_targets[i] != null) {
				m_targets[i].SetActive(_regularTargetsValue);
			}
		}

		// Inverse targets
		for(int i = 0; i < m_inverseTargets.Length; ++i) {
			if(m_inverseTargets[i] != null) {
				m_inverseTargets[i].SetActive(_inverseTargetsValue);
			}
		}
	}

	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//
#if UNITY_EDITOR
	/// <summary>
	/// Debug purposes.
	/// </summary>
	/// <param name="_isOn">Toggle on or off?</param>
	public void DEBUG_Apply(DebugMode _mode) {
		switch(_mode) {
			case DebugMode.ON: Apply(true); break;
			case DebugMode.OFF: Apply(false); break;
			case DebugMode.ALL_ON: Apply(true, true); break;
			case DebugMode.ALL_OFF: Apply(false, false); break;
		}
	}
#endif

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The toggle state has changed.
	/// </summary>
	/// <param name="_newValue">Toggle on or off?</param>
	private void OnToggle(bool _newValue) {
		Apply(_newValue);
	}
}