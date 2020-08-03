// UINotificationEditor.cs
// 
// Created by Alger Ortín Castellví on 13/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the UINotification class.
/// </summary>
[CustomEditor(typeof(UINotification), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class UINotificationEditor : ShowHideAnimatorEditor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	override protected void OnEnable() {
		// Call parent
		base.OnEnable();
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	override protected void OnDisable() {
		// Call parent
		base.OnDisable();
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	override public void OnInspectorGUI() {
		// Call parent
		base.OnInspectorGUI();

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Draw separator and UI Notification serialized properties
		EditorGUILayoutExt.Separator("UI Notification");
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_targetScale"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_pauseDuration"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_scaleUpDuration"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_scaleDownDuration"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_scaleUpEase"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_scaleDownEase"));

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}
}