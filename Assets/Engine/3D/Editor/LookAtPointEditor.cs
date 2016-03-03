// LookAtPointEditor.cs
// 
// Created by Alger Ortín Castellví on 29/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the lookAt point script.
/// </summary>
[CustomEditor(typeof(LookAtPoint))]
[CanEditMultipleObjects]
public class LookAtPointEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	LookAtPoint lookAtTarget = null;
	Vector3[] points = new Vector3[2];

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Editor enabled.
	/// </summary>
	public void OnEnable() {
		lookAtTarget = (LookAtPoint)target;
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
		if(lookAtTarget) {
			// Draw and get positioning handles
			if(Tools.pivotRotation == PivotRotation.Global) {
				lookAtTarget.lookAtPoint = Handles.PositionHandle(lookAtTarget.lookAtPoint, Quaternion.identity);
			} else {
				lookAtTarget.lookAtPoint = Handles.PositionHandle(lookAtTarget.lookAtPoint, lookAtTarget.transform.rotation);	// [AOC] Use object's rotation
			}

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
}