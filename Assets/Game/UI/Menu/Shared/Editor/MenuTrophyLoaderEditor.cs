// MenuTrophyLoaderEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MenuTrophyLoader class.
/// </summary>
[CustomEditor(typeof(MenuTrophyLoader))]
public class MenuTrophyLoaderEditor : Editor {
	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Force loading the pet
		if(GUILayout.Button("Load Trophy")) {
			MenuTrophyLoader targetLoader = target as MenuTrophyLoader;
			targetLoader.Load(targetLoader.leagueSku);
		}
	}
}
