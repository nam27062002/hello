// UIAspectRatioInitializerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the UIAspectRatioInitializer class.
/// </summary>
[CustomEditor(typeof(UIAspectRatioInitializer), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class UIAspectRatioInitializerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	UIAspectRatioInitializer m_targetUIAspectRatioInitializer = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUIAspectRatioInitializer = target as UIAspectRatioInitializer;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUIAspectRatioInitializer = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Draw default inspector
		DrawDefaultInspector();

		// Manual apply button
		if(GUILayout.Button("APPLY", GUILayout.Height(30f))) {
			m_targetUIAspectRatioInitializer.Apply();
		}
	}
}