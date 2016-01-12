﻿// DragonManagerEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// External window to setup the dragon manager's content.
/// </summary>
public class DragonManagerEditorWindow : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly float COLUMN_WIDTH = 350f;
	private static readonly float TOOLBAR_WIDTH = 100f;

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public SerializedObject m_target;
	private Vector2 m_scrollPos = Vector2.zero;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Constructor
	/// </summary>
	public DragonManagerEditorWindow() {
		// Nothing to do
	}

	/// <summary>
	/// Update window's view.
	/// </summary>
	public void OnGUI() {
		// If the target is not valid, close the window (probably window was kept open from another session)
		if(m_target == null) {
			this.Close();
			return;
		}

		// Group everything into a scroll view
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, false, false); {
			// We want to display dragons horizontally for easier comparison
			EditorGUILayout.BeginHorizontal(); {
				// Display current dragons
				// Force exactly 1 dragon per DragonId
				SerializedProperty dragonsArrayProp = m_target.FindProperty("m_dragons");
				dragonsArrayProp.arraySize = DefinitionsManager.dragons.Count;	// This will delete any unnecessary entries or add any missing entries
				for(int i = 0; i < dragonsArrayProp.arraySize; i++) {
					// Vertical layout to group everything on this column
					EditorGUILayout.BeginVertical(GUILayout.Width(COLUMN_WIDTH)); {	// A fixed width is more suitable in this case
						// Space above
						GUILayout.Space(10);

						// Get the data for this dragon ID
						SerializedProperty dragonProp = dragonsArrayProp.GetArrayElementAtIndex(i);

						// Make sure it has the right ID assigned
						// [AOC] Tricky: The serialized property expects the real index of the enum entry [0..N-1], we can't use 
						//		 the DragonId vaule directly since it's [-1..N]. Luckily, C# gives us the tools to do so via the enum name.
						// [AOC] Remove for now, DragonId exists no more
						//SerializedProperty idProp = dragonProp.FindPropertyRelative("m_id");
						//idProp.enumValueIndex = Array.IndexOf(idProp.enumNames, ((DragonId)i).ToString());

						// Default dragon drawer
						EditorGUILayout.PropertyField(dragonProp, GUIContent.none, true);

						// Space below
						GUILayout.Space(10);
					} EditorGUILayout.EndVertical();

					// Draw a vertical separator to the next dragon
					GUILayout.Space(5);
					GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));
					GUILayout.Space(5);
				}

				// Extra column for extended functions
				EditorGUILayout.BeginVertical(GUILayout.Width(TOOLBAR_WIDTH), GUILayout.ExpandHeight(true)); {
					GUILayout.Space(10);

					// Add button to save & close
					if(GUILayout.Button("Close", GUILayout.Height(50))) {
						// Make sure all changes are saved
						m_target.ApplyModifiedProperties();

						// Close the window
						this.Close();
					}
				} EditorGUILayout.EndVertical();
			} EditorGUILayout.EndHorizontal();
		} EditorGUILayout.EndScrollView();

		// Save any change performed
		m_target.ApplyModifiedProperties();
	}
}