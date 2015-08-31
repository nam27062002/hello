// GameCameraControllerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/08/2015.
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
/// Custom editor for the Game Camera class.
/// </summary>
[CustomEditor(typeof(GameCameraController))]
public class GameCameraControllerEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private GameCameraController targetCamera {
		get { return target as GameCameraController; }
	}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// If not running, modify camera's position based on zoom setup
		// During execution the camera logic itself will do it
		if(!Application.isPlaying) {
			targetCamera.gameObject.transform.SetPosZ(-targetCamera.m_zoomRange.Lerp(targetCamera.defaultZoom));
		} else if(Application.isEditor) {
			// Otherwise, add an extra slider to allow "hot" modification of the current zoom level together with some extra info related to the zoom
			GUILayout.BeginVertical("Current Zoom Values", GUI.skin.box);
			GUILayout.Space(20); // spacer to account for the title

			EditorGUI.BeginChangeCheck();
			float newZoom = EditorGUILayout.Slider("Current Zoom", targetCamera.zoom, 0f, 1f);
			if(EditorGUI.EndChangeCheck()) {
				targetCamera.Zoom(newZoom, 0f);
			}

			EditorGUILayout.LabelField("zoom range", targetCamera.m_zoomRange.min.ToString() + ", " + targetCamera.m_zoomRange.max.ToString());
			EditorGUILayout.LabelField("transform Z", targetCamera.transform.position.z.ToString());

			GUILayout.EndVertical();
		}
	}
}