// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using AI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomPropertyDrawer(typeof(MachineMotion))]
public class MachineMotionEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public MachineMotionEditor() {
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
		if (_property.isExpanded) {
			// Aux vars
			int baseDepth = _property.depth;
			int rootIndent = EditorGUI.indentLevel;

			// Iterate through all the children of the property
			bool loop = _property.Next(true);	// Enter to the first level of depth
			while(loop) {
				// Set indentation
				EditorGUI.indentLevel = rootIndent + _property.depth;

				// Properties requiring special treatment
				if (_property.name == "m_mass") {
					_property.floatValue = Mathf.Max(1f, _property.floatValue);
					DrawAndAdvance(_property);
				} else if (_property.name == "m_faceDirection") {
					DrawToggleLeftAndAdvance(_property);
					if (_property.boolValue) {
						EditorGUI.indentLevel++;
						DrawAndAdvance(m_rootProperty.FindPropertyRelative("m_rollRotation"));
						EditorGUI.indentLevel--;
					}
				} else if (_property.name == "m_limitHorizontalRotation") {
					DrawToggleLeftAndAdvance(_property);

					if (_property.boolValue) {
						EditorGUI.indentLevel++;
						DrawAndAdvance(m_rootProperty.FindPropertyRelative("m_faceLeftAngle"));
						DrawAndAdvance(m_rootProperty.FindPropertyRelative("m_faceRightAngle"));
						EditorGUI.indentLevel--;
					}
				} else if (_property.name == "m_limitVerticalRotation") {
					DrawToggleLeftAndAdvance(_property);
					if (_property.boolValue) {
						EditorGUI.indentLevel++;
						DrawAndAdvance(m_rootProperty.FindPropertyRelative("m_faceUpAngle"));
						DrawAndAdvance(m_rootProperty.FindPropertyRelative("m_faceDownAngle"));
						EditorGUI.indentLevel--;
					}
				} else {
					// Default
					DrawAndAdvance(_property);
				}

				// Move to next property
				// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)
				loop = _property.NextVisible(false);

				// If within an array, Next() will give the next element of the array, which will be already be drawn by itself afterwards, so we don't want it - check depth to prevent it
				if(_property.depth <= baseDepth) loop = false;
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

	void DrawToggleLeftAndAdvance(SerializedProperty _property) {
		m_pos.height = EditorGUI.GetPropertyHeight(_property);
		_property.boolValue = EditorGUI.ToggleLeft(m_pos, _property.displayName, _property.boolValue);
		AdvancePos();
	}
}