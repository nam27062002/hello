// FontManagerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/03/2018.
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
/// Custom editor for the FontManager class.
/// </summary>
[CustomEditor(typeof(FontManager), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class FontManagerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	FontManager m_targetFontManager = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetFontManager = target as FontManager;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetFontManager = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Draw default inspector
		DrawDefaultInspector();

		// Add some tools
		EditorGUILayoutExt.Separator();

		// Dump button
		EditorGUI.BeginDisabledGroup(!Application.isPlaying); {
			if(GUILayout.Button("Dump Font Manager", GUILayout.Height(50f))) {
				m_targetFontManager.Dump();
			}
		} EditorGUI.EndDisabledGroup();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}