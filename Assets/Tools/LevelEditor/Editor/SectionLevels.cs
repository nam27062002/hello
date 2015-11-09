// SectionLevels.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.IO;

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
		private static readonly string RESOURCES_DIR = "Game/Levels";
		private static readonly float AUTOSAVE_FREQUENCY = 60f;	// Seconds
		
		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		// Current level
		private Level m_activeLevel = null;
		public Level activeLevel { get { return m_activeLevel; }}

		// Internal
		private FileInfo[] m_fileList = null;
		private float m_autoSaveTimer = 0f;
		
		//------------------------------------------------------------------//
		// INTERFACE IMPLEMENTATION											//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialize this section.
		/// </summary>
		public void Init() {
			// Find all loaded levels in the current scene (shold only be one)
			Level[] levelsInScene = GameObject.FindObjectsOfType<Level>();
			
			// Several cases:
			// a) There are no levels, start without level
			if(levelsInScene.Length == 0) {
				m_activeLevel = null;
			}
			
			// b) There is only one level, use it as current level
			else if(levelsInScene.Length == 1) {
				m_activeLevel = levelsInScene[0];
			}
			
			// c) There are multiple levels in the scene
			else {
				// Iterate them and prompt user what to do with each of them
				int newLevelIdx = -1;
				for(int i = 0; i < levelsInScene.Length; i++) {
					// Unity makes it easy for us ^_^
					m_activeLevel = levelsInScene[i];
					int whatToDo = EditorUtility.DisplayDialogComplex(
						m_activeLevel.gameObject.name,
						"There are multiple levels loaded into the current scene, but only one should be loaded at a time.\n" +
						"What would you like to do with level " + m_activeLevel.gameObject.name + "?",
						"Make Active", "Save and Unload", "Unload"
						);
					
					// a) Make it active
					if(whatToDo == 0) {
						// If there was already a selected level, unload it
						if(newLevelIdx >= 0) {
							m_activeLevel = levelsInScene[newLevelIdx];
							
							// Prompt user to save
							if(EditorUtility.DisplayDialog(
								"Changing Active Level",
								"Level " + m_activeLevel.gameObject.name + " was already chosen as active one.\n" +
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
					m_activeLevel = levelsInScene[newLevelIdx];
				} else {
					m_activeLevel = null;
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
			bool levelLoaded = (m_activeLevel != null);
			bool playing = EditorApplication.isPlaying;
			DragonId oldDragon = LevelEditor.settings.testDragon;
			DragonId newDragon = oldDragon;

			// Some spacing
			GUILayout.Space(5f);
			
			// Big juicy text showing the current level being edited
			GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel);
			titleStyle.fontSize = 20;
			titleStyle.alignment = TextAnchor.MiddleCenter;
			if(m_activeLevel != null) {
				GUILayout.Label(m_activeLevel.gameObject.name, titleStyle);
			} else {
				GUILayout.Label("No level loaded", titleStyle);
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
				if(levelLoaded && m_activeLevel == null) return;
				
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
				
				// Dragon test tools
				EditorGUILayout.BeginHorizontal(); {
					// Show/Create spawn point
					GameObject spawnPointObj = null;
					if(levelLoaded) spawnPointObj = m_activeLevel.GetDragonSpawnPoint(newDragon, false);
					if(spawnPointObj == null) {
						GUI.enabled = levelLoaded && !playing;
						if(GUILayout.Button("Create Spawn")) {
							spawnPointObj = m_activeLevel.GetDragonSpawnPoint(newDragon, true);
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
						spawnPointObj = m_activeLevel.GetDragonSpawnPoint(DragonId.NONE);
						EditorUtils.FocusObject(spawnPointObj);
						EditorUtils.SetObjectIcon(spawnPointObj, EditorUtils.ObjectIcon.LABEL_ORANGE);	// Make sure we can see something :P
					}
					
					GUI.enabled = true;
				} EditorGUILayoutExt.EndHorizontalSafe();
				
				// Test button
				// Reuse the same button to stop the test
				if(EditorApplication.isPlaying) {
					if(GUILayout.Button("Stop Test", GUILayout.Height(40f))) {
						// Just stop execution mode
						EditorApplication.isPlaying = false;
					}
				} else {
					// Only if we have a valid level
					GUI.enabled = (m_activeLevel != null);
					if(GUILayout.Button("Test Level", GUILayout.Height(40f))) {
						// Just start execution mode
						EditorApplication.isPlaying = true;
					}
				}
				GUI.enabled = true;
				
				// Separator
				EditorGUILayoutExt.Separator(new SeparatorAttribute(5f));
				
				// Snap size
				EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent("Snap Size:")).x;
				float newSnapSize = EditorGUILayout.FloatField("Snap Size:", LevelEditor.settings.snapSize);
				newSnapSize = MathUtils.Snap(newSnapSize, 0.01f);	// Round up to 2 decimals
				newSnapSize = Mathf.Max(newSnapSize, 0f);	// Not negative
				LevelEditor.settings.snapSize = newSnapSize;
				EditorGUIUtility.labelWidth = 0;
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
			if(m_activeLevel == null) return;
			
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
		/// Applies changes to current loaded level's prefab.
		/// </summary>
		private void SaveLevel() {
			// Nothing to do if there is no loaded level
			if(m_activeLevel == null) return;
			
			// Save changes
			// If the level is linked to a prefab, replace it
			// Otherwise create a new prefab for it
			GameObject prefabObj = PrefabUtility.GetPrefabParent(m_activeLevel.gameObject) as GameObject;
			if(prefabObj != null) {
				// Replace prefab with the new data
				prefabObj = PrefabUtility.ReplacePrefab(m_activeLevel.gameObject, prefabObj, ReplacePrefabOptions.ConnectToPrefab) as GameObject;
			} else {
				// Generate a unique name
				string dirPath = "Assets/Resources/" + RESOURCES_DIR;
				string path = AssetDatabase.GenerateUniqueAssetPath(dirPath + "/" + m_activeLevel.gameObject.name + ".prefab");
				
				// GenerateUniqueAssetPath creates the name with spaces, whereas we don't want them in our prefab names (naming convention)
				string filename = Path.GetFileName(path);
				filename = filename.Replace(' ', '_');
				path = dirPath + "/" + filename;
				
				// Create the prefab
				prefabObj = PrefabUtility.CreatePrefab(path, m_activeLevel.gameObject, ReplacePrefabOptions.ConnectToPrefab) as GameObject;
				
				// Save assets to disk!!
				AssetDatabase.SaveAssets();
			}
			
			// Highlight prefab in project browser
			EditorGUIUtility.PingObject(prefabObj);
		}
		
		/// <summary>
		/// Unloads the current level, asking the user for saving changes first.
		/// </summary>
		/// <param name="_promptSave">Optionally ask whether to save or not before unloading (and do it).</param>
		private void UnloadLevel(bool _promptSave) {
			// Nothing to do if there is no loaded level
			if(m_activeLevel == null) return;
			
			// Ask for save
			if(_promptSave) PromptSaveDialog();
			
			// Delete current level
			GameObject.DestroyImmediate(m_activeLevel.gameObject);
			
			// Clear some references
			m_activeLevel = null;
		}
		
		/// <summary>
		/// Check changes performed on the current level vs its prefab.
		/// </summary>
		/// <returns><c>true</c>, if the instance was modified, <c>false</c> otherwise.</returns>
		private bool CheckChanges() {
			// Nothing to do if there is no loaded level
			if(m_activeLevel == null) return false;
			
			/*
			// Get prefab object
			GameObject prefabObj = PrefabUtility.GetPrefabParent(m_activeLevel.gameObject) as GameObject;
			if(prefabObj == null) return true;	// There is no prefab for this level, mark it as changed

			// Get changes list
			PropertyModification[] changes = PrefabUtility.GetPropertyModifications(m_activeLevel.gameObject);

			// Unfortunately, some properties are always marked as changed, so we must check them manually (ty Unity -_-')
			// Specifically it's always the root object's transform position and rotation, as well as the rootOrder property
			// We will simplify the way of checking it, even if it's not that reliable as checking property by property
			// Position XYZ + Rotation XYZW + RootOrder are 7 properties, if there are more modified properties, it means at least one legit change exists
			if(changes.Length != 8) {
				return true;
			} else {
				// Check position, rotation and root order compared to the prefab
				if(m_activeLevel.gameObject.transform.localPosition != prefabObj.transform.localPosition) return true;
				if(m_activeLevel.gameObject.transform.localRotation != prefabObj.transform.localRotation) return true;

				// We don't care about root order - we don't have an easy way to check it
			}

			// No legit changes were detected
			return false;
			*/
			
			// [AOC] TODO!! Unfortunately not all changes are detected with this method (i.e. Adding object to the prefab's hierarchy), so let's just spam the save dialog
			return true;
		}
		
		/// <summary>
		/// Refresh the list of stored levels in the resources folder. Will be stored in m_fileList.
		/// </summary>
		private void RefreshLevelsList() {
			// C# makes it easy for us
			string dirPath = Application.dataPath + "/Resources/" + RESOURCES_DIR;
			DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
			m_fileList = dirInfo.GetFiles("*.prefab");
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
			
			// Find out a suitable name for the new level
			int i = 0;
			string name = "";
			string path = Application.dataPath + "/Resources/" + RESOURCES_DIR + "/";
			do {
				name = "PF_Level_" + i;
				i++;
			} while(File.Exists(path + name + ".prefab"));
			
			// Create a new game object and add to it the Level component
			// It will automatically be initialized with the required hierarchy
			GameObject newLevelObj = new GameObject(name, typeof(Level));
			m_activeLevel = newLevelObj.GetComponent<Level>();
		}
		
		/// <summary>
		/// The "Open" button has been pressed.
		/// </summary>
		private void OnOpenLevelButton() {
			// Open a dialog showing all the levels stored in resources
			// Strip filename from full file path
			RefreshLevelsList();
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
				// Find level prefab and delete it
				GameObject prefabObj = PrefabUtility.GetPrefabParent(m_activeLevel.gameObject) as GameObject;
				if(prefabObj != null) {
					// Do it
					string path = "Assets/Resources/" + RESOURCES_DIR + "/" + prefabObj.name + ".prefab";
					AssetDatabase.DeleteAsset(path);
				}
				
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
			
			// Load the new level from resources and instantiate it into the scene
			string path = RESOURCES_DIR + "/" + Path.GetFileNameWithoutExtension(m_fileList[_selectedIdx].Name);
			GameObject levelPrefab = Resources.Load<GameObject>(path);
			GameObject levelObj = PrefabUtility.InstantiatePrefab(levelPrefab) as GameObject;
			m_activeLevel = levelObj.GetComponent<Level>();
			
			// Focus the prefab in the project window as well as the new instance in the hierarchy
			EditorGUIUtility.PingObject(levelPrefab);
			Selection.activeObject = m_activeLevel.gameObject;
		}
	}
}