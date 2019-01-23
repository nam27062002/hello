// LeagueLeaderboardGenerator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using SimpleJSON;

using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Editor tool to generate a sample league leaderboard JSON.
/// </summary>
public class LeagueLeaderboardGeneratorEditor : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string TITLE = "League Leaderboard Generator";

	//------------------------------------------------------------------------//
	// STATICS																  //
	//------------------------------------------------------------------------//
	// Windows instance
	public static LeagueLeaderboardGeneratorEditor instance {
		get {
			return (LeagueLeaderboardGeneratorEditor)EditorWindow.GetWindow(typeof(LeagueLeaderboardGeneratorEditor), false, TITLE, true);
		}
	}

	/// <summary>
	/// Shows the window.
	/// </summary>
	public static void ShowWindow() {
		// Setup window
		instance.titleContent = new GUIContent(TITLE);
		instance.minSize = new Vector2(100f, 100f);
		instance.maxSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		// Show it
		instance.Show();
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Setup
	private const string LEADERBOARD_SIZE_KEY = "LeagueLeaderboardGenerator.LeaderboardSize";
	private int leaderboardSize {
		get { return Prefs.GetIntEditor(LEADERBOARD_SIZE_KEY, 100); }
		set { Prefs.SetIntEditor(LEADERBOARD_SIZE_KEY, value); }
	}

	private const string SCORE_RANGE_KEY = "LeagueLeaderboardGenerator.ScoreRange";
	private RangeInt scoreRange {
		get { return Prefs.GetRangeIntEditor(SCORE_RANGE_KEY, new RangeInt(100, 500000)); } 
		set { Prefs.SetRangeIntEditor(SCORE_RANGE_KEY, value); }
	}

	private const string DRAGON_LEVEL_RANGE_KEY = "LeagueLeaderboardGenerator.DragonLevelRange";
	private RangeInt dragonLevelRange {
		get { return Prefs.GetRangeIntEditor(DRAGON_LEVEL_RANGE_KEY, new RangeInt(0, 30)); }
		set { Prefs.SetRangeIntEditor(DRAGON_LEVEL_RANGE_KEY, value); }
	}

	private const string OUTPUT_PATH_KEY = "LeagueLeaderboardGenerator.OutputPath";
	private string outputPath {
		get { return Prefs.GetStringEditor(DRAGON_LEVEL_RANGE_KEY, "HDLiveEventsTest/league_leaderboard_data.json"); }
		set { Prefs.SetStringEditor(DRAGON_LEVEL_RANGE_KEY, value); }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Show setup fields
		leaderboardSize = EditorGUILayout.IntField("Leaderboard Size", leaderboardSize);

		RangeInt tmpRange = scoreRange;
		EditorGUI.BeginChangeCheck(); {
			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.PrefixLabel("Score Range");
				tmpRange.min = EditorGUILayout.IntField("min", tmpRange.min);
				tmpRange.max = EditorGUILayout.IntField("max", tmpRange.max);
			} EditorGUILayout.EndHorizontal();
		} if(EditorGUI.EndChangeCheck()) {
			scoreRange = tmpRange;
		}

		tmpRange = dragonLevelRange;
		EditorGUI.BeginChangeCheck(); {
			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.PrefixLabel("Dragon Level Range");
				tmpRange.min = EditorGUILayout.IntField("min", tmpRange.min);
				tmpRange.max = EditorGUILayout.IntField("max", tmpRange.max);
			} EditorGUILayout.EndHorizontal();
		} if(EditorGUI.EndChangeCheck()) {
			dragonLevelRange = tmpRange;
		}

		GUI.color = Color.green;
		if(GUILayout.Button("GENERATE LEADERBOARD JSON", GUILayout.Height(50))) {
			GenerateLeaderboard();
		}
		GUI.color = Color.white;
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Called less times as if it was OnGUI/Update
	/// </summary>
	public void OnInspectorUpdate() {
		
	}

	/// <summary>
	/// Called multiple times per second on all visible windows.
	/// </summary>
	public void Update() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do it!
	/// </summary>
	private void GenerateLeaderboard() {
		// Read names files
		string[] firstNames = File.ReadAllLines(StringUtils.SafePath(Application.dataPath + "/HDLiveEventsTest/first_names.txt"));
		string[] lastNames = File.ReadAllLines(StringUtils.SafePath(Application.dataPath + "/HDLiveEventsTest/last_names.txt"));

		// Cache setup params to avoid constant reading/writing from prefs
		int size = leaderboardSize;
		RangeInt score = scoreRange;
		RangeInt dragonLevel = dragonLevelRange;

		// Cache some other aux vars
		// If definitions are not loaded, do it now
		if(!ContentManager.ready) ContentManager.InitContent(true, false);
		List<DefinitionNode> specialDragonDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DRAGONS, "type", DragonDataSpecial.TYPE_CODE);
		List<DefinitionNode> petDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);

		// Do it!
		List<HDLiveData.Leaderboard.Record> records = new List<HDLiveData.Leaderboard.Record>();
		for(int i = 0; i < size; ++i) {
			// Create new entry
			// Ranks will be initialized later, when list is sorted
			HDLiveData.Leaderboard.Record newRecord = new HDLiveData.Leaderboard.Record();

			// Player name
			newRecord.name = firstNames.GetRandomValue() + " " + lastNames.GetRandomValue();

			// Score
			newRecord.score = (long)score.GetRandom();

			// Dragon level
			newRecord.build.level = (uint)dragonLevel.GetRandom();

			// Dragon
			newRecord.build.dragon = specialDragonDefs.GetRandomValue().sku;

			// Stats - Distribute randomly until level is reached
			for(int j = 0; j < newRecord.build.level; ++j) {
				switch(UnityEngine.Random.Range(0, 2)) {
					case 0: newRecord.build.health++; break;
					case 1: newRecord.build.speed++; break;
					case 2: newRecord.build.energy++; break;
				}
			}

			// Pets
			int petCount = UnityEngine.Random.Range(0, 4);
			for(int j = 0; j < petCount; ++j) {
				newRecord.build.pets.Add(petDefs.GetRandomValue().sku);
			}

			// Store new record :)
			records.Add(newRecord);
		}

		// Sort by score
		records.Sort(
			(HDLiveData.Leaderboard.Record _r1, HDLiveData.Leaderboard.Record _r2) => {
				return _r1.score.CompareTo(_r2.score);
			}
		);

		// Initialize ranks
		for(int i = 0; i < records.Count; ++i) {
			records[i].rank = i;
		}

		// Select a random entry to be our player
		HDLiveData.Leaderboard.Record playerRecord = records.GetRandomValue();

		// Generate json!
		JSONClass data = new JSONClass();

		// Player
		JSONClass playerData = playerRecord.SaveData();
		playerData.Add("rank", playerRecord.rank);
		data.Add("u", playerData);

		// Records
		JSONArray array = new JSONArray();
		for(int i = 0; i < records.Count; ++i) {
			array.Add(records[i].SaveData());
		}

		data.Add("n", records.Count);
		data.Add("l", array);

		// Print to console
		JsonFormatter fmt = new JsonFormatter();
		Debug.Log(fmt.PrettyPrint(data.ToString()));

		// Print to file
		File.WriteAllText(
			StringUtils.SafePath(Application.dataPath + "/" + outputPath),
			fmt.PrettyPrint(data.ToString())
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}