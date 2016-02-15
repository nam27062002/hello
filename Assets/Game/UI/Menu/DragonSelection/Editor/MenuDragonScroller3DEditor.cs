// MenuDragonScroller3DEditor.cs
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
/// Custom editor for the MenuDragonScroller3D class.
/// </summary>
[CustomEditor(typeof(MenuDragonScroller3D))]
[CanEditMultipleObjects]
public class MenuDragonScroller3DEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Casted target object
	MenuDragonScroller3D targetMenuDragonScroller3D { get { return target as MenuDragonScroller3D; }}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		
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

		// Show GUI controls
		// Use default inspector
		DrawDefaultInspector();

		// Space
		EditorGUILayout.Space();

		// Interactive controls - if required fields are set
		if(targetMenuDragonScroller3D.cameraPath == null || targetMenuDragonScroller3D.lookAtPath == null) {
			// Info box
			EditorGUILayout.HelpBox("Required fields not set", MessageType.Error);
		} else {
			// Use properties rather than directly setting the values
			// Delta
			EditorGUI.BeginChangeCheck();
			float newDelta = EditorGUILayout.Slider("Delta", targetMenuDragonScroller3D.delta, 0f, 1f);
			if(EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(target, "MenuDragonScroller3D.delta");
				targetMenuDragonScroller3D.delta = newDelta;
			}

			// Debug buttons - only when application is playing, otherwise tweens don't work
			if(Application.isPlaying) {
				EditorGUILayout.BeginHorizontal(); {
					if(GUILayout.Button("<-")) {
						targetMenuDragonScroller3D.SnapTo(targetMenuDragonScroller3D.cameraPath.snapPoint - 1);
					}

					if(GUILayout.Button("->")) {
						targetMenuDragonScroller3D.SnapTo(targetMenuDragonScroller3D.cameraPath.snapPoint + 1);
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