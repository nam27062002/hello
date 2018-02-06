// MenuCameraGraphTesterEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace MenuCameraTest {
	/// <summary>
	/// Custom editor for the MenuCameraGraphTester class.
	/// </summary>
	[CustomEditor(typeof(MenuCameraGraphTester), true)]	// True to be used by heir classes as well
	[CanEditMultipleObjects]
	public class MenuCameraGraphTesterEditor : Editor {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		// Casted target object
		MenuCameraGraphTester m_targetTester = null;

		//------------------------------------------------------------------------//
		// METHODS																  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// The editor has been enabled - target object selected.
		/// </summary>
		private void OnEnable() {
			// Get target object
			m_targetTester = target as MenuCameraGraphTester;
		}

		/// <summary>
		/// The editor has been disabled - target object unselected.
		/// </summary>
		private void OnDisable() {
			// Clear target object
			m_targetTester = null;
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

				// Properties we don't want to show
				else if(p.name == "m_ObjectHideFlags") {
					// Do nothing
				}

				// Default property display
				else {
					EditorGUILayout.PropertyField(p, true);
				}
			} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

			// Test Button
			if(GUILayout.Button("TEST", GUILayout.Height(50f))) {
				m_targetTester.Test();
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
}