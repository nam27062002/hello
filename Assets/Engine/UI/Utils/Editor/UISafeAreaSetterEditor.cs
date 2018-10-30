// UISafeAreaSetterEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/10/2018.
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
/// Custom editor for the UISafeAreaSetter class.
/// </summary>
[CustomEditor(typeof(UISafeAreaSetter), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class UISafeAreaSetterEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	UISafeAreaSetter m_targetUISafeAreaSetter = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUISafeAreaSetter = target as UISafeAreaSetter;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUISafeAreaSetter = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Do default inspector
		DrawDefaultInspector();

		// Add debug buttons
#if DEBUG
		// Only in play mode!
		EditorGUI.BeginDisabledGroup(!Application.isPlaying); {
			if(GUILayout.Button("Apply (Play mode only)")) {
				m_targetUISafeAreaSetter.Apply();
			}

			if(GUILayout.Button("Backup Original Values (Play mode only)")) {
				m_targetUISafeAreaSetter.BackupOriginalValues();
			}

			if(GUILayout.Button("Restore Original Values (Play mode only)")) {
				m_targetUISafeAreaSetter.RestoreOriginalValues();
			}
		}
#endif
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}