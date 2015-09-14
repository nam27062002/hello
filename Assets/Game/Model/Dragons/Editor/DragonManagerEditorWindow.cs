// DragonManagerEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// External window to setup the dragon manager's content.
/// </summary>
public class DragonManagerEditorWindow : EditorWindow {
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
	/// Show existing window instance. If one doesn't exist, make one.
	/// </summary>
	public static void ShowWindow(SerializedObject _target) {
		DragonManagerEditorWindow window = (DragonManagerEditorWindow)EditorWindow.GetWindow(typeof(DragonManagerEditorWindow));
		window.m_target = _target;
		window.titleContent = new GUIContent("Dragon Manager Editor");
		window.ShowUtility();	// To avoid window getting automatically closed when losing focus
	}

	/// <summary>
	/// Update window's view.
	/// </summary>
	public void OnGUI() {
		// Group everything into a scroll view
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, false, false);

		// We want to display dragons horizontally for easier comparison
		EditorGUILayout.BeginHorizontal();

		// Display current dragons
		SerializedProperty dragonsProperty = m_target.FindProperty("m_dragons");
		for(int i = 0; i < dragonsProperty.arraySize; i++) {
			// Vertical layout to group everything on this column
			EditorGUILayout.BeginVertical(GUILayout.Width(350));	// A fixed width is more suitable in this case
			GUILayout.Space(10);

			// Add a button to remove this entry (except if we only have one dragon, we want to have at least one dragon)
			if(dragonsProperty.arraySize > 1) {
				if(GUILayout.Button("Remove Dragon", GUILayout.Height(25))) {
					// Do it! remove this entry
					// [AOC] TODO!! Add a confirmation popup
					dragonsProperty.DeleteArrayElementAtIndex(i);

					// Stop iterating
					break;
				}
			}

			// Get dragon data
			SerializedProperty dragonProperty = dragonsProperty.GetArrayElementAtIndex(i);

			// Replace array's "Element X" by dragon ID
			SerializedProperty idProperty = dragonProperty.FindPropertyRelative("m_id");
			GUIContent label = new GUIContent(idProperty.enumDisplayNames[idProperty.enumValueIndex]);

			// Default dragon drawer
			EditorGUILayout.PropertyField(dragonProperty, label, true);

			// Done
			GUILayout.Space(10);
			EditorGUILayout.EndVertical();

			// Draw a separator
			GUILayout.Space(5);
			GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));
			GUILayout.Space(5);
		}

		// Extra column for extended functions
		GUILayout.BeginVertical(GUILayout.Width(100), GUILayout.ExpandHeight(true));
		GUILayout.Space(10);

		// Add button to create a new dragon entry
		if(GUILayout.Button("Add Dragon", GUILayout.Height(50))) {
			// [AOC] Append a new dragon at the end of the array
			dragonsProperty.InsertArrayElementAtIndex(dragonsProperty.arraySize);

			// Scroll to the last column
			m_scrollPos.y = 0;
			m_scrollPos.x = float.PositiveInfinity;
		}

		// Add button to save & close
		if(GUILayout.Button("Close", GUILayout.Height(50))) {
			// Make sure all changes are saved
			m_target.ApplyModifiedProperties();

			// Close the window
			this.Close();
		}

		// Done
		GUILayout.EndVertical();

		// We're done!
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();

		// Save any change performed
		m_target.ApplyModifiedProperties();
	}
}