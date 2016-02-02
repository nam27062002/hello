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

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomEditor(typeof(MonoBehaviourTemplate))]
[CanEditMultipleObjects]
public class MonoBehaviourTemplateEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static GUIStyle s_customStyle = null;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Casted target object
	MonoBehaviourTemplate targetMonoBehaviourTemplate { get { return target as MonoBehaviourTemplate; }}

	// Store a reference of interesting properties for faster access
	SerializedProperty m_myValueProp = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Initialize custom styles if not done
		if(s_customStyle == null) {
			// Is style initialized?
			s_customStyle = new GUIStyle(EditorStyles.textField);
			s_customStyle.normal.textColor = Colors.darkGray;
		}

		// Store a reference of interesting properties for faster access
		m_myValueProp = serializedObject.FindProperty("m_myValue");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Show the custom GUI controls
		EditorGUILayout.PropertyField(m_myValueProp);

		// Show a progress bar only if all the objects have the same value:
		if(!m_myValueProp.hasMultipleDifferentValues) {
			// Get a rect for the progress bar using the same margins as a textfield:
			Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
			EditorGUI.ProgressBar(rect, m_myValueProp.intValue / 100f, "MyValue");
			EditorGUILayout.Space();
		}

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}