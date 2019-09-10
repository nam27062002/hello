// MenuDragonsTestEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MenuDragonsTest class.
/// </summary>
[CustomEditor(typeof(MenuDragonsTest), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MenuDragonsTestEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private MenuDragonsTest m_targetMenuDragonsTest = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetMenuDragonsTest = target as MenuDragonsTest;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetMenuDragonsTest = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		EditorGUILayoutExt.Separator();

		// Debug GUI
		DoDebugGUI(m_targetMenuDragonsTest);
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	/// <summary>
	/// 
	/// </summary>
	private static void DoDebugGUI(MenuDragonsTest _target) {
		// Navigation Buttons
		EditorGUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("<-", GUILayout.Height(30f))) {
				_target.FocusPreviousDragon();
			}

			if(GUILayout.Button("->", GUILayout.Height(30f))) {
				_target.FocusNextDragon();
			}
		}
		EditorGUILayout.EndHorizontal();

		// Edition buttons
		EditorGUILayout.BeginHorizontal();
		{
			GUI.color = Colors.paleYellow;
			if(GUILayout.Button("Reset Scales", GUILayout.Height(30f))) {
				_target.ResetScales();
			}

			GUI.color = Colors.skyBlue;
			if(GUILayout.Button("Apply Curve", GUILayout.Height(30f))) {
				_target.ApplyCurve();
			}
		}
		EditorGUILayout.EndHorizontal();

		// Save button
		EditorGUILayout.Space();
		GUI.color = Colors.paleGreen;
		EditorGUI.BeginDisabledGroup(Application.isPlaying);
		if(GUILayout.Button("SAVE PREFABS" + (Application.isPlaying ? "\n(EDIT MODE ONLY)" : ""), GUILayout.Height(30f))) {
			foreach(MenuDragonPreview dragon in _target.m_dragons) {
				PrefabUtility.ReplacePrefab(
					dragon.gameObject,
					PrefabUtility.GetPrefabParent(dragon.gameObject),
					ReplacePrefabOptions.ConnectToPrefab
				);
			}
		}
		EditorGUI.EndDisabledGroup();
		GUI.color = Color.white;

		// Final space
		EditorGUILayout.Space();
	}
}