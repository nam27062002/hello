// HUDMessageEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/06/2016.
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
/// Custom editor for the HUDMessage class.
/// </summary>
[CustomEditor(typeof(HUDMessage), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class HUDMessageEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Store a reference of interesting properties for faster access
	SerializedProperty m_typeProp = null;
	SerializedProperty m_hideModeProp = null;
	SerializedProperty m_idleDurationProp = null;

	SerializedProperty m_boostReminderTriggerTimeProp = null;

	SerializedProperty m_onShowProp = null;
	SerializedProperty m_onHideProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Store a reference of interesting properties for faster access
		m_typeProp = serializedObject.FindProperty("m_type");
		m_hideModeProp = serializedObject.FindProperty("m_hideMode");
		m_idleDurationProp = serializedObject.FindProperty("m_idleDuration");

		m_boostReminderTriggerTimeProp = serializedObject.FindProperty("m_boostReminderTriggerTime");

		m_onShowProp = serializedObject.FindProperty("OnShow");
		m_onHideProp = serializedObject.FindProperty("OnHide");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear references to properties
		m_typeProp = null;
		m_hideModeProp = null;
		m_idleDurationProp = null;

		m_boostReminderTriggerTimeProp = null;

		m_onShowProp = null;
		m_onHideProp = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Show custom GUI controls
		// Idle timer: only if hide mode is TIMER
		if(m_hideModeProp.enumValueIndex == (int)HUDMessage.HideMode.TIMER) {	// [AOC] Unsafe operation, we can do it cause we know enum starts with 0
			EditorGUILayout.PropertyField(m_idleDurationProp);
		}

		// Custom properties based on type
		// Add here any types requiring extra setup
		switch((HUDMessage.Type)m_typeProp.enumValueIndex) {
			case HUDMessage.Type.BOOST_REMINDER: {
				EditorGUILayoutExt.Separator();
				EditorGUILayout.PropertyField(m_boostReminderTriggerTimeProp);
			} break;
		}

		// Events
		EditorGUILayoutExt.Separator();
		EditorGUILayout.PropertyField(m_onShowProp);
		EditorGUILayout.PropertyField(m_onHideProp);

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