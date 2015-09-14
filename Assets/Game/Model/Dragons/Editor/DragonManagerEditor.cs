// DragonManagerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the DragonLevelProgression class.
/// </summary>
[CustomEditor(typeof(DragonManager))]
public class DragonManagerEditor : Editor {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private List<bool> m_dragonFoldouts = new List<bool>();

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Constructor
	/// </summary>
	public DragonManagerEditor() {
		// Nothing to do
	}

	/// <summary>
	/// A change has occurred in the inspector, draw the object and store new values.
	/// </summary>
	override public void OnInspectorGUI() {
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Current dragons list
		EditorGUILayout.BeginVertical();
		GUILayout.Space(10);
		if(GUILayout.Button("Edit Content", GUILayout.Height(50))) {
			DragonManagerEditorWindow.ShowWindow(serializedObject);
		}
		GUILayout.Space(10);
		EditorGUILayout.EndVertical();
	}
}