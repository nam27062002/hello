// PetPowerTooltipTriggerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/06/2020.
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
/// Custom editor for the PetPowerTooltipTrigger class.
/// </summary>
[CustomEditor(typeof(PetPowerTooltipTrigger), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class PetPowerTooltipTriggerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private class TooltipProperties {
		public SerializedProperty tooltipInstanceProp = null;
		public SerializedProperty tooltipPrefabProp = null;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private PetPowerTooltipTrigger m_targetPetPowerTooltipTrigger = null;

	// Cached properties
	private TooltipProperties m_defaultTooltipProps = new TooltipProperties();
	private TooltipProperties m_babyTooltipProps = new TooltipProperties();
	private SerializedProperty m_instantiationModeProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetPetPowerTooltipTrigger = target as PetPowerTooltipTrigger;

		// Cache important properties
		m_defaultTooltipProps.tooltipInstanceProp = serializedObject.FindProperty("m_defaultTooltipInstance");
		m_defaultTooltipProps.tooltipPrefabProp = serializedObject.FindProperty("m_defaultPrefabPath");
		
		m_babyTooltipProps.tooltipInstanceProp = serializedObject.FindProperty("m_babyPetTooltipInstance");
		m_babyTooltipProps.tooltipPrefabProp = serializedObject.FindProperty("m_babyPetPrefabPath");
		
		m_instantiationModeProp = serializedObject.FindProperty("m_instantiationMode");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetPetPowerTooltipTrigger = null;

		// Clear cached properties
		m_defaultTooltipProps.tooltipInstanceProp = null;
		m_defaultTooltipProps.tooltipPrefabProp = null;

		m_babyTooltipProps.tooltipInstanceProp = null;
		m_babyTooltipProps.tooltipPrefabProp = null;

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
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Properties we don't want to show or that will be displayed together with another property
			if(p.name == "m_ObjectHideFlags"
			|| p.name == "m_Script"
			|| p.name == "m_tooltip"	// Parent props we're not using
			|| p.name == "m_prefabPath" // Parent props we're not using
			|| p.name == m_defaultTooltipProps.tooltipInstanceProp.name
			|| p.name == m_defaultTooltipProps.tooltipPrefabProp.name
			|| p.name == m_babyTooltipProps.tooltipInstanceProp.name
			|| p.name == m_babyTooltipProps.tooltipPrefabProp.name
			) {
				// Do nothing
			}

			// Other properties requiring special treatment
			else if(p.name == m_instantiationModeProp.name) {
				// Show property
				EditorGUILayout.PropertyField(m_instantiationModeProp, true);
				UITooltipTrigger.TooltipInstantiationMode instantiationMode = (UITooltipTrigger.TooltipInstantiationMode)m_instantiationModeProp.enumValueIndex;

				EditorGUI.indentLevel++;

				// Show instance fields, enabled/disabled based on instantiation mode
				EditorGUI.BeginDisabledGroup(instantiationMode != UITooltipTrigger.TooltipInstantiationMode.INSTANCE);
				EditorGUILayout.PropertyField(m_defaultTooltipProps.tooltipInstanceProp, true);
				EditorGUILayout.PropertyField(m_babyTooltipProps.tooltipInstanceProp, true);
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.Space();
				
				// Show prefab fields, enabled/disabled based on instantiation mode
				EditorGUI.BeginDisabledGroup(instantiationMode != UITooltipTrigger.TooltipInstantiationMode.PREFAB);
				EditorGUILayout.PropertyField(m_defaultTooltipProps.tooltipPrefabProp, true);
				EditorGUILayout.PropertyField(m_babyTooltipProps.tooltipPrefabProp, true);
				EditorGUI.EndDisabledGroup();

				EditorGUI.indentLevel--;
				EditorGUILayout.Space();
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