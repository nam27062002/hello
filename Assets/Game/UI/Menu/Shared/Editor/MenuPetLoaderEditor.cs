// MenuPetLoaderEditor.cs
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
/// Custom editor for the MenuPetLoader class.
/// </summary>
[CustomEditor(typeof(MenuPetLoader))]
public class MenuPetLoaderEditor : Editor {
	//----------------------------------------------------------------------//
	// CONSTANTS															//
	//----------------------------------------------------------------------//
	private const float BUTTON_HEIGHT = 25f;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Tools
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal(); {
			// Clear loaded pet instance
			GUI.color = Colors.coral;
			if(GUILayout.Button("UNLOAD", GUILayout.Height(BUTTON_HEIGHT))) {
				((MenuPetLoader)target).Unload();
			}

			// Force loading the pet
			GUI.color = Colors.paleGreen;
			if(GUILayout.Button("RELOAD", GUILayout.Height(BUTTON_HEIGHT))) {
				((MenuPetLoader)target).Reload();
			}

			// Reset color
			GUI.color = Color.white;
		} EditorGUILayout.EndHorizontal();
	}
}
