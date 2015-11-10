// LevelManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/01/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
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
	// The data
	[SerializeField] private LevelData[] m_levels = null;
	public static LevelData[] levels { get { return instance.m_levels; }}

	// Currently selected level
	public static int currentLevel { get; set; }
	public static LevelData currentLevelData {
		get { return GetLevelData(currentLevel); }
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
	// DRAGON DATA GETTERS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Given a level index, get its data.
	/// </summary>
	/// <returns>The data corresponding to the level with the given index. Null if not found.</returns>
	/// <param name="_levelIdx">The index of the level whose data we want.</param>
	public static LevelData GetLevelData(int _levelIdx) {
		LevelData data = null;
		if(_levelIdx >= 0 && _levelIdx < instance.m_levels.Length) {
			return instance.m_levels[_levelIdx];
		}
		return null;
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Starts loading the scene of the level with the given index.
	/// Deletes any level in the current scene.
	/// Loading is asynchronous, use the returned object to check when the level has finished loading.
	/// </summary>
	/// <returns>The loading request, where you can check the loading progress.</returns>
	/// <param name="_levelIdx">The index of the level we want to load.</param>
	public static AsyncOperation LoadLevel(int _levelIdx) {
		// Destroy any existing level in the game scene
		LevelEditor.Level[] activeLevels = Component.FindObjectsOfType<LevelEditor.Level>();
		for(int i = 0; i < activeLevels.Length; i++) {
			DestroyImmediate(activeLevels[i].gameObject);
		}

		// Get the data for the new level
		LevelData data = LevelManager.GetLevelData(_levelIdx);
		DebugUtils.SoftAssert(data != null, "Attempting to load level with index " + _levelIdx + ", but the manager has no data linked to this index");

		// Load the scene for the level with the given index
		AsyncOperation loadingTask = Application.LoadLevelAdditiveAsync(data.sceneName);
		DebugUtils.SoftAssert(loadingTask != null, "The prefab defined to level " + _levelIdx + " couldn't be found");
		return loadingTask;
	}
}