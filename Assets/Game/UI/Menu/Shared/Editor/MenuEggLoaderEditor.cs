// MenuEggLoaderEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MenuEggLoader class.
/// </summary>
[CustomEditor(typeof(MenuEggLoader))]
public class MenuEggLoaderEditor : Editor {
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
		EditorGUILayout.BeginHorizontal(); {
			// Clear loaded egg instance
			GUI.color = Colors.coral;
			if(GUILayout.Button("UNLOAD", GUILayout.Height(BUTTON_HEIGHT))) {
				((MenuEggLoader)target).Unload();
			}

			// Force loading the egg
			GUI.color = Colors.paleGreen;
			if(GUILayout.Button("RELOAD", GUILayout.Height(BUTTON_HEIGHT))) {
				((MenuEggLoader)target).Reload();
			}

			// Reset color
			GUI.color = Color.white;
		} EditorGUILayout.EndHorizontal();
	}
}
