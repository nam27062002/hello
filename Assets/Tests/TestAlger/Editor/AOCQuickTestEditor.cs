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
	}

	[MenuItem("Hungry Dragon/AOC/Clear Map Layer")]
	public static void ClearMapLayer() {
		// Convert all objects in the map layer to the default layer
		int mapLayerIdx = LayerMask.NameToLayer("Map");
		int defaultLayerIdx = LayerMask.NameToLayer("Default");
		int mapAndGameLayerIdx = LayerMask.NameToLayer("MapAndGame");
		GameObject[] objs = Resources.FindObjectsOfTypeAll<GameObject>();
		foreach(GameObject go in objs) {
			if(go.layer == mapLayerIdx) {
				if(go.GetComponent<Collider>() != null) {
					go.layer = mapAndGameLayerIdx;
				} else {
					go.layer = defaultLayerIdx;
				}
			}
		}
	}
}