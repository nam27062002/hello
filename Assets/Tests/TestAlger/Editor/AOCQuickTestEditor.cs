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
[CustomEditor(typeof(AOCQuickTest))]
public class AOCQuickTestEditor : Editor {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private static GameObject m_instanceObj = null;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI() {
		// Default
		DrawDefaultInspector();

		// Button to load a prefab
		if(GUILayout.Button("Load Prefab")) {
			// Delete current instance and prefab if any
			if(m_instanceObj != null) {
				DestroyImmediate(m_instanceObj);
				m_instanceObj = null;
			}

			// Load prefab
			GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Tests/TestAlger/PF_Test_0.prefab");
			m_instanceObj = PrefabUtility.InstantiatePrefab(prefabObj) as GameObject;
		}

		// Save changes
		GUI.enabled = (m_instanceObj != null);
		if(GUILayout.Button("Apply Changes")) {
			GameObject prefabObj = PrefabUtility.GetPrefabParent(m_instanceObj) as GameObject;
			PrefabUtility.ReplacePrefab(m_instanceObj, prefabObj, ReplacePrefabOptions.ConnectToPrefab);
		}
		GUI.enabled = true;

		if(GUILayout.Button("Prompt Dialog")) {
			if(EditorUtility.DisplayDialog("Dialog Title", "Dialog Message", "Ok", "Ko")) {
				Debug.Log("USER SAID YES!!");
			} else {
				Debug.Log("User refused");
			}

			Debug.Log("Doing some extra stuff whether it's a yes or no");
		}

		if(GUILayout.Button("Create new prefab")) {
			// Create new object
			GameObject go = new GameObject("PF_MyNewPrefab");
			string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/" + go.name + ".prefab");
			Debug.Log(path);
			PrefabUtility.CreatePrefab(path, go, ReplacePrefabOptions.ConnectToPrefab);
		}
	}
}