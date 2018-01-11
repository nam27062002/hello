// DragonSelectionTutorialEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/12/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the DragonSelectionTutorial class.
/// </summary>
[CustomEditor(typeof(DragonSelectionTutorial), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class DragonSelectionTutorialEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	DragonSelectionTutorial m_targetDragonSelectionTutorial = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetDragonSelectionTutorial = target as DragonSelectionTutorial;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetDragonSelectionTutorial = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Add a test button to trigger the tutorial
		EditorGUILayout.Space();
		GUI.enabled = !m_targetDragonSelectionTutorial.isPlaying;
		if(GUILayout.Button("TEST", GUILayout.Height(50f))) {
			m_targetDragonSelectionTutorial.StartTutorial();
		}
		GUI.enabled = true;
		EditorGUILayout.Space();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}