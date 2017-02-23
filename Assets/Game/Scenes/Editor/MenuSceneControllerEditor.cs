// MenuSceneControllerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/12/2017.
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
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomEditor(typeof(MenuSceneController), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MenuSceneControllerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	MenuSceneController m_targetMenuSceneController = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetMenuSceneController = target as MenuSceneController;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetMenuSceneController = null;
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

			// Fog Setup
			else if(p.name == "m_fogSetup") {
				// Draw default property
				EditorGUILayout.PropertyField(p, true);

				// Add test button
				EditorGUILayout.Space();
				bool wasEnabled = GUI.enabled;
				GUI.enabled = EditorApplication.isPlaying;
				if(GUILayout.Button("APPLY\n(Only in playing mode)", GUILayout.Height(50f))) {
					m_targetMenuSceneController.RecreateFogTexture();
				}
				GUI.enabled = wasEnabled;
				EditorGUILayout.Space();
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
		float posY = 50f;
		Range xRange = new Range(-500, 500);
		Range zRange = new Range(m_targetMenuSceneController.fogSetup.m_fogStart, m_targetMenuSceneController.fogSetup.m_fogEnd);
		Vector3[] verts = new Vector3[] {
			new Vector3(xRange.min, posY, zRange.min),
			new Vector3(xRange.min, posY, zRange.max),
			new Vector3(xRange.max, posY, zRange.max),
			new Vector3(xRange.max, posY, zRange.min),
		};
		Handles.DrawSolidRectangleWithOutline(verts, Colors.WithAlpha(Colors.cyan, 0.25f), Colors.cyan);
	}
}