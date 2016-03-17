// HungryDragonEditorMenu.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Setup of the Hungry Dragon menu entry in the Unity Editor menu, giving fast access to several tools.
/// </summary>
public class HungryDragonEditorMenu {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string DEFINITIONS_FOLDER = "Assets/Resources/Definitions/";
	private static readonly string SINGLETONS_FOLDER = "Assets/Resources/Singletons/";

	//------------------------------------------------------------------//
	// MENU SETUP														//
	//------------------------------------------------------------------//
	//-------------------------------------------------- DEFINITIONS -------------------------------------------------//
	[MenuItem("Hungry Dragon/Content/Definitions Manager", false, 0)]
	public static void ShowDefintionsManager() { OpenFile("DefinitionsManager.asset", SINGLETONS_FOLDER); }

	[MenuItem("Hungry Dragon/Content/EntityDefinitions", false, 50)]
	public static void ShowDefintions4() { OpenFile("EntityDefinitions.asset", DEFINITIONS_FOLDER); }

	[MenuItem("Hungry Dragon/Content/EntityCategoryDefinitions", false, 51)]
	public static void ShowDefintions5() { OpenFile("EntityCategoryDefinitions.asset", DEFINITIONS_FOLDER); }

	[MenuItem("Hungry Dragon/Content/LevelDefinitions", false, 100)]
	public static void ShowDefintions6() { OpenFile("LevelDefinitions.asset", DEFINITIONS_FOLDER); }

	[MenuItem("Hungry Dragon/Content/MissionDefinitions", false, 101)]
	public static void ShowDefintions7() { OpenFile("MissionDefinitions.asset", DEFINITIONS_FOLDER); }

	//------------------------------------------------ OTHER MANAGERS ------------------------------------------------//
	[MenuItem("Hungry Dragon/Content/Chest Manager", false, 150)]
	public static void ShowManager1() { OpenFile("ChestManager.asset", SINGLETONS_FOLDER); }

	[MenuItem("Hungry Dragon/Content/Missions Manager", false, 151)]
	public static void ShowManager2() { OpenFile("PF_MissionManager.prefab", SINGLETONS_FOLDER); }

	[MenuItem("Hungry Dragon/Content/Rewards Manager", false, 152)]
	public static void ShowManager3() { OpenFile("PF_RewardManager.prefab", SINGLETONS_FOLDER); }

	//--------------------------------------------------- SETTINGS ---------------------------------------------------//
	[MenuItem("Hungry Dragon/Content/Game Settings", false, 200)]
	public static void ShowSettings1() { OpenFile("GameSettings.asset", SINGLETONS_FOLDER); }

	//---------------------------------------------------- TOOLS -----------------------------------------------------//
	/// <summary>
	/// Regenerate the icon for all the spawners in the scene.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Generate Spawner Icons", false, 2)]
	public static void GenerateSpawnerIcons() {
		SpawnerIconGeneratorEditor.GenerateSpawnerIconsInScene();
	}

	/// <summary>
	/// Saves all assets to disk. Useful to make sure changes in scriptable object instances are stored.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Save Assets", false, 3)]
	public static void SaveAssets() {
		AssetDatabase.SaveAssets();
	}

	//--------------------------------------------------- OTHERS -----------------------------------------------------//
	/// <summary>
	/// Add menu item to be able to open the level editor.
	/// </summary>
	[MenuItem("Hungry Dragon/Level Editor", false, 10)]
	public static void ShowLevelEditorWindow() {
		// Show existing window instance. If one doesn't exist, make one.
		LevelEditor.LevelEditorWindow window = LevelEditor.LevelEditorWindow.instance;
		
		// Setup window
		window.titleContent = new GUIContent("Level Editor");
		window.minSize = new Vector2(330f, 350f);	// Min required width to properly fit all the content
		//window.maxSize = new Vector2(window.minSize.x, window.minSize.y);
		window.position = new Rect(100f, 100f, 540f, Screen.currentResolution.height - 110f);
		
		// Make sure everything is initialized properly
		window.Init();
		
		// Show it
		window.ShowTab();
	}

	/// <summary>
	/// Add menu item to be open the persistence profiles editor.
	/// </summary>
	[MenuItem("Hungry Dragon/Persistence Profiles", false, 11)]
	public static void ShowPersistenceProfilesWindow() {
		// Show existing window instance. If one doesn't exist, make one.
		PersistenceProfilesEditorWindow window = PersistenceProfilesEditorWindow.instance;
		
		// Setup window
		window.titleContent = new GUIContent("Persistence Profiles");
		window.minSize = new Vector2(PersistenceProfilesEditorWindow.SPACING + PersistenceProfilesEditorWindow.PROFILE_LIST_COLUMN_WIDTH + PersistenceProfilesEditorWindow.PROFILE_VIEW_COLUMN_WIDTH + PersistenceProfilesEditorWindow.SPACING, PersistenceProfilesEditorWindow.MIN_WINDOW_HEIGHT);	// Fixed width, arbitrary minimum
		window.maxSize = new Vector2(window.minSize.x, float.PositiveInfinity);						// Fixed width, limitless
		
		// Reset some vars
		window.m_newProfileName = "";
		
		// Show it
		window.Show();	// In this case we actually want the window to be closed when losing focus so the temp object created to display savegames is properly destroyed
	}

	//----------------------------------------------- SCENE SHORTCUTS -------------------------------------------------//
	[MenuItem("Hungry Dragon/Scenes/SC_Loading", false, 50)]
	public static void OpenScene1() { OpenScene("Assets/Game/Scenes/SC_Loading.unity"); }

	[MenuItem("Hungry Dragon/Scenes/SC_Menu", false, 51)]
	public static void OpenScene2() { OpenScene("Assets/Game/Scenes/SC_Menu.unity"); }

	[MenuItem("Hungry Dragon/Scenes/SC_Game", false, 52)]
	public static void OpenScene3() { OpenScene("Assets/Game/Scenes/SC_Game.unity"); }

	[MenuItem("Hungry Dragon/Scenes/SC_LevelEditor", false, 101)]
	public static void OpenScene4() { OpenScene("Assets/Tools/LevelEditor/SC_LevelEditor.unity"); }

	[MenuItem("Hungry Dragon/Scenes/SC_Popups", false, 151)]
	public static void OpenScene5() { OpenScene("Assets/Tests/SC_Popups.unity"); }

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Open and select a file (scriptable object, prefab).
	/// </summary>
	/// <param name="_fileName">The name of the file, including extension.</param>
	/// <param name="_folderPath">The path of folder containing such file, optional. From project's root and ending with a '/' (i.e. "Assets/Resources/MyFolder/").</param>
	public static void OpenFile(string _fileName, string _folderPath) {
		// Just find and select the scriptable object
		Object targetObj = AssetDatabase.LoadMainAssetAtPath(_folderPath + _fileName);
		EditorUtils.FocusObject(targetObj, true, false, true);
	}

	/// <summary>
	/// Open the scene with the given name.
	/// </summary>
	/// <param name="_sceneName">The path of the scene starting at project root and with extension (e.g. "Assets/MyScenesFolder/MyScene.unity").</param>
	public static void OpenScene(string _scenePath) {
		// Just do it
		EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Single);	// Will close all open scenes
	}
}