// EaseCurveTestEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[CustomEditor(typeof(EaseCurveTest))]
public class EaseCurveTestEditor : Editor {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private EaseCurveTest targetTest { get { return target as EaseCurveTest; }}
	public static float m_curvePreviewSize = 100f;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI() {
		GUI.changed = false;

		// Params
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_overshoot"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_amplitude"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_period"));

		// Curve preview size
		m_curvePreviewSize = EditorGUILayout.FloatField("Curve Preview Size", m_curvePreviewSize);
		if(m_curvePreviewSize < 1f) m_curvePreviewSize = 1f;

		if(GUI.changed) {
			serializedObject.ApplyModifiedProperties();
			targetTest.OnClick();
		}

		EditorGUILayoutExt.Separator();

		// Test button
		if(GUILayout.Button("TEST", GUILayout.Height(50))) {
			targetTest.OnClick();
		}

		EditorGUILayoutExt.Separator();

		// Curves
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_curves"), true);

		// Default
		//DrawDefaultInspector();
	}

	//------------------------------------------------------------------//
	// MENU ENTRIES														//
	//------------------------------------------------------------------//
	[MenuItem("Hungry Dragon/AOC/Scene Tweeners", false, 101)]
	public static void OpenScene1() { HungryDragonEditorMenu.OpenScene("Assets/Tests/TestAlger/Tweeners/TweenersTest.unity", true); }
}

/// <summary>
/// AOC quick test ease curve editor.
/// </summary>
[CustomPropertyDrawer(typeof(EaseCurveTest.EaseCurve))]
public class EaseCurveTestEaseCurveEditor : ExtendedPropertyDrawer {
	/// <summary>
	/// Raises the GUI impl event.
	/// </summary>
	/// <param name="_property">Property.</param>
	/// <param name="_label">Label.</param>
	protected override void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Get properties
		SerializedProperty easeTypeProp = _property.FindPropertyRelative("easeType");
		SerializedProperty curveProp = _property.FindPropertyRelative("curve");

		// Draw curve - don't assign, we don't want to overwrite values
		m_pos.height = EaseCurveTestEditor.m_curvePreviewSize;
		EditorGUI.CurveField(m_pos, easeTypeProp.enumNames[easeTypeProp.enumValueIndex], curveProp.animationCurveValue);
		AdvancePos();
	}
}