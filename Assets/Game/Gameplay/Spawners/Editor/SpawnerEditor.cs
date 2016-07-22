// SpawnerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/07/2016.
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
/// Custom editor for the Spawner class.
/// </summary>
[CustomEditor(typeof(Spawner), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class SpawnerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	Spawner m_targetSpawner = null;

	// Store a reference of interesting properties for faster access
	SerializedProperty m_activationTimeProp = null;
	SerializedProperty m_activationXPProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetSpawner = target as Spawner;

		// Store a reference of interesting properties for faster access
		m_activationTimeProp = serializedObject.FindProperty("m_activationTime");
		m_activationXPProp = serializedObject.FindProperty("m_activationXP");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetSpawner = null;

		// Clear properties
		m_activationTimeProp = null;
		m_activationXPProp = null;
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
			if(p.name == "m_alwaysActive") {
				// Draw the property
				EditorGUILayout.PropertyField(p);

				// If active, draw activation conditions
				if(!p.boolValue) {
					// Draw activation properties
					EditorGUILayout.PropertyField(m_activationTimeProp);
					EditorGUILayout.PropertyField(m_activationXPProp);

					// If both min values are < 0, show error message
					if(m_targetSpawner.activationTime.min < 0 && m_targetSpawner.activationXP.min < 0) {
						EditorGUILayout.HelpBox("Either activation time or activation XP must be at least 0!", MessageType.Error);
					}
				}
			}

			// Default
			else {
				EditorGUILayout.PropertyField(p);
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