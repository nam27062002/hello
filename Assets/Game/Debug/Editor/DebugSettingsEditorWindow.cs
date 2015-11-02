// DebugSettingsEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom inspector window to define different persistence profiles.
/// </summary>
public class DebugSettingsEditorWindow : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		EditorGUILayout.BeginVertical(); {
			// Some spacing at the start
			GUILayout.Space(10f);

			bool b = EditorGUILayout.Toggle("Test bool", PlayerPrefs.GetInt("test", 0) != 0);
			PlayerPrefs.SetInt("test", b ? 1 : 0);

			// Some spacing at the end
			GUILayout.Space(10f);
		} EditorGUILayout.EndVertical();
	}
}