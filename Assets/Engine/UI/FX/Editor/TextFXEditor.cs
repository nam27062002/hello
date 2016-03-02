// TextFXEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/03/2016.
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
/// Custom editor for the TextFX class.
/// </summary>
[CustomEditor(typeof(TextFX), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class TextFXEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Casted target object
	private TextFX m_targetTextFX = null;

	// Important properties
	private SerializedProperty m_boldProp = null;
	private SerializedProperty m_outlineProp = null;
	private SerializedProperty m_shadowProp = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetTextFX = target as TextFX;

		// Initialize properties
		m_boldProp = serializedObject.FindProperty("m_bold");
		m_outlineProp = serializedObject.FindProperty("m_outline");
		m_shadowProp = serializedObject.FindProperty("m_shadow");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetTextFX = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Bold group
		m_boldProp.boolValue = EditorGUILayout.ToggleLeft(" Bold", m_boldProp.boolValue);
		if(m_boldProp.boolValue) {
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_boldSize"));
			EditorGUILayout.Space();
			EditorGUI.indentLevel--;
		}

		// Outline
		m_outlineProp.boolValue = EditorGUILayout.ToggleLeft(" Outline", m_outlineProp.boolValue);
		if(m_outlineProp.boolValue) {
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_outlineQuality"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_outlineSize"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_outlineColor"));
			EditorGUILayout.Space();
			EditorGUI.indentLevel--;
		}

		// Shadow
		m_shadowProp.boolValue = EditorGUILayout.ToggleLeft(" Shadow", m_shadowProp.boolValue);
		if(m_shadowProp.boolValue) {
			EditorGUI.indentLevel++;
			//SerializedProperty angleProp = serializedObject.FindProperty("m_shadowAngle"); 
			//angleProp.floatValue = EditorGUILayout.Knob(Vector2.one * 100f, angleProp.floatValue, 0f, 360f, "º", Colors.black, Colors.green, true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_shadowAngle"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_shadowSize"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_shadowColor"));
			EditorGUILayout.Space();
			EditorGUI.indentLevel--;
		}

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}
}