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

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Setup of the Hungry Dragon menu entry in the Unity Editor menu, giving fast access to several tools.
/// </summary>
public class HungryDragonEditorMenu {
	//------------------------------------------------------------------//
	// CONTENT															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Show the dragon manager content editor.
	/// </summary>
	[MenuItem("Hungry Dragon/Content/Dragon Manager", false, 0)]
	public static void ShowDragonManagerWindow() {
		// Serialize manager to be able to show private members
		DragonManagerEditorWindow window = (DragonManagerEditorWindow)EditorWindow.GetWindow(typeof(DragonManagerEditorWindow));
		window.m_target = new SerializedObject(DragonManager.instance);
		window.titleContent = new GUIContent("Dragon Manager Editor");
		window.ShowUtility();	// To avoid window getting automatically closed when losing focus
	}

	/// <summary>
	/// Show the level manager content editor.
	/// </summary>
	[MenuItem("Hungry Dragon/Content/Level Manager", false, 1)]
	public static void ShowLevelManagerWindow() {
		// Just select the object
		EditorUtils.FocusObject(LevelManager.instance, true, false, true);
	}

	/// <summary>
	/// Show the missions definitions content editor.
	/// </summary>
	[MenuItem("Hungry Dragon/Content/Mission Definitions", false, 2)]
	public static void ShowMissionDefintions() {
		// Find and select the scriptable object (by name)
		Object targetObj = AssetDatabase.LoadMainAssetAtPath("Assets/Resources/Definitions/MissionDefinitions.asset");
		EditorUtils.FocusObject(targetObj, true, false, true);
	}

	//------------------------------------------------------------------//
	// TOOLS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Regenerate the icon for all the spawners in the scene.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Generate Spawner Icons", false, 2)]
	public static void GenerateSpawnerIcons() {
		SpawnerIconGeneratorEditor.GenerateSpawnerIconsInScene();
	}

	//------------------------------------------------------------------//
	// OTHERS															//
	//------------------------------------------------------------------//
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
		PersistenceProfilesEditorWindow window = (PersistenceProfilesEditorWindow)EditorWindow.GetWindow(typeof(PersistenceProfilesEditorWindow));
		
		// Setup window
		window.titleContent = new GUIContent("Persistence Profiles");
		window.minSize = new Vector2(PersistenceProfilesEditorWindow.SPACING + PersistenceProfilesEditorWindow.PROFILE_LIST_COLUMN_WIDTH + PersistenceProfilesEditorWindow.PROFILE_VIEW_COLUMN_WIDTH + PersistenceProfilesEditorWindow.SPACING, PersistenceProfilesEditorWindow.MIN_WINDOW_HEIGHT);	// Fixed width, arbitrary minimum
		window.maxSize = new Vector2(window.minSize.x, float.PositiveInfinity);						// Fixed width, limitless
		
		// Reset some vars
		window.m_newProfileName = "";
		
		// Show it
		window.Show();	// In this case we actually want the window to be closed when losing focus so the temp object created to display savegames is properly destroyed
	}
}