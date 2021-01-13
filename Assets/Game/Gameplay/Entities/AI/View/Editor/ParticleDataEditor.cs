// ParticleDataEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ParticleData class.
/// </summary>
[CustomPropertyDrawer(typeof(ParticleData))]
public class ParticleDataPropertyDrawer : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public ParticleDataPropertyDrawer() {
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
		m_pos.height = EditorStyles.largeLabel.lineHeight;	// Advance pointer just the size of the label
		_property.isExpanded = EditorGUI.Foldout(m_pos, _property.isExpanded, _label);
		AdvancePos();

		// If unfolded, draw children
		if(_property.isExpanded) {
			// Aux vars
			int baseDepth = _property.depth;
			int rootIndent = EditorGUI.indentLevel;

			// Iterate through all the children of the property
			bool loop = _property.Next(true);	// Enter to the first level of depth
			while(loop) {
				// Set indentation
				EditorGUI.indentLevel = rootIndent + _property.depth;

				// Properties requiring special treatment
				// Start color
				if(_property.name == "changeStartColor") {
					// Prefix Label and Checkbox
					DrawAndAdvance(_property);

					// If toggled, draw color boxes (indented)
					if(_property.boolValue) {
						// Add small offset to make it clear it's a sub-property
						Rect pos = new Rect(m_pos);
						pos.x += 10f;
						pos.width -= 10f;

						// 1st Color
						SerializedProperty colorProp = m_rootProperty.FindPropertyRelative("startColor");
						pos.height = EditorGUI.GetPropertyHeight(colorProp);	// Make sure it has the adequate height!
						EditorGUI.PropertyField(pos, colorProp);
						AdvancePos(pos.height);
						pos.y = m_pos.y;

						// 2nd Color
						colorProp = m_rootProperty.FindPropertyRelative("startColorTwo");
						pos.height = EditorGUI.GetPropertyHeight(colorProp);	// Make sure it has the adequate height!
						EditorGUI.PropertyField(pos, colorProp);
						AdvancePos(pos.height);
					}
				}

				// Color overtime
				else if(_property.name == "changeColorOvertime") {
					// Prefix Label and Checkbox
					DrawAndAdvance(_property);

					// If toggled, draw color boxes (indented)
					if(_property.boolValue) {
						// Add small offset to make it clear it's a sub-property
						Rect pos = new Rect(m_pos);
						pos.x += 10f;
						pos.width -= 10f;

						// Color prop
						SerializedProperty colorProp = m_rootProperty.FindPropertyRelative("colorOvertime");
						pos.height = EditorGUI.GetPropertyHeight(colorProp);	// Make sure it has the adequate height!
						EditorGUI.PropertyField(pos, colorProp);
						AdvancePos(pos.height);
					}
				}

				// To be ignored
				else if(_property.name == "startColor"
					|| _property.name == "startColorTwo"
					|| _property.name == "colorOvertime") {
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