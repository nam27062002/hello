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
		SerializedProperty p;

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Unity's "script" property
		p = serializedObject.FindProperty("m_Script");
		if(p != null) {
			// Draw the property, disabled
			bool wasEnabled = GUI.enabled;
			GUI.enabled = false;
			EditorGUILayout.PropertyField(p, true);
			GUI.enabled = wasEnabled;
		}

		// Dragon-based visibility
		// Toggle group
		p = serializedObject.FindProperty("m_checkSelectedDragon");
		p.boolValue = EditorGUILayout.ToggleLeft(p.displayName, p.boolValue);
		if(p.boolValue) {
			// Indent in
			EditorGUI.indentLevel++;

			// Disable the whole group if m_hideForDragons property is set to either NONE or ALL
			SerializedProperty hideForDragonsProp = serializedObject.FindProperty("m_hideForDragons");
			EditorGUILayout.PropertyField(hideForDragonsProp, true);
			bool wasEnabled = GUI.enabled;
			GUI.enabled = wasEnabled
				&& (hideForDragonsProp.enumValueIndex != (int)MenuShowConditionally.HideForDragons.NONE) 
				&& (hideForDragonsProp.enumValueIndex != (int)MenuShowConditionally.HideForDragons.ALL);

			// Draw all ownership properties
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_targetDragonSku"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_showIfLocked"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_showIfAvailable"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_showIfOwned"), true);

			// Restore enabled state
			GUI.enabled = wasEnabled;

			// Indent back out
			EditorGUI.indentLevel--;
		}

		// Screen-based visibility
		// Separator
		EditorGUILayoutExt.Separator();

		// Toggle group
		p = serializedObject.FindProperty("m_checkScreens");
		p.boolValue = EditorGUILayout.ToggleLeft(p.displayName, p.boolValue);
		if(p.boolValue) {
			// Indent in
			EditorGUI.indentLevel++;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_mode"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_screens"), true);

			// Indent back out
			EditorGUI.indentLevel--;
		}

		// Animation options
		// Separator
		EditorGUILayoutExt.Separator();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_restartShowAnimation"), true);

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