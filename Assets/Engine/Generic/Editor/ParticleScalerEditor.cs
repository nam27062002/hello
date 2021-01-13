// ParticleScalerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2017.
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
/// Custom editor for the ParticleScaler class.
/// </summary>
[CustomEditor(typeof(ParticleScaler), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class ParticleScalerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private ParticleScaler m_targetParticleScaler = null;
	private SerializedProperty m_scaleOriginProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetParticleScaler = target as ParticleScaler;
		m_scaleOriginProp = serializedObject.FindProperty("m_scaleOrigin");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetParticleScaler = null;
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

			else if(p.name == "m_scale") {
				// Only for ScaleOrigin.ATTRIBUTE_SCALE
				if(m_scaleOriginProp.enumValueIndex == (int)ParticleScaler.ScaleOrigin.ATTRIBUTE_SCALE) {
					EditorGUILayout.PropertyField(p, true);
				}
			}

			else if(p.name == "m_transform") {
				// Only for ScaleOrigin.TRANSFORM
				if(m_scaleOriginProp.enumValueIndex == (int)ParticleScaler.ScaleOrigin.TRANSFORM_SCALE) {
					EditorGUILayout.PropertyField(p, true);
				}
			}

			else if(p.name == "m_whenScale") {
				// Show a warning message when ALWAYS mode selected
				EditorGUILayout.PropertyField(p, true);
				if(p.enumValueIndex == (int)ParticleScaler.WhenScale.ALWAYS) {
					EditorGUILayout.HelpBox("ALWAYS mode can be very CPU heavy. Use carefully!", MessageType.Warning);
				}


			}

			else if (p.name == "m_framesDelay")
			{
				// Only for AFTER_ENABLE mode

				if (m_targetParticleScaler.m_whenScale == ParticleScaler.WhenScale.AFTER_ENABLE)
				{
					EditorGUILayout.PropertyField(p, true);
				}
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Test button (only in Play Mode)
		GUI.enabled = EditorApplication.isPlaying;
		EditorGUILayout.Space();
		if(GUILayout.Button("APPLY!\n(Only in Play Mode)", GUILayout.Height(35f))) {
			m_targetParticleScaler.DoScale();
		}
		GUI.enabled = true;

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