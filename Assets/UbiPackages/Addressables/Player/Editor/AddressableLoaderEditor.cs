// AddressableLoaderEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the AddressableLoader class.
/// </summary>
[CustomEditor(typeof(AddressableLoader), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class AddressableLoaderEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float BUTTON_HEIGHT = 30f;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	AddressableLoader m_targetAddressableLoader = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetAddressableLoader = target as AddressableLoader;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetAddressableLoader = null;
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

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// Editor tools
		EditorGUILayoutExt.Separator("Debug Tools (only in play mode)");
		EditorGUI.BeginDisabledGroup(!Application.isPlaying); {
			// Load/Unload buttons
			EditorGUILayout.BeginHorizontal(); {
				// Load (sync)
				if(GUILayout.Button("LOAD\n(sync)", GUILayout.Height(BUTTON_HEIGHT))) {
					bool wasAsync = m_targetAddressableLoader.async;
					m_targetAddressableLoader.async = true;
					m_targetAddressableLoader.Load(true);
					m_targetAddressableLoader.async = wasAsync;
				}

				// Load (async)
				if(GUILayout.Button("LOAD\n(async)", GUILayout.Height(BUTTON_HEIGHT))) {
					bool wasAsync = m_targetAddressableLoader.async;
					m_targetAddressableLoader.async = false;
					m_targetAddressableLoader.Load(true);
					m_targetAddressableLoader.async = wasAsync;
				}

				// Unload
				if(GUILayout.Button("UNLOAD", GUILayout.Height(BUTTON_HEIGHT))) {
					m_targetAddressableLoader.Unload();
				}
			} EditorGUILayout.EndHorizontal();
		} EditorGUI.EndDisabledGroup();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}