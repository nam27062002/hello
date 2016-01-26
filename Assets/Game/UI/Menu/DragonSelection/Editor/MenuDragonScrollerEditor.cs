// MenuDragonScrollerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/11/2015.
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
/// Custom editor for the MenuDragonScroller class.
/// </summary>
[CustomEditor(typeof(MenuDragonScroller))]
[CanEditMultipleObjects]
public class MenuDragonScrollerEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	MenuDragonScroller targetScroller { get { return target as MenuDragonScroller; }}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private bool m_liveModeEnabled = true;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Load prefs
		m_liveModeEnabled = EditorPrefs.GetBool("MenuDragonScrollerLiveModeEnabled");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Save prefs
		EditorPrefs.SetBool("MenuDragonScrollerLiveModeEnabled", m_liveModeEnabled);
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		GUI.changed = false;
		DrawDefaultInspector();

		EditorGUILayout.Space();

		// Rearrange section
		EditorGUILayout.BeginHorizontal(); {
			// Live preview toggle
			m_liveModeEnabled = EditorGUILayout.Toggle("Live Mode", m_liveModeEnabled, GUILayout.ExpandWidth(false));

			// If live mode enabled and there were changes, rearrange dragons automatically
			if(m_liveModeEnabled) {
				if(GUI.changed) targetScroller.RearrangeDragons();
			}

			// If not enabled, show button to manually re-arrange dragons
			else if(GUILayout.Button("Rearrange Dragons")) {
				targetScroller.RearrangeDragons();
			}
		} EditorGUILayoutExt.EndHorizontalSafe();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}