// MenuScreensControllerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
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
/// Custom editor for the MenuScreensController class.
/// </summary>
[CustomEditor(typeof(MenuScreensController), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MenuScreensControllerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	MenuScreensController m_target = null;

	// Important serialized properties
	SerializedProperty m_screensProp = null;
	SerializedProperty m_scenesProp = null;
	SerializedProperty m_cameraSnapPointsProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_target = target as MenuScreensController;

		// Store important properties
		m_screensProp = serializedObject.FindProperty("m_screens");
		m_scenesProp = serializedObject.FindProperty("m_scenes");
		m_cameraSnapPointsProp = serializedObject.FindProperty("m_cameraSnapPoints");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_target = null;

		// Clear properties
		m_screensProp = null;
		m_scenesProp = null;
		m_cameraSnapPointsProp = null;
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
			if(p.name == m_screensProp.name) {
				// Make sure both screens and scenes array properties have exactly one entry per menu screen
				m_screensProp.arraySize = (int)MenuScreens.COUNT;
				m_scenesProp.arraySize = (int)MenuScreens.COUNT;
				m_cameraSnapPointsProp.arraySize = (int)MenuScreens.COUNT;

				// Group in a foldout
				EditorGUILayout.Space();
				m_screensProp.isExpanded = EditorGUILayout.Foldout(m_screensProp.isExpanded, "Screens");
				if(m_screensProp.isExpanded) {
					// Indent in
					EditorGUI.indentLevel++;

					// Show them nicely formatted
					for(int i = 0; i < (int)MenuScreens.COUNT; i++) {
						// Label + foldout
						// Use screen's property to store foldout state
						SerializedProperty currentScreenProp = m_screensProp.GetArrayElementAtIndex(i);
						currentScreenProp.isExpanded = EditorGUILayout.Foldout(currentScreenProp.isExpanded, ((MenuScreens)(i)).ToString(), true);

						// Folded content
						if(currentScreenProp.isExpanded) {
							// Indent in
							EditorGUI.indentLevel++;

							// Screen field
							EditorGUILayout.PropertyField(m_screensProp.GetArrayElementAtIndex(i), new GUIContent("Screen"));

							// Scene field
							EditorGUILayout.PropertyField(m_scenesProp.GetArrayElementAtIndex(i), new GUIContent("Scene"));

							// Camera snap point field
							EditorGUILayout.PropertyField(m_cameraSnapPointsProp.GetArrayElementAtIndex(i), new GUIContent("Camera Snap Point (optional)"));

							// Indent out
							EditorGUI.indentLevel--;
						}
					}

					// Indent out
					EditorGUI.indentLevel--;
				}

				// Space below
				EditorGUILayout.Space();
			}

			// Properties to skip
			else if(p.name == m_scenesProp.name
				 || p.name == m_cameraSnapPointsProp.name) {
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