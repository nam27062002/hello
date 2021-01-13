// MenuTransitionManagerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Text;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MenuTransitionManager class.
/// </summary>
[CustomEditor(typeof(MenuTransitionManager), true)]	// True to be used by heir classes as well
public class MenuTransitionManagerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	MenuTransitionManager m_targetMenuTransitionManager = null;

	// Important properties
	SerializedProperty m_screensProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetMenuTransitionManager = target as MenuTransitionManager;

		// Gather important properties
		m_screensProp = serializedObject.FindProperty("m_screens");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetMenuTransitionManager = null;

		// Clear properties
		m_screensProp = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Unity's "script" property - draw disabled
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
		EditorGUI.EndDisabledGroup();

		// Debug tools
		EditorGUILayoutExt.Separator("DEBUG");
		if(GUILayout.Button("DUMP TRANSITIONS", GUILayout.Height(30f))) {
			DumpTransitions();
		}

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Properties we don't want to show
			if(p.name == "m_ObjectHideFlags" || p.name == "m_Script") {
				// Do nothing
			}

			// Screens list
			else if(p.name == m_screensProp.name) {
				// Make sure both screens and scenes array properties have exactly one entry per menu screen
				m_screensProp.arraySize = (int)MenuScreen.COUNT;

				// Group in a foldout
				EditorGUILayout.Space();
				m_screensProp.isExpanded = EditorGUILayout.Foldout(m_screensProp.isExpanded, "Screens");
				if(m_screensProp.isExpanded) {
					// Indent in
					EditorGUI.indentLevel++;

					// Show them nicely formatted
					for(int i = 0; i < (int)MenuScreen.COUNT; i++) {
						// Default drawer, using screen's name as label
						SerializedProperty currentScreenProp = m_screensProp.GetArrayElementAtIndex(i);
						EditorGUILayout.PropertyField(currentScreenProp, new GUIContent(((MenuScreen)i).ToString()), true);

						// Make sure screen ID is properly assigned
						SerializedProperty idProp = currentScreenProp.FindPropertyRelative("screenId");
						idProp.intValue = i;
					}

					// Indent out
					EditorGUI.indentLevel--;
				}
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Print all transitions defined.
	/// </summary>
	private void DumpTransitions() {
		// [AOC] FEEL FREE TO CUSTOMIZE PRINTED INFO
		StringBuilder sb = new StringBuilder();
		sb.AppendLine(Colors.magenta.Tag("Transitions with overriden duration:"));

		// Go through all screens
		for(int i = 0; i < m_targetMenuTransitionManager.screens.Length; ++i) {
			// Get screen data
			ScreenData data = m_targetMenuTransitionManager.screens[i];
			sb.AppendLine(data.screenId.ToString());

			// Go through all transitions defined for this screen
			foreach(Transition t in data.transitions) {
				// Only print if duration is overriden
				if(t.overrideDuration) {
					sb.Append("\t").Append(t.destination.ToString()).AppendLine();
					sb.Append("\t\t").Append(t.duration).AppendLine();
				}
			}
		}

		// Print!
		Debug.Log(sb.ToString());
	}
}