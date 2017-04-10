// LevelManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/01/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global manager of levels.
/// </summary>
public class LevelManager : Singleton<LevelManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const string LEVEL_DATA_PATH = "Game/Levels/";

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private static string m_currentArea = "";
	public static string currentArea{
		get{ return m_currentArea; }
	}
	private static List<string> m_currentAreaScenes = new List<string>();

	// Shortcut to get the data of the currently selected level
	private static LevelData m_currentLevelData = null;
	public static LevelData currentLevelData {
		get { return m_currentLevelData; }
	}

	private static List<string> m_toSplitScenes = new List<string>();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Given a level, get its definition.
	/// </summary>
	/// <returns>The definition for the requested level. <c>null</c> if the given sku doesn't correspond to any known level.</returns>
	/// <param name="_sku">The sku of the level whose definition we want.</param>
	public static DefinitionNode GetLevelDef(string _sku) {
		return DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVELS, _sku);
	}

	/// <summary>
	/// Given a level, find and load its data object.
	/// </summary>
	/// <returns>The level data object for the requested level. <c>null</c> if the given sku doesn't correspond to any known level or the level data object doesn't exist.</returns>
	/// <param name="_sku">The sku of the level whose data we want.</param>
	public static LevelData GetLevelData(string _sku) {
		// Get the definition
		DefinitionNode def = GetLevelDef(_sku);

		// Load and initialize the level data
		// If the data object doesn't exist, content is wrong!!
		LevelData data = null;
		if(def != null) {
			data = Resources.Load<LevelData>(LEVEL_DATA_PATH + def.GetAsString("dataFile"));
			Debug.Assert(m_currentLevelData != null);
			data.Init(def);
		}
		return data;
	}

	/// <summary>
	/// Define a level as current and load its data.
	/// </summary>
	/// <param name="_sku">The level to be defined as current.</param>
	public static void SetCurrentLevel(string _sku) {
		// Ignore if already loaded
		if(m_currentLevelData != null && m_currentLevelData.def.sku == _sku) {
			return;
		}

		// Clear current data
		m_currentLevelData = null;

		// Load new data
		m_currentLevelData = GetLevelData(_sku);

		m_toSplitScenes = m_currentLevelData.def.GetAsList<string>("split");

	}

	/// <summary>
	/// Starts loading the scenes of the current level (as defined via the <see cref="SetCurrentLevel"/> method).
	/// Deletes any level in the current scene.
	/// Loading is asynchronous, use the returned objects to check when the level has finished loading.
	/// </summary>
	/// <returns>The loading requests, where you can check the loading progress.</returns>
	public static AsyncOperation[] LoadLevel() {
		// Destroy any existing level in the game scene
		LevelEditor.Level[] activeLevels = Component.FindObjectsOfType<LevelEditor.Level>();
		for(int i = 0; i < activeLevels.Length; i++) {
			GameObject.DestroyImmediate(activeLevels[i].gameObject);
		}

		// Has the current level data been loaded?
		DebugUtils.SoftAssert(m_currentLevelData != null, "Current level has not been set!");
		DefinitionNode def = m_currentLevelData.def;

		// Load additively all the scenes of the current level
		List<AsyncOperation> loadingTasks = new List<AsyncOperation>();
		AsyncOperation loadingTask = null;

		// Common Scenes
		List<string> commonScenes = def.GetAsList<string>("common");
		for( int i = 0; i<commonScenes.Count; i++ )
		{
			// TODO: Check if is splitted to use different name
			string sceneName = GetRealSceneName(commonScenes[i]);
			loadingTask = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			if(DebugUtils.SoftAssert(loadingTask != null, "The common scene " + commonScenes[i] + " for level " + def.sku + " couldn't be found (probably mispelled or not added to Build Settings)")) {
				loadingTasks.Add(loadingTask);
			}	
		}
		// Load area by dragon
		m_currentArea = def.Get(UsersManager.currentUser.currentDragon);
		List<AsyncOperation> areaOperations = LoadArea( m_currentArea );
		if ( areaOperations != null )
			loadingTasks.AddRange( areaOperations );

		// Disable auto-scene activation: activating the scenes abuses the CPU, causing fps drops. Since we want the loading screen to be fluid, we will activate all the scene at once when the loading is finished.
		for(int i = 0; i < loadingTasks.Count; i++) {
			loadingTasks[i].allowSceneActivation = false;
		}
		
		return loadingTasks.ToArray();
	}


	public static List<AsyncOperation> LoadArea( string area )
	{
		List<AsyncOperation> loadingTasks = new List<AsyncOperation>();
		AsyncOperation loadingTask = null;
		DefinitionNode def = m_currentLevelData.def;
		m_currentArea = area;
		m_currentAreaScenes = def.GetAsList<string>(m_currentArea);
		for( int i = 0; i<m_currentAreaScenes.Count && !string.IsNullOrEmpty( m_currentAreaScenes[i] ); i++ )
		{
			string sceneName = GetRealSceneName(m_currentAreaScenes[i]);
			loadingTask = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			if(DebugUtils.SoftAssert(loadingTask != null, "The spawners scene " + m_currentAreaScenes[i] + " for level " + def.sku + " couldn't be found (probably mispelled or not added to Build Settings)")) {
				loadingTasks.Add(loadingTask);
			}	
		}
		return loadingTasks;
	}

	public static List<AsyncOperation> UnloadCurrentArea()
	{
		List<AsyncOperation> loadingTasks = new List<AsyncOperation>();
		AsyncOperation loadingTask = null;
		for( int i = 0;i< m_currentAreaScenes.Count; i++ )
		{
			loadingTask = SceneManager.UnloadSceneAsync(m_currentAreaScenes[i]);
			if ( loadingTask != null )
				loadingTasks.Add(loadingTask);
		}
		m_currentAreaScenes.Clear();
		m_currentArea = "";
		return loadingTasks;
	}


	/// <summary>
	/// Make sure the active scene is the one marked as "activeScene" in the current level definition.
	/// Current level must have been set via the <see cref="SetCurrentLevel"/> method.
	/// To be called typically after all the scenes in the level have been loaded.
	/// </summary>
	public static void SetArtSceneActive()
	{
		// [AOC] For some reason, the lightning settings of the Art scene makes the OSX Editor to crash
		// #if !UNITY_EDITOR_OSX
		Scene scene = SceneManager.GetSceneByName(m_currentLevelData.def.GetAsString(m_currentArea + "Active"));
		SceneManager.SetActiveScene(scene);
		// #endif
	}

	public static AsyncOperation[] SwitchArea( string nextArea )
	{
		AsyncOperation[] ret = null;
		if ( m_currentArea != nextArea )
		{
			// Unload current area scenes
			UnloadCurrentArea();

			// Load new area scenes
			ret = LoadArea( nextArea ).ToArray();

		}
		return ret;
	}


	private static string GetRealSceneName( string sceneName )
	{
		if (m_toSplitScenes.Contains( sceneName ))
		{
			switch( FeatureSettingsManager.instance.LevelsLOD )
			{	
				default:
				case FeatureSettings.ELevel3Values.low:
				{
					sceneName += "_low";
				}break;
				case FeatureSettings.ELevel3Values.mid:
				{
					sceneName += "_medium";
				}break;
				case FeatureSettings.ELevel3Values.high:
				{
					sceneName += "_high";
				}break;
			}
		}
		return sceneName;
	}


}