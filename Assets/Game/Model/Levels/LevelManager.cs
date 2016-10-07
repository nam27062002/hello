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
/// Global manager of levels. Contains the definitions for all the levels in
/// the game.
/// Has its own asset in the Resources/Singletons folder, all content must be
/// initialized there.
/// </summary>
public class LevelManager : SingletonScriptableObject<LevelManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Shortcut to get the data of the currently selected level
	public static DefinitionNode currentLevelDef {
		get { return DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVELS, UsersManager.currentUser.currentLevel); }
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
	/// Check whether a level is unlocked with the current game status or not.
	/// Might need a more complex implementation in the future.
	/// </summary>
	/// <returns><c>true</c> the level with the specified _sku is unlocked; <c>false</c> otherwise.</returns>
	/// <param name="_sku">The sku of the level to be checked.</param>
	public static bool IsLevelUnlocked(string _sku) {
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVELS, _sku);
		if(def == null) return false;

		// Special case for "Coming Soon" levels
		if(def.GetAsBool("comingSoon")) return false;

		// Just do  the math
		return def.GetAsInt("dragonsToUnlock") <= DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
	}

	/// <summary>
	/// Starts loading the scenes of the level with the given index.
	/// Deletes any level in the current scene.
	/// Loading is asynchronous, use the returned objects to check when the level has finished loading.
	/// </summary>
	/// <returns>The loading requests, where you can check the loading progress.</returns>
	/// <param name="_sku">The sku of the level we want to load.</param>
	public static AsyncOperation[] LoadLevel(string _sku) {
		// Destroy any existing level in the game scene
		LevelEditor.Level[] activeLevels = Component.FindObjectsOfType<LevelEditor.Level>();
		for(int i = 0; i < activeLevels.Length; i++) {
			DestroyImmediate(activeLevels[i].gameObject);
		}

		// Get the data for the new level
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVELS, _sku);
		DebugUtils.SoftAssert(def != null, "Attempting to load level with sku " + _sku + ", but the manager has no data linked to this index");

		// Load additively all the scenes for the level with the given index
		List<AsyncOperation> loadingTasks = new List<AsyncOperation>();
		AsyncOperation loadingTask = null;

		List<string> spawnersScene = def.GetAsList<string>("spawnersScene");
		for( int i = 0; i<spawnersScene.Count; i++ )
		{
			loadingTask = SceneManager.LoadSceneAsync(spawnersScene[i], LoadSceneMode.Additive);
			if(DebugUtils.SoftAssert(loadingTasks != null, "The spawners scene defined to level " + _sku + " couldn't be found (probably mispelled or not added to build settings)")) {
				loadingTasks.Add(loadingTask);
			}	
		}

		List<string> collisionScene = def.GetAsList<string>("collisionScene");
		for( int i = 0; i<collisionScene.Count; i++ )
		{
			loadingTask = SceneManager.LoadSceneAsync(collisionScene[i], LoadSceneMode.Additive);
			if(DebugUtils.SoftAssert(loadingTasks != null, "The collision scene defined to level " + _sku + " couldn't be found (probably mispelled or not added to build settings)")) {
				loadingTasks.Add(loadingTask);
			}
		}

		List<string> soundScene = def.GetAsList<string>("soundScene");
		for( int i = 0; i<soundScene.Count; i++ )
		{
			if ( !string.IsNullOrEmpty( soundScene[i]) ){
				loadingTask = SceneManager.LoadSceneAsync(soundScene[i], LoadSceneMode.Additive);
				if(DebugUtils.SoftAssert(loadingTasks != null, "The sound scene defined to level " + _sku + " couldn't be found (probably mispelled or not added to build settings)")) {
					loadingTasks.Add(loadingTask);
				}
			}
		}

		List<string> artScene = def.GetAsList<string>("artScene");
		for( int i = 0; i<artScene.Count; i++ )
		{
			loadingTask = SceneManager.LoadSceneAsync(artScene[i], LoadSceneMode.Additive);
			if(DebugUtils.SoftAssert(loadingTasks != null, "The art scene defined to level " + _sku + " couldn't be found (probably mispelled or not added to build settings)")) 
			{
				loadingTasks.Add(loadingTask);
			}
		}



		if ( loadingTask != null )
			loadingTask.allowSceneActivation = true;
		return loadingTasks.ToArray();
	}

	public static void SetArtSceneActive( string _sku )
	{
		// [AOC] For some reason, the lightning settings of the Art scene makes the OSX Editor to crash
		// #if !UNITY_EDITOR_OSX
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVELS, _sku);
		Scene scene = SceneManager.GetSceneByName(def.GetAsString("activeScene"));
		SceneManager.SetActiveScene(scene);
		// #endif
	}
}