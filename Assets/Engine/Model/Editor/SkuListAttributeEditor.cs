// SkuListAttributeEditor.cs
// 
// Created by Alger Ortín Castellví on 14/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

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
[CustomPropertyDrawer(typeof(SkuListAttribute))]
public class SkuListAttributeEditor : ExtendedPropertyDrawer {
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
			EditorGUI.LabelField(m_pos, _label.text, "ERROR! SkuList attribute can only be applied to string properties!");
			AdvancePos();
			return;
		}

		// Obtain the attribute
		SkuListAttribute attr = attribute as SkuListAttribute;
		// [AOC] POTENTIAL!! base.fieldInfo.GetType();

		// Get the definitions
		// Tricky reflection stuff to deal with unknown generics
		// Black magic from https://msdn.microsoft.com/en-us/library/b8ytshk6(v=vs.110).aspx
		// and http://stackoverflow.com/questions/17519078/initializing-a-generic-variable-from-a-c-sharp-type-variable
		MethodInfo genericMethod = typeof(DefinitionsManager).GetMethod("GetDefSet");	// Get generic method
		MethodInfo specificMethod = genericMethod.MakeGenericMethod(new Type[] { attr.m_defType });	// Make it specific to our custom definition type
		object defSet = specificMethod.Invoke(null, null);	// Get the target defSet (no target (static method), no arguments) - unfortunately we can't use "dynamic" keyword in Mono

		// Figure out the list of options to be displayed
		Type setType = typeof(DefinitionSet<>).MakeGenericType(new Type[] { attr.m_defType });	// Get the type of the definition set with our custom definition type
		List<string> skus = (List<string>)setType.GetProperty("skus").GetValue(defSet, null);	// Use reflection once again to access the "skus" property, which we know for sure the DefinitionSet will have regardless of its definition type

		// Insert "NONE" option
		if(attr.m_allowNullValue) {
			skus.Insert(0, "---- NONE");
		}

		// Find out current selected value
		// If current value was not found, force it to first value (which will be "NONE" if allowed)
		int selectedIdx = skus.FindIndex(_sku => _property.stringValue.Equals(_sku, StringComparison.Ordinal));
		if(selectedIdx < 0) {
			selectedIdx = 0;
		}

		// Display the property
		m_pos.height = EditorStyles.popup.lineHeight + 5;	// [AOC] Default popup field height + some margin
		int newSelectedIdx = EditorGUI.Popup(m_pos, _label.text, selectedIdx, skus.ToArray());

		// Store new value - "NONE" will be stored as "" when allowed
		if(attr.m_allowNullValue && newSelectedIdx == 0) {
			_property.stringValue = "";
		} else {
			_property.stringValue = skus[newSelectedIdx];	// Options array match the definition skus, so no need to do anything else :)
		}

		// Leave room for next property drawer
		AdvancePos();
	}
}

