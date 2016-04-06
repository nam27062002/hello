// AOCQuickTestEditor.cs
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
[CanEditMultipleObjects]
[CustomEditor(typeof(AOCQuickTest))]
public class AOCQuickTestEditor : Editor {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private AOCQuickTest targetTest { get { return target as AOCQuickTest; }}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI() {
		targetTest.m_setMask = (TutorialStep)EditorGUILayout.EnumMaskField("Set Mask", targetTest.m_setMask);
		targetTest.m_setValue = EditorGUILayout.Toggle("Set Value", targetTest.m_setValue);
		EditorGUILayout.Space();
		targetTest.m_tutorialStep = (TutorialStep)EditorGUILayout.EnumMaskField("Current Value", targetTest.m_tutorialStep);
		EditorGUILayout.Space();

		// Test button
		if(GUILayout.Button("TEST", GUILayout.Height(50))) {
			targetTest.OnTestButton();
		}

		// Default
		EditorGUILayout.Space();
		DrawDefaultInspector();
	}
}