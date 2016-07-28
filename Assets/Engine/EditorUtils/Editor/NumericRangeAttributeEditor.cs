// NumericRangeAttributeEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/10/2015.
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
/// Drawer for the Numeric Range custom attribute.
/// </summary>
[CustomPropertyDrawer(typeof(NumericRangeAttribute), true)]
public class NumericRangeAttributeEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------------//
	// PROPERTIES															  //
	//------------------------------------------------------------------------//
	private NumericRangeAttribute targetAttribute { 
		get { return attribute as NumericRangeAttribute; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Check both int and float cases
		switch(_property.propertyType) {
			case SerializedPropertyType.Float: {
				// Draw property with its default drawer and, if it changes, validate the new value
				EditorGUI.BeginChangeCheck();
				m_pos.height = EditorGUI.GetPropertyHeight(_property);	// Default height for this property
				float newValue = EditorGUI.FloatField(m_pos, _label, _property.floatValue);
				if(EditorGUI.EndChangeCheck()) {
					_property.floatValue = Mathf.Clamp(newValue, targetAttribute.floatMin, targetAttribute.floatMax);
				}
			} break;

			case SerializedPropertyType.Integer: {
				// Draw property with its default drawer and, if it changes, validate the new value
				EditorGUI.BeginChangeCheck();
				m_pos.height = EditorGUI.GetPropertyHeight(_property);	// Default height for this property
				int newValue = EditorGUI.IntField(m_pos, _label, _property.intValue);
				if(EditorGUI.EndChangeCheck()) {
					_property.intValue = Mathf.Clamp(newValue, targetAttribute.intMin, targetAttribute.intMax);
				}
			} break;

			default: {
				m_pos.height = 40f;
				EditorGUI.HelpBox(m_pos, _property.displayName + "\nNumeric Range Attribute is only valid on numeric types (int, float)", MessageType.Error);
			} break;
		}

		// Go on
		AdvancePos();
	}
}

