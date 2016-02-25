// MenuScreensControllerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MenuScreensController class.
/// </summary>
[CustomEditor(typeof(MenuScreensController))]
public class MenuScreensControllerEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static GUIStyle s_customStyle = null;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Casted target object
	MenuScreensController targetMenuScreensController { get { return target as MenuScreensController; }}

	// Store a reference of interesting properties for faster access
	SerializedProperty m_screensProp = null;
	SerializedProperty m_cameraSnapPointsProp = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Initialize custom styles if not done
		if(s_customStyle == null) {
			// Is style initialized?
			s_customStyle = new GUIStyle(EditorStyles.textField);
			s_customStyle.normal.textColor = Colors.darkGray;
		}

		// Store a reference of interesting properties for faster access
		m_screensProp = serializedObject.FindProperty("m_screens");
		m_cameraSnapPointsProp = serializedObject.FindProperty("m_screensCameraSnapPoints");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Edition screen selector
		EditorGUILayoutExt.Separator();
		EditorGUI.BeginChangeCheck();
		MenuScreensController.Screens screenToEdit = (MenuScreensController.Screens)EditorPrefs.GetInt("MenuEditScreen", 0);
		screenToEdit = (MenuScreensController.Screens)EditorGUILayout.EnumPopup("Screen To Edit", screenToEdit);
		if(EditorGUI.EndChangeCheck()) {
			SetEditingScreen(screenToEdit);
		}

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	/// <summary>
	/// Define the screen to edit.
	/// </summary>
	/// <param name="_screenToEdit">The screen to be edited.</param>
	private void SetEditingScreen(MenuScreensController.Screens _screenToEdit) {
		// Store in editor prefs
		EditorPrefs.SetInt("MenuEditScreen", (int)_screenToEdit);

		// Disable all screens except the target one
		NavigationScreen scr = null;
		for(int i = 0; i < m_screensProp.arraySize; i++) {
			scr = m_screensProp.GetArrayElementAtIndex(i).objectReferenceValue as NavigationScreen;
			if(scr != null) {
				scr.gameObject.SetActive(i == (int)_screenToEdit);
			}
		}

		// Move main camera to screen's snap point (if any)
		if(_screenToEdit != MenuScreensController.Screens.NONE && (int)_screenToEdit < m_cameraSnapPointsProp.arraySize) {
			CameraSnapPoint snapPoint = m_cameraSnapPointsProp.GetArrayElementAtIndex((int)_screenToEdit).objectReferenceValue as CameraSnapPoint;
			if(snapPoint != null) {
				snapPoint.Apply(targetMenuScreensController.camera);
			}
		}
	}
}