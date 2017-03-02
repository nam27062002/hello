// DragControlEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the DragControl class.
/// </summary>
[CustomEditor(typeof(DragControl), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class DragControlEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	DragControl m_targetDragControl = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetDragControl = target as DragControl;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetDragControl = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Aux vars
		bool wasEnabled = GUI.enabled;

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
				GUI.enabled = false;
				EditorGUILayout.PropertyField(p, true);
				GUI.enabled = wasEnabled;
			}

			// Axis toggles
			else if(p.name == "m_axisEnabled") {
				// Fixed length! One element per axis
				p.arraySize = 2;
				for(int i = 0; i < 2; i++) {
					EditorGUILayout.PropertyField(
						p.GetArrayElementAtIndex(i),
						new GUIContent(i == 0 ? "Horizontal" : "Vertical"),
						true
					);
				}
			}

			// Inertia toggle
			else if(p.name == "m_inertia") {
				EditorGUILayout.Space();
				EditorGUILayoutExt.TogglePropertyField(p, serializedObject.FindProperty("m_inertiaAcceleration"), p.displayName);
			}

			// Clamp setup
			else if(p.name == "m_clampSetup") {
				// Fixed length! One element per axis
				EditorGUILayout.Space();
				p.arraySize = 2;

				// Toggle group per axis
				for(int i = 0; i < 2; i++) {
					SerializedProperty clampProp = p.GetArrayElementAtIndex(i);
					SerializedProperty toggleProp = clampProp.FindPropertyRelative("clamp");
					toggleProp.boolValue = EditorGUILayout.BeginToggleGroup("Clamp " + (i == 0 ? "X" : "Y"), toggleProp.boolValue); {
						EditorGUI.indentLevel++;
						EditorGUILayout.PropertyField(clampProp.FindPropertyRelative("range"), true);
						EditorGUILayout.PropertyField(clampProp.FindPropertyRelative("mode"), true);
						EditorGUI.indentLevel--;
					} EditorGUILayout.EndToggleGroup();
				}
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags") {
				// Do nothing
			}

			// Properties to ignore (have already been processed together with other properties)
			else if(p.name == "m_inertiaAcceleration"
				|| p.name == "m_clampSetup"
				|| p.name == "m_axisEnabled") {
				// Do nothing
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