// HideEnumValuesAttributeEditor.cs
// 
// Created by Alger Ortín Castellví on 22/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Drawer for the HideEnumValues custom attribute.
/// </summary>
[CustomPropertyDrawer(typeof(HideEnumValuesAttribute))]
public class HideEnumValuesAttributeEditor : ExtendedPropertyDrawer {
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
		// Check type
		if(_property.propertyType != SerializedPropertyType.Enum) {
			m_pos.height = EditorStyles.largeLabel.lineHeight;
			EditorGUI.LabelField(m_pos, _label.text, "ERROR! HideEnumValues attribute can only be applied to Enum properties");
			AdvancePos();
			return;
		}

		// Obtain the attribute
		HideEnumValuesAttribute attr = attribute as HideEnumValuesAttribute;
		
		// Figure out the list of options to be displayed
		int numOptions = _property.enumNames.Length;
		if(attr.m_excludeFirstElement) numOptions--;
		if(attr.m_excludeLastElement) numOptions--;
		string[] options = new string[numOptions];
		int[] values = new int[numOptions];
		int enumIdx = attr.m_excludeFirstElement ? 1 : 0;
		for(int i = 0; i < numOptions; i++, enumIdx++) {
			options[i] = _property.enumDisplayNames[enumIdx];
			values[i] = enumIdx;
		}
		
		// Clamp current value, in case it's either the first or the last and they're excluded
		_property.enumValueIndex = Mathf.Clamp(_property.enumValueIndex, values[0], values[numOptions - 1]);
		
		// Display the property and store new value
		m_pos.height = EditorGUI.GetPropertyHeight(_property);	// [AOC] Default enum field height
		_property.enumValueIndex = EditorGUI.IntPopup(m_pos, _property.displayName, _property.enumValueIndex, options, values);
		AdvancePos();
	}
}

