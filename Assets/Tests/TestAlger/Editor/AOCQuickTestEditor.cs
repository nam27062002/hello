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
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private int[] m_testResults = new int[0];
	private int m_testRepetitions = 100;
	private int m_lastTestRepetitions = 100;

	private AOCQuickTest castedTarget { get { return target as AOCQuickTest; }}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI() {
		// Default
		DrawDefaultInspector();

		EditorGUILayoutExt.Separator();

		// Test repetitions
		m_testRepetitions = EditorGUILayout.IntField("Test Repetitions", m_testRepetitions);

		// Test button
		if(GUILayout.Button("TEST", GUILayout.Height(50))) {
			// Clear results
			m_testResults = new int[castedTarget.m_myProbSet.numElements];
			m_lastTestRepetitions = m_testRepetitions;

			// Perform as much repetitions as defined
			for(int i = 0; i < m_testRepetitions; i++) {
				m_testResults[castedTarget.m_myProbSet.GetWeightedRandomElement()]++;
			}
		}

		GUILayout.Space(10f);

		// Draw test results
		if(m_lastTestRepetitions > 0) {
			for(int i = 0; i < m_testResults.Length; i++) {
				// Single row per element
				EditorGUILayout.BeginHorizontal(); {
					// Label (use element index)
					GUILayout.Label(StringUtils.FormatNumber(i), GUILayout.Width(20f));

					// Fillbar
					Rect pos = GUILayoutUtility.GetRect(18, 18, EditorStyles.textField);	// Get a rect for the progress bar using the same margins as a textfield:
					EditorGUI.ProgressBar(pos, (float)m_testResults[i]/(float)m_lastTestRepetitions, StringUtils.FormatNumber(m_testResults[i]));
				} EditorGUILayoutExt.EndHorizontalSafe();
			}
		}

		EditorGUILayoutExt.Separator();

		// Simulate chest rewards owning 1, 2, 3... 10 dragons
		int totalDragons = 10;
		for(int ownedDragons = 1; ownedDragons <= totalDragons; ownedDragons++) {
			// Single row per dragon owned
			EditorGUILayout.BeginHorizontal(); {
				// Label (dragons owned)
				GUILayout.Label(ownedDragons.ToString(), GUILayout.Width(20f));

				// Coins
				{
					// [AOC] Formula defined in the chestsRewards table
					float reward = (Mathf.Log((float)ownedDragons) * 140f + 140f * (float)ownedDragons)/1f;
					int rewardAmount = (int)MathUtils.Snap(reward, 100f);
					rewardAmount = Mathf.Max(1, rewardAmount);	// At least 1 coin
					GUILayout.Label(StringUtils.FormatNumber(reward, 2), EditorStyles.numberField, GUILayout.Width(50f));
					GUILayout.Label(StringUtils.FormatNumber(rewardAmount), EditorStyles.numberField, GUILayout.Width(50f));
					GUILayout.Space(10f);
				}

				// PC
				{
					// [AOC] Formula defined in the chestsRewards table
					float reward = (Mathf.Log((float)ownedDragons) * 50f + 50f * (float)ownedDragons)/100f;
					int rewardAmount = (int)MathUtils.Snap(reward, 1f);
					rewardAmount = Mathf.Max(1, rewardAmount);	// At least 1 coin
					GUILayout.Label(StringUtils.FormatNumber(reward, 2), EditorStyles.numberField, GUILayout.Width(50f));
					GUILayout.Label(StringUtils.FormatNumber(rewardAmount), EditorStyles.numberField, GUILayout.Width(50f));
				}

				// Boosters
				// [AOC] TODO!!

				// Eggs
				// [AOC] TODO!!

			} EditorGUILayoutExt.EndHorizontalSafe();
		}
	}
}