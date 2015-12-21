// SectionLevels.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// 
	/// </summary>
	public class SectionLevels : ILevelEditorSection {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		private static readonly string ASSETS_DIR = "Assets/Resources/Game/Levels";
		private static readonly float AUTOSAVE_FREQUENCY = 60f;	// Seconds

		//------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											//
		//------------------------------------------------------------------//
		// Current level - one level for each tab
		private Level[] m_activeLevel = new Level[(int)LevelEditorSettings.Mode.COUNT];
		public Level activeLevel { 
			get { return m_activeLevel[(int)LevelEditor.settings.selectedMode]; }
			set { m_activeLevel[(int)LevelEditor.settings.selectedMode] = value; }
		}	

		// Internal
		private FileInfo[] m_fileList = null;
		private float m_autoSaveTimer = 0f;

		// Every type of scene goes in a different sub-folder
		private string assetDirForCurrentMode {
			get { 
				switch(LevelEditor.settings.selectedMode) {
					case LevelEditorSettings.Mode.SPAWNERS:		return ASSETS_DIR + "/" + "Spawners";	break;
					case LevelEditorSettings.Mode.COLLISION:	return ASSETS_DIR + "/" + "Collision";	break;
					case LevelEditorSettings.Mode.ART:			return ASSETS_DIR + "/" + "Art";		break;
				}
				return ASSETS_DIR;
			}
		}
		
		//------------------------------------------------------------------//
		// INTERFACE IMPLEMENTATION											//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		public void Init() {
			// Figure out active level for each mode
			// This is called every time the editor is loaded or when switching to/from play mode
			for(int mode = 0; mode < (int)LevelEditorSettings.Mode.COUNT; mode++) {
				// Find all loaded levels of the target type in the current scene (should only be one)
				List<Level> levelsInScene = new List<Level>();
				switch((LevelEditorSettings.Mode)mode) {
					case LevelEditorSettings.Mode.SPAWNERS:		levelsInScene.AddRange(GameObject.FindObjectsOfType<LevelTypeSpawners>());	break;
					case LevelEditorSettings.Mode.COLLISION:	levelsInScene.AddRange(GameObject.FindObjectsOfType<LevelTypeCollision>());	break;
					case LevelEditorSettings.Mode.ART:			levelsInScene.AddRange(GameObject.FindObjectsOfType<LevelTypeArt>());		break;
				}

				// Several cases:
				// a) There are no levels, start without level
				if(levelsInScene.Count == 0) {
					m_activeLevel[mode] = null;
				}

				// b) There is only one level, use it as current level
				else if(levelsInScene.Count == 1) {
					m_activeLevel[mode] = levelsInScene[0];
				}

				// c) There are multiple levels in the scene
				else {
					// Iterate them and prompt user what to do with each of them
					int newLevelIdx = -1;
					for(int i = 0; i < levelsInScene.Count; i++) {
						// Unity makes it easy for us ^_^
						m_activeLevel[mode] = levelsInScene[i];
						int whatToDo = EditorUtility.DisplayDialogComplex(
							m_activeLevel[mode].gameObject.scene.name,
							"There are multiple levels loaded into the current scene, but only one should be loaded at a time.\n" +
							"What would you like to do with level " + activeLevel.gameObject.scene.name + "?",
							"Make Active", "Save and Unload", "Unload"
						);

						// a) Make it active
						if(whatToDo == 0) {
							// If there was already a selected level, unload it
							if(newLevelIdx >= 0) {
								m_activeLevel[mode] = levelsInScene[newLevelIdx];

								// Prompt user to save
								if(EditorUtility.DisplayDialog(
									"Changing Active Level",
									"Level " + activeLevel.gameObject.scene.name + " was already chosen as active one.\n" +
									"What you want to do with it?",
									"Save and Unload", "Unload"
								)) {
									// Save level
									SaveLevel();
								}

								// Unload in any case
								UnloadLevel(false);
							}

							// Store new level index
							newLevelIdx = i;
						} 

						// b) Save and unload
						else if(whatToDo == 1) {
							SaveLevel();
							UnloadLevel(false);
						} 

						// c) Just unload
						else {
							UnloadLevel(false);
						}
					}

					// Store new active level
					if(newLevelIdx >= 0) {
						m_activeLevel[mode] = levelsInScene[newLevelIdx];
					} else {
						m_activeLevel[mode] = null;
					}
				}
			}
			
			// Reset autosave timer
			m_autoSaveTimer = 0f;
		}
		
		/// <summary>
		/// Draw the section.
		/// </summary>
		public void OnGUI() {
			// Aux vars
			bool levelLoaded = (activeLevel != null);
			bool playing = EditorApplication.isPlaying;
			DragonId oldDragon = LevelEditor.settings.testDragon;
			DragonId newDragon = oldDragon;

			// Some spacing
			GUILayout.Space(5f);
			
			// Big juicy text showing the current level being edited
			GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel);
			titleStyle.fontSize = 20;
			titleStyle.alignment = TextAnchor.MiddleCenter;
			if(activeLevel != null) {
				GUILayout.Label(activeLevel.gameObject.scene.name, titleStyle);
			} else {
				EditorGUILayout.BeginHorizontal(); {
					GUILayout.FlexibleSpace();
					GUILayout.Label("No level loaded", titleStyle);
					if(GUILayout.Button("Detect", GUILayout.Height(30f))) {
						Init();
					}
					GUILayout.FlexibleSpace();
				} EditorGUILayoutExt.EndHorizontalSafe();
			}
			
			// Some more spacing
			GUILayout.Space(5f);
			
			// Toolbar
			EditorGUILayout.BeginVertical(LevelEditorWindow.instance.styles.boxStyle, GUILayout.Height(1)); {	// [AOC] Requesting a very small size fits the group to its content's actual size
				EditorGUILayout.BeginHorizontal(); {
					// Create
					GUI.enabled = !playing;
					if(GUILayout.Button("New")) {
						OnNewLevelButton();
					}
					
					// Load
					if(GUILayout.Button("Open")) {
						OnOpenLevelButton();
					}
					
					// Save - only if there is a level loaded and changes have been made
					GUI.enabled = levelLoaded && !playing && CheckChanges();
					if(GUILayout.Button("Save")) { 
						OnSaveLevelButton(); 
					}
					
					// Separator
					EditorGUILayoutExt.Separator(new SeparatorAttribute(5f), SeparatorAttribute.Orientation.VERTICAL);
					
					// Unload - only if there is a level loaded
					GUI.enabled = levelLoaded && !playing;
					if(GUILayout.Button("Close")) { 
						OnCloseLevelButton(); 
					}
					
					// Delete - only if there is a level loaded
					GUI.enabled = levelLoaded && !playing;
					if(GUILayout.Button("Delete")) { 
						OnDeleteLevelButton();
					}
					
					GUI.enabled = true;
				} EditorGUILayoutExt.EndHorizontalSafe();
				
				// If level was deleted or closed, don't continue (avoid null references)
				if(levelLoaded && activeLevel == null) return;
				
				// Separator
				EditorGUILayoutExt.Separator(new SeparatorAttribute(5f));
				
				// Dragon selector
				GUI.enabled = levelLoaded && !playing;
				EditorGUILayout.BeginHorizontal(); {
					// Label
					GUILayout.Label("Test Dragon:");
					
					// Dragon selector
					string[] enumNames = System.Enum.GetNames(typeof(DragonId));
					string[] options = new string[(int)DragonId.COUNT];
					for(int i = 0; i < options.Length; i++) {
						options[i] = enumNames[i];
					}
					newDragon = (DragonId)EditorGUILayout.Popup((int)oldDragon, options);
					if(oldDragon != newDragon) LevelEditor.settings.testDragon = newDragon;
				} EditorGUILayoutExt.EndHorizontalSafe();
				GUI.enabled = true;
				
				// Dragon spawners - only in spawner mode
				if(activeLevel is LevelTypeSpawners) {
					EditorGUILayout.BeginHorizontal(); {
						// Show/Create spawn point
						GameObject spawnPointObj = null;
						LevelTypeSpawners spawnersLevel = levelLoaded ? activeLevel as LevelTypeSpawners : null;
						if(levelLoaded) spawnPointObj = spawnersLevel.GetDragonSpawnPoint(newDragon, false);
						if(spawnPointObj == null) {
							GUI.enabled = levelLoaded && !playing;
							if(GUILayout.Button("Create Spawn")) {
								spawnPointObj = spawnersLevel.GetDragonSpawnPoint(newDragon, true);
								EditorUtils.SetObjectIcon(spawnPointObj, EditorUtils.ObjectIcon.LABEL_ORANGE);
								EditorUtils.FocusObject(spawnPointObj);
							}
						} else {
							GUI.enabled = true;
							if(GUILayout.Button("Show Spawn")) {
								EditorUtils.FocusObject(spawnPointObj);
							}
						}
						
						// Focus default spawn point
						GUI.enabled = levelLoaded;
						if(GUILayout.Button("Show Default Spawn")) {
							spawnPointObj = spawnersLevel.GetDragonSpawnPoint(DragonId.NONE);
							EditorUtils.FocusObject(spawnPointObj);
							EditorUtils.SetObjectIcon(spawnPointObj, EditorUtils.ObjectIcon.LABEL_ORANGE);	// Make sure we can see something :P
						}
						
						GUI.enabled = true;
					} EditorGUILayoutExt.EndHorizontalSafe();
				}
				GUI.enabled = true;
				
				// Separator
				EditorGUILayoutExt.Separator(new SeparatorAttribute(5f));

				// Other settings
				EditorGUILayout.BeginHorizontal(); {
					// Snap size
					EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent("Snap Size:")).x;
					float newSnapSize = EditorGUILayout.FloatField("Snap Size:", LevelEditor.settings.snapSize);
					newSnapSize = MathUtils.Snap(newSnapSize, 0.01f);	// Round up to 2 decimals
					newSnapSize = Mathf.Max(newSnapSize, 0f);	// Not negative
					LevelEditor.settings.snapSize = newSnapSize;

					// Handler size
					EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent("Handlers Size:")).x;
					float newHandlersSize = EditorGUILayout.FloatField("Handlers Size:", LevelEditor.settings.handlersSize);
					newHandlersSize = MathUtils.Snap(newHandlersSize, 0.1f);	// Round up to 1 decimal
					newHandlersSize = Mathf.Max(newHandlersSize, 0f);	// Not negative
					LevelEditor.settings.handlersSize = newHandlersSize;
					EditorGUIUtility.labelWidth = 0;
				} EditorGUILayoutExt.EndHorizontalSafe();
			} EditorGUILayoutExt.EndVerticalSafe();
		}

		//------------------------------------------------------------------//
		// INTERNAL METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Prompts the save dialog and saves/discards the current level based on user's answer.
		/// </summary>
		private void PromptSaveDialog() {
			// Nothing to do if there is no loaded level
			if(activeLevel == null) return;
			
			// Only prompt if there are changes to actually be saved
			if(CheckChanges()) {
				// Prompt dialog
				if(EditorUtility.DisplayDialog("Save Level", "Save changes in current level?", "Yes", "No")) {
					// Just do it
					SaveLevel();
				}
			}
		}
		
		/// <summary>
		/// Applies changes to current loaded level's scene.
		/// </summary>
		/// <param name="_name">Optional name to be given to the file. If empty, the scene's current name will be used.</param>
		private void SaveLevel(string _name = "") {
			// Nothing to do if there is no loaded level
			if(activeLevel == null) return;

			// Figure out file name
			if(string.IsNullOrEmpty(_name)) {
				_name = activeLevel.gameObject.scene.name;
			}

			// Save scene to disk - will automatically overwrite any existing scene with the same name
			EditorSceneManager.SaveScene(activeLevel.gameObject.scene, assetDirForCurrentMode + "/" + _name + ".unity");

			// Save assets to disk!!
			AssetDatabase.SaveAssets();
		}
		
		/// <summary>
		/// Unloads the current level, asking the user for saving changes first.
		/// </summary>
		/// <param name="_promptSave">Optionally ask whether to save or not before unloading (and do it).</param>
		private void UnloadLevel(bool _promptSave) {
			// Nothing to do if there is no loaded level
			if(activeLevel == null) return;
			
			// Ask for save
			if(_promptSave) PromptSaveDialog();

			// Close the scene containing the active level
			EditorSceneManager.CloseScene(activeLevel.gameObject.scene, true);
			
			// Clear some references
			activeLevel = null;
		}
		
		/// <summary>
		/// Check changes performed on the current level vs its prefab.
		/// </summary>
		/// <returns><c>true</c>, if the instance was modified, <c>false</c> otherwise.</returns>
		private bool CheckChanges() {
			// Nothing to do if there is no loaded level
			if(activeLevel == null) return false;

			// Unity makes it easy for us
			return activeLevel.gameObject.scene.isDirty;
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// The "New" button has been pressed.
		/// </summary>
		private void OnNewLevelButton() {
			// Unload current level - will ask to save first
			UnloadLevel(true);

			// Find out suffix for the level based on current mode
			string modeSuffix = "";
			switch(LevelEditor.settings.selectedMode) {
				case LevelEditorSettings.Mode.SPAWNERS:		modeSuffix = "_Spawners";	break;
				case LevelEditorSettings.Mode.COLLISION:	modeSuffix = "_Collision";	break;
				case LevelEditorSettings.Mode.ART:			modeSuffix = "_Art";		break;
			}
			
			// Find out a suitable name for the new level
			int i = 0;
			string name = "";
			string path = Application.dataPath + assetDirForCurrentMode.Replace("Assets", "") + "/";	// dataPath already includes the "Assets" directory
			do {
				name = "SC_Level_" + i + modeSuffix;
				i++;
			} while(File.Exists(path + name + ".unity"));

			// Create a new scene with the target name and make it the main one
			Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
			EditorSceneManager.SetActiveScene(newScene);

			// Create a new game object and add to it the Level component corresponding to the current edition mode
			// It will automatically be initialized with the required hierarchy
			// Since the new scene is the active one, it will be added to the root of it
			GameObject newLevelObj = null;
			switch(LevelEditor.settings.selectedMode) {
				case LevelEditorSettings.Mode.SPAWNERS:		newLevelObj = new GameObject("Level", typeof(LevelTypeSpawners));	break;
				case LevelEditorSettings.Mode.COLLISION:	newLevelObj = new GameObject("Level", typeof(LevelTypeCollision));	break;
				case LevelEditorSettings.Mode.ART:			newLevelObj = new GameObject("Level", typeof(LevelTypeArt));		break;
			}
			activeLevel = newLevelObj.GetComponent<Level>();

			// Save the new scene to the default dir with the name we figured out before
			SaveLevel(name);

			// Select the level and ping the scene file
			Selection.activeObject = activeLevel.gameObject;
			EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(assetDirForCurrentMode + "/" + name + ".unity"));
		}
		
		/// <summary>
		/// The "Open" button has been pressed.
		/// </summary>
		private void OnOpenLevelButton() {
			// Open a dialog showing all the levels stored in resources
			// Refresh the list
			string dirPath = Application.dataPath + assetDirForCurrentMode.Replace("Assets", "");	// dataPath already contains the "Assets" directory
			DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
			m_fileList = dirInfo.GetFiles("*.unity");	// Levels are scenes

			// Strip filename from full file path
			string[] fileNames = new string[m_fileList.Length];
			for(int i = 0; i < fileNames.Length; i++) {
				fileNames[i] = Path.GetFileNameWithoutExtension(m_fileList[i].Name);
			}

			// Open a popup displaying all the files
			if(fileNames.Length > 0) {
				// Show selection popup
				SelectionPopupWindow.Show(fileNames, OnLoadLevelSelected);
			}
			
			// If there are no saved levels whatsoever, show a notification instead
			else {
				LevelEditorWindow.instance.ShowNotification(new GUIContent("There are no saved levels"));
			}
		}
		
		/// <summary>
		/// The "Save" button has been pressed.
		/// </summary>
		private void OnSaveLevelButton() {
			// Just do it
			SaveLevel();
		}
		
		/// <summary>
		/// The "Close" button has been pressed.
		/// </summary>
		private void OnCloseLevelButton() {
			// Just do it - ask for saving first
			UnloadLevel(true);
		}
		
		/// <summary>
		/// The "Delete" button has been pressed.
		/// </summary>
		private void OnDeleteLevelButton() {
			// Show confirmation dialog
			if(EditorUtility.DisplayDialog("Delete Level", "Are you sure?", "Yes", "No")) {
				// Just do it
				AssetDatabase.MoveAssetToTrash(assetDirForCurrentMode + "/" + activeLevel.gameObject.scene.name + ".unity");

				// Unload level - don't prompt for saving, of course
				UnloadLevel(false);
			}
		}
		
		/// <summary>
		/// A level has been selected to be loaded.
		/// </summary>
		/// <param name="_selectedIdx">The index of the selected option.</param>
		public void OnLoadLevelSelected(int _selectedIdx) {
			// Check index (just in case)
			if(_selectedIdx < 0 || _selectedIdx >= m_fileList.Length) return;
			
			// Do it!!
			// Unload current level - will ask to save it
			UnloadLevel(true);

			// Load the new level scene and store reference to the level object
			EditorSceneManager.OpenScene(assetDirForCurrentMode + "/" + m_fileList[_selectedIdx].Name, OpenSceneMode.Additive);
			switch(LevelEditor.settings.selectedMode) {
				case LevelEditorSettings.Mode.SPAWNERS:		activeLevel = Object.FindObjectOfType<LevelTypeSpawners>();		break;
				case LevelEditorSettings.Mode.COLLISION:	activeLevel = Object.FindObjectOfType<LevelTypeCollision>();	break;
				case LevelEditorSettings.Mode.ART:			activeLevel = Object.FindObjectOfType<LevelTypeArt>();			break;
			}
			
			// Focus the level object in the hierarchy and ping the opened scene in the project window
			Selection.activeObject = activeLevel.gameObject;
			EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(assetDirForCurrentMode + "/" + activeLevel.gameObject.scene.name + ".unity"));
		}
	}
}