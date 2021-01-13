// SerializableDictionaryEditor.cs
// 
// Created by Alger Ortín Castellví on 26/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the SerializableDictionary class.
/// Doesn't work as-is, must be inherited for each SerializableDictionary implementation.
/// </summary>
//[CustomPropertyDrawer(typeof(SerializableDictionary<string, GameOject>), true)]		// Implement in your own editor class inheriting from this one!
public class SerializableDictionaryEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const float REMOVE_BUTTON_WIDTH = 30f;

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public SerializableDictionaryEditor() {
		// Nothing to do
	}

	//------------------------------------------------------------------//
	// PARENT IMPLEMENTATION											//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Draw property label + foldout widget
		m_pos.height = EditorGUIUtility.singleLineHeight;	// Advance pointer just the size of the label
		_property.isExpanded = EditorGUI.Foldout(m_pos, _property.isExpanded, _label);
		AdvancePos();

		// If unfolded, draw children
		if(_property.isExpanded) {
			// Indent in
			EditorGUI.indentLevel++;

			// Get keys and values array properties
			SerializedProperty keysProp = _property.FindPropertyRelative("m_keyList");
			SerializedProperty valuesProp = _property.FindPropertyRelative("m_valueList");

			// Ensure both properties have the same size
			valuesProp.arraySize = keysProp.arraySize;

			// Store all entry to be removed to do it after the loop
			int toRemove = -1;

			// Aux vars
			Rect pos = m_pos;
			float maxHeight = m_pos.height;
			float availableWidth = m_pos.width - REMOVE_BUTTON_WIDTH;
			SerializedProperty prop = null;

			// Optionally draw a header
			DrawHeader();

			// Draw each entry
			for(int i = 0; i < keysProp.arraySize; ++i) {	// Assuming keys and values have the same length
				// Reset some vars
				pos = new Rect(m_pos);
				maxHeight = m_pos.height;

				// KEY
				prop = keysProp.GetArrayElementAtIndex(i);

				// Update positioning vars
				pos.width = GetKeyFieldWidth(availableWidth);
				pos.height = EditorGUI.GetPropertyHeight(prop, GUIContent.none, true);
				maxHeight = Mathf.Max(maxHeight, pos.height);

				// If of unknown type, display the type as label to give the user a hint on what it is
				// If string, use a delayed textfield to detect duplicated keys
				if(prop.propertyType == SerializedPropertyType.Generic) {
					EditorGUI.PropertyField(pos, prop, new GUIContent(prop.propertyType.ToString()), true);
				} else if(prop.propertyType == SerializedPropertyType.String) {
					EditorGUI.DelayedTextField(pos, prop, GUIContent.none);
				} else {
					EditorGUI.PropertyField(pos, prop, GUIContent.none, true);
				}

				// VALUE
				prop = valuesProp.GetArrayElementAtIndex(i);

				// Update positioning vars
				pos.x += pos.width;
				pos.width = availableWidth - pos.width;
				pos.height = EditorGUI.GetPropertyHeight(prop, GUIContent.none, true);
				maxHeight = Mathf.Max(maxHeight, pos.height);

				// Values have short space, reduce label size a little bit for children properties
				EditorGUIUtility.labelWidth = pos.width * 0.33f;

				// If of unknown type, display the type as label to give the user a hint on what it is
				if(prop.propertyType == SerializedPropertyType.Generic) {
					EditorGUI.PropertyField(pos, prop, new GUIContent(prop.type), true);
				} else {
					EditorGUI.PropertyField(pos, prop, GUIContent.none, true);
				}

				// Reset label width to default
				EditorGUIUtility.labelWidth = 0f;

				// REMOVE BUTTON
				pos.x += pos.width;
				pos.width = REMOVE_BUTTON_WIDTH;
				pos.height = m_pos.height;
				GUI.color = Color.red;
				if(GUI.Button(pos, "-")) {
					toRemove = i;
				}
				GUI.color = Color.white;

				// Next line
				AdvancePos(maxHeight, 1f);
			}

			// Add button
			// Centered, shorter
			AdvancePos(5f);	// Extra spacing
			pos = m_pos;
			pos.width = 100f;
			pos.height = 20f;
			pos.x += m_pos.width/2f - pos.width/2f;
			GUI.color = Color.green;
			if(GUI.Button(pos, "+")) {
				// Insert a new key
				// Inserting a new element duplicates the last one, and keys must be unique
				// Give the new key a default value based on its type to solve that
				int idx = keysProp.arraySize;
				keysProp.InsertArrayElementAtIndex(idx);
				prop = keysProp.GetArrayElementAtIndex(idx);
				prop.ResetValue();
				prop.isExpanded = true;	// Expanded by default to hint the user he has to fill in the data!

				// Insert a new value
				valuesProp.InsertArrayElementAtIndex(idx);
				prop = valuesProp.GetArrayElementAtIndex(idx);
				ResetElementValues(prop);
				prop.isExpanded = true;	// Expanded by default to hint the user he has to fill in the data!
			}
			GUI.color = Color.white;
			AdvancePos(pos.height);

			// Process item to be removed
			if(toRemove >= 0) {
				keysProp.DeleteArrayElementAtIndex(toRemove);
				valuesProp.DeleteArrayElementAtIndex(toRemove);
			}

			// Indent out
			EditorGUI.indentLevel--;
		}
	}

	/// <summary>
	/// Optionally override to give a custom label for this property field.
	/// </summary>
	/// <returns>The new label for this property.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_defaultLabel">The default label for this property.</param>
	override protected GUIContent GetLabel(SerializedProperty _property, GUIContent _defaultLabel) {
		return _defaultLabel;
	}

	//------------------------------------------------------------------//
	// OVERRIDE CANDIDATES												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Optionally draw a header line giving extra info on the dictionary's content.
	/// Will only be displayed when expanded.
	/// </summary>
	protected virtual void DrawHeader() {
		// To be implemented by heirs if needed.
	}

	/// <summary>
	/// Get the width for the key field.
	/// </summary>
	/// <returns>The width for the key field.</returns>
	/// <param name="_availableWidth">Total available width for the property content.</param>
	protected virtual float GetKeyFieldWidth(float _availableWidth) {
		return _availableWidth * 0.3f;  // Usually key is a basic type and short
	}

	/// <summary>
	/// Reset the values of a given dictionary element.
	/// Called when adding a new entry to the dictionary for example.
	/// </summary>
	/// <param name="_p">Property to be reset.</param>
	protected virtual void ResetElementValues(SerializedProperty _p) {
		// Reset to default values
		_p.ResetValue();
	}
}