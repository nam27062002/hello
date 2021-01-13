// UI3DLoaderEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/03/2017.
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
/// Custom editor for the UI3DLoader class.
/// </summary>
[CustomEditor(typeof(UI3DLoader), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class UI3DLoaderEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float BUTTON_HEIGHT = 25f;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	UI3DLoader m_targetUI3DLoader = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUI3DLoader = target as UI3DLoader;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUI3DLoader = null;
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
		} while(p.NextVisible(false));      // Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Tools
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		{
			// Clear loaded egg instance
			GUI.color = Colors.coral;
			if(GUILayout.Button("UNLOAD", GUILayout.Height(BUTTON_HEIGHT))) {
				m_targetUI3DLoader.Unload();
			}

			// Force loading the egg
			GUI.color = Colors.paleGreen;
			if(GUILayout.Button("RELOAD", GUILayout.Height(BUTTON_HEIGHT))) {
				m_targetUI3DLoader.Load();
			}

			// Load placeholder
			GUI.color = Colors.paleYellow;
			if(GUILayout.Button("LOAD PLACEHOLDER", GUILayout.Height(BUTTON_HEIGHT))) {
				GameObject newInstance = m_targetUI3DLoader.Load();
				if(newInstance != null) {
					// Destroy as soon as awaken (this is meant to be used in edit mode)
					newInstance.AddComponent<SelfDestroy>().seconds = 0f;

					// Rename
					newInstance.gameObject.name = "PLACEHOLDER";
				}
			}

			// Reset color
			GUI.color = Color.white;
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		if(GUILayout.Button("LOAD NOW", GUILayout.Height(50f))) {
			m_targetUI3DLoader.Load();
		}

		if(GUILayout.Button("LOAD PLACEHOLDER", GUILayout.Height(50f))) {
			GameObject newInstance = m_targetUI3DLoader.Load();
			if(newInstance != null) {
				// Destroy as soon as awaken (this is meant to be used in edit mode)
				newInstance.AddComponent<SelfDestroy>().seconds = 0f;

				// Rename
				newInstance.gameObject.name = "PLACEHOLDER";
			}
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