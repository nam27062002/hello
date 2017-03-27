// FlowManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Class responsible for making all the transitions in the game, such as go to the menu, start a game, etc.
/// This transitions may involve loading a new scene, sending some metrics, communication with the server to let it know
/// that the user is changing its state, etc.
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class FlowManager : UbiBCN.SingletonMonoBehaviour<FlowManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// SCENE NAVIGATION													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Navigate to the menu scene.
	/// </summary>
	public static void GoToMenu() {
		// Skip if next scene is already menu
		if(GameSceneManager.nextScene == MenuSceneController.NAME) return;

		// Change scene
		GameSceneManager.SwitchScene(MenuSceneController.NAME);
	}

	/// <summary>
	/// Navigate to the game scene.
	/// </summary>
	public static void GoToGame() {
		// Skip if next scene is already game
		if(GameSceneManager.nextScene == GameSceneController.NAME) return;

		// Change scene
		GameSceneManager.SwitchScene(GameSceneController.NAME);
	}

    public static void GoToProfilerMemory()
    {
        // Skip if next scene is already menu
        if (GameSceneManager.nextScene == ProfilerMemoryController.NAME) return;

        // Change scene
        GameSceneManager.SwitchScene(ProfilerMemoryController.NAME);
    }

    /// <summary>
    /// Returns whether or not the flow is in the game scene and it has been completely loaded
    /// </summary>
    /// <returns></returns>
    public static bool IsInGameScene()
    {
        return GameSceneManager.currentScene == GameSceneController.NAME && !GameSceneManager.isLoading;
    }

	/// <summary>
	/// Interrupts current flow and restarts the application.
	/// </summary>
	public static void Restart() {
		// Delete key singletons that must be reloaded		
		GameVars.DestroyInstance();
		
        //[DGR] We need to destroy SaveFacade system because a new instance of UserProfile will be created when restarting so we need to make sure this system
        //is going to use the new UserProfile instance
        //SaveFacade.DestroyInstance();     
        SaveFacade.Instance.Reset();   

        // Change to the loading scene. This change might be needed from the LoadingSceneController itself because of the save game flow (for exaple when clicking of update the game version
        // from the editor)
        GameSceneManager.SwitchScene(LoadingSceneController.NAME, "", true);
	}
}

