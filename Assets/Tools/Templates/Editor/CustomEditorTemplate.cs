﻿// MonoBehaviourTemplateEditor.cs
// @projectName
// 
// Created by @author on @dd/@mm/@yyyy.
// Copyright (c) @yyyy Ubisoft. All rights reserved.

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
[CustomEditor(typeof(MonoBehaviourTemplate), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MonoBehaviourTemplateEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private MonoBehaviourTemplate m_targetMonoBehaviourTemplate = null;

	// Cached properties
	private SerializedProperty m_scriptProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetMonoBehaviourTemplate = target as MonoBehaviourTemplate;

		// Cache frequent properties
		m_scriptProp = serializedObject.FindProperty("m_Script");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetMonoBehaviourTemplate = null;

		// Clear cached properties
		m_scriptProp = null;
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
		EditorGUILayout.PropertyField(m_scriptProp, true);
		EditorGUI.EndDisabledGroup();

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Properties we don't want to show or that will be displayed together with another property
			if(p.name == "m_ObjectHideFlags"
			|| p.name == m_scriptProp.name
			) {
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