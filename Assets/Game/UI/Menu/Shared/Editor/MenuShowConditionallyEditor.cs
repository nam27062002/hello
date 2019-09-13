// MenuShowConditionallyEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/03/2017.
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
/// Custom editor for the MenuShowConditionally class.
/// </summary>
[CustomEditor(typeof(MenuShowConditionally), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class MenuShowConditionallyEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private MenuShowConditionally m_targetMenuShowConditionally = null;

	// Serialized properties
	private SerializedProperty m_scriptProp = null;
	private SerializedProperty m_targetAnimatorProp = null;

	private SerializedProperty m_restartAnimProp = null;
	private SerializedProperty m_showDelayProp = null;
	private SerializedProperty m_hideDelayProp = null;

	private SerializedProperty m_checkSelectedDragonProp = null;
	private SerializedProperty m_hideForDragonsProp = null;
    private SerializedProperty m_showOnlyForType = null;
    private SerializedProperty m_targetDragonSkuProp = null;
	private SerializedProperty m_showIfShadowProp = null;
	private SerializedProperty m_showIfLockedProp = null;
	private SerializedProperty m_showIfAvailableProp = null;
	private SerializedProperty m_showIfOwnedProp = null;

	private SerializedProperty m_checkScreensProp = null;
	private SerializedProperty m_modeProp = null;
	private SerializedProperty m_targetScreensProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetMenuShowConditionally = target as MenuShowConditionally;

		// Get all properties
		m_scriptProp = serializedObject.FindProperty("m_Script");
		m_targetAnimatorProp = serializedObject.FindProperty("m_targetAnimator");

		m_restartAnimProp = serializedObject.FindProperty("m_restartShowAnimation");
		m_showDelayProp = serializedObject.FindProperty("m_showDelay");
		m_hideDelayProp = serializedObject.FindProperty("m_hideDelay");

		m_checkSelectedDragonProp = serializedObject.FindProperty("m_checkSelectedDragon");
		m_hideForDragonsProp = serializedObject.FindProperty("m_hideForDragons");
        m_showOnlyForType = serializedObject.FindProperty("m_showOnlyForType");
        m_targetDragonSkuProp = serializedObject.FindProperty("m_targetDragonSku");
		m_showIfShadowProp = serializedObject.FindProperty("m_showIfShadow");
		m_showIfLockedProp = serializedObject.FindProperty("m_showIfLocked");
		m_showIfAvailableProp = serializedObject.FindProperty("m_showIfAvailable");
		m_showIfOwnedProp = serializedObject.FindProperty("m_showIfOwned");

		m_checkScreensProp = serializedObject.FindProperty("m_checkScreens");
		m_modeProp = serializedObject.FindProperty("m_mode");
		m_targetScreensProp = serializedObject.FindProperty("m_targetScreens");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetMenuShowConditionally = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.
		GUIContent content = new GUIContent();

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Unity's "script" property
		if(m_scriptProp != null) {
			// Draw the property, disabled
			EditorGUI.BeginDisabledGroup(true); {
				EditorGUILayout.PropertyField(m_scriptProp, true);
			} EditorGUI.EndDisabledGroup();
		}

		// Target
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(m_targetAnimatorProp, true);
		bool validAnimator = m_targetAnimatorProp.objectReferenceValue != null;

		// Animation options
		// Separator
		EditorGUILayoutExt.Separator();

		// Restart animation
		EditorGUI.BeginDisabledGroup(!validAnimator); {
			EditorGUILayout.PropertyField(m_restartAnimProp, true);
		} EditorGUI.EndDisabledGroup();

		// Delay - only if no target is selected
		EditorGUI.BeginDisabledGroup(validAnimator); {
			content.tooltip = "If a target animator is defined, the animator's delay will be used instead.";

			content.text = m_showDelayProp.displayName;
			EditorGUILayout.PropertyField(m_showDelayProp, content, true);

			content.text = m_hideDelayProp.displayName;
			EditorGUILayout.PropertyField(m_hideDelayProp, content, true);
		} EditorGUI.EndDisabledGroup();

		// Dragon-based visibility
		// Separator
		EditorGUILayoutExt.Separator();

		// Toggle group
		m_checkSelectedDragonProp.boolValue = EditorGUILayout.ToggleLeft(m_checkSelectedDragonProp.displayName, m_checkSelectedDragonProp.boolValue);
		if(m_checkSelectedDragonProp.boolValue) {
			// Indent in
			EditorGUI.indentLevel++;

			EditorGUILayout.PropertyField(m_hideForDragonsProp, true);
            
            EditorGUILayout.PropertyField(m_showOnlyForType, true);

            // Disable the whole group if m_hideForDragons property is set to either NONE or ALL
            EditorGUI.BeginDisabledGroup(
                (m_hideForDragonsProp.enumValueIndex == (int)MenuShowConditionally.HideForDragons.ALL)
                || (m_hideForDragonsProp.enumValueIndex == (int)MenuShowConditionally.HideForDragons.NONE)
            );
            {
				// Draw all ownership properties
				EditorGUILayout.PropertyField(m_targetDragonSkuProp, true);
				EditorGUILayout.PropertyField(m_showIfShadowProp, true);
				EditorGUILayout.PropertyField(m_showIfLockedProp, true);
				EditorGUILayout.PropertyField(m_showIfAvailableProp, true);
				EditorGUILayout.PropertyField(m_showIfOwnedProp, true);
			} EditorGUI.EndDisabledGroup();

			// Indent back out
			EditorGUI.indentLevel--;
		}

		// Screen-based visibility
		// Separator
		EditorGUILayoutExt.Separator();

		// Toggle group
		m_checkScreensProp.boolValue = EditorGUILayout.ToggleLeft(m_checkScreensProp.displayName, m_checkScreensProp.boolValue);
		if(m_checkScreensProp.boolValue) {
			// Indent in
			EditorGUI.indentLevel++;

			EditorGUILayout.PropertyField(m_modeProp, true);
			EditorGUILayout.PropertyField(m_targetScreensProp, true);

			// Indent back out
			EditorGUI.indentLevel--;
		}

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