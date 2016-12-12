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
	// Shortcut to get the data of the currently selected level
	private static LevelData m_currentLevelData = null;
	public static LevelData currentLevelData {
		get { return m_currentLevelData; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void OnEnable() {

	}

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

		// Spawners
		List<string> spawnersScenes = def.GetAsList<string>("spawnersScene");
		for( int i = 0; i<spawnersScenes.Count; i++ )
		{
			loadingTask = SceneManager.LoadSceneAsync(spawnersScenes[i], LoadSceneMode.Additive);
			if(DebugUtils.SoftAssert(loadingTask != null, "The spawners scene " + spawnersScenes[i] + " for level " + def.sku + " couldn't be found (probably mispelled or not added to Build Settings)")) {
				loadingTask.allowSceneActivation = true;
				loadingTasks.Add(loadingTask);
			}	
		}

		// Collision
		List<string> collisionScenes = def.GetAsList<string>("collisionScene");
		for( int i = 0; i<collisionScenes.Count; i++ )
		{
			loadingTask = SceneManager.LoadSceneAsync(collisionScenes[i], LoadSceneMode.Additive);
			if(DebugUtils.SoftAssert(loadingTask != null, "The collision scene " + collisionScenes[i] + " for level " + def.sku + " couldn't be found (probably mispelled or not added to Build Settings)")) {
				loadingTask.allowSceneActivation = true;
				loadingTasks.Add(loadingTask);
			}
		}

		// Sound
		List<string> soundScenes = def.GetAsList<string>("soundScene");
		for( int i = 0; i<soundScenes.Count; i++ )
		{
			if ( !string.IsNullOrEmpty( soundScenes[i]) ){
				loadingTask = SceneManager.LoadSceneAsync(soundScenes[i], LoadSceneMode.Additive);
				if(DebugUtils.SoftAssert(loadingTask != null, "The sound scene " + soundScenes[i] + " for level " + def.sku + " couldn't be found (probably mispelled or not added to Build Settings)")) {
					loadingTask.allowSceneActivation = true;
					loadingTasks.Add(loadingTask);
				}
			}
		}

		// Art
		List<string> artScenes = def.GetAsList<string>("artScene");
		for( int i = 0; i<artScenes.Count; i++ )
		{
			loadingTask = SceneManager.LoadSceneAsync(artScenes[i], LoadSceneMode.Additive);
			if(DebugUtils.SoftAssert(loadingTask != null, "The art scene " + artScenes[i] + " for level " + def.sku + " couldn't be found (probably mispelled or not added to Build Settings)"))  {
				loadingTask.allowSceneActivation = true;
				loadingTasks.Add(loadingTask);
			}
		}
		
		return loadingTasks.ToArray();
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
		Scene scene = SceneManager.GetSceneByName(m_currentLevelData.def.GetAsString("activeScene"));
		SceneManager.SetActiveScene(scene);
		// #endif
	}
}