// App.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Singleton application instance, containing all the managers and other static classes in the game.
/// <para>Implementation roughly based in HSE's King and App classes.</para>
/// <para>Should be instantiated once in the first scene of the game.</para>
/// </summary>
public class App : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------
	private static readonly string PREFAB_PATH = "PROTO/Prefabs/App/PF_App";
	#endregion

	#region SINGLETON INSTANCE -----------------------------------------------------------------------------------------
	// [AOC] Roughly based in http://kleber-swf.com/singleton-monobehaviour-unity-projects/
	private static App instance = null;
	public static App Instance {
		get {
			// If the singleton instance wasn't yet created, do it now and add it to the scene so it gets updated
			if(instance == null) {
				// Load the prefab
				GameObject newInstanceObj = Instantiate(Resources.Load<GameObject>(PREFAB_PATH)) as GameObject;
				instance = newInstanceObj.GetComponent<App>();
				DebugUtils.Assert(instance != null, "Singleton's prefab must contain a component of this singleton!");

				// Make sure our object is not destroyed between scenes
				GameObject.DontDestroyOnLoad(newInstanceObj);
			}
			return instance;
		}
	}
	#endregion

	#region MANAGER PREFABS AND INSTANCES ------------------------------------------------------------------------------
	public UserData userData;
	public GameStats gameStats;
	public GameStats gameStatsGlobal;
	public GameLogic gameLogic;
	public FlowManager_OLD flowManager;
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization previous to Start.
	/// </summary>
	public void Awake() {
		// Just check that everything is properly initialized
		DebugUtils.Assert(userData != null, "Required property not initialized");
		DebugUtils.Assert(gameStats != null, "Required property not initialized");
		DebugUtils.Assert(gameStatsGlobal != null, "Required property not initialized");
		DebugUtils.Assert(gameLogic != null, "Required property not initialized");
		DebugUtils.Assert(flowManager != null, "Required property not initialized");

		// [AOC] TODO!! Make sure there is only one singleton instance!!
		instance = this;
	}

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// [AOC] TODO!! Load persistence
	}
	#endregion
}
#endregion