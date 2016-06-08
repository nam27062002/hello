// AlignAndDistribute.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom toolbar for Hungry Dragon project.
/// </summary>
public class HungryDragonEditorToolbar : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static readonly float MARGIN = 5f;
	private static readonly float BUTTON_SIZE = 25f;
	private static readonly float SEPARATOR_SIZE = 10f;

	private static readonly int NUM_BUTTONS = 7;		// Update as needed
	private static readonly int NUM_SEPARATORS = 1;		// Update as needed

	//------------------------------------------------------------------------//
	// STATICS																  //
	//------------------------------------------------------------------------//
	// Windows instance
	//private static HungryDragonEditorToolbar m_instance = null;
	public static HungryDragonEditorToolbar instance {
		get {
			/*if(m_instance == null) {
				m_instance = (HungryDragonEditorToolbar)EditorWindow.GetWindow(typeof(HungryDragonEditorToolbar), false, "Hungry Dragon Toolbar", true);
			}
			return m_instance;
			*/
			return (HungryDragonEditorToolbar)EditorWindow.GetWindow(typeof(HungryDragonEditorToolbar), false, "Hungry Dragon Toolbar", true);
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the window.
	/// </summary>
	public static void ShowWindow() {
		// Setup window
		instance.titleContent = new GUIContent("Hungry Dragon Toolbar");
		instance.minSize = new Vector2(2 * MARGIN + BUTTON_SIZE * NUM_BUTTONS + SEPARATOR_SIZE * NUM_SEPARATORS, EditorStyles.toolbarButton.lineHeight + 2 * MARGIN + 5f);	// Enough room for all the buttons + spacings in a single row
		instance.maxSize = new Vector2(float.PositiveInfinity, instance.minSize.y);	// Fixed height

		// Show it
		instance.ShowTab();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Although singletons shouldn't be destroyed, Unity may want to destroy the window when reloading the layout
		//m_instance = null;
	}

	/// <summary>
	/// Creates custom GUI styles if not already done.
	/// Must only be called from the OnGUI() method.
	/// </summary>
	private void InitStyles() {
		// TODO!!
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Make sure styles are initialized - must be done in the OnGUI call
		InitStyles();

		// Top margin
		GUILayout.Space(MARGIN);

		// Group in an horizontal layout
		EditorGUILayout.BeginHorizontal(); {
			// Left margin
			GUILayout.Space(MARGIN);

			// Add scene buttons
			// Loading
			if(GUILayout.Button(new GUIContent("L", "Loading Scene"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene1();
			}

			// Menu
			if(GUILayout.Button(new GUIContent("M", "Menu Scene"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene2();
			}

			// Game
			if(GUILayout.Button(new GUIContent("G", "Game Scene"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene3();
			}

			// Level Editor
			if(GUILayout.Button(new GUIContent("LE", "Level Editor"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene4();
			}

			// Popups
			if(GUILayout.Button(new GUIContent("P", "Popups Scene"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene5();
			}

			// Add a separator
			GUILayout.Space(SEPARATOR_SIZE);

			// Some more dummy buttons
			for(int i = 0; i < 2; i++) {
				// Button
				if(GUILayout.Button(new GUIContent(i.ToString(), "Dummy button " + i), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
					Debug.Log("Dummy button " + i + " pressed!");
				}
			}

			// Right margin
			GUILayout.Space(MARGIN);
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Bottom margin
		GUILayout.Space(MARGIN);
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Called less times as if it was OnGUI/Update
	/// </summary>
	public void OnInspectorUpdate() {
		// Force repainting to update with current selection
		Repaint();
	}
}