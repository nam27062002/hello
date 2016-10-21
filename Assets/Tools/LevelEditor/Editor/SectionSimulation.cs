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
			get { return Prefs.GetBoolEditor("LevelEditor.SimulationEnabled", true); }
			set { Prefs.SetBoolEditor("LevelEditor.SimulationEnabled", value); }
		}

		private float time {
			get { return Prefs.GetFloatEditor("LevelEditor.SimulationTime", 0f); }
			set { Prefs.SetFloatEditor("LevelEditor.SimulationTime", value); }
		}

		private float timeMax {
			get { return Prefs.GetFloatEditor("LevelEditor.SimulationTimeMax", 1000f); }
			set { Prefs.SetFloatEditor("LevelEditor.SimulationTimeMax", Mathf.Max(value, 1f)); }	// Not less than 1
		}

		private float xp {
			get { return Prefs.GetFloatEditor("LevelEditor.SimulationXp", 0f); }
			set { Prefs.SetFloatEditor("LevelEditor.SimulationXp", value); }
		}

		private float xpMax {
			get { return Prefs.GetFloatEditor("LevelEditor.SimulationXpMax", 100000f); }
			set { Prefs.SetFloatEditor("LevelEditor.SimulationXpMax", Mathf.Max(value, 1f)); }	// Not less than 1
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
			// Title - encapsulate in a nice button to make it foldable
			GUI.backgroundColor = Colors.gray;
			bool folded = Prefs.GetBoolEditor("LevelEditor.SectionSimulation.folded", false);
			if(GUILayout.Button((folded ? "►" : "▼") + " Game Simulator", LevelEditorWindow.styles.sectionHeaderStyle, GUILayout.ExpandWidth(true))) {
				folded = !folded;
				Prefs.SetBoolEditor("LevelEditor.SectionSimulation.folded", folded);
			}
			GUI.backgroundColor = Colors.white;

			// -Only show if unfolded
			if(!folded) {
				// Group in a box
				EditorGUILayout.BeginVertical(LevelEditorWindow.styles.sectionContentStyle, GUILayout.Height(1)); {	// [AOC] Requesting a very small size fits the group to its content's actual size
					// Only show if a Spawners Level is loaded
					List<Level> spawnersLevel = LevelEditorWindow.sectionLevels.GetLevel(LevelEditorSettings.Mode.SPAWNERS);
					if(spawnersLevel == null || spawnersLevel.Count == 0) {
						EditorGUILayout.HelpBox("A Spawners scene is required to run the simulator", MessageType.Error);
					} else {
						// If any parameter changes, update simulation
						EditorGUI.BeginChangeCheck();

						// Enable simulation?
						simulationEnabled = EditorGUILayout.ToggleLeft(" Enable Simulation", simulationEnabled);

						// Time slider
						bool wasEnabled = GUI.enabled;
						GUI.enabled = simulationEnabled;
						EditorGUILayout.BeginHorizontal(); {
							// Slider
							EditorGUILayout.LabelField("Time", GUILayout.Width(40f));
							time = EditorGUILayout.Slider(time, 0f, timeMax);

							// Max XP
							EditorGUILayout.LabelField("Max", GUILayout.Width(40f));
							timeMax = EditorGUILayout.FloatField(timeMax, GUILayout.Width(50f));
						} EditorGUILayout.EndHorizontal();

						// XP slider
						EditorGUILayout.BeginHorizontal(); {
							// Slider
							EditorGUILayout.LabelField("XP", GUILayout.Width(40f));
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
					}
				} EditorGUILayout.EndVertical();
			}
		}

		//--------------------------------------------------------------------//
		// INTERNAL METHODS													  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Refreshes the scene with the current simulation parameters.
		/// </summary>
		private void UpdateSimulation() {
			// Aux vars (outside the loop)
			float simTime = time;
			float simXp = xp;

			// Iterate all spawners in the scene, reset stats
			m_enemiesCount.Set(0, 0);
			Spawner[] spawners = Object.FindObjectsOfType<Spawner>();
			for(int i = 0; i < spawners.Length; i++) {
				// Show this spawner with the given filters?
				bool canSpawn = true;
				spawners[i].showSpawnerInEditor = true;

				// If simulation is disabled, spawner is active for sure
				if(simulationEnabled) {
					canSpawn = spawners[i].CanSpawn(simTime, simXp);
					spawners[i].showSpawnerInEditor = canSpawn;
				}

				// If spawner is to be active, update stats
				if(canSpawn) {
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