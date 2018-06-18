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
		if(GUILayout.Button("TEST", GUILayout.Height(50))) {
			targetTest.OnTestButton();
		}

		if(GUILayout.Button("0 -> 1", GUILayout.Height(50))) {
			targetTest.OnTestButton2();
		}

		if(GUILayout.Button("1 -> 0", GUILayout.Height(50))) {
			targetTest.OnTestButton3();
		}

		if(GUILayout.Button("INTERRUPT", GUILayout.Height(50))) {
			targetTest.OnTestButton4();
		}

		EditorGUILayout.Space();
		if(GUILayout.Button("CHECK BUILD SCENES", GUILayout.Height(50))) {
			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			for(int i = 0; i < scenes.Length; ++i) {
				EditorBuildSettingsScene scene = scenes[i];
				if(EditorUtility.DisplayCancelableProgressBar("Checking scenes...", (i+1) + "/" + scenes.Length + ": " + scene.path, ((float)(i+1)/(float)scenes.Length))) {
					break;
				}
				Debug.Log("<color=green>OPENING " + scene.path + "</color>");
				UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene.path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
				Debug.Log("<color=red>CLOSING " + scene.path + "</color>");
				UnityEditor.SceneManagement.EditorSceneManager.CloseScene(UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scene.path), true);
			}
			EditorUtility.ClearProgressBar();
		}
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