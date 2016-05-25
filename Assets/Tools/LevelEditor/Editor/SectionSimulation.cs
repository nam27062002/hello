// SectionSimulation.cs
// 
// Created by Alger Ortín Castellví on 23/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

#pragma warning disable 0414

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Simulate the progress of a single game run.
	/// TODO:
	/// 	- Use XP instead of time (local variables have already been refactored for XP)
	/// </summary>
	public class SectionSimulation : ILevelEditorSection {
		//--------------------------------------------------------------------//
		// CONSTANTS														  //
		//--------------------------------------------------------------------//

		//--------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											  //
		//--------------------------------------------------------------------//
		// Prefs
		private bool simulationEnabled {
			get { return Prefs.GetBool("LevelEditorSimulationEnabled", true); }
			set { Prefs.SetBool("LevelEditorSimulationEnabled", value); }
		}

		private float xp {
			get { return Prefs.GetFloat("LevelEditorSimulationXp", 0f); }
			set { Prefs.SetFloat("LevelEditorSimulationXp", value); }
		}

		private float xpMax {
			get { return Prefs.GetFloat("LevelEditorSimulationXpMax", 1000f); }
			set { Prefs.SetFloat("LevelEditorSimulationXpMax", Mathf.Max(value, 1f)); }	// Not less than 1
		}

		// Non-persistent data
		private RangeInt m_enemiesCount = new RangeInt(0, 0);

		//--------------------------------------------------------------------//
		// INTERFACE IMPLEMENTATION											  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		public void Init() {
			// Perform a first simulation update
			UpdateSimulation();
		}
		
		/// <summary>
		/// Draw the section.
		/// </summary>
		public void OnGUI() {
			// Group in a box
			EditorGUILayout.BeginVertical(LevelEditorWindow.instance.styles.boxStyle, GUILayout.Height(1)); {	// [AOC] Requesting a very small size fits the group to its content's actual size
				// Title
				EditorGUILayout.BeginHorizontal(); {
					GUILayout.FlexibleSpace();
					GUILayout.Label("Game Simulator", EditorStyles.boldLabel);
					GUILayout.FlexibleSpace();
				} EditorGUILayout.EndHorizontal();

				// If any parameter changes, update simulation
				EditorGUI.BeginChangeCheck();

				// Enable simulation?
				simulationEnabled = EditorGUILayout.ToggleLeft(" Enable Simulation", simulationEnabled);

				// XP slider
				bool wasEnabled = GUI.enabled;
				GUI.enabled = simulationEnabled;
				EditorGUILayout.BeginHorizontal(); {
					// Slider
					EditorGUILayout.LabelField("Time", GUILayout.Width(40f));
					xp = EditorGUILayout.Slider(xp, 0f, xpMax);

					// Max XP
					EditorGUILayout.LabelField("Max", GUILayout.Width(40f));
					xpMax = EditorGUILayout.FloatField(xpMax, GUILayout.Width(50f));
				} EditorGUILayout.EndHorizontal();
				GUI.enabled = wasEnabled;

				// If any parameter has changed, update simulation
				if(EditorGUI.EndChangeCheck()) {
					UpdateSimulation();
				}

				// Enemy count stats
				const float columnWidth = 60f;
				EditorGUILayout.BeginHorizontal(); {
					EditorGUILayout.LabelField("Enemies", GUILayout.Width(columnWidth));
					EditorGUILayout.LabelField("MIN", GUILayout.Width(columnWidth));
					EditorGUILayout.LabelField("MAX", GUILayout.Width(columnWidth));
					EditorGUILayout.LabelField("AVG", GUILayout.Width(columnWidth));
				} EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(); {
					GUILayout.Space(columnWidth);
					EditorGUILayout.LabelField(m_enemiesCount.min.ToString(), GUILayout.Width(columnWidth));
					EditorGUILayout.LabelField(m_enemiesCount.max.ToString(), GUILayout.Width(columnWidth));
					EditorGUILayout.LabelField(m_enemiesCount.center.ToString(), GUILayout.Width(columnWidth));
				} EditorGUILayout.EndHorizontal();
			} EditorGUILayout.EndVertical();
		}

		//--------------------------------------------------------------------//
		// INTERNAL METHODS													  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Refreshes the scene with the current simulation parameters.
		/// </summary>
		private void UpdateSimulation() {
			// Iterate all spawners in the scene, reset stats
			m_enemiesCount.Set(0, 0);
			Spawner[] spawners = Object.FindObjectsOfType<Spawner>();
			for(int i = 0; i < spawners.Length; i++) {
				// Show this spawner with the given filters?
				spawners[i].showSpawnerInEditor = true;

				// If simulation is disabled, spawner is active for sure
				if(simulationEnabled) {
					// Check required xp
					// Min
					if(xp < spawners[i].enableTime) {
						spawners[i].showSpawnerInEditor = false;
					}

					// Max
					if((spawners[i].disableTime > 0f) && (xp > spawners[i].disableTime)) {
						spawners[i].showSpawnerInEditor = false;
					}
				}

				// If spawner is to be active, update stats
				if(spawners[i].showSpawnerInEditor) {
					m_enemiesCount.min += spawners[i].m_quantity.min;
					m_enemiesCount.max += spawners[i].m_quantity.max;
				}
			}
		}

		//--------------------------------------------------------------------//
		// CALLBACKS														  //
		//--------------------------------------------------------------------//

	}
}