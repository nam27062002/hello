// RenderQueueSetterEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/11/2017.
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
/// Custom editor for the RenderQueueSetter class.
/// </summary>
[CustomEditor(typeof(RenderQueueSetter), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class RenderQueueSetterEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	RenderQueueSetter m_targetRenderQueueSetter = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetRenderQueueSetter = target as RenderQueueSetter;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetRenderQueueSetter = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Just draw the default inspector
		DrawDefaultInspector();

		// Add a button to apply the changes
		if(GUILayout.Button("Apply", GUILayout.Height (30f))) {
			m_targetRenderQueueSetter.Apply();
		}
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}