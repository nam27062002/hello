// ProceduralMeshEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
	/// Custom editor for the ProceduralMesh class.
	/// </summary>
	[CustomEditor(typeof(ProceduralMeshGenerator))]
	public class ProceduralMeshGeneratorEditor : Editor {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											//
		//------------------------------------------------------------------//
		private ProceduralMeshGenerator targetMesh { get { return target as ProceduralMeshGenerator; }}

		private SerializedProperty m_showDebugGizmosProp = null;

		//------------------------------------------------------------------//
		// METHODS															//
		//------------------------------------------------------------------//
		/// <summary>
		/// The editor has been enabled - target object selected.
		/// </summary>
		private void OnEnable() {
			m_showDebugGizmosProp = serializedObject.FindProperty("m_showDebugGizmos");
		}		

		/// <summary>
		/// The editor has been disabled - target object unselected.
		/// </summary>
		private void OnDisable() {

		}

		/// <summary>
		/// The scene is being refreshed.
		/// </summary>
		public void OnSceneGUI() {
			
		}

		/// <summary>
		/// Draw the inspector.
		/// </summary>
		public override void OnInspectorGUI() {
			// Custom editor
			EditorGUILayout.BeginHorizontal(GUILayout.Height(50)); {
				EditorGUILayout.BeginVertical(GUILayout.Height(50)); {
					GUILayout.FlexibleSpace();

					// Track changes
					GUI.changed = false;

					// Show sliders
					int sides = EditorGUILayout.IntSlider(targetMesh.sides, 1, 30);
					float angle = EditorGUILayout.Slider(targetMesh.angle, 1f, 360f);

					// If sliders have changed, update mesh
					if(GUI.changed) {
						targetMesh.GenerateMesh(sides, angle);
					}

					GUILayout.FlexibleSpace();
				} EditorGUILayoutExt.EndVerticalSafe();

				// Manually regenerate the mesh
				if(GUILayout.Button("REGENERATE MESH", GUILayout.ExpandHeight(true))) {
					targetMesh.GenerateMesh(targetMesh.sides, targetMesh.angle);
				}
			} EditorGUILayoutExt.EndHorizontalSafe();

			// Debug toggle
			EditorGUILayout.PropertyField(m_showDebugGizmosProp);

			// Save changes
			serializedObject.ApplyModifiedProperties();
		}
	}
}