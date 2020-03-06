// ItemPrefabSetupEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ItemPrefabSetup class.
/// </summary>
[CustomPropertyDrawer(typeof(ItemPrefabSetup))]
public class ItemPrefabSetupPropertyDrawer : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private static GUIStyle s_header = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public ItemPrefabSetupPropertyDrawer() {
		// Init styles
		if(s_header == null) {
			s_header = new GUIStyle("ShurikenModuleTitle");
		}
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
		m_pos.height = s_header.lineHeight;    // Advance pointer just the size of the header (plus some extra space for better visuals)
		_label.text = Color.gray.Tag((_property.isExpanded ? "▼ " : "▶ ")) + _label.text;	// Add expanded arrow (a bit darker than actual text)
		_property.isExpanded = GUI.Toggle(m_pos, _property.isExpanded, _label, s_header);
		AdvancePos();

		// If unfolded, draw children
		if(_property.isExpanded) {
			// If unfolded, add some extra space to separate from the content
			AdvancePos(8);

			// Aux vars
			int baseDepth = _property.depth;
			int rootIndent = EditorGUI.indentLevel;

			// Iterate through all the children of the property
			bool loop = _property.Next(true);	// Enter to the first level of depth
			while(loop) {
				// Set indentation
				EditorGUI.indentLevel = rootIndent + _property.depth;

				// Properties requiring special treatment
				if(_property.name == "name_of_the_property_requiring_special_treatment") {

				}

				// Properties to hide
				else if(_property.name == "type") {
					// Do nothing
				}

				// Default
				else {
					DrawAndAdvance(_property);
				}

				// Move to next property
				// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)
				loop = _property.Next(false);

				// If within an array, Next() will give the next element of the array, which will 
				// already be drawn by itself afterwards, so we don't want it - check depth to prevent it
				if(loop) {
					if(_property.depth <= baseDepth) loop = false;
				}
			}
		}

		// Add some extra space at the end to separate from next item
		AdvancePos(5);
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
}