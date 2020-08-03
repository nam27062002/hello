// UIGameObjectToggleEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/03/2019.
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
/// Custom editor for the UIGameObjectToggle class.
/// </summary>
[CustomEditor(typeof(UIGameObjectToggle), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class UIGameObjectToggleEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float BUTTON_HEIGHT = 25f;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	UIGameObjectToggle m_targetUIGameObjectToggle = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUIGameObjectToggle = target as UIGameObjectToggle;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUIGameObjectToggle = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Debug tools
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal(); {
			// ON
			GUI.color = Colors.paleGreen;
			if(GUILayout.Button("ON", GUILayout.Height(BUTTON_HEIGHT))) {
				m_targetUIGameObjectToggle.DEBUG_Apply(UIGameObjectToggle.DebugMode.ON);
			}

			// OFF
			GUI.color = Colors.coral;
			if(GUILayout.Button("OFF", GUILayout.Height(BUTTON_HEIGHT))) {
				m_targetUIGameObjectToggle.DEBUG_Apply(UIGameObjectToggle.DebugMode.OFF);
			}

			// OFF
			GUI.color = Colors.lime;
			if(GUILayout.Button("ALL ON", GUILayout.Height(BUTTON_HEIGHT))) {
				m_targetUIGameObjectToggle.DEBUG_Apply(UIGameObjectToggle.DebugMode.ALL_ON);
			}

			// OFF
			GUI.color = Colors.red;
			if(GUILayout.Button("ALL OFF", GUILayout.Height(BUTTON_HEIGHT))) {
				m_targetUIGameObjectToggle.DEBUG_Apply(UIGameObjectToggle.DebugMode.ALL_OFF);
			}

			// Reset color
			GUI.color = Color.white;
		} EditorGUILayout.EndHorizontal();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}