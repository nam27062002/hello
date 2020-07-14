// UITooltipTriggerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the UITooltipTrigger class.
/// </summary>
[CustomEditor(typeof(UITooltipTrigger), true)]    // True to be used by heir classes as well
[CanEditMultipleObjects]
public class UITooltipTriggerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private UITooltipTrigger m_targetUITooltipTrigger = null;

	// Cached properties
	private SerializedProperty m_tooltipInstanceProp = null;
	private SerializedProperty m_tooltipPrefabProp = null;
	private SerializedProperty m_instantiationModeProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetUITooltipTrigger = target as UITooltipTrigger;

		// Cache important properties
		m_tooltipInstanceProp = serializedObject.FindProperty("m_tooltip");
		m_tooltipPrefabProp = serializedObject.FindProperty("m_prefabPath");
		m_instantiationModeProp = serializedObject.FindProperty("m_instantiationMode");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetUITooltipTrigger = null;

		// Clear cached properties
		m_tooltipInstanceProp = null;
		m_tooltipPrefabProp = null;
		m_instantiationModeProp = null;
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

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);   // To get first element
		do {
			// Properties requiring special treatment
			// Properties we don't want to show or that will be displayed together with another property
			if(p.name == "m_ObjectHideFlags"
			|| p.name == "m_Script"
			|| p.name == m_tooltipInstanceProp.name
			|| p.name == m_tooltipPrefabProp.name
			) {
				// Do nothing
			}

			// Other properties requiring special treatment
			else if(p.name == m_instantiationModeProp.name) {
				// Show property
				EditorGUILayout.PropertyField(m_instantiationModeProp, true);
				UITooltipTrigger.TooltipInstantiationMode instantiationMode = (UITooltipTrigger.TooltipInstantiationMode)m_instantiationModeProp.enumValueIndex;

				// Show instance field, enabled/disabled based on instantiation mode
				EditorGUI.BeginDisabledGroup(instantiationMode != UITooltipTrigger.TooltipInstantiationMode.INSTANCE);
				EditorGUILayout.PropertyField(m_tooltipInstanceProp, true);
				EditorGUI.EndDisabledGroup();

				// Show prefab field, enabled/disabled based on instantiation mode
				EditorGUI.BeginDisabledGroup(instantiationMode != UITooltipTrigger.TooltipInstantiationMode.PREFAB);
				EditorGUILayout.PropertyField(m_tooltipPrefabProp, true);
				EditorGUI.EndDisabledGroup();
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));      // Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

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