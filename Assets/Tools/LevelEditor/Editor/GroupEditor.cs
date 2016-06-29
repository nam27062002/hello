// GroupEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/11/2015.
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
	/// Custom editor for the Group class.
	/// </summary>
	[CustomEditor(typeof(Group))]
	public class GroupEditor : Editor {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly float PREFIX_LABEL_WIDTH = 100f;
		private static readonly float COLUMN_WIDTH = 50f;
		private static GUIStyle s_labelStyle = null;

		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//
		Group targetGroup { get { return target as Group; }}

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		// Length 3 to store min, max and avg rewards (when appliable)
		private Reward[] m_decosReward;
		private Reward[] m_spawnersReward;
		private Reward[] m_totalReward;
		
		//------------------------------------------------------------------//
		// METHODS															//
		//------------------------------------------------------------------//
		/// <summary>
		/// The editor has been enabled - target object selected.
		/// </summary>
		private void OnEnable() {
			// Initialize style if not done
			if(s_labelStyle == null) {
				// Is style initialized?
				s_labelStyle = new GUIStyle(EditorStyles.textField);
				s_labelStyle.normal.textColor = Colors.darkGray;
			}

			// Compute the accumulated reward of this group
			ComputeRewards();
		}

		/// <summary>
		/// Draw the inspector.
		/// </summary>
		public override void OnInspectorGUI() {
			// Default inspector just in case
			DrawDefaultInspector();

			// Show deco rewards
			LevelEditor.settings.groupRewardsFolding[0] = EditorGUILayout.Foldout(LevelEditor.settings.groupRewardsFolding[0], "Decorations Rewards");
			if(LevelEditor.settings.groupRewardsFolding[0]) {
				DrawReward(m_decosReward);
			}

			// Show spawners rewards
			LevelEditor.settings.groupRewardsFolding[1] = EditorGUILayout.Foldout(LevelEditor.settings.groupRewardsFolding[1], "Spawner Rewards");
			if(LevelEditor.settings.groupRewardsFolding[1]) {
				DrawReward(m_spawnersReward);
			}

			// Separator
			EditorGUILayoutExt.Separator(new SeparatorAttribute(5));

			// Show Total rewards
			LevelEditor.settings.groupRewardsFolding[2] = EditorGUILayout.Foldout(LevelEditor.settings.groupRewardsFolding[2], "Total Rewards");
			if(LevelEditor.settings.groupRewardsFolding[2]) {
				DrawReward(m_totalReward);
			}
		}

		//------------------------------------------------------------------//
		// INTERNAL METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Comput the total rewards contained in this group.
		/// </summary>
		private void ComputeRewards() {
			// Length 3 to store min, max and avg rewards (when appliable)
			m_decosReward = new Reward[1];
			m_spawnersReward = new Reward[3];
			m_totalReward = new Reward[3];

			// Decos
			// Find all items that contain a prey stats script
			Entity_Old[] decosStats = targetGroup.GetComponentsInChildren<Entity_Old>();
			for(int i = 0; i < decosStats.Length; i++) {
				m_decosReward[0] = m_decosReward[0] + decosStats[i].reward;
			}

			// Spawners
			// Find all items that contain a spawner script
			Spawner[] spawners = targetGroup.GetComponentsInChildren<Spawner>();
			for(int i = 0; i < spawners.Length; i++) {
				// Skip if spawn prefab not initialized
				if(spawners[i].m_entityPrefab == null) continue;

				// Get spawned prefab's stats
				Entity_Old entityStats = spawners[i].m_entityPrefab.GetComponent<Entity_Old>();
				if(entityStats == null) continue;

				// Add to total reward taking in account spawning amounts
				m_spawnersReward[0] = m_spawnersReward[0] + (entityStats.reward * spawners[i].m_quantity.min);
				m_spawnersReward[1] = m_spawnersReward[1] + (entityStats.reward * spawners[i].m_quantity.max);
				m_spawnersReward[2] = (m_spawnersReward[0] + m_spawnersReward[1]) * 0.5f;
			}

			// Total
			m_totalReward[0] = m_decosReward[0] + m_spawnersReward[0];
			m_totalReward[1] = m_decosReward[0] + m_spawnersReward[1];
			m_totalReward[2] = (m_totalReward[0] + m_totalReward[1]) * 0.5f;
		}

		/// <summary>
		/// Custom compact drawing of a reward object.
		/// </summary>
		/// <param name="_reward">The reward array to be displayed.</param>
		private void DrawReward(Reward[] _reward) {
			EditorGUILayout.BeginVertical(); {
				// Header row (if needed)
				if(_reward.Length >= 2) {
					EditorGUILayout.BeginHorizontal(); {
						GUILayout.Space(PREFIX_LABEL_WIDTH + 5);
						GUILayout.Label("min", GUILayout.Width(COLUMN_WIDTH));
						GUILayout.Label("max", GUILayout.Width(COLUMN_WIDTH));
						if(_reward.Length == 3) {
							GUILayout.Label("avg", GUILayout.Width(COLUMN_WIDTH));
						}
					} EditorGUILayoutExt.EndHorizontalSafe();
				}

				// Row for every stat
				DrawStat("Score", _reward);
				DrawStat("Coins", _reward);
				DrawStat("PC", _reward);
				DrawStat("Health", _reward);
				DrawStat("Energy", _reward);
				DrawStat("Fury", _reward);
				DrawStat("XP", _reward);
			} EditorGUILayoutExt.EndVerticalSafe();
		}

		/// <summary>
		/// Draws a row for a specific stat in the given rewards set.
		/// </summary>
		/// <param name="_label">The label of the stat to be displayed. Spelling is relevant! (but not casing)</param>
		/// <param name="_reward">The data to be displayed.</param>
		private void DrawStat(string _label, Reward[] _reward) {
			// Aux vars
			string toDisplay = "";

			// Put everything under the same line
			EditorGUILayout.BeginHorizontal(); {
				// Prefix label
				GUILayout.Label(_label, GUILayout.Width(PREFIX_LABEL_WIDTH));

				// Columns
				for(int i = 0; i < _reward.Length; i++) {
					// Which value should we display?
					switch(_label.ToLowerInvariant()) {
						case "score":	toDisplay = _reward[i].score.ToString();	break;
						case "coins":	toDisplay = _reward[i].coins.ToString();	break;
						case "pc":		toDisplay = _reward[i].pc.ToString();		break;
						case "health":	toDisplay = _reward[i].health.ToString();	break;
						case "energy":	toDisplay = _reward[i].energy.ToString();	break;
						// case "fury":	toDisplay = _reward[i].fury.ToString();		break;
						case "xp":		toDisplay = _reward[i].xp.ToString();		break;
					}

					// Do it
					GUILayout.Label(toDisplay, s_labelStyle, GUILayout.Width(COLUMN_WIDTH));
				}
			} EditorGUILayoutExt.EndHorizontalSafe();
		}
	}
}