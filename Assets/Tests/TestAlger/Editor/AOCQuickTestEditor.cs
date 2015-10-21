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

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI() {
		// Default
		DrawDefaultInspector();
		/*
		EditorUtils.Separator(EditorUtils.Orientation.HORIZONTAL, 10);
		AOCData[] data = AOCDataManager.GetData();
		for(int i = 0; i < data.Length; i++) {
			GUILayout.Label(data[i].m_id.ToString());
		}

		EditorUtils.Separator(EditorUtils.Orientation.HORIZONTAL, 10);

		if(GUILayout.Button("Save AOCDataManager")) {
			AOCDataManager mng = AOCDataManager.GetInstance();
			AssetDatabase.CreateAsset(mng, "Assets/Resources/Singletons/AOCDataManager.asset");
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = mng;
		}*/
	}
}