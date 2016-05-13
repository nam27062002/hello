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

	//------------------------------------------------------------------------//
	// STATICS																  //
	//------------------------------------------------------------------------//
	// Windows instance
	private static HungryDragonEditorToolbar m_instance = null;
	public static HungryDragonEditorToolbar instance {
		get {
			if(m_instance == null) {
				m_instance = (HungryDragonEditorToolbar)EditorWindow.GetWindow(typeof(HungryDragonEditorToolbar), false, "Hungry Dragon Toolbar", true);
			}
			return m_instance;
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
		int numButtons = 7;		// Update as needed
		int numSeparators = 1;	// Update as needed
		instance.minSize = new Vector2(2 * MARGIN + BUTTON_SIZE * numButtons + SEPARATOR_SIZE * numSeparators, EditorStyles.toolbarButton.lineHeight + 2 * MARGIN + 5f);	// Enough room for all the buttons + spacings in a single row
		instance.maxSize = new Vector2(float.PositiveInfinity, instance.minSize.y);	// Fixed height

		// Show it
		instance.ShowTab();
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
			if(GUILayout.Button("L", EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene1();
			}

			// Menu
			if(GUILayout.Button("M", EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene2();
			}

			// Game
			if(GUILayout.Button("G", EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene3();
			}

			// Level Editor
			if(GUILayout.Button("LE", EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene4();
			}

			// Popups
			if(GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
				HungryDragonEditorMenu.OpenScene5();
			}

			// Add a separator
			GUILayout.Space(SEPARATOR_SIZE);

			// Some more dummy buttons
			for(int i = 0; i < 2; i++) {
				// Button
				if(GUILayout.Button(i.ToString(), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SIZE))) {
					Debug.Log("Dummy button " + i + " pressed!");
				}
			}

			// Right margin
			GUILayout.Space(MARGIN);
		} EditorGUILayoutExt.EndHorizontalSafe();

		// Bottom margin
		GUILayout.Space(MARGIN);
	}
}