// LevelEditorWindow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/09/2015.
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
/// <summary>
/// Custom editor windows to simplify Hungry Dragon's level design.
/// </summary>
public class LevelEditorWindow : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string RESOURCES_DIR = "Game/Levels";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Level control
	[SerializeField] private Level m_activeLevel = null;
	[SerializeField] private FileInfo[] m_fileList = null;
	[SerializeField] private string m_sceneName = "";

	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Add menu item to be able to open the editor.
	/// </summary>
	[MenuItem("Hungry Dragon/Level Editor")]
	public static void ShowWindow() {
		// Show existing window instance. If one doesn't exist, make one.
		LevelEditorWindow window = (LevelEditorWindow)EditorWindow.GetWindow(typeof(LevelEditorWindow));
		
		// Setup window
		window.titleContent = new GUIContent("Level Editor");
		window.minSize = new Vector2(250f, 200f);
		//window.maxSize = new Vector2(window.minSize.x, window.minSize.y);

		// Make sure everything is initialized properly
		window.Init();
		
		// Show it
		window.ShowTab();
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// The window has been enabled - similar to the constructor.
	/// </summary>
	public void OnEnable() {
		// We must detect when application goes to play mode and back, so subscribe to the event
		EditorApplication.playmodeStateChanged += OnPlayModeChanged;

		// Make sure we parse the scene properly the first time
		Init();
	}

	/// <summary>
	/// The window has been disabled - similar to the destructor.
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from the event
		EditorApplication.playmodeStateChanged -= OnPlayModeChanged;
	}

	/// <summary>
	/// Called 100 times per second on all visible windows.
	/// </summary>
	public void Update() {
		// If loaded scene has changed, check it.
		if(EditorApplication.currentScene != m_sceneName) {
			m_sceneName = EditorApplication.currentScene;
			Init();
		}
	}

	/// <summary>
	/// OnDestroy is called when the EditorWindow is closed.
	/// </summary>
	public void OnDestroy() {
		// Unload (prompting to save) active level
		UnloadLevel(true);

		// Clear any other reference
		m_fileList = null;
	}

	//------------------------------------------------------------------//
	// CUSTOM METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Editor initialization.
	/// </summary>
	public void Init() {
		// Store scene name
		m_sceneName = EditorApplication.currentScene;

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
	}

	//------------------------------------------------------------------//
	// WINDOW METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Reset indentation
		EditorGUI.indentLevel = 0;

		EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true)); {
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
			GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox);
			boxStyle.padding = new RectOffset(10, 10, 10, 10);
			EditorGUILayout.BeginVertical(boxStyle); {
				EditorGUILayout.BeginHorizontal(); {
					// Create
					GUI.enabled = !EditorApplication.isPlaying;
					if(GUILayout.Button("New")) {
						OnNewLevelButton();
					}
					GUI.enabled = true;

					// Load
					GUI.enabled = !EditorApplication.isPlaying;
					if(GUILayout.Button("Open")) {
						OnOpenLevelButton();
					}
					GUI.enabled = true;
					
					// Save - only if there is a level loaded and changes have been made
					GUI.enabled = (m_activeLevel != null && !EditorApplication.isPlaying) && CheckChanges();
					if(GUILayout.Button("Save")) { 
						OnSaveLevelButton(); 
					}
					GUI.enabled = true;

					// Separator
					EditorUtils.Separator(EditorUtils.Orientation.VERTICAL, 5f, "", 1f, Colors.silver);

					// Unload - only if there is a level loaded
					GUI.enabled = (m_activeLevel != null && !EditorApplication.isPlaying);
					if(GUILayout.Button("Close")) { 
						OnCloseLevelButton(); 
					}
					GUI.enabled = true;

					// Delete - only if there is a level loaded
					GUI.enabled = (m_activeLevel != null && !EditorApplication.isPlaying);
					if(GUILayout.Button("Delete")) { 
						OnDeleteLevelButton();
					}
					GUI.enabled = true;
				} EditorUtils.EndHorizontalSafe();

				// Separator
				EditorUtils.Separator(EditorUtils.Orientation.HORIZONTAL, 5f, "", 1, Colors.silver);

				// Dragon selector
				// Only enabled if a level is active and application is not active
				GUI.enabled = (m_activeLevel != null && !EditorApplication.isPlaying);
				EditorGUILayout.BeginHorizontal(); {
					// Label
					GUILayout.Label("Test Dragon:");

					// Dragon selector
					string[] enumNames = System.Enum.GetNames(typeof(DragonId));
					string[] options = new string[(int)DragonId.COUNT];
					for(int i = 0; i < options.Length; i++) {
						options[i] = enumNames[i];
					}
					LevelEditor.testDragon = (DragonId)EditorGUILayout.Popup((int)LevelEditor.testDragon, options);
				}  EditorUtils.EndHorizontalSafe();
				GUI.enabled = true;

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
					GUI.enabled = true;
				}
				GUI.enabled = true;
			} EditorUtils.EndVerticalSafe();

			// A level is loaded, show editing stuff
			if(m_activeLevel != null && !EditorApplication.isPlaying) {
				// Spacing
				GUILayout.Space(10f);

				// Duplicate selected
				// Only for game objects within the level (and not the level itself or their main objects)
				GUI.enabled = Selection.activeGameObject != null
						&& Selection.activeGameObject != m_activeLevel.gameObject
						&& Selection.activeGameObject != m_activeLevel.decoObj
						&& Selection.activeGameObject != m_activeLevel.editorObj
						&& Selection.activeGameObject != m_activeLevel.spawnersObj
						&& Selection.activeGameObject != m_activeLevel.terrainObj
						&& Selection.activeGameObject.GetComponentInParent<Level>() != null;
				if(GUILayout.Button("Duplicated Selected Object")) {
					GameObject selectedObj = Selection.activeGameObject;
					GameObject copyObj = GameObject.Instantiate<GameObject>(selectedObj);
					copyObj.name = selectedObj.name;
					copyObj.transform.SetParent(selectedObj.transform.parent, false);
					Selection.activeGameObject = copyObj;
					EditorGUIUtility.PingObject(copyObj);
				}
				GUI.enabled = true;

				// Add ground piece
				if(GUILayout.Button("Add Ground Piece")) {
					// An external window will manage it
					AddGroundPieceWindow.Show(m_activeLevel);
				}

				// Add spawner
				if(GUILayout.Button("Add Spawner")) {
					// [AOC] TODO!!
					ShowNotification(new GUIContent("TODO!!"));
				}
			}

			// Flexible space to fill up the rest of the window
			GUILayout.FlexibleSpace();
		} EditorUtils.EndVerticalSafe();
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
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
		DestroyImmediate(m_activeLevel.gameObject);
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
			ShowNotification(new GUIContent("There are no saved levels"));
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
				// Ping it before deleting it
				EditorGUIUtility.PingObject(prefabObj);

				// Do it
				string path = "Assets/Resources/" + RESOURCES_DIR + "/" + prefabObj.name + ".prefab";
				AssetDatabase.DeleteAsset(path);
			}

			// Unload level - don't prompt for saving, of course
			UnloadLevel(false);
		}
	}

	/// <summary>
	/// The application is being played or stopped.
	/// </summary>
	public void OnPlayModeChanged() {
		// Window is recreated and all values are lost, so re-init everything
		Init();
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

	/// <summary>
	/// Called whenever the scene hierarchy has changed.
	/// </summary>
	private void OnHierarchyChange() {
		// Just force a repaint
		Repaint();
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// </summary>
	private void OnInspectorUpdate() {
		// Just force a repaint
		Repaint();
	}
}
