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
public class FlowManager : Singleton<FlowManager> {
    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//

    //------------------------------------------------------------------//
    // MEMBERS															//
    //------------------------------------------------------------------//
    private bool m_isInGameScene = false;

    //------------------------------------------------------------------//
    // PROPERTIES														//
    //------------------------------------------------------------------//

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    protected override void OnCreateInstance() {
        Messenger.AddListener<string>(MessengerEvents.SCENE_LOADED, OnSceneLoaded);
    }

    protected override void OnDestroyInstance() {        
        Messenger.RemoveListener<string>(MessengerEvents.SCENE_LOADED, OnSceneLoaded);
    }

    private void OnSceneLoaded(string _currentScene)
    {
        // This value is stored because IsInGameScene() is called evey tick and the comparison GameSceneManager.currentScene == GameSceneController.NAME turned out to be expensive when profiling (0.3ms in Samsung S4)
        m_isInGameScene = _currentScene == GameSceneController.NAME;        
    }

    /// <summary>
    /// Switch to a different scene. Nothing happens if given scene is the same as active one.
    /// </summary>
    private void SwitchScene(string _nextScene) {
		// Skip if target scene has already been set
		if(GameSceneManager.nextScene == _nextScene) return;

		// Make sure we don't carry any cached stuff into the game scene
		PopupManager.Clear(false);
		ShareScreensManager.Clear();
		FrozenMaterialManager.CleanFrozenMaterials();

		// Change scene
		GameSceneManager.SwitchScene(_nextScene);
	}

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

        HDTrackingManager.Instance.GoToMenu();
        OffersManager.instance.enabled = true;

        // Change scene
        GameSceneManager.SwitchScene(nextScene);
    }

	/// <summary>
	/// Navigate to the game scene.
	/// </summary>
	public static void GoToGame() {
        HDTrackingManager.Instance.GoToGame();
        OffersManager.instance.enabled = false;
        instance.SwitchScene(GameSceneController.NAME);
	}

	/// <summary>
	/// Navigate to the game scene.
	/// </summary>
	public static void GoToGameNoUI() {
		HDTrackingManager.Instance.GoToGame();
		OffersManager.instance.enabled = false;
		instance.SwitchScene("SC_Game_NoUI");
	}

	/// <summary>
	/// Navigate to the results scene.
	/// </summary>
	public static void GoToResults() {
        OffersManager.instance.enabled = false;
		instance.SwitchScene(ResultsScreenController.NAME);
    }

    public static void GoToProfilerMemoryScene()
    {
		instance.SwitchScene(ProfilerMemoryController.NAME);
    }

    public static void GoToProfilerLoadScenesScene()
    {
		instance.SwitchScene(ProfilerLoadScenesController.NAME);
    }    

    /// <summary>
    /// Returns whether or not the flow is in the game scene and it has been completely loaded
    /// </summary>
    /// <returns></returns>
    public static bool IsInGameScene()
    {
        return instance.m_isInGameScene && !GameSceneManager.isLoading;
    }

	/// <summary>
	/// Interrupts current flow and restarts the application.
	/// </summary>
	public static void Restart()
    {
#if UNITY_EDITOR
        // Restart needs to be notified so login stuff can be spoofed properly
        ApplicationManager.instance.PersistenceTester.OnAppLaunched();
#endif

        // Change to the loading scene. This change might be needed from the LoadingSceneController itself because of the save game flow (for exaple when clicking of update the game version
        // from the editor)
        GameSceneManager.SwitchScene(LoadingSceneController.NAME, "", true);                
    }

    public static void Reset()
    {
        GameServerManager.SharedInstance.Reset();

        // Delete key singletons that must be reloaded		
        GameVars.DestroyInstance();

        HDTrackingManager.Instance.Destroy();

        HDCustomizerManager.instance.Destroy();
        
        // [DGR] We need to destroy SaveFacade system because a new instance of UserProfile will be created when restarting so we need to make sure this system
        // is going to use the new UserProfile instance        
        PersistenceFacade.instance.Reset();

        
        TransactionManager.instance.Reset();
        HDCustomizerManager.instance.Reset();

        SocialPlatformManager.SharedInstance.Reset();

        ContentManager.Reset();

        HDAddressablesManager.Instance.Reset();        
    }
}

