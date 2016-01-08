// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on //2015.
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
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomPropertyDrawer(typeof(MonoBehaviourTemplate))]
public class MonoBehaviourTemplatePropertyDrawer : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public MonoBehaviourTemplatePropertyDrawer() {
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
			// Indent in
			int rootIndent = EditorGUI.indentLevel;

			// Iterate through all the children of the property
			bool loop = _property.Next(true);	// Enter to the first level of depth
			int targetDepth = _property.depth;
			while(loop /*&& _property.depth == targetDepth*/) {		// Only direct children, not brothers or grand-children (the latter will be drawn by default if using the default EditorGUI.PropertyField)
				// Set indentation
				EditorGUI.indentLevel = rootIndent + _property.depth;

				// Properties requiring special treatment
				if(_property.name == "name_of_the_property_requiring_special_treatment") {

				}

				// Default
				else {
					m_pos.height = EditorGUI.GetPropertyHeight(_property);
					EditorGUI.PropertyField(m_pos, _property, true);
					AdvancePos();
				}

				// Move to next property
				loop = _property.Next(true);
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