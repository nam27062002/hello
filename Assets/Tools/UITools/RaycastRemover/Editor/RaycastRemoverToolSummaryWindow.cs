// RaycastRemoverToolSummaryWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/09/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Summary window for Raycast Remover Tool.
/// </summary>
public class RaycastRemoverToolSummaryWindow : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const float WINDOW_MARGIN = 10f;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Window instance
	private static RaycastRemoverToolSummaryWindow m_instance = null;
	public static RaycastRemoverToolSummaryWindow instance {
		get {
			if(m_instance == null) {
				m_instance = (RaycastRemoverToolSummaryWindow)EditorWindow.GetWindow(typeof(RaycastRemoverToolSummaryWindow));
			}
			return m_instance;
		}
	}

	// Setup
	private string m_message = string.Empty;
	private string m_button = string.Empty;

	// Internal
	private Vector2 m_scrollPos = Vector2.zero;

	//------------------------------------------------------------------//
	// WINDOW METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	public static void OpenWindow(string _title, string _message, string _button) {
		// Save config
		instance.titleContent = new GUIContent(_title);
		instance.m_message = _message;
		instance.m_button = _button;

		// Show!
		instance.Show();
		//instance.ShowTab();
		//instance.ShowPopup();
		//instance.ShowUtility();
		//instance.ShowAuxWindow();
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
		// Left Margin
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(WINDOW_MARGIN);

		// Top margin
		EditorGUILayout.BeginVertical();
		GUILayout.Space(WINDOW_MARGIN);

		// Scroll Rect
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
		{
			// Message
			GUILayout.TextArea(m_message, GUILayout.ExpandHeight(true));
		}
		EditorGUILayout.EndScrollView();

		// Ok button
		if(GUILayout.Button(m_button, GUILayout.Height(50f))) {
			// Close window
			this.Close();
		}

		// Bottom margin
		GUILayout.Space(WINDOW_MARGIN);
		EditorGUILayout.EndVertical();

		// Right margin
		GUILayout.Space(WINDOW_MARGIN);
		EditorGUILayout.EndHorizontal();
	}
}