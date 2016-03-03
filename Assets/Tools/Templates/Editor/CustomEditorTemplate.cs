// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomEditor(typeof(MonoBehaviourTemplate), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MonoBehaviourTemplateEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static GUIStyle s_customStyle = null;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Casted target object
	MonoBehaviourTemplate m_targetMonoBehaviourTemplate = null;

	// Store a reference of interesting properties for faster access
	SerializedProperty m_myValueProp = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetMonoBehaviourTemplate = target as MonoBehaviourTemplate;

		// Store a reference of interesting properties for faster access
		m_myValueProp = serializedObject.FindProperty("m_myValue");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetMonoBehaviourTemplate = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Initialize custom styles if not done
		InitStyles();

		// Default inspector
		DrawDefaultInspector();

		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Show the custom GUI controls
		// Serialized property:
		EditorGUILayout.PropertyField(m_myValueProp);	// Serialized fields automatically detect changes and store Undo actions

		// Other fields (proper way to do it, for each field)
		EditorGUI.BeginChangeCheck();
		string newName = EditorGUILayout.TextField("Name", target.name);
		if(EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(target, "MonoBehaviourTemplate - name");
			target.name = newName;
		}

		// Show a progress bar only if all the objects have the same value:
		if(!m_myValueProp.hasMultipleDifferentValues) {
			// Get a rect for the progress bar using the same margins as a textfield:
			Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
			EditorGUI.ProgressBar(rect, m_myValueProp.intValue / 100f, "MyValue");
			EditorGUILayout.Space();
		}

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