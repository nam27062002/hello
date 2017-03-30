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
using TMPro;

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
	[SerializeField] private int m_defaultValue = 0;
	[SerializeField] private string m_enumTypeName = "";
	[SerializeField] private TMP_Dropdown m_dropdown = null;

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

		// Init dropdown
		m_dropdown.onValueChanged.AddListener(OnValueChanged);

		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Inits from enum.
	/// </summary>
	/// <param name="_propertyId">ID of the property to be used.</param>
	/// <param name="_enumType">Enum type.</param>
	/// <param name="_defaultValue">Default value if property doesn't have one.</param>
	public void InitFromEnum(string _propertyId, Type _enumType, int _defaultValue) {
		// Don't allow empty property id
		if(string.IsNullOrEmpty(_propertyId)) return;

		// If type is null, clear everything
		if(_enumType == null) {
			m_enumTypeName = "";
			m_enumType = null;
			m_enumValues = null;
		} else {
			// Ignore if given type is not an enum
			if(!_enumType.IsEnum) return;

			// Update internal vars
			m_id.id = _propertyId;
			m_enumTypeName = _enumType.AssemblyQualifiedName;
			m_enumType = _enumType;
			m_defaultValue = _defaultValue;

			// Force a refresh
			Refresh();
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_dropdown.onValueChanged.RemoveListener(OnValueChanged);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
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
				m_dropdown.options.Add(new TMP_Dropdown.OptionData(optionNames[i]));

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
			Messenger.Broadcast<string, int>(GameEvents.CP_ENUM_CHANGED, id, m_enumValues[_newValueIdx]);
			Messenger.Broadcast<string>(GameEvents.CP_PREF_CHANGED, id);
		}
	}
}