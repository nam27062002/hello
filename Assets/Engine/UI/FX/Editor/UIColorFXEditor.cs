// UIColorFXEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/05/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// PREPROCESSOR																  //
//----------------------------------------------------------------------------//
//#define ALLOW_RAMP_INTENSITY   // [AOC] Disable to make it more optimal by just getting the full value from the gradient

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the UIColorFX class.
/// </summary>
[CustomEditor(typeof(UIColorFX), true)]	// True to be used by heir classes as well
//[CanEditMultipleObjects]
public class UIColorFXEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private UIColorFX m_targetUIColorFX = null;

	// Important properties
	private SerializedProperty m_colorRampEnabledProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUIColorFX = target as UIColorFX;

		// Get important properties
		m_colorRampEnabledProp = serializedObject.FindProperty("m_colorRampEnabled");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUIColorFX = null;
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
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.PropertyField(p, true);
				EditorGUI.EndDisabledGroup();
			}

			// Color Ramp group
			else if(p.name == m_colorRampEnabledProp.name) {
				// Enabled toggle
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(p);
				if(EditorGUI.EndChangeCheck()) {
					// Toggle changed, force a reload of the material to the one using the right shader variant
					m_targetUIColorFX.DestroyMaterials();
				}

				// Disabled if ramp is not enabled
				EditorGUI.BeginDisabledGroup(!m_colorRampEnabledProp.boolValue);

				// Ramp texture
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_colorRamp"));

				// Intensity
#if ALLOW_RAMP_INTENSITY
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_colorRampIntensity"));
#endif

				// Shortcut to ramps editor
				// Uncomment only if you have the ColorRampEditor tool imported in your project
				// You can download it at https://bcn-mb-git/tools/color_ramp_editor
				if(GUILayout.Button("Color Ramps Editor")) {
					ColorRampEditor.OpenWindow();
				}

				EditorGUI.EndDisabledGroup();
			}

			// Properties to be ignored
			else if(p.name == "m_ObjectHideFlags"
				 || p.name == "m_colorRamp"
				 || p.name == "m_colorRampIntensity") {
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