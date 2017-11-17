// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/11/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple editor tool to Search for TID references in open scenes, content and code.
/// </summary>
public class FindTidTool : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static string[] UI_SCENES = {
		"Assets/Game/Scenes/SC_Loading.unity",
		"Assets/Game/Scenes/SC_Menu.unity",
		"Assets/Game/Scenes/SC_Game.unity",
		"Assets/Game/Scenes/SC_ResultsScreen.unity",
		"Assets/Tests/SC_Popups.unity"
	};

	private static string[] CODE_FOLDERS = {
		"Calety",
		"Engine",
		"FGOL",
		"Game"
	};

	private const string CONTENT_PATH = "Resources/Rules";
	private const string TEXT_PATH = "Localization/english";	// From resources

	private class Match {
		public string tid;
		public List<string> localizerPaths = new List<string>();	// [AOC] BUG!! For some reason Localizers references are lost after the OnGUI call :(
		public List<FileInfo> contentFiles = new List<FileInfo>();
		public List<FileInfo> codeFiles = new List<FileInfo>();

		public int totalCount {
			get { return localizerPaths.Count + contentFiles.Count + codeFiles.Count; }
		}

		public Match(string _tid) { tid = _tid; }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Where to scan? Store in the editor prefs
	private static bool scanUI {
		get { return EditorPrefs.GetBool("FindTidTool.scanUI", true); }
		set { EditorPrefs.SetBool("FindTidTool.scanUI", value); }
	}

	private static bool scanContent {
		get { return EditorPrefs.GetBool("FindTidTool.scanContent", true); }
		set { EditorPrefs.SetBool("FindTidTool.scanContent", value); }
	}

	private static bool scanCode {
		get { return EditorPrefs.GetBool("FindTidTool.scanCode", true); }
		set { EditorPrefs.SetBool("FindTidTool.scanCode", value); }
	}

	private static bool findAllMatches {
		get { return EditorPrefs.GetBool("FindTidTool.findAllMatches", false); }
		set { EditorPrefs.SetBool("FindTidTool.findAllMatches", value); }
	}

	private static bool showEmptyMatchesOnly {
		get { return EditorPrefs.GetBool("FindTidTool.showEmptyMatchesOnly", false); }
		set { EditorPrefs.SetBool("FindTidTool.showEmptyMatchesOnly", value); }
	}

	// Assets management
	private static bool keepScenesLoaded {
		get { return EditorPrefs.GetBool("FindTidTool.keepScenesLoaded", true); }
		set { EditorPrefs.SetBool("FindTidTool.keepScenesLoaded", value); }
	}

	// Internal vars
	private static string m_targetTids = "";
	private static Dictionary<string, Match> m_matches = new Dictionary<string, Match>();

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the window.
	/// </summary>
	public static void ShowWindow() {
		FindTidTool window = (FindTidTool)EditorWindow.GetWindow(typeof(FindTidTool));
		window.titleContent.text = "Find TID Tool";
		window.minSize = new Vector2(475f, 400f);
		window.Show();
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	private void OnGUI() {
		// initial space
		EditorGUILayout.Space();

		// Look for a specific TID(s)
		GUILayout.Label("Manually insert a list of TIDs to be checked (separated by ;) or do a full pass for all TIDs", CustomEditorStyles.commentLabelLeft);
		EditorGUILayout.BeginHorizontal(); {
			// Aux
			float buttonWidth = 150f;

			// Target tids
			m_targetTids = EditorGUILayout.TextArea(
				m_targetTids, 
				CustomEditorStyles.wrappedTextAreaStyle,
				GUILayout.Width(Screen.width - 15f - buttonWidth), // Super random margin
				GUILayout.Height(40)
			);

			// Button: Disabled if target tid is empty
			GUI.enabled = !string.IsNullOrEmpty(m_targetTids);
			if(GUILayout.Button("Find Specific TID(s)\n(Use ; for multiple TIDs)", GUILayout.Width(buttonWidth), GUILayout.Height(40))) {
				// Split target tids
				List<string> targetTids = m_targetTids.Split(new char[] { ';' }).ToList();
				ScanTids(targetTids);
			}
			GUI.enabled = true;
		} EditorGUILayout.EndHorizontal();

		// Scan all TIDs
		if(GUILayout.Button("Scan All TIDs\n(takes several minutes)", GUILayout.Height(40))) {
			// Load english language file
			TextAsset tidsText = Resources.Load<TextAsset>(TEXT_PATH);
			string text = tidsText.text;

			// Scan all TIDs and put them in the list
			List<string> targetTids = new List<string>();
			using(StringReader sr = new StringReader(text)) {
				// Read line by line
				string line;
				char[] separator = new char[] { '=' };
				while((line = sr.ReadLine()) != null) {
					// Split the line at the separator char
					// TID will always be the first split section
					targetTids.Add(line.Split(separator)[0]);
				}
			}

			// Do it!
			ScanTids(targetTids);
		}

		// Setup Section
		EditorGUILayoutExt.Separator("Setup");
		scanUI = EditorGUILayout.Toggle("Scan UI Localizers", scanUI);
		scanContent = EditorGUILayout.Toggle("Scan Content", scanContent);
		scanCode = EditorGUILayout.Toggle("Scan Source Code", scanCode);

		EditorGUILayout.Space();

		findAllMatches = EditorGUILayout.Toggle("Find All Matches", findAllMatches);
		showEmptyMatchesOnly = EditorGUILayout.Toggle("Show Empty Matches Only", showEmptyMatchesOnly);
		keepScenesLoaded = EditorGUILayout.Toggle("Keep Scenes Loaded", keepScenesLoaded);

		// Output Section
		EditorGUILayoutExt.Separator("Results Output");
		Vector2 buttonSize = new Vector2(
			(Screen.width - 19f)/3f,	// Super-random margin
			40f
		);

		// Console Output
		EditorGUILayout.BeginHorizontal(); {
			GUI.enabled = m_matches.Count > 0;

			if(GUILayout.Button("Console\nEmpty Matches", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y))) {
				PrintMatchesSummary(true);
			}

			if(GUILayout.Button("Console\nAll Matches (Summary)", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y))) {
				PrintMatchesSummary(false);
			}

			if(GUILayout.Button("Console\nAll Matches (Detailed)", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y))) {
				PrintAllMatches();
			}

			GUI.enabled = true;
		} EditorGUILayout.EndHorizontal();

		// Clipboard Output
		EditorGUILayout.BeginHorizontal(); {
			GUI.enabled = m_matches.Count > 0;

			if(GUILayout.Button("Clipboard\nEmpty Matches", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y))) {
				CopyToClipboard(true, false);
			}

			if(GUILayout.Button("Clipboard\nAll Matches (Summary)", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y))) {
				CopyToClipboard(false, false);
			}

			if(GUILayout.Button("Clipboard\nAll Matches (Detailed)", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y))) {
				CopyToClipboard(false, true);
			}

			GUI.enabled = true;
		} EditorGUILayout.EndHorizontal();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Scan the target tids using current setup.
	/// Show the output in the console.
	/// </summary>
	/// <param name="_targetTids">Tids to be checked..</param>
	private void ScanTids(List<string> _targetTids) {
		// Clear matches
		m_matches.Clear();

		// Ignore if no TIDS
		if(_targetTids.Count == 0) {
			Debug.ClearDeveloperConsole();
			Debug.LogError("<color=red>No tids to be scanned!</color>");
			return;
		}

		// Aux vars
		Match match = null;
		float processedTids = 0f;
		bool canceled = false;
		bool stopAtMatch = !findAllMatches;
		DateTime startTimestamp = DateTime.Now;
		TimeSpan elapsedTime = new TimeSpan();
		string localizerPath = "";

		// Contextual aux vars, need to be declared outside any condition
		List<Localizer> localizers = null;
		List<Localizer> localizersToRemove = null;
		List<FileInfo> contentFiles = null;
		List<FileInfo> codeFiles = null;

		// Contextual initializations
		if(scanUI) {
			// Load required scenes
			LoadUIScenes();

			// Find all Localizer objects (and any other type that might be referencing TIDs)
			localizers = GameObjectExt.FindObjectsOfType<Localizer>(true);	// Include inactive!
			localizersToRemove = new List<Localizer>();

			// Skip localizers with empty tid - will never be a match
			localizers.RemoveAll(
				(Localizer _loc) => {
					return string.IsNullOrEmpty(_loc.tid);
				}
			);
		}

		if(scanContent) {
			// Scan content directory
			contentFiles = new List<FileInfo>();
			DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/" + CONTENT_PATH);
			ScanFiles(dirInfo, "*.xml", ref contentFiles);
		}

		if(scanCode) {
			// Scan all code directories recursively
			codeFiles = new List<FileInfo>();
			for(int i = 0; i < CODE_FOLDERS.Length; ++i) {
				DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/" + CODE_FOLDERS[i]);
				ScanFiles(dirInfo, "*.cs", ref codeFiles);
			}
		}

		// Iterate all tids!
		foreach(string tid in _targetTids) {
			// Update vars
			processedTids++;

			// Create match entry for this tid
			match = new Match(tid);
			m_matches[tid] = match;

			// Scan UI?
			if(scanUI) {
				// Aux vars
				float processedLocs = 0f;

				// Check all localizers until match found
				foreach(Localizer loc in localizers) {
					// Show progress bar
					processedLocs++;
					elapsedTime = DateTime.Now - startTimestamp;
					if(EditorUtility.DisplayCancelableProgressBar(
						"Scanning " + tid + " (" + processedTids + "/" + _targetTids.Count + ")",
						string.Format("{0:D2}:{1:D2}", (int)elapsedTime.TotalMinutes, elapsedTime.Seconds) +
						" Checking Localizer " + processedLocs + "/" + localizers.Count + ": " + loc.name,
						processedLocs/(float)localizers.Count
					)) {
						canceled = true;
						break;
					}

					// Match?
					if(loc.tid == tid) {
						// Yes! Figure out and store path!
						if(loc.transform != loc.transform.root) {
							localizerPath = loc.transform.root.name + "/" + AnimationUtility.CalculateTransformPath(loc.transform, loc.transform.root);
						} else {
							localizerPath = loc.name;
						}
						match.localizerPaths.Add(localizerPath);

						// We wont need this localizer anymore!
						localizersToRemove.Add(loc);

						// Stop at first match?
						if(stopAtMatch) break;	// [AOC] TODO!! No need to check content or code!
					}
				}

				// Purge localizers list
				localizers.RemoveAll(
					(Localizer _loc) => {
						return localizersToRemove.Contains(_loc);
					}
				);
				localizersToRemove.Clear();
			}

			// No need to check anything else if stopping at first match and already got a match for this TID
			if(stopAtMatch && match.totalCount > 0) continue;

			// Scan content?
			if(scanContent) {
				// Aux vars
				float processedFiles = 0f;
				string fileData = "";

				// Check all content files until match found
				foreach(FileInfo file in contentFiles) {
					// Show progress bar
					processedFiles++;
					elapsedTime = DateTime.Now - startTimestamp;
					if(EditorUtility.DisplayCancelableProgressBar(
						"Scanning " + tid + " (" + processedTids + "/" + _targetTids.Count + ")",
						string.Format("{0:D2}:{1:D2}", (int)elapsedTime.TotalMinutes, elapsedTime.Seconds) +
						" Checking Content " + processedFiles + "/" + contentFiles.Count + ": " + file.Name,
						processedFiles/(float)contentFiles.Count
					)) {
						canceled = true;
						break;
					}

					// Open file
					try {
						fileData = file.OpenText().ReadToEnd();
					} catch(System.Exception _e) {
						Debug.LogError(_e.Message);
					}

					// Match?
					if(fileData.Contains(tid)) {
						// Yes! Store
						match.contentFiles.Add(file);

						// Stop at first match?
						if(stopAtMatch) break;	// [AOC] TODO!! No need to check content or code!
					}
				}
			}

			// No need to check anything else if stopping at first match and already got a match for this TID
			if(stopAtMatch && match.totalCount > 0) continue;

			// Scan code?
			if(scanCode) {
				// Aux vars
				float processedFiles = 0f;
				string fileData = "";

				// Check all code files until match found
				foreach(FileInfo file in codeFiles) {
					// Show progress bar
					processedFiles++;
					elapsedTime = DateTime.Now - startTimestamp;
					if(EditorUtility.DisplayCancelableProgressBar(
						"Scanning " + tid + " (" + processedTids + "/" + _targetTids.Count + ")",
						string.Format("{0:D2}:{1:D2}", (int)elapsedTime.TotalMinutes, elapsedTime.Seconds) +
						" Checking Code " + processedFiles + "/" + codeFiles.Count + ": " + file.Name,
						processedFiles/(float)codeFiles.Count
					)) {
						canceled = true;
						break;
					}

					// Open file
					try {
						fileData = file.OpenText().ReadToEnd();
					} catch(System.Exception _e) {
						Debug.LogError(_e.Message);
					}

					// Match?
					if(fileData.Contains(tid)) {
						// Yes! Store
						match.codeFiles.Add(file);

						// Stop at first match?
						if(stopAtMatch) break;	// [AOC] TODO!! No need to check content or code!
					}
				}
			}

			// Stop iterating TIDs if canceled
			if(canceled) break;
		}


		// Contextual finalizations
		if(scanUI) {
			// Unload stuff
			if(!keepScenesLoaded) UnloadUIScenes();
		}

		// Comon finalizations
		EditorUtility.ClearProgressBar();

		// Show summary and copy results to clipboard
		PrintMatchesSummary(false);
		CopyToClipboard(false, true);
	}

	/// <summary>
	/// Loads the user interface scenes.
	/// </summary>
	private void LoadUIScenes() {
		for(int i = 0; i < UI_SCENES.Length; ++i) {
			// Is scene already opened?
			UnityEngine.SceneManagement.Scene sc = EditorSceneManager.GetSceneByPath(UI_SCENES[i]);
			if(!sc.IsValid()) {
				// Open it!
				EditorSceneManager.OpenScene(UI_SCENES[i], OpenSceneMode.Additive);
			}
		}
	}

	/// <summary>
	/// Unload all loaded UI scenes.
	/// </summary>
	private void UnloadUIScenes() {
		for(int i = 0; i < UI_SCENES.Length; ++i) {
			// Is scene already opened?
			UnityEngine.SceneManagement.Scene sc = EditorSceneManager.GetSceneByPath(UI_SCENES[i]);
			if(sc.IsValid()) {
				// Close it!
				EditorSceneManager.CloseScene(sc, true);
			}
		}
	}

	/// <summary>
	/// Scan all the files (recursively) within a folder.
	/// Unity's *.meta files will be excluded.
	/// </summary>
	/// <param name="_dirInfo">Directory to scan.</param>
	/// <param name="_filter">The search string to match against the names of files in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
	/// <param name="_fileList">List where the files will be stored.</param>
	private void ScanFiles(DirectoryInfo _dirInfo, string _filter, ref List<FileInfo> _fileList) {
		// Nothing to do if _dirInfo is not valid or dir doesn't exist
		if(_dirInfo == null) return;
		if(!_dirInfo.Exists) return;

		try {
			// Scan target dir's files
			FileInfo[] files = _dirInfo.GetFiles(_filter).Where(_file => !_file.Extension.EndsWith(".meta")).ToArray();	// Use file filter, exclude .meta files

			// Add files to files list
			_fileList.AddRange(files);

			// Recursively scan subdirs as well
			DirectoryInfo[] dirs = _dirInfo.GetDirectories();
			for(int i = 0; i < dirs.Length; i++) {
				ScanFiles(dirs[i], _filter, ref _fileList);
			}
		} catch(System.Exception _e) {
			Debug.LogError(_e.Message);
		}
	}

	/// <summary>
	/// Print a summary of the matches.
	/// </summary>
	/// <param name="_onlyEmptyMatches">Show only empty matches!</param>
	private void PrintMatchesSummary(bool _onlyEmptyMatches) {
		// Clear console
		Debug.ClearDeveloperConsole();

		// Iterate all matches
		StringBuilder sb = new StringBuilder();
		foreach(KeyValuePair<string, Match> kvp in m_matches) {
			// Do summary
			Match m = kvp.Value;
			if(m.totalCount > 0) {
				// Skip if only doing empty matches
				if(_onlyEmptyMatches) continue;
				sb.Append("<color=green>");
			} else {
				sb.Append("<color=red>");
			}
			sb.Append(m.tid).Append("</color>: ").Append(m.totalCount);

			string nextSeparator = "";
			sb.AppendLine();
			sb.Append("(");
			if(scanUI) {
				sb.Append(m.localizerPaths.Count).Append(" Localizers");
				nextSeparator = " | ";
			}

			if(scanContent) {
				sb.Append(nextSeparator).Append(m.contentFiles.Count).Append(" Content Files");
				nextSeparator = " | ";
			}

			if(scanCode) {
				sb.Append(nextSeparator).Append(m.codeFiles.Count).Append(" Code Files");
				nextSeparator = " | ";
			}
			sb.Append(")");

			// Print!
			Debug.Log(sb.ToString());
			sb.Length = 0;
		}
	}

	/// <summary>
	/// Prints all matches with detailed info.
	/// </summary>
	private void PrintAllMatches() {
		// Clear console
		Debug.ClearDeveloperConsole();

		// Iterate all matches
		foreach(KeyValuePair<string, Match> kvp in m_matches) {
			// Get match
			Match m = kvp.Value;

			// Header
			Debug.Log("<color=green>" + m.tid + "</color>");

			// Localizers
			foreach(string path in m.localizerPaths) {
				// Log!
				Debug.TaggedLog(m.tid + " LOC", path);
			}

			foreach(FileInfo contentFile in m.contentFiles) {
				// Log!
				Debug.TaggedLog(m.tid + " CONT", contentFile.Name);
			}

			foreach(FileInfo codeFile in m.codeFiles) {
				// Log!
				Debug.TaggedLog(m.tid + " CODE", codeFile.Name);
			}
		}
	}

	/// <summary>
	/// Copies a matches summary to the clipboard.
	/// </summary>
	/// <param name="_onlyEmptyMatches">Show only empty matches?</param>
	/// <param name="_detailed">Show extra info?</param>
	private void CopyToClipboard(bool _onlyEmptyMatches, bool _detailed) {
		// Aux vars
		StringBuilder sb = new StringBuilder();

		// Go through matches
		foreach(KeyValuePair<string, Match> kvp in m_matches) {
			// Skip if not empty?
			Match m = kvp.Value;
			if(_onlyEmptyMatches && m.totalCount > 0) continue;

			// Main info
			sb.Append(kvp.Key).Append(": ");
			if(m.totalCount > 0) {
				sb.Append(m.totalCount);
			} else {
				sb.Append("MISS!");
				sb.AppendLine().AppendLine();
				continue;	// Nothing else to do!
			}

			// Show count recap
			string nextSeparator = "";
			sb.Append(" (");
			if(scanUI) {
				sb.Append(m.localizerPaths.Count).Append(" LOC");
				nextSeparator = " | ";
			}

			if(scanContent) {
				sb.Append(nextSeparator).Append(m.contentFiles.Count).Append(" CONT");
				nextSeparator = " | ";
			}

			if(scanCode) {
				sb.Append(nextSeparator).Append(m.codeFiles.Count).Append(" CODE");
				nextSeparator = " | ";
			}
			sb.Append(")");

			// Show detailed info?
			if(_detailed) {	
				// New line
				sb.AppendLine();

				// Localizers
				foreach(string path in m.localizerPaths) {
					sb.Append("\t")
						.Append("LOC ")
						.Append(path)
						.AppendLine();
				}

				foreach(FileInfo contentFile in m.contentFiles) {
					sb.Append("\t")
						.Append("CONT ")
						.Append(contentFile.Name)
						.AppendLine();
				}

				foreach(FileInfo codeFile in m.codeFiles) {
					sb.Append("\t")
						.Append("CODE ")
						.Append(codeFile.Name)
						.AppendLine();
				}
			}

			// Final line
			sb.AppendLine();
		}

		// Copy to clipboard :)
		EditorGUIUtility.systemCopyBuffer = sb.ToString();
	}
}