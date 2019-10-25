// SelfDestroyEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/10/2019.
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
/// Custom editor for the SelfDestroy class.
/// </summary>
[CustomEditor(typeof(SelfDestroy), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class SelfDestroyEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	SelfDestroy m_targetSelfDestroy = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetSelfDestroy = target as SelfDestroy;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetSelfDestroy = null;
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

		// Unity's "script" property - draw disabled
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
		EditorGUI.EndDisabledGroup();

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			if(p.name == "m_mode") {
				// Do property
				EditorGUILayout.PropertyField(p, true);

				// Show either seconds or frames properties afterwards
				if(p.enumValueIndex == (int)SelfDestroy.Mode.FRAMES) {
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_frames"), true);
				} else {
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_seconds"), true);
				}
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags" || p.name == "m_Script"
			|| p.name == "m_seconds" || p.name == "m_frames") {
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