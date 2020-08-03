// ResultsScreenStepEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/10/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ResultsScreenStep class.
/// </summary>
[CustomEditor(typeof(ResultsScreenStep), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class ResultsScreenStepEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	ResultsScreenStep m_targetResultsScreenStep = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetResultsScreenStep = target as ResultsScreenStep;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetResultsScreenStep = null;
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
			// Properties requiring special treatment
			// Unity's "script" property
			if(p.name == "m_Script") {
				// Draw the property, disabled
				bool wasEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.PropertyField(p, true);
				GUI.enabled = wasEnabled;
			}

			// Dark screen color: only show if dark screen is enabled! (and do it in the same line)
			else if(p.name == "m_darkScreen") {
				if(p.boolValue) {
					// Active: Show color picker next to it
					EditorGUILayout.BeginHorizontal(); {
						// Toggle
						EditorGUILayout.PropertyField(p, true);

						// Color
						SerializedProperty colorProp = serializedObject.FindProperty("m_darkScreenColor");
						colorProp.colorValue = EditorGUILayout.ColorField(colorProp.colorValue);
					} EditorGUILayout.EndHorizontal();
				} else {
					// Not active: default drawer
					EditorGUILayout.PropertyField(p, true);
				}
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags"
				|| p.name == "m_darkScreenColor") {
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