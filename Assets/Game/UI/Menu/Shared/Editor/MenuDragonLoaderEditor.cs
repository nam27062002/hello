// MenuDragonLoaderEditor.cs
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
/// Custom editor for the MenuDragonLoader class.
/// </summary>
[CustomEditor(typeof(MenuDragonLoader))]
public class MenuDragonLoaderEditor : Editor {
	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Force loading the dragon
		if(GUILayout.Button("Load Dragon")) {
			((MenuDragonLoader)target).RefreshDragon();
		}
	}
}
