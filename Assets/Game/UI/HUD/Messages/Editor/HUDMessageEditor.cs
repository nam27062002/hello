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

	SerializedProperty m_boostSetup = null;
	SerializedProperty m_boostTutorialSetup = null;

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

		m_boostSetup = serializedObject.FindProperty("m_boostMessageSetup");
		m_boostTutorialSetup = serializedObject.FindProperty("m_boostMessageTutorialSetup");

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

		m_boostSetup = null;
		m_boostTutorialSetup = null;

		m_onShowProp = null;
		m_onHideProp = null;
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

			// Idle timer: only if hide mode is TIMER and type different than BOOST_REMINDER
			else if(p.name == m_idleDurationProp.name) {
				if(m_hideModeProp.enumValueIndex == (int)HUDMessage.HideMode.TIMER
				&& m_typeProp.enumValueIndex != (int)HUDMessage.Type.BOOST_REMINDER) {	// [AOC] Unsafe operation, we can do it cause we know enum starts with 0
					EditorGUILayout.PropertyField(p, true);
				}
			}

			// Boost setup properties
			else if(p.name == m_boostSetup.name) {
				// Only for BOOST_REMINDER messages
				if(m_typeProp.enumValueIndex == (int)HUDMessage.Type.BOOST_REMINDER) {	// [AOC] Unsafe operation, we can do it cause we know enum starts with 0
					EditorGUILayout.PropertyField(m_boostSetup, true);
					EditorGUILayout.PropertyField(m_boostTutorialSetup, true);
				}
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags"
				 || p.name == m_boostTutorialSetup.name) {
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