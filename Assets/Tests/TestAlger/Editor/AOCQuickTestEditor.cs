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
using System.IO;
using System.Collections.Generic;
using DG.Tweening;
using SimpleJSON;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(AOCQuickTest))]
public class AOCQuickTestEditor : Editor {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private AOCQuickTest targetTest { get { return target as AOCQuickTest; }}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI() {
		// Default
		DrawDefaultInspector();

		// Test button
		EditorGUILayout.Space();
		if(GUILayout.Button("TEST", GUILayout.Height(50))) {
			targetTest.OnTestButton();
		}

		if(GUILayout.Button("0 -> 1", GUILayout.Height(50))) {
			targetTest.OnTestButton2();
		}

		if(GUILayout.Button("1 -> 0", GUILayout.Height(50))) {
			targetTest.OnTestButton3();
		}

		if(GUILayout.Button("INTERRUPT", GUILayout.Height(50))) {
			targetTest.OnTestButton4();
		}

		EditorGUILayout.Space();
		if(GUILayout.Button("CHECK BUILD SCENES", GUILayout.Height(50))) {
			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			for(int i = 0; i < scenes.Length; ++i) {
				EditorBuildSettingsScene scene = scenes[i];
				if(EditorUtility.DisplayCancelableProgressBar("Checking scenes...", (i+1) + "/" + scenes.Length + ": " + scene.path, ((float)(i+1)/(float)scenes.Length))) {
					break;
				}
				Debug.Log("<color=green>OPENING " + scene.path + "</color>");
				UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene.path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
				Debug.Log("<color=red>CLOSING " + scene.path + "</color>");
				UnityEditor.SceneManagement.EditorSceneManager.CloseScene(UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scene.path), true);
			}
			EditorUtility.ClearProgressBar();
		}

		EditorGUILayout.Space();
		EditorGUILayoutExt.Separator("Tournament Leaderboard Generator");

		int leaderboardSize = EditorPrefs.GetInt("AOCQickTest.TournamentLeaderboardSize", 100);
		leaderboardSize = EditorGUILayout.IntField("Leaderboard Size", leaderboardSize);
		EditorPrefs.SetInt("AOCQickTest.TournamentLeaderboardSize", leaderboardSize);

		int maxScore = EditorPrefs.GetInt("AOCQickTest.TournamentLeaderboardMaxScore", 5000);
		maxScore = EditorGUILayout.IntField("Max Score", maxScore);
		EditorPrefs.SetInt("AOCQickTest.TournamentLeaderboardMaxScore", maxScore);

		if(GUILayout.Button("GENERATE TOURNAMENT LEADERBOARD JSON", GUILayout.Height(50))) {
			string[] firstNames = File.ReadAllLines(StringUtils.SafePath(Application.dataPath + "/HDLiveEventsTest/first_names.txt"));
			string[] lastNames = File.ReadAllLines(StringUtils.SafePath(Application.dataPath + "/HDLiveEventsTest/last_names.txt"));
			int picRandomSeed = UnityEngine.Random.Range(1, 500);
			int score = maxScore;
			JSONClass playerData = null;

			JSONClass data = new JSONClass();
			JSONArray array = new JSONArray();
			for(int i = 0; i < leaderboardSize; ++i) {
				string name = firstNames.GetRandomValue() + " " + lastNames.GetRandomValue();

				score = UnityEngine.Random.Range((int)(score * 0.9f), score);

				playerData = new JSONClass();
				playerData.Add("name", name);
				playerData.Add("pic", "https://picsum.photos/200/200/?image=" + (picRandomSeed + i).ToString());
				playerData.Add("score", score);
				array.Add(playerData);
			}

			data.Add("l", array);
			data.Add("n", leaderboardSize);

			int rank = UnityEngine.Random.Range(0, array.Count);
			JSONNode randomPlayerData = array[rank];
			JSONClass currentPlayerData = new JSONClass();
			currentPlayerData.Add("userId", UnityEngine.Random.Range(0, 5000000));
			currentPlayerData.Add("score", randomPlayerData["score"]);
			currentPlayerData.Add("rank", rank);
			data.Add("u", currentPlayerData);

			JsonFormatter fmt = new JsonFormatter();
			Debug.Log(fmt.PrettyPrint(data.ToString()));

			File.WriteAllText(
				StringUtils.SafePath(Application.dataPath + "/HDLiveEventsTest/tournament_leaderboard.json"), 
				fmt.PrettyPrint(data.ToString())
			);
		}
	}

	[MenuItem("Hungry Dragon/AOC/Clear Map Layer")]
	public static void ClearMapLayer() {
		// Convert all objects in the map layer to the default layer
		int mapLayerIdx = LayerMask.NameToLayer("Map");
		int defaultLayerIdx = LayerMask.NameToLayer("Default");
		int mapAndGameLayerIdx = LayerMask.NameToLayer("MapAndGame");
		GameObject[] objs = Resources.FindObjectsOfTypeAll<GameObject>();
		foreach(GameObject go in objs) {
			if(go.layer == mapLayerIdx) {
				if(go.GetComponent<Collider>() != null) {
					go.layer = mapAndGameLayerIdx;
				} else {
					go.layer = defaultLayerIdx;
				}
			}
		}
	}
}