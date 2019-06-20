// Sprite2RampEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/06/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main window for the Sprite2Ramp tool.
/// </summary>
public class Sprite2RampEditor : EditorWindow {
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
	private static Sprite2RampEditor s_instance = null;
	public static Sprite2RampEditor instance {
		get {
			if(s_instance == null) {
				s_instance = (Sprite2RampEditor)EditorWindow.GetWindow(typeof(Sprite2RampEditor));
				s_instance.titleContent.text = "Sprite2Ramp";
			}
			return s_instance;
		}
	}

	// Editor GUI
	private Vector2 m_scrollPos = Vector2.zero;
	private Sprite m_toReplaceSprite = null;
	private Sprite m_grayscaleBaseSprite = null;
	private Texture2D m_gradientTex = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	[MenuItem("Tools/Sprite2Ramp", false, 301)]
	public static void OpenWindow() {
		instance.Show();
		//instance.ShowUtility();
		//instance.ShowTab();
		//instance.ShowPopup();
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
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos); {
			// Left Margin
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(WINDOW_MARGIN);

			// Top margin
			EditorGUILayout.BeginVertical();
			GUILayout.Space(WINDOW_MARGIN);

			// DO STUFF!
			// Sprite to replace
			m_toReplaceSprite = EditorGUILayout.ObjectField("To Replace", m_toReplaceSprite, typeof(Sprite), true) as Sprite;

			// Replacements
			EditorGUILayout.Space();
			//m_grayscaleBaseSprite = EditorGUILayout.ObjectField("Grayscale Base Sprite", m_grayscaleBaseSprite, typeof(Sprite), true) as Sprite);
			//m_gradientTex = EditorGUILayout.ObjectField("Gradient Texture", m_gradientTex, typeof(Texture2D), true) as Texture2D;


			// Button
			EditorGUILayout.Space();
			if(GUILayout.Button("REPLACE!!")) {
				// Perform the replacement!
				DoReplace();
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
	/// <summary>
	/// Perform the replacement in all open sceen!
	/// </summary>
	private void DoReplace() {

	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}