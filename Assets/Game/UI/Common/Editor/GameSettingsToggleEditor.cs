// GameSettingsToggleEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

using System.Reflection;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the GameSettingsToggle class.
/// </summary>
[CustomEditor(typeof(GameSettingsToggle), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class GameSettingsToggleEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	GameSettingsToggle m_targetGameSettingsToggle = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetGameSettingsToggle = target as GameSettingsToggle;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetGameSettingsToggle = null;
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

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Unity's "script" property
			if(p.name == "m_Script") {
				// Draw the property, disabled
				bool wasEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.PropertyField(p, true);
				GUI.enabled = wasEnabled;
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags") {
				// Do nothing
			}

			// Setting ID: choose from a specific list
			else if(p.name == "m_settingId") {
				// Use reflection to grab all constants in GameSettings
				FieldInfo[] fields = typeof(GameSettings).GetFields(
					BindingFlags.Public |
					BindingFlags.Static | 
					BindingFlags.FlattenHierarchy
				);

				// Filter those with type string and find out current value
				int selectedIdx = 0;
				List<string> optionsList = new List<string>();	// Displayed options
				List<string> valuesList = new List<string>();
				for(int i = 0; i < fields.Length; i++) {
					// We only care about strings
					if(fields[i].FieldType == typeof(string)) {
						// Add label to the options list
						optionsList.Add(fields[i].Name);

						// Add value to the list
						string value = fields[i].GetValue(null) as string;
						valuesList.Add(value);

						// Is it the current value?
						if(value == p.stringValue) {
							selectedIdx = optionsList.Count - 1;
						}
					}
				}

				// Do a popup field
				selectedIdx = EditorGUILayout.Popup(p.displayName, selectedIdx, optionsList.ToArray());

				// Store new value
				p.stringValue = valuesList[selectedIdx];
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
}