// PathFollowerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/02/2016.
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
/// Custom editor for the PathFollower class.
/// </summary>
[CustomEditor(typeof(PathFollower))]
[CanEditMultipleObjects]
public class PathFollowerEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Casted target object
	PathFollower targetPathFollower { get { return target as PathFollower; }}

	// Store a reference of interesting properties for faster access
	SerializedProperty m_targetProp = null;
	SerializedProperty m_pathProp = null;
	SerializedProperty m_linkModeProp = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Store a reference of interesting properties for faster access
		m_targetProp = serializedObject.FindProperty("m_target");
		m_pathProp = serializedObject.FindProperty("m_path");
		m_linkModeProp = serializedObject.FindProperty("m_linkMode");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {

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

		// Show the custom GUI controls
		// References
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(m_targetProp);
		EditorGUILayout.PropertyField(m_pathProp);

		// Space
		EditorGUILayout.Space();

		// Interactive controls - if required fields are set
		if(targetPathFollower.target == null || targetPathFollower.path == null) {
			// Info box
			EditorGUILayout.HelpBox("Required fields not set", MessageType.Error);
		} else {
			// Use properties rather than directly setting the values
			// Delta
			EditorGUI.BeginChangeCheck();
			float newDelta = EditorGUILayout.Slider("Delta", targetPathFollower.delta, 0f, 1f);
			if(EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(target, "PathFollower.delta");
				targetPathFollower.delta = newDelta;
			}

			// Snap point
			EditorGUI.BeginChangeCheck();
			int newSnapPoint = EditorGUILayout.IntSlider("SnapPoint", targetPathFollower.snapPoint, 0, targetPathFollower.path.pointCount - 1);
			if(EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(target, "PathFollower.snapPoint");
				targetPathFollower.snapPoint = newSnapPoint;
			}

			// Link mode
			EditorGUILayout.PropertyField(m_linkModeProp);

			// Debug buttons - only when application is playing, otherwise tweens don't work
			if(Application.isPlaying) {
				EditorGUILayout.BeginHorizontal(); {
					if(GUILayout.Button("<-")) {
						targetPathFollower.SnapTo(targetPathFollower.snapPoint - 1, 1f);
					}

					if(GUILayout.Button("->")) {
						targetPathFollower.SnapTo(targetPathFollower.snapPoint + 1, 1f);
					}
				} EditorGUILayoutExt.EndHorizontalSafe();
			}
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
}