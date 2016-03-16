// StringListAttributeEditor.cs
// 
// Created by Alger Ortín Castellví on 16/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Drawer for the SkuList custom attribute.
/// </summary>
[CustomPropertyDrawer(typeof(StringListAttribute))]
public class StringListAttributeEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Check field type
		if(_property.propertyType != SerializedPropertyType.String) {
			m_pos.height = EditorStyles.largeLabel.lineHeight;
			EditorGUI.LabelField(m_pos, _label.text, "ERROR! StringList attribute can only be applied to string properties!");
			AdvancePos();
			return;
		}

		// Obtain the attribute
		StringListAttribute attr = attribute as StringListAttribute;
		// [AOC] POTENTIAL!! base.fieldInfo.GetType();

		// Options list - at least empty value!
		List<string> options = new List<string>(attr.m_options);
		if(options.Count == 0) options.Add("");

		// Find out current selected value
		// If current value was not found, force it to first value (which will be "NONE" if allowed)
		int selectedIdx = options.FindIndex(_option => _property.stringValue.Equals(_option, StringComparison.Ordinal));
		if(selectedIdx < 0) {
			selectedIdx = 0;
		}

		// Display the property
		m_pos.height = EditorStyles.popup.lineHeight + 5;	// [AOC] Default popup field height + some margin
		int newSelectedIdx = EditorGUI.Popup(m_pos, _label.text, selectedIdx, options.ToArray());

		// Store new value
		_property.stringValue = options[newSelectedIdx];

		// Leave room for next property drawer
		AdvancePos();
	}
}

