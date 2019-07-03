// QuestScoreParticleTestEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/06/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the QuestScoreParticleTest class.
/// </summary>
[CustomEditor(typeof(QuestScoreParticleTest), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class QuestScoreParticleTestEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	QuestScoreParticleTest m_target = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_target = target as QuestScoreParticleTest;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_target = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("startPoint"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("endPoint"));

		if(GUILayout.Button("TEST", GUILayout.Height(50))) {
			if(Application.isPlaying) {
                ParticlesTrailFX fx = m_target.GetComponentInChildren<ParticlesTrailFX>();
				if(fx == null) {
					fx = ParticlesTrailFX.LoadAndLaunch(
						"UI/FX/PF_ScoreTransferFX",
						m_target.transform,
						m_target.startPoint.position + new Vector3(0f, 0f, -0.5f),
						m_target.endPoint.transform.position + new Vector3(0f, 0f, -0.5f)
					);
					fx.totalDuration = 5f;
					fx.autoKill = false;
				}

				if(fx.active) {
					fx.StopFX();
				} else {
					fx.pool.pool.ClearInstances();
					fx.StartFX();
				}
			} else {
				if(EditorWindow.focusedWindow != null) {
					EditorWindow.focusedWindow.ShowNotification(new GUIContent("Only in Play mode!"));
				}
			}
		}

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}