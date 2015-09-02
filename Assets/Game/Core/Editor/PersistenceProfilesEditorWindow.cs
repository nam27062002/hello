// PersistenceProfilesEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom inspector window to define different persistence profiles.
/// </summary>
public class PersistenceProfilesEditorWindow : EditorWindow {
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
	/// Add menu item to be able to open the editor.
	/// </summary>
	[MenuItem("Hungry Dragon/Persistence Profiles")]
	public static void ShowWindow() {
		// Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(PersistenceProfilesEditorWindow));
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Let's do it babe!

	}
}