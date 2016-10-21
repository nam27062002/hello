// CPEnumPref.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/11/2015.
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
/// Widget to set/get a boolean value stored in Prefs (i.e. cheats).
/// </summary>
public class CPEnumPref : CPPrefBase {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed references
	[Comment("System.Type.AssemblyQualifiedName")]
	[SerializeField] private string m_enumTypeName = "";
	[SerializeField] private Dropdown m_dropdown = null;
	[SerializeField] private int m_defaultValue = 0;

	// Internal
	private Type m_enumType = null;
	private int[] m_enumValues = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Check requirements
		DebugUtils.Assert(m_dropdown != null, "Required component!");

		// Call parent
		base.Awake();
	}

	private void Update() {
		// Initialize if not already done
		if(isActiveAndEnabled && m_enumType == null) {
			m_enumType = Type.GetType(m_enumTypeName, false);
			if(m_enumType != null) {
				// Make sure the type represents an enum!!
				if(!m_enumType.IsEnum) {
					m_enumType = null;
				} else {
					Refresh();
				}
			}
		}
	}

	/// <summary>
	/// Refresh value.
	/// </summary>
	override public void Refresh() {
		base.Refresh();

		// Update dropdown
		m_dropdown.ClearOptions();
		int selectedIdx = 0;

		// Init options based on enum values
		if(m_enumType != null && m_enumType.IsEnum) {
			m_enumValues = (int[])Enum.GetValues(m_enumType);
			string[] optionNames = Enum.GetNames(m_enumType);
			int currentValue = Prefs.GetIntPlayer(id, m_defaultValue);
			for(int i = 0; i < m_enumValues.Length; i++) {
				// Add the option
				m_dropdown.options.Add(new Dropdown.OptionData(optionNames[i]));

				// Is it the current option?
				if(currentValue == m_enumValues[i]) {
					selectedIdx = i;
				}
			}
		}

		// Set selection
		m_dropdown.value = selectedIdx;

		// [AOC] Dropdown seems to be bugged -_-
		if(m_dropdown.options.Count > 0) {
			m_dropdown.captionText.text = m_dropdown.options[m_dropdown.value].text;
		} else {
			m_dropdown.captionText.text = "";
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A new option has been picked on the dropdown.
	/// </summary>
	public void OnValueChanged(int _newValueIdx) {
		if(m_enumValues != null) {
			Prefs.SetIntPlayer(id, _newValueIdx);
		}
	}
}