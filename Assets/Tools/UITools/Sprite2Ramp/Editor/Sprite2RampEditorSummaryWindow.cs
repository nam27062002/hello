// Sprite2RampEditorSummaryWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/06/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Summary window for the Sprite2Ramp tool.
/// </summary>
public class Sprite2RampEditorSummaryWindow : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Layout constants
	private const float SPACING = 2f;
	private const float WINDOW_MARGIN = 10f;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Window instance
	private static Sprite2RampEditorSummaryWindow s_instance = null;
	public static Sprite2RampEditorSummaryWindow instance {
		get {
			if(s_instance == null) {
				s_instance = (Sprite2RampEditorSummaryWindow)EditorWindow.GetWindow(typeof(Sprite2RampEditorSummaryWindow));
				s_instance.titleContent.text = "Sprite2Ramp Summary";
			}
			return s_instance;
		}
	}

	// Editor GUI
	private Vector2 m_scrollPos = Vector2.zero;
	private HashSet<GameObject> m_processedObjects = new HashSet<GameObject>();

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	/// <param name="_processedObjects">List of objects that have been modified.</param>
	public static void OpenWindow(HashSet<GameObject> _processedObjects) {
		// Store params
		instance.m_processedObjects = _processedObjects;

		//instance.Show();
		//instance.ShowUtility();
		//instance.ShowTab();
		//instance.ShowPopup();
		instance.ShowAuxWindow();
	}

	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
        
    }

	/// <summary>
	/// Called 100 times per second on all visible windows.
	/// </summary>
	public void Update() {
		
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Called less times as if it was OnGUI/Update
	/// </summary>
	public void OnInspectorUpdate() {
		
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Scroll Rect
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); {
			// Left Margin
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(WINDOW_MARGIN);

			// Top margin
			EditorGUILayout.BeginVertical();
			GUILayout.Space(WINDOW_MARGIN);

			// DO STUFF!
			// Compose message
			GUIContent messageContent = new GUIContent();
			messageContent.text = "Replacement Done!";

			// Different message if no objects were processed
			if(m_processedObjects.Count == 0) {
				messageContent.text += "\nNo objects were modified.";
			} else {
				messageContent.text += "\n" + m_processedObjects.Count + " objects modified:";
				messageContent.text += "\n";
				foreach(GameObject obj in m_processedObjects) {
					messageContent.text += "\n\t" + obj;
				}
			}

			// Show message
			GUILayout.Label(messageContent);

			// Ok buton
			EditorGUILayout.Space();
			if(GUILayout.Button("OK", GUILayout.Height(30f))) {
				// Close window
				instance.Close();
				GUIUtility.ExitGUI();
			}

			// Bottom margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndVertical();

			// Right margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndHorizontal();
		} EditorGUILayout.EndScrollView();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}