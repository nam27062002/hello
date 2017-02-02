// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple tool to easily edit the time scale from the editor.
/// Will be override by the game if the game manually changes time scale.
/// Very simple implementation for quick tests.
/// </summary>
public class TimeScaler : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	float m_scale = 1.0f;
	Range m_range = new Range(0f, 2f);

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the window.
	/// </summary>
	public static void ShowWindow() {
		TimeScaler window = (TimeScaler)EditorWindow.GetWindow(typeof(TimeScaler));
		window.minSize = new Vector2(100f, 20f);
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	private void OnGUI() {
		// Detect changes
		EditorGUI.BeginChangeCheck();

		// Label - min - slider - max
		EditorGUILayout.BeginHorizontal(); {
			GUILayout.Label("Time Scale", GUILayout.Width(70f));
			m_range.min = EditorGUILayout.FloatField(m_range.min, GUILayout.Width(20f));
			m_scale = EditorGUILayout.Slider(Time.timeScale, m_range.min, m_range.max);
			m_range.max = EditorGUILayout.FloatField(m_range.max, GUILayout.Width(20f));
		} EditorGUILayout.EndHorizontal();

		// If changed, validate values and apply new timescale
		if(EditorGUI.EndChangeCheck()) {
			// Minimum 1 distance, between 0 and 99
			m_range.min = Mathf.Clamp(m_range.min, 0f, 98f);
			m_range.max = Mathf.Clamp(m_range.max, 1f, 99f);
			if(m_range.min >= m_range.max) {
				m_range.max = m_range.min + 1f;
			}
			if(m_range.max <= m_range.min) {
				m_range.min = m_range.max - 1f;
			}

			// Apply timescale
			Time.timeScale = m_scale;
		}
	}
}
