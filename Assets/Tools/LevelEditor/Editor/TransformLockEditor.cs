// TransformEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/10/2015.
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
	/// Custom editor for the TransformLock class.
	/// Everything will be done here actually.
	/// </summary>
	[CustomEditor(typeof(TransformLock))]
	public class TransformLockEditor : Editor {
		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//
		TransformLock targetLock { get { return target as TransformLock; }}

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private Vector3 m_lastPos;
		private Vector3 m_lastRot;
		private Vector3 m_lastScale;
		
		//------------------------------------------------------------------//
		// METHODS															//
		//------------------------------------------------------------------//
		/// <summary>
		/// The editor has been enabled - target object selected.
		/// </summary>
		private void OnEnable() {
			// Store initial transform
			m_lastPos = targetLock.transform.position;
			m_lastRot = targetLock.transform.rotation.eulerAngles;
			m_lastScale = targetLock.transform.localScale;
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
			// Get new values
			Vector3 newPos = targetLock.transform.position;
			Vector3 newRot = targetLock.transform.rotation.eulerAngles;
			Vector3 newScale = targetLock.transform.localScale;

			// If component is not enabled or the ignore flag is active, just update cached values
			if(!targetLock.isActiveAndEnabled || targetLock.ignoreLock) {
				m_lastPos = newPos;
				m_lastRot = newRot;
				m_lastScale = newScale;
				targetLock.ignoreLock = false;
				return;
			}

			// Just compare current values with last ones, if they're different, update them with locks
			// Position
			if(newPos != m_lastPos) {
				// Undo locked axes
				for(int i = 0; i < 3; i++) {
					if(targetLock.m_positionLock[i]) {
						newPos[i] = m_lastPos[i];
					}
				}
				
				// Allow undoing and set new position
				Undo.RecordObject(targetLock.transform, "targetLock.transform position");
				targetLock.transform.position = newPos;
			}
			m_lastPos = newPos;

			// Rotation
			if(newRot != m_lastRot) {
				// Undo locked axes
				for(int i = 0; i < 3; i++) {
					if(targetLock.m_rotationLock[i]) {
						newRot[i] = m_lastRot[i];
					}
				}
				
				// Allow undoing and set new position
				Undo.RecordObject(targetLock.transform, "targetLock.transform rotation");
				targetLock.transform.rotation = Quaternion.Euler(newRot);
			}
			m_lastRot = newRot;

			// Scale
			if(newScale != m_lastScale) {
				// Undo locked axes
				for(int i = 0; i < 3; i++) {
					if(targetLock.m_scaleLock[i]) {
						newScale[i] = m_lastScale[i];
					}
				}
				
				// Allow undoing and set new position
				Undo.RecordObject(targetLock.transform, "targetLock.transform scale");
				targetLock.transform.localScale = newScale;
			}
			m_lastScale = newScale;
		}

		/// <summary>
		/// Draw the inspector.
		/// </summary>
		public override void OnInspectorGUI() {
			// Aux vars
			float firstColumnWidth = 50f;
			float columnWidth = 15f;

			// Styles
			GUIStyle centeredLabelStyle = new GUIStyle(EditorStyles.label);
			centeredLabelStyle.alignment = TextAnchor.MiddleCenter;

			GUIStyle rightLabelStyle = new GUIStyle(EditorStyles.label);
			rightLabelStyle.alignment = TextAnchor.MiddleRight;

			GUIStyle centeredToggleStyle = new GUIStyle(EditorStyles.toggle);
			centeredToggleStyle.alignment = TextAnchor.MiddleCenter;

			// Just show a compact version of the locks
			EditorGUILayout.BeginVertical(); {
				// XYZ Labels
				EditorGUILayout.BeginHorizontal(); {
					//GUILayout.Space(firstColumnWidth);	// [AOC] Y U NO WORK??
					EditorGUILayout.LabelField("", rightLabelStyle, GUILayout.Width(firstColumnWidth));
					EditorGUILayout.LabelField("X", centeredLabelStyle, GUILayout.Width(columnWidth));
					EditorGUILayout.LabelField("Y", centeredLabelStyle, GUILayout.Width(columnWidth));
					EditorGUILayout.LabelField("Z", centeredLabelStyle, GUILayout.Width(columnWidth));
				} EditorUtils.EndHorizontalSafe();

				// Position
				EditorGUILayout.BeginHorizontal(); {
					//EditorGUILayout.PrefixLabel("Position");
					EditorGUILayout.LabelField("Position", rightLabelStyle, GUILayout.Width(firstColumnWidth));
					for(int i = 0; i < 3; i++) {
						targetLock.m_positionLock[i] = EditorGUILayout.Toggle(targetLock.m_positionLock[i], centeredToggleStyle, GUILayout.Width(columnWidth));
					}
				} EditorUtils.EndHorizontalSafe();

				// Rotation
				EditorGUILayout.BeginHorizontal(); {
					//EditorGUILayout.PrefixLabel("Rotation");
					EditorGUILayout.LabelField("Rotation", rightLabelStyle, GUILayout.Width(firstColumnWidth));
					for(int i = 0; i < 3; i++) {
						targetLock.m_rotationLock[i] = EditorGUILayout.Toggle(targetLock.m_rotationLock[i], GUILayout.Width(columnWidth));
					}
				} EditorUtils.EndHorizontalSafe();

				// Scale
				EditorGUILayout.BeginHorizontal(); {
					//EditorGUILayout.PrefixLabel("Scale");
					EditorGUILayout.LabelField("Scale", rightLabelStyle, GUILayout.Width(firstColumnWidth));
					for(int i = 0; i < 3; i++) {
						targetLock.m_scaleLock[i] = EditorGUILayout.Toggle(targetLock.m_scaleLock[i], GUILayout.Width(columnWidth));
					}
				} EditorUtils.EndHorizontalSafe();
			} EditorUtils.EndVerticalSafe();
		}
	}
}