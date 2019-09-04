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
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum DefaultValuePolicy {
		NONE,
		ONLY_FOR_DEBUG_BUILD
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed setup
	[Space]
	[SerializeField] private bool m_defaultValue = false;
	protected bool defaultValue {
		get {
			switch(m_defaultValuePolicy) {
				case DefaultValuePolicy.NONE: {
					return m_defaultValue;
				} break;

				case DefaultValuePolicy.ONLY_FOR_DEBUG_BUILD: {
					return m_defaultValue && UnityEngine.Debug.isDebugBuild;
				} break;
			}
			return m_defaultValue;
		}
	}

	[SerializeField] private Toggle m_toggle;
	public Toggle toggle {
		get { return m_toggle; }
	}

	[Space]
	[SerializeField] protected DefaultValuePolicy m_defaultValuePolicy = DefaultValuePolicy.NONE;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Check requirements
		DebugUtils.Assert(m_toggle != null, "Required component!");

		// Init toggle
		m_toggle.onValueChanged.AddListener(OnValueChanged);

		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_toggle.onValueChanged.RemoveListener(OnValueChanged);
	}

	/// <summary>
	/// Refresh value.
	/// </summary>
	override public void Refresh() {
		base.Refresh();
		m_toggle.isOn = Prefs.GetBoolPlayer(id, defaultValue);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The toggle has changed.
	/// </summary>
	public void OnValueChanged(bool _newValue) {
		Prefs.SetBoolPlayer(id, _newValue);
		Messenger.Broadcast<string, bool>(MessengerEvents.CP_BOOL_CHANGED, id, _newValue);
		Messenger.Broadcast<string>(MessengerEvents.CP_PREF_CHANGED, id);
	}
}