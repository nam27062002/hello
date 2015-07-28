// Monster Rage
// 
// Created by Alger Ortín Castellví on 29/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Custom editor for the launch point.
/// </summary>
[CustomEditor(typeof(LaunchPoint))]
public class LaunchPointEditor : Editor {
	#region MEMBERS ----------------------------------------------------------------------------------------------------
	LaunchPoint pointTarget = null;
	Vector3[] points = new Vector3[2];
	#endregion

	#region METHODS ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		pointTarget = (LaunchPoint)target;
	}

	/// <summary>
	/// Updates stuff on the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		if(pointTarget) {
			pointTarget.launchPoint = EditorGUILayout.Vector3Field("Launch Direction", pointTarget.launchPoint);
			if(GUI.changed) {
				EditorUtility.SetDirty(pointTarget);
			}
		}
	}

	/// <summary>
	/// Updates stuff on the scene.
	/// </summary>
	public void OnSceneGUI() {
		if(pointTarget) {
			// Draw and get positioning handles
			pointTarget.launchPoint = Handles.PositionHandle(pointTarget.launchPoint, Quaternion.identity);
			if(GUI.changed) {
				EditorUtility.SetDirty(pointTarget);
			}

			// Draw line from the object (camera) to the lookAt for clarity
			points[0] = pointTarget.gameObject.transform.position;
			points[1] = pointTarget.launchPoint;
			Handles.color = Color.magenta;
			Handles.DrawAAPolyLine(4f, points);
		}
	}
	#endregion
}
#endregion