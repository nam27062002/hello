// MonoBehaviourTemplateEditor.cs
// @projectName
// 
// Created by @author on @dd/@mm/@yyyy.
// Copyright (c) @yyyy Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor window.
/// </summary>
public class TemplateEditorWindow : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const float WINDOW_MARGIN = 10f;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Window instance
	private static TemplateEditorWindow m_instance = null;
	public static TemplateEditorWindow instance {
		get {
			if(m_instance == null) {
				m_instance = (TemplateEditorWindow)EditorWindow.GetWindow(typeof(TemplateEditorWindow));
			}
			return m_instance;
		}
	}

	// Internal
	private Vector2 m_scrollPos = Vector2.zero;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	//[MenuItem("Hungry Dragon/Tools/TemplateEditorWindow")]	// UNCOMMENT TO ADD MENU ENTRY!!!
	public static void OpenWindow() {
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
		// Scroll Rect
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
		{
			// Left Margin
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(WINDOW_MARGIN);

			// Top margin
			EditorGUILayout.BeginVertical();
			GUILayout.Space(WINDOW_MARGIN);

			// Window Content!
			// <-------- WINDOW CONTENT GOES HERE

			// Bottom margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndVertical();

			// Right margin
			GUILayout.Space(WINDOW_MARGIN);
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
	}
}