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
/// Custom editor for the lookAt point.
/// </summary>
[CustomEditor(typeof(LookAtPoint))]
public class LookAtPointEditor : Editor {
	#region MEMBERS ----------------------------------------------------------------------------------------------------
	LookAtPoint lookAtTarget = null;
	Vector3[] points = new Vector3[2];
	#endregion

	#region METHODS ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		lookAtTarget = (LookAtPoint)target;
	}

	/// <summary>
	/// Updates stuff on the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		if(lookAtTarget) {
			lookAtTarget.lookAtPoint = EditorGUILayout.Vector3Field("Look At Point", lookAtTarget.lookAtPoint);
			if(GUI.changed) {
				EditorUtility.SetDirty(lookAtTarget);
			}
		}
	}

	/// <summary>
	/// Updates stuff on the scene.
	/// </summary>
	public void OnSceneGUI() {
		if(lookAtTarget) {
			// Draw and get positioning handles
			lookAtTarget.lookAtPoint = Handles.PositionHandle(lookAtTarget.lookAtPoint, Quaternion.identity);
			if(GUI.changed) {
				EditorUtility.SetDirty(lookAtTarget);
			}

			// Draw line from the object (camera) to the lookAt for clarity
			points[0] = lookAtTarget.gameObject.transform.position;
			points[1] = lookAtTarget.lookAtPoint;
			Handles.color = Color.cyan;
			Handles.DrawAAPolyLine(4f, points);
		}
	}
	#endregion
}
#endregion