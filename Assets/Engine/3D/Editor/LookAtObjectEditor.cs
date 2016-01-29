// LookAtObjectEditor.cs
// 
// Created by Alger Ortín Castellví on 29/01/2016.
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
/// Custom editor for the lookAt object script.
/// </summary>
[CustomEditor(typeof(LookAtObject))]
public class LookAtObjectEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	LookAtObject lookAtTarget = null;
	Vector3[] points = new Vector3[2];

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Editor enabled.
	/// </summary>
	public void OnEnable() {
		lookAtTarget = (LookAtObject)target;
	}

	/// <summary>
	/// Updates stuff on the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector works for us
		DrawDefaultInspector();
	}

	/// <summary>
	/// Updates stuff on the scene.
	/// </summary>
	public void OnSceneGUI() {
		if(lookAtTarget != null && lookAtTarget.lookAtObject) {
			// Draw and get positioning handles on target object
			if(Tools.pivotRotation == PivotRotation.Global) {
				lookAtTarget.lookAtObject.transform.position = Handles.PositionHandle(lookAtTarget.lookAtObject.transform.position, Quaternion.identity);
			} else {
				lookAtTarget.lookAtObject.transform.position = Handles.PositionHandle(lookAtTarget.lookAtObject.transform.position, lookAtTarget.lookAtObject.transform.rotation);
			}

			if(GUI.changed) {
				EditorUtility.SetDirty(lookAtTarget);
			}

			// Draw line from the object (camera) to the lookAt for clarity
			points[0] = lookAtTarget.gameObject.transform.position;
			points[1] = lookAtTarget.lookAtObject.transform.position;
			Handles.color = Color.cyan;
			Handles.DrawAAPolyLine(4f, points);
		}
	}
}