// ParticlesTrailFXEditor.cs
// @projectName
// 
// Created by @author on 18/10/2017.
// Copyright (c) @yyyy Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the CurrencyTransferFX class.
/// </summary>
[CustomEditor(typeof(ParticlesTrailFX), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class ParticlesTrailFXEditor : Editor {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Casted target object
    ParticlesTrailFX m_targetCurrencyTransferFX = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetCurrencyTransferFX = target as ParticlesTrailFX;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetCurrencyTransferFX = null;
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
			switch(p.name) {
				// Properties requiring special treatment
				// Unity's "script" property
				case "m_Script": {
					// Draw the property, disabled
					bool wasEnabled = GUI.enabled;
					GUI.enabled = false;
					EditorGUILayout.PropertyField(p, true);
					GUI.enabled = wasEnabled;
				} break;

				case "m_totalDuration": {
					EditorGUILayoutExt.Separator("System Setup");
					EditorGUILayout.PropertyField(p, true);
				} break;

				case "m_inRelativeDuration": {
					SerializedProperty outProp = serializedObject.FindProperty("m_outRelativeDuration");
					float minValue = p.floatValue;
					float maxValue = 1f - outProp.floatValue;
					EditorGUILayoutExt.MinMaxSliderWithLabels(new GUIContent("In/Out Distribution"), ref minValue, ref maxValue, 0.01f, 1f);
					p.floatValue = minValue;
					outProp.floatValue = 1f - maxValue;
				} break;

				case "m_speed": {
					EditorGUILayoutExt.Separator("Particle Setup");

					SerializedProperty minProp = p.FindPropertyRelative("min");
					SerializedProperty maxProp = p.FindPropertyRelative("max");
					float minValue = minProp.floatValue;
					float maxValue = maxProp.floatValue;
					GUIContent label = new GUIContent("Speed", "World units per second");
					EditorGUILayoutExt.MinMaxSliderWithLabels(label, ref minValue, ref maxValue, 0.01f, 100f);
					minProp.floatValue = minValue;
					maxProp.floatValue = maxValue;
				} break;

				case "m_rotationSpeed": {
					SerializedProperty minProp = p.FindPropertyRelative("min");
					SerializedProperty maxProp = p.FindPropertyRelative("max");
					float minValue = minProp.floatValue;
					float maxValue = maxProp.floatValue;
					GUIContent label = new GUIContent("Rotation Speed", "Degrees per second, will be randomized for each axis");
					EditorGUILayoutExt.MinMaxSliderWithLabels(label, ref minValue, ref maxValue, 0f, 1080f);
					minProp.floatValue = minValue;
					maxProp.floatValue = maxValue;
				} break;

				case "m_amplitude": {
					SerializedProperty minProp = p.FindPropertyRelative("min");
					SerializedProperty maxProp = p.FindPropertyRelative("max");
					float minValue = minProp.floatValue;
					float maxValue = maxProp.floatValue;
					GUIContent label = new GUIContent("Amplitude", "Percentage of the total distance to go, so the curve aperture is proportional regardless of the distance.");
					EditorGUILayoutExt.MinMaxSliderWithLabels(label, ref minValue, ref maxValue, -1f, 1f);
					minProp.floatValue = minValue;
					maxProp.floatValue = maxValue;
				} break;
				
				// Properties we don't want to show
				case "m_outRelativeDuration":
				case "m_ObjectHideFlags": {
					// Do nothing
				} break;

				// Default property display
				default: {
					EditorGUILayout.PropertyField(p, true);
				} break;
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