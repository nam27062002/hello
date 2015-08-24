// PlaneBehaviourEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Custom editor for the plane behaviour.
/// </summary>
[CustomEditor(typeof(PlaneBehaviour))]
public class PlaneBehaviourEditor : Editor {
	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	PlaneBehaviour mTargetPlane;
	Vector3 mPlanePos;
	Vector3[] mPoints;
	#endregion

	#region METHODS ----------------------------------------------------------------------------------------------------
	public void Awake() {
		// Get target object
		mTargetPlane = (PlaneBehaviour)target;
		mPlanePos = mTargetPlane.transform.position;

		// Create empty points array
		mPoints = new Vector3[5];	// 5 points to draw the rectangle
	}

	/// <summary>
	/// Updates stuff on the scene editor window.
	/// </summary>
	public void OnSceneGUI() {
		// Don't move rectangle around during playback (the plane will be flying around)
		if(!Application.isPlaying) {
			mPlanePos = mTargetPlane.transform.position;
		}

		// Draw a rectangle displaying the fly area of the plane
		// Refresh points list
		mPoints[0].Set(mPlanePos.x, mPlanePos.y + mTargetPlane.flyArea.y/2, mPlanePos.z);
		mPoints[1].Set(mPlanePos.x + mTargetPlane.flyArea.x, mPlanePos.y + mTargetPlane.flyArea.y/2, mPlanePos.z);
		mPoints[2].Set(mPlanePos.x + mTargetPlane.flyArea.x, mPlanePos.y - mTargetPlane.flyArea.y/2, mPlanePos.z);
		mPoints[3].Set(mPlanePos.x, mPlanePos.y - mTargetPlane.flyArea.y/2, mPlanePos.z);
		mPoints[4] = mPoints[0];

		// Do the drawing :-)
		Handles.color = Color.cyan;
		Handles.DrawPolyLine(mPoints);
	}
	#endregion
}
#endregion