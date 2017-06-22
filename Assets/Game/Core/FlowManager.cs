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
        string nextScene = MenuSceneController.NAME;

        // Skip if next scene has already been set
        if (GameSceneManager.nextScene == nextScene) return;

		LevelManager.SetCurrentLevel(null);

        // Change scene
        GameSceneManager.SwitchScene(nextScene);
    }

	/// <summary>
	/// Navigate to the game scene.
	/// </summary>
	public static void GoToGame() {
        string nextScene = GameSceneController.NAME;

        // Skip if next scene has already been set
        if (GameSceneManager.nextScene == nextScene) return;

        // Change scene
        GameSceneManager.SwitchScene(nextScene);       
	}

    /// <summary>
	/// Navigate to the results scene.
	/// </summary>
	public static void GoToResults() {
        string nextScene = ResultsScreenController.NAME;
        
        // Skip if next scene has already been set
        if (GameSceneManager.nextScene == nextScene) return;

        // Change scene
        GameSceneManager.SwitchScene(nextScene);
    }

    public static void GoToProfilerMemoryScene()
    {
        string nextScene = ProfilerMemoryController.NAME;

        // Skip if next scene has already been set
        if (GameSceneManager.nextScene == nextScene) return;

        // Change scene
        GameSceneManager.SwitchScene(nextScene);       
    }

    public static void GoToProfilerLoadScenesScene()
    {
        string nextScene = ProfilerLoadScenesController.NAME;

        // Skip if next scene has already been set
        if (GameSceneManager.nextScene == nextScene) return;

        // Change scene
        GameSceneManager.SwitchScene(nextScene);            
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
        // The game will be started again so we need to end the current tracking session
        HDTrackingManager.Instance.NotifyEndSession();

        // Delete key singletons that must be reloaded		
        GameVars.DestroyInstance();
		
        //[DGR] We need to destroy SaveFacade system because a new instance of UserProfile will be created when restarting so we need to make sure this system
        //is going to use the new UserProfile instance
        //SaveFacade.DestroyInstance();     
        SaveFacade.Instance.Reset();   

        // Change to the loading scene. This change might be needed from the LoadingSceneController itself because of the save game flow (for exaple when clicking of update the game version
        // from the editor)
        GameSceneManager.SwitchScene(LoadingSceneController.NAME, "", true);
        
        // A new tracking session is started
        HDTrackingManager.Instance.NotifyStartSession();
    }
}

