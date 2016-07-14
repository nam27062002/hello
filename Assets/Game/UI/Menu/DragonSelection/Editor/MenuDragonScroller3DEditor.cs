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

		// Debug camera section
		EditorGUILayout.Space();
		EditorGUILayoutExt.Separator(new SeparatorAttribute("Debug Utils"));
		PathFollower posPath = targetMenuDragonScroller3D.FindComponentRecursive<PathFollower>("PosPath");
		PathFollower lookAtPath = targetMenuDragonScroller3D.FindComponentRecursive<PathFollower>("LookAtPath");
		CameraSnapPoint camSnapPoint = targetMenuDragonScroller3D.FindComponentRecursive<CameraSnapPoint>();
		GameObject menuCameraObj = GameObject.Find("Camera3D");
		if(posPath == null || lookAtPath == null) {
			EditorGUILayout.HelpBox("Either the PosPath or the LookAtPath followers couldn't be found.", MessageType.Error);
		} else if(camSnapPoint == null) {
			EditorGUILayout.HelpBox("Camera snap point couldn't be found.", MessageType.Error);
		} else if(menuCameraObj == null || menuCameraObj.GetComponent<Camera>() == null) {
			EditorGUILayout.HelpBox("Menu camera couldn't be found.", MessageType.Error);
		} else {
			// Aux vars
			float debugDelta = EditorPrefs.GetFloat("MenuDragonScroller3D.debugDelta", 0f);
			int debugSnapPoint = EditorPrefs.GetInt("MenuDragonScroller3D.debugSnapPoint", 0);

			// By delta
			EditorGUI.BeginChangeCheck();
			debugDelta = EditorGUILayout.Slider("Camera Preview Delta", debugDelta, 0f, 1f);
			if(EditorGUI.EndChangeCheck()) {
				// Set new delta
				posPath.delta = debugDelta;
				lookAtPath.delta = debugDelta;

				// Apply to camera
				camSnapPoint.Apply(menuCameraObj.GetComponent<Camera>());

				// Update matching snap point
				debugSnapPoint = posPath.snapPoint;

				// Store to prefs
				EditorPrefs.SetFloat("MenuDragonScroller3D.debugDelta", debugDelta);
				EditorPrefs.SetInt("MenuDragonScroller3D.debugSnapPoint", debugSnapPoint);
			}

			// By snap point
			EditorGUI.BeginChangeCheck();
			debugSnapPoint = EditorGUILayout.IntSlider("Camera Preview Point", debugSnapPoint, 0, posPath.path.pointCount - 1);
			if(EditorGUI.EndChangeCheck()) {
				// Set new snap point
				posPath.snapPoint = debugSnapPoint;
				lookAtPath.snapPoint = debugSnapPoint;

				// Apply to camera
				camSnapPoint.Apply(menuCameraObj.GetComponent<Camera>());

				// Update matching delta
				debugDelta = posPath.delta;

				// Store to prefs
				EditorPrefs.SetInt("MenuDragonScroller3D.debugSnapPoint", debugSnapPoint);
				EditorPrefs.SetFloat("MenuDragonScroller3D.debugDelta", debugDelta);
			}
		}
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}