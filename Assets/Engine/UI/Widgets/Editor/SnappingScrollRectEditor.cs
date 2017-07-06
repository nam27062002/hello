// SnapScrollRectEditor.cs
// 
// Created by Alger Ortín Castellví on 28/01/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the SnapScrollRect class.
/// </summary>
[CustomEditor(typeof(SnappingScrollRect), true)]
[CanEditMultipleObjects]
public class SnappingScrollRectEditor : ScrollRectEditor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Cache serialized properties
	private SerializedProperty m_snapPosProp;
	private SerializedProperty m_snapAnimDurationProp;
	private SerializedProperty m_snapAfterDraggingProp;
	private SerializedProperty m_snapEaseProp;
	private SerializedProperty m_selectedPointProp;
	private SerializedProperty m_onSelectionChangedProp;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	protected override void OnEnable() {
		// Call parent
		base.OnEnable();

		// Acquire properties
		m_snapPosProp = serializedObject.FindProperty("m_snapPos");
		m_snapAnimDurationProp = serializedObject.FindProperty("m_snapAnimDuration");
		m_snapAfterDraggingProp = serializedObject.FindProperty("m_snapAfterDragging");
		m_snapEaseProp = serializedObject.FindProperty("m_snapEase");
		m_selectedPointProp = serializedObject.FindProperty("m_selectedPoint");
		m_onSelectionChangedProp = serializedObject.FindProperty("m_onSelectionChanged");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	protected override void OnDisable() {
		// Call parent
		base.OnDisable();
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Call parent - this draws the default inspector for scroll rect
		base.OnInspectorGUI();

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Just draw our custom properties one by one
		// This keeps all custom attributes defined in each property such as Separators and Comments
		EditorGUILayout.PropertyField(m_snapPosProp);
		EditorGUILayout.PropertyField(m_snapAnimDurationProp);
		EditorGUILayout.PropertyField(m_snapAfterDraggingProp);
		EditorGUILayout.PropertyField(m_snapEaseProp);

		// Update view whenever the selected point changes
		bool selectionChanged = EditorGUILayout.PropertyField(m_selectedPointProp);

		// On selection changed event listeners
		EditorGUILayout.PropertyField(m_onSelectionChangedProp);

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// If selected point has been manually changed, scroll to it
		if(selectionChanged) {
			((SnappingScrollRect)target).ScrollToSelection(false);
		}
	}
}