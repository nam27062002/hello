// AssetsDownloadFlowProgressBarEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the AssetsDownloadFlowProgressBar class.
/// </summary>
[CustomEditor(typeof(AssetsDownloadFlowProgressBar), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class AssetsDownloadFlowProgressBarEditor : Editor {
	//----------------------------------------------------------------------//
	// CONSTANTS															//
	//----------------------------------------------------------------------//
	private const float BUTTON_HEIGHT = 25f;

	//----------------------------------------------------------------------//
	// METHODS																//
	//----------------------------------------------------------------------//
	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Tools
		EditorGUILayout.Space();
		EditorGUILayoutExt.Separator("Test States");
		EditorGUILayout.BeginHorizontal(); {
			GUI.color = Colors.coral;
			if(GUILayout.Button("ERROR", GUILayout.Height(BUTTON_HEIGHT))) {
				ApplyTestState(AssetsDownloadFlowProgressBar.State.ERROR);
			}

			GUI.color = Colors.paleYellow;
			if(GUILayout.Button("IN PROGRESS", GUILayout.Height(BUTTON_HEIGHT))) {
				ApplyTestState(AssetsDownloadFlowProgressBar.State.IN_PROGRESS);
			}

			GUI.color = Colors.paleGreen;
			if(GUILayout.Button("COMPLETED", GUILayout.Height(BUTTON_HEIGHT))) {
				ApplyTestState(AssetsDownloadFlowProgressBar.State.COMPLETED);
			}

			// Reset color
			GUI.color = Color.white;
		}
		EditorGUILayout.EndHorizontal();
	}

	/// <summary>
	/// Apply a test state to the target.
	/// </summary>
	/// <param name="_state">State used as test.</param>
	private void ApplyTestState(AssetsDownloadFlowProgressBar.State _state) {

		// Apply!
		(target as AssetsDownloadFlowProgressBar).DEBUG_SetState(_state);

	}
}