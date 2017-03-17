// MenuShowConditionallyEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/03/2017.
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
/// Custom editor for the MenuShowConditionally class.
/// </summary>
[CustomEditor(typeof(MenuShowConditionally), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MenuShowConditionallyEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	MenuShowConditionally m_targetMenuShowConditionally = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetMenuShowConditionally = target as MenuShowConditionally;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetMenuShowConditionally = null;
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

			// Dragon ownership variables
			else if(p.name == "m_targetDragonSku") {
				// Disable the whole group if m_hideForDragons property is set to either NONE or ALL
				SerializedProperty hideForDragonsProp = serializedObject.FindProperty("m_hideForDragons");
				bool wasEnabled = GUI.enabled;
				GUI.enabled = (hideForDragonsProp.enumValueIndex != (int)MenuShowConditionally.HideForDragons.NONE) 
					&& (hideForDragonsProp.enumValueIndex != (int)MenuShowConditionally.HideForDragons.ALL);

				// Draw all ownership properties
				EditorGUILayout.PropertyField(p, true);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_showIfLocked"), true);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_showIfAvailable"), true);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_showIfOwned"), true);

				// Restore enabled state
				GUI.enabled = wasEnabled;
			}

			// Properties we don't want to show (or that are processed elsewhere)
			else if(p.name == "m_ObjectHideFlags"
				|| p.name == "m_showIfLocked"
				|| p.name == "m_showIfAvailable"
				|| p.name == "m_showIfOwned") {
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