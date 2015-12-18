// CPListPrefEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Custom editor for the CPListPref class.
	/// </summary>
	[CustomEditor(typeof(CPListPref))]
	public class CPListPrefEditor : Editor {
		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//
		CPListPref targetPref { get { return target as CPListPref; }}

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// METHODS															//
		//------------------------------------------------------------------//
		/// <summary>
		/// The editor has been enabled - target object selected.
		/// </summary>
		private void OnEnable() {

		}

		/// <summary>
		/// The editor has been disabled - target object unselected.
		/// </summary>
		private void OnDisable() {

		}

		/// <summary>
		/// Draw the inspector.
		/// </summary>
		public override void OnInspectorGUI() {
			// Init
			EditorGUIUtility.LookLikeInspector();
			EditorGUI.BeginChangeCheck();

			// Draw default properties
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_id"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_label"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_text"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_type"), true);

			// Exposed array will depend on selected type
			switch(serializedObject.FindProperty("m_type").enumValueIndex) {
				case 0: EditorGUILayout.PropertyField(serializedObject.FindProperty("m_intValues"), true);		break;
				case 1: EditorGUILayout.PropertyField(serializedObject.FindProperty("m_floatValues"), true);	break;
				case 2: EditorGUILayout.PropertyField(serializedObject.FindProperty("m_stringValues"), true);	break;
			}

			// Save changes and finalize
			if(EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
			}
			EditorGUIUtility.LookLikeControls();
		}
	}
}