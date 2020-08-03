// Spawner.SpawnConditionEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the Spawner.SpawnCondition class.
/// </summary>
[CustomPropertyDrawer(typeof(Spawner.SpawnKillCondition))]
public class SpawnerSpawnKillConditionPropertyEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public SpawnerSpawnKillConditionPropertyEditor() {
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
		// Type and value in the same line
		SerializedProperty categoryProp = _property.FindPropertyRelative("category");
		SerializedProperty valueProp = _property.FindPropertyRelative("value");
		m_pos.height = Mathf.Max(EditorGUI.GetPropertyHeight(categoryProp), EditorGUI.GetPropertyHeight(valueProp));

		// Label
		Rect contentPos = EditorGUI.PrefixLabel(m_pos, _label);
		Rect pos = new Rect(contentPos);

		// Clear indentation (between props)
		int indentBckp = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Type
		pos.width = contentPos.width * 0.33f;
		EditorGUI.PropertyField(pos, categoryProp, new GUIContent(), true);

		// Value
		pos.x += pos.width;
		pos.width = contentPos.width - pos.width;
		EditorGUI.PropertyField(pos, valueProp, new GUIContent(), true);

		// Restore indentation
		EditorGUI.indentLevel = indentBckp;

		// Done!
		AdvancePos();
	}
}