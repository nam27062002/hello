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
[CustomEditor(typeof(Machine), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
	public class MachineEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static GUIStyle s_customStyle = null;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Store a reference of interesting properties for faster access
	protected SerializedProperty m_motionProp 	 = null;

	private SerializedProperty m_sensorProp 	 = null;
	private SerializedProperty m_edibleProp 	 = null;
	private SerializedProperty m_inflammableProp = null;
	private SerializedProperty m_enableSensorProp= null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	protected virtual void OnEnable() {
		// Store a reference of interesting properties for faster access
		m_sensorProp 		= serializedObject.FindProperty("m_sensor");
		m_edibleProp		= serializedObject.FindProperty("m_edible");
		m_inflammableProp 	= serializedObject.FindProperty("m_inflammable");
		m_enableSensorProp	= serializedObject.FindProperty("m_enableSensor");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		m_motionProp 	  = null;
		m_sensorProp 	  = null;
		m_edibleProp 	  = null;
		m_inflammableProp = null;
		m_enableSensorProp= null;
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

		//print components
		EditorGUILayoutExt.Separator(new SeparatorAttribute("Components"));

		EditorGUI.indentLevel++; {
			if (m_motionProp != null) EditorGUILayout.PropertyField(m_motionProp, true);
			EditorGUILayout.PropertyField(m_edibleProp, true);
			EditorGUILayout.PropertyField(m_inflammableProp, true);
		} EditorGUI.indentLevel--;

		m_enableSensorProp.boolValue = EditorGUILayout.ToggleLeft(m_enableSensorProp.displayName, m_enableSensorProp.boolValue);
		if (m_enableSensorProp.boolValue) {// If active, draw activation triggers
			EditorGUI.indentLevel++; {
				EditorGUILayout.PropertyField(m_sensorProp, true);
			} EditorGUI.indentLevel--;
		}
		
		EditorGUILayoutExt.Separator(new SeparatorAttribute());
		//

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element

		do {
			if (p.name == "m_ObjectHideFlags" || p.name == "m_Script") {
				// do nothing
			} else if ((m_motionProp != null && p.name == m_motionProp.name) || p.name == m_edibleProp.name || p.name == m_inflammableProp.name || p.name == m_enableSensorProp.name || p.name == m_sensorProp.name) {
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