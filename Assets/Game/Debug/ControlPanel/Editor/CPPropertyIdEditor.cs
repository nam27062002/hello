// CPPropertyIdEditor.cs
// 
// Created by Alger Ortín Castellví on 27/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the CPPropertyId class.
/// </summary>
[CustomPropertyDrawer(typeof(CPPropertyId))]
public class CPPropertyIdEditor : PropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// </summary>
	/// <param name="_pos">The area in the inspector assigned to draw this property.</param>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	public override void OnGUI(Rect _pos, SerializedProperty _property, GUIContent _label) {
		// Aux vars
		int selectedIdx = -1;	// Reuse the loop to find out the currently selected value
		SerializedProperty idProp = _property.FindPropertyRelative("id");

		// Use reflection to get all the debug settings
		// @see http://stackoverflow.com/questions/12474908/how-to-get-all-static-property-and-its-values-of-a-class-using-reflection
		// Public static fields
		FieldInfo[] fields = typeof(DebugSettings).GetFields(BindingFlags.Public | BindingFlags.Static);
		List<string> optionsList = new List<string>();
		for(int i = 0; i < fields.Length; i++) {
			// We only care about strings
			if(fields[i].FieldType == typeof(string)) {
				// Add option to the list
				string option = fields[i].GetValue(null) as string;
				optionsList.Add(option);

				// Is it the current value?
				if(option == idProp.stringValue) {
					selectedIdx = optionsList.Count - 1;
				}
			}
		}

		// Add the "Custom" option
		optionsList.Add("Custom...");

		// If not id is selected, choose custom
		if(selectedIdx < 0) selectedIdx = optionsList.Count - 1;

		// Do the drawing!
		EditorGUI.BeginProperty(_pos, _label, _property); {
			// Draw label and initialize position helpers
			Rect contentRect = EditorGUI.PrefixLabel(_pos, _label);	// Remaining space for the content
			Rect cursor = contentRect;
			
			// Reset indentation level since we're already drawing within the content rect
			int indentLevelBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Show in a drop-down list, hardcoded the length if selected index is "Custom..."
			if(selectedIdx == optionsList.Count - 1) {
				cursor.width = 80f;
			}
			int newSelectedIdx = EditorGUI.Popup(cursor, selectedIdx, optionsList.ToArray());

			// Store new value
			// If selected value is "Custom...", show an editable textfield next to the dropdown list
			if(newSelectedIdx == optionsList.Count - 1) {
				// Reset value if it's the first time we select the "Custom..." option
				if(newSelectedIdx != selectedIdx) {
					idProp.stringValue = "";
				}

				// Store new custom value - add some spacign
				cursor.x += cursor.width + 5;
				cursor.width = contentRect.width - cursor.width - 5;	// Fill the rest of the content rect
				idProp.stringValue = EditorGUI.TextField(cursor, idProp.stringValue);
			} else {
				// Directly store new value
				idProp.stringValue = optionsList[newSelectedIdx];
			}

			// Restore indentation level
			EditorGUI.indentLevel = indentLevelBackup;
		} EditorGUI.EndProperty();
	}
}