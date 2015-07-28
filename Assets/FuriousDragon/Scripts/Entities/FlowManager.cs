// FlowManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Behaviour to control the flow navigation between scenes.
/// </summary>
public class FlowManager : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------
	public enum EScenes {
		MAIN_MENU,
		GAME
	};

	// Should match library!
	public static readonly string[] SCENE_NAMES = {
		"SC_MainMenu",
		"Proto"
	};
	#endregion
	
	#region EXPOSED MEMBERS --------------------------------------------------------------------------------------------
	// [AOC] Initialize all these from the inspector :-)
	
	#endregion
	
	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------

	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start() {

	}

	/// <summary>
	/// Load a new scene and forget current one.
	/// </summary>
	/// <param name="_eNewScene">The scene to go to.</param>
	public void GoToScene(FlowManager.EScenes _eNewScene) {
		// [AOC] TODO!! Check:
		//		 - Async
		//		 - DontDestroyOnLoad
		switch(_eNewScene) {
			case EScenes.MAIN_MENU: {
				// Load menu scene
				Application.LoadLevel(GetSceneName(_eNewScene));
			} break;

			case EScenes.GAME: {
				// Reset game stats
				App.Instance.gameStats.Reset();

				// Load game scene
				Application.LoadLevel(GetSceneName(_eNewScene));
			} break;
		}
	}

	/// <summary>
	/// A new scene was loaded.
	/// </summary>
	/// <param name="_iLevelIdx">The index of the level that was loaded. Use the menu item File->Build Settings... to see what scene the index refers to. See Also: Application.LoadLevel..</param>
	void OnLevelWasLoaded(int _iLevelIdx) {
		// [AOC] We don't trust the order on the build settings, so use scene name instead
		if(Application.loadedLevelName == GetSceneName(EScenes.GAME)) {

			// Start logic
			App.Instance.gameLogic.StartGame();
		}
	}
	#endregion

	#region STATIC UTILS ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Given a scene, get its name.
	/// </summary>
	/// <param name="_eScene">The scene whose name we want.</param>
	/// <returns>The scene name.</returns>
	public static string GetSceneName(FlowManager.EScenes _eScene) {
		return SCENE_NAMES[(int)_eScene];
	}
	#endregion
}
#endregion