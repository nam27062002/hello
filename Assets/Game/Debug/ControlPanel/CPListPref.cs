// CPListPref.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Widget to set/get a value stored in Prefs (i.e. cheats), choosing from a list.
/// </summary>
[System.Serializable]
public class CPListPref : CPPrefBase {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum PrefType {
		INT,
		FLOAT,
		STRING
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[Space]
	[InfoBox("Don't forget to connect callbacks to this component!")]
	[SerializeField] private TextMeshProUGUI m_text;

	// Internal
	[SerializeField] private PrefType m_type = PrefType.STRING;
	private int m_selectedIdx = 0;

	// Store values based on type
	[SerializeField] private string[] m_stringValues;
	[SerializeField] private int[] m_intValues;
	[SerializeField] private float[] m_floatValues;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Check requirements
		DebugUtils.Assert(m_text != null, "Required component!");
		switch(m_type) {
			case PrefType.INT:		DebugUtils.Assert(m_intValues.Length > 0, "At least one value on the list!!");		break;
			case PrefType.FLOAT:	DebugUtils.Assert(m_floatValues.Length > 0, "At least one value on the list!!");	break;
			case PrefType.STRING:	DebugUtils.Assert(m_stringValues.Length > 0, "At least one value on the list!!");	break;
		}

		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Refresh value.
	/// </summary>
	override public void Refresh() {
		base.Refresh();

		// Update textfield selecting from the selected type
		switch(m_type) {
			case PrefType.INT:		m_text.text = Prefs.GetIntPlayer(id, m_intValues[m_selectedIdx]).ToString();		break;
			case PrefType.FLOAT:	m_text.text = Prefs.GetFloatPlayer(id, m_floatValues[m_selectedIdx]).ToString();	break;
			case PrefType.STRING:	m_text.text = Prefs.GetStringPlayer(id, m_stringValues[m_selectedIdx]);			break;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Change selected value.
	/// </summary>
	public void OnValueChanged() {
		// Loop selected index
		// Store new value
		m_selectedIdx++;
		switch(m_type) {
			case PrefType.INT: {
				m_selectedIdx = m_selectedIdx % m_intValues.Length;
				Prefs.SetIntPlayer(id, m_intValues[m_selectedIdx]);
				Messenger.Broadcast<string, int>(MessengerEvents.CP_INT_CHANGED, id, m_intValues[m_selectedIdx]);
			} break;

			case PrefType.FLOAT: {
				m_selectedIdx = m_selectedIdx % m_floatValues.Length;
				Prefs.SetFloatPlayer(id, m_floatValues[m_selectedIdx]);
				Messenger.Broadcast<string, float>(MessengerEvents.CP_FLOAT_CHANGED, id, m_floatValues[m_selectedIdx]);
			} break;

			case PrefType.STRING: {
				m_selectedIdx = m_selectedIdx % m_stringValues.Length;
				Prefs.SetStringPlayer(id, m_stringValues[m_selectedIdx]);
				Messenger.Broadcast<string, string>(MessengerEvents.CP_PREF_CHANGED, id, m_stringValues[m_selectedIdx]);
			} break;
		}
		Messenger.Broadcast<string>(MessengerEvents.CP_PREF_CHANGED, id);

		// Update text
		Refresh();
	}
}