// CloudWallEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/08/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the CloudWall class.
/// </summary>
[CustomEditor(typeof(CloudWall), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class CloudWallEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	CloudWall m_targetCloudWall = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetCloudWall = target as CloudWall;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetCloudWall = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		if(EditorGUI.EndChangeCheck()) {
			m_targetCloudWall.Generate();
		}
	}
}