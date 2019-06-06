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
using System.IO;
using System.Collections.Generic;
using DG.Tweening;
using SimpleJSON;

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
		// Default
		DrawDefaultInspector();

		// Test button
		EditorGUILayout.Space();
		if(GUILayout.Button("TEST 1", GUILayout.Height(50))) {
			targetTest.OnTestButton();
		}

		if(GUILayout.Button("TEST 2", GUILayout.Height(50))) {
			targetTest.OnTestButton2();
		}

		if(GUILayout.Button("TEST 3", GUILayout.Height(50))) {
			targetTest.OnTestButton3();
		}

		if(GUILayout.Button("TEST 4", GUILayout.Height(50))) {
			targetTest.OnTestButton4();
		}
	}
}