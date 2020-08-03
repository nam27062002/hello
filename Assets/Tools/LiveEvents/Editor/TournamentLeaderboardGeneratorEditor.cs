// TournamentLeaderboardGeneratorEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/06/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Collections.Generic;
using SimpleJSON;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Utility script which deletes all empty folders under Assets folder
/// </summary>
public class TournamentLeaderboardGeneratorEditor : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show the window!
	/// </summary>
	public static TournamentLeaderboardGeneratorEditor ShowWindow() {
		// Get window!
		TournamentLeaderboardGeneratorEditor window = EditorWindow.GetWindow<TournamentLeaderboardGeneratorEditor>(true);

		// Windows title
		window.titleContent = new GUIContent("Tournament Leaderboard Generator");

		// Set window size
		window.minSize = new Vector2(400f, EditorGUIUtility.singleLineHeight * 8);//100f);
		window.maxSize = window.minSize;	// Not resizeable

		// Show!
		window.Show();

		return window;
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	private void OnGUI() {
		int leaderboardSize = EditorPrefs.GetInt("TournamentLeaderboardGeneratorEditor.TournamentLeaderboardSize", 100);
		leaderboardSize = EditorGUILayout.IntField("Leaderboard Size", leaderboardSize);
		EditorPrefs.SetInt("TournamentLeaderboardGeneratorEditor.TournamentLeaderboardSize", leaderboardSize);

		int maxScore = EditorPrefs.GetInt("TournamentLeaderboardGeneratorEditor.TournamentLeaderboardMaxScore", 5000);
		maxScore = EditorGUILayout.IntField("Max Score", maxScore);
		EditorPrefs.SetInt("TournamentLeaderboardGeneratorEditor.TournamentLeaderboardMaxScore", maxScore);

		if(GUILayout.Button("RELOAD NAME POOLS", GUILayout.Height(50))) {
			LeaderboardGenerator.ReloadPools();
		}

		if(GUILayout.Button("GENERATE TOURNAMENT LEADERBOARD JSON", GUILayout.Height(50))) {
			int picRandomSeed = UnityEngine.Random.Range(1, 500);
			int score = maxScore;
			JSONClass playerData = null;

			JSONClass data = new JSONClass();
			JSONArray array = new JSONArray();
			for(int i = 0; i < leaderboardSize; ++i) {
				score = UnityEngine.Random.Range((int)(score * 0.9f), score);

				playerData = new JSONClass();
				playerData.Add("name", LeaderboardGenerator.GenerateRandomName());
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
}
