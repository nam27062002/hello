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
	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Force loading the egg
		if(GUILayout.Button("Load Egg")) {
			((MenuEggLoader)target).Reload();
		}
	}
}
