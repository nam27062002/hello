// MenuCameraAnimatorByCurvesByCurvesEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/10/2016.
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
/// Custom editor for the MenuCameraAnimatorByCurves class.
/// </summary>
[CustomEditor(typeof(MenuCameraAnimatorByCurves))]
[CanEditMultipleObjects]
public class MenuCameraAnimatorByCurvesByCurvesEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Casted target object
	MenuCameraAnimatorByCurves targetMenuCameraAnimatorByCurves { get { return target as MenuCameraAnimatorByCurves; }}

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
		if(targetMenuCameraAnimatorByCurves.cameraPath == null || targetMenuCameraAnimatorByCurves.lookAtPath == null) {
			// Info box
			EditorGUILayout.HelpBox("Required fields not set", MessageType.Error);
		} else {
			// Use properties rather than directly setting the values
			// Delta
			EditorGUI.BeginChangeCheck();
			float newDelta = EditorGUILayout.Slider("Delta", targetMenuCameraAnimatorByCurves.delta, 0f, 1f);
			if(EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(target, "MenuCameraAnimatorByCurves.delta");
				targetMenuCameraAnimatorByCurves.delta = newDelta;
			}

			// Debug buttons - only when application is playing, otherwise tweens don't work
			if(Application.isPlaying) {
				EditorGUILayout.BeginHorizontal(); {
					if(GUILayout.Button("<-")) {
						targetMenuCameraAnimatorByCurves.SnapTo(targetMenuCameraAnimatorByCurves.cameraPath.snapPoint - 1);
					}

					if(GUILayout.Button("->")) {
						targetMenuCameraAnimatorByCurves.SnapTo(targetMenuCameraAnimatorByCurves.cameraPath.snapPoint + 1);
					}
				} EditorGUILayoutExt.EndHorizontalSafe();
			}
		}

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// Debug camera section
		EditorGUILayout.Space();
		EditorGUILayoutExt.Separator(new SeparatorAttribute("Debug Utils"));
		PathFollower posPath = targetMenuCameraAnimatorByCurves.FindComponentRecursive<PathFollower>("PosPath");
		PathFollower lookAtPath = targetMenuCameraAnimatorByCurves.FindComponentRecursive<PathFollower>("LookAtPath");
		CameraSnapPoint camSnapPoint = targetMenuCameraAnimatorByCurves.FindComponentRecursive<CameraSnapPoint>();
		GameObject menuCameraObj = GameObject.Find("Camera3D");
		if(posPath == null || lookAtPath == null) {
			EditorGUILayout.HelpBox("Either the PosPath or the LookAtPath followers couldn't be found.", MessageType.Error);
		} else if(camSnapPoint == null) {
			EditorGUILayout.HelpBox("Camera snap point couldn't be found.", MessageType.Error);
		} else if(menuCameraObj == null || menuCameraObj.GetComponent<Camera>() == null) {
			EditorGUILayout.HelpBox("Menu camera couldn't be found.", MessageType.Error);
		} else {
			// Aux vars
			float debugDelta = EditorPrefs.GetFloat("MenuCameraAnimatorByCurves.debugDelta", 0f);
			int debugSnapPoint = EditorPrefs.GetInt("MenuCameraAnimatorByCurves.debugSnapPoint", 0);

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
				EditorPrefs.SetFloat("MenuCameraAnimatorByCurves.debugDelta", debugDelta);
				EditorPrefs.SetInt("MenuCameraAnimatorByCurves.debugSnapPoint", debugSnapPoint);
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
				EditorPrefs.SetInt("MenuCameraAnimatorByCurves.debugSnapPoint", debugSnapPoint);
				EditorPrefs.SetFloat("MenuCameraAnimatorByCurves.debugDelta", debugDelta);
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