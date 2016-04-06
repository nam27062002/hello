// EnumMaskAttributeEditor.cs
// 
// Created by Alger Ortín Castellví on 6/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Drawer for the Enum Mask custom attribute.
/// From http://www.codingjargames.com/blog/2014/11/10/bitfields-in-unity/
/// From http://pastebin.com/1xkdHF6w
/// </summary>
[CustomPropertyDrawer(typeof(EnumMaskAttribute), true)]
public class EnumMaskAttributeEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	private Type m_objectType;

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
		// From http://www.codingjargames.com/blog/2014/11/10/bitfields-in-unity/
		// From http://pastebin.com/1xkdHF6w

		// [AOC] Luckily Unity now provides us directly the fieldInfo and we don't need to use any reflection tricks whatsoever
		m_objectType = fieldInfo.FieldType;

		// Help us out if we messed up
		if(_property.propertyType != SerializedPropertyType.Enum) {
			m_pos.height = 40f;
			EditorGUI.HelpBox(m_pos, _property.displayName + "\nSystem.Flags is only valid on Enumeration types", MessageType.Error);
		} else if(m_objectType == null) {
			m_pos.height = 40f;
			EditorGUI.HelpBox(m_pos, _property.displayName + "\nCould not deduce Enumeration Type", MessageType.Error);
		} else {
			// Just add-on some text so we know we're working...
			_label.text += " (Flags)";
			m_pos.height = EditorGUI.GetPropertyHeight(_property);	// Default height for an enum field
			object valueObj = Enum.ToObject(m_objectType, _property.intValue);
			Enum newValue = EditorGUI.EnumMaskField(m_pos, _label, valueObj as Enum);
			if(GUI.changed) {
				_property.intValue = newValue.GetHashCode();
			}
		}

		// Go on
		AdvancePos();
	}
}

