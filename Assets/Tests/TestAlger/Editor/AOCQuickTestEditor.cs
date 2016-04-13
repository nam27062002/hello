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
using DG.Tweening;

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
		if(GUILayout.Button("TEST", GUILayout.Height(50))) {
			targetTest.OnTestButton();
		}

		EditorGUILayout.Space();
		if(targetTest.m_tweener != null) {
			Tween t = (Tween)targetTest.m_tweener;
			EditorGUILayout.LabelField("Elapsed Perc.", t.ElapsedPercentage().ToString());
			EditorGUILayout.LabelField("Elapsed Perc. (looped)", t.ElapsedPercentage(true).ToString());
			EditorGUILayout.LabelField("Full Position", t.fullPosition.ToString());
			EditorGUILayout.TextArea(t.ToString());
			EditorUtility.SetDirty(targetTest);
		}
	}
}