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
[CustomPropertyDrawer(typeof(Actions.Action))]
public class ActionsEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public ActionsEditor() {
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
		SerializedProperty pId = m_rootProperty.FindPropertyRelative("id");
		SerializedProperty pActive = m_rootProperty.FindPropertyRelative("active");
		DrawToggleLeftAndAdvance(m_rootProperty.displayName, pActive);
		if ( pActive.boolValue )
		{
			SerializedProperty pProbability = m_rootProperty.FindPropertyRelative("probability");
			pProbability.floatValue = EditorGUI.FloatField(m_pos, "\tProbability", pProbability.floatValue);
			AdvancePos();
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

	void DrawToggleLeftAndAdvance(string _name, SerializedProperty _property) {
		m_pos.height = EditorGUI.GetPropertyHeight(_property);
		_property.boolValue = EditorGUI.ToggleLeft(m_pos, _name, _property.boolValue);
		AdvancePos();
	}
}