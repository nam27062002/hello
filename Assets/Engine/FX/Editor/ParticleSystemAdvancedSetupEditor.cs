// ParticleSystemAdvancedSetupEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ParticleSystemAdvancedSetup class.
/// </summary>
[CustomEditor(typeof(ParticleSystemAdvancedSetup), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class ParticleSystemAdvancedSetupEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	ParticleSystemAdvancedSetup m_target = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_target = target as ParticleSystemAdvancedSetup;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_target = null;
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

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Delay Range
			if(p.name == "m_delayRange") {
				// Group in a horizontal group
				EditorGUILayout.BeginHorizontal(); {
					// Toggle
					SerializedProperty toggleProp = serializedObject.FindProperty("m_overrideDelay");
					toggleProp.boolValue = GUILayout.Toggle(toggleProp.boolValue, GUIContent.none, GUILayout.Width(10f));

					// Reset indentation
					int indentLevelBackup = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 0;

					// Draw the property - Only enabled if toggle is checked
					GUI.enabled = toggleProp.boolValue;
					EditorGUILayout.PropertyField(p);
					GUI.enabled = true;

					// Restore indentation
					EditorGUI.indentLevel = indentLevelBackup;
				} EditorGUILayout.EndHorizontal();
			}

			// Start Colors
			else if(p.name == "m_startColors") {
				// Group in a horizontal group
				EditorGUILayout.BeginHorizontal(); {
					// Toggle
					SerializedProperty toggleProp = serializedObject.FindProperty("m_overrideColor");
					toggleProp.boolValue = GUILayout.Toggle(toggleProp.boolValue, GUIContent.none, GUILayout.Width(25f));

					// Reset indentation
					int indentLevelBackup = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 0;

					// Draw the property - Only enabled if toggle is checked
					GUI.enabled = toggleProp.boolValue;
					EditorGUILayout.PropertyField(p, true);
					GUI.enabled = true;

					// Restore indentation
					EditorGUI.indentLevel = indentLevelBackup;
				} EditorGUILayout.EndHorizontal();
			}

			// Properties to skip
			else if(p.name == "m_overrideDelay"
				 || p.name == "m_overrideColor") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

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