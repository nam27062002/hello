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

		if(GUILayout.Button("TEST", GUILayout.Height(50))) {
			//string[] options = DefinitionsManager.entities.skus.ToArray();
			string[] options = {
				"1", "2", "3", "4", "5"
			};
			SelectionPopupWindow.Show(options, null);
		}
	}

	private void OnSelectionChanged(int _selectedIdx) {
		Debug.Log(_selectedIdx);
	}
}