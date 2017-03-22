// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using AI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomEditor(typeof(MachineOld), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MachineEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static GUIStyle s_customStyle = null;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	MachineOld m_targetMachine = null;

	// Store a reference of interesting properties for faster access
	SerializedProperty m_motionProp = null;
	SerializedProperty m_sensorProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetMachine = target as MachineOld;

		// Store a reference of interesting properties for faster access
		m_motionProp = serializedObject.FindProperty("m_motion");
		m_sensorProp = serializedObject.FindProperty("m_sensor");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetMachine = null;

		m_motionProp = null;
		m_sensorProp = null;
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
			if (p.name == "m_enableMotion") {
				// Draw the property
				p.boolValue = EditorGUILayout.ToggleLeft(p.displayName, p.boolValue);

				// If active, draw activation triggers
				if (p.boolValue) {
					// Draw activation properties
					EditorGUI.indentLevel++; {
						EditorGUILayout.PropertyField(m_motionProp, true);
					} EditorGUI.indentLevel--;
				}
			} else if (p.name == "m_enableSensor") {
				// Draw the property
				p.boolValue = EditorGUILayout.ToggleLeft(p.displayName, p.boolValue);

				// If active, draw activation triggers
				if (p.boolValue) {
					// Draw activation properties
					EditorGUI.indentLevel++; {
						EditorGUILayout.PropertyField(m_sensorProp, true);
					} EditorGUI.indentLevel--;
				}
			} else if (p.name == m_motionProp.name || p.name == m_sensorProp.name) {
				// do nothing
			} else {
				// Default
				EditorGUILayout.PropertyField(p, true);
			}
		} while (p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	/// <summary>
	/// Init custom styles if not done.
	/// Should only be done during the OnGUI/OnSceneGUI calls.
	/// </summary>
	private void InitStyles() {
		// Do it for every custom style
		if(s_customStyle == null) {
			s_customStyle = new GUIStyle(EditorStyles.textField);
			s_customStyle.normal.textColor = Colors.darkGray;
		}
	}
}