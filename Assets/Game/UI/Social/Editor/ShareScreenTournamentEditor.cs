// ShareScreenTournamentEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ShareScreenTournament class.
/// </summary>
[CustomEditor(typeof(ShareScreenTournament), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class ShareScreenTournamentEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string TEST_RANK = "ShareScreenTournamentEditor.TEST_RANK";
	private int testRank {
		get { return Prefs.GetIntEditor(TEST_RANK, 0); }
		set { Prefs.SetIntEditor(TEST_RANK, value); }
	}

	private const string TEST_SCORE= "ShareScreenTournamentEditor.TEST_SCORE";
	private int testScore {
		get { return Prefs.GetIntEditor(TEST_SCORE, 100000); }
		set { Prefs.SetIntEditor(TEST_SCORE, value); }
	}

	private const string TEST_TIMED = "ShareScreenTournamentEditor.TEST_TIMED";
	private bool testTimed {
		get { return Prefs.GetBoolEditor(TEST_TIMED, false); }
		set { Prefs.SetBoolEditor(TEST_TIMED, value); }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	ShareScreenTournament m_targetShareScreenTournament = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetShareScreenTournament = target as ShareScreenTournament;
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetShareScreenTournament = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Default inspector
		DrawDefaultInspector();

		// Tools
		EditorGUILayoutExt.Separator("Tools");

		// Rank slider
		testRank = EditorGUILayout.IntSlider("Test Rank", testRank, 0, 99); // Max 100, 0-indexed

		// Score reference
		testScore = EditorGUILayout.IntField("Test Score", testScore);

		// Timed?
		testTimed = EditorGUILayout.Toggle("Timed Score?", testTimed);

		// Test button
		GUI.color = Colors.paleGreen;
		if(GUILayout.Button("TEST!", GUILayout.Height(30f))) {
			m_targetShareScreenTournament.TEST_InitAtRank(testRank, testScore, testTimed);
		}

		// Clear button
		GUI.color = Colors.coral;
		if(GUILayout.Button("Clear", GUILayout.Height(30f))) {
			m_targetShareScreenTournament.TEST_Clear();
		}

		// Finalize
		GUI.color = Color.white;
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}