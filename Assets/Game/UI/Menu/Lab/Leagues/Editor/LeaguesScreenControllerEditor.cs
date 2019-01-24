// LeaguesScreenControllerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the LeaguesScreenController class.
/// </summary>
[CustomEditor(typeof(LeaguesScreenController), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class LeaguesScreenControllerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	LeaguesScreenController m_targetLeaguesScreenController = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetLeaguesScreenController = target as LeaguesScreenController;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetLeaguesScreenController = null;
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

			// Panels array
			else if(p.name == "m_panels") {
				// Fixed length
				p.arraySize = (int)LeaguesScreenController.Panel.COUNT;

				// Show the expected panel as label for each entry
				p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, "Panels");
				if(p.isExpanded) {
					// Indent in
					EditorGUI.indentLevel++;

					// Show them nicely formatted
					for(int i = 0; i < (int)LeaguesScreenController.Panel.COUNT; i++) {
						// Label + default property inspector
						EditorGUILayout.PropertyField(
							p.GetArrayElementAtIndex(i), 
							new GUIContent(((LeaguesScreenController.Panel)(i)).ToString()), 
							true
						);
					}

					// Indent out
					EditorGUI.indentLevel--;
				}
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags") {
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