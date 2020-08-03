// ListAttributeEditor.cs
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
[CustomPropertyDrawer(typeof(ListAttribute), true)]
public class ListAttributeEditor : ExtendedPropertyDrawer {
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
		// Obtain the attribute
		ListAttribute attr = attribute as ListAttribute;

		// Get current value of the target property
		object currentValue = _property.GetValue();

		// Validate list values
		attr.ValidateOptions();

		// Options list - at least empty value!
		// Find out current selected value as well
		int selectedIdx = -1;
		List<string> optionsLabels = new List<string>(attr.m_options.Length);
		string label = "";
		if(attr.m_options.Length == 0) {
			optionsLabels.Add("-");
		} else {
			for(int i = 0; i < attr.m_options.Length; i++) {
				// Add string representing this option
				// Empty strings don't appear on the list, use "-" instead
				label = attr.m_options[i].ToString();
				if(string.IsNullOrEmpty(label)) label = "-";
				optionsLabels.Add(label);

				// Is it the current value?
				if(currentValue.Equals(attr.m_options[i])) {
					selectedIdx = i;
				}
			}
		}

		// If current value was not found, force it to first value
		if(selectedIdx < 0) {
			selectedIdx = 0;
		}

		// Display the property
		m_pos.height = EditorStyles.popup.lineHeight + 5;	// [AOC] Default popup field height + some margin
		int newSelectedIdx = EditorGUI.Popup(m_pos, _label.text, selectedIdx, optionsLabels.ToArray());

		// Store new value
		_property.SetValue(attr.m_options[newSelectedIdx]);

		// Leave room for next property drawer
		AdvancePos();
	}
}

