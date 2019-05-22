// SceneManager.cs
// 
// Created by Alger Ortín Castellví on 20/08/2015.
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
/// Generic class to properly manage transitions through scenes.
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class GameSceneManager : UbiBCN.SingletonMonoBehaviour<GameSceneManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum ESceneState {
		RESET,		// Reclaim some memory before loading a scene. 1-Frame state.
		PRELOAD,	// Start loading the scene asynchronously. Run here any process you need to run before loading. 1-Frame state.ç
		LOADING_LOADING_SCENE,	// Loading the intermediate loading screen
		UNLOADING,	// Unload unused resources from the scene we're leaving.
		LOADING,	// Stay here until the loading is done.
		UNLOADING_LOADING_SCENE,	// Unload the intermediate loading screen
		POSTLOAD,	// Run here any process required immediately after loading. 1-Frame state.
		READY,		// Just before running, force one last memory cleanup. Run here any process required before running the scene, e.g. activate user input, open welcome popups, etc. 1-Frame state.
		RUN,		// We stay here until a new scene change is requested.
		COUNT
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Scene tracking
	private string m_prevScene = "";
	public static string prevScene { get { return instance.m_prevScene; }}

	private string m_currentScene = "";
	public static string currentScene { get { return instance.m_currentScene; }}

	private string m_nextScene = "";
	public static string nextScene { get { return instance.m_nextScene; }}

	private string m_loadingScene = "";
	public static string loadingScene { get { return instance.m_loadingScene; }}

	private ESceneState m_sceneState;
	public static ESceneState sceneState { get { return instance.m_sceneState; }}

	// Loading tech
	private AsyncOperation m_unloadTask;
	private UbiAsyncOperation m_loadTask;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// The percentage of loading completed [0..1]
	public static float loadProgress {
		get {
			// Only makes sense during loading state
			if(sceneState < ESceneState.LOADING) {
				return 0;
			} else if(sceneState == ESceneState.LOADING) {
				return instance.m_loadTask.progress;
			} else {
				return 1;
			}
		}
	}

	// Whether is any load in progress
	public static bool isLoading {
		get { return sceneState != ESceneState.RUN; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake () {
		// [AOC] Pick current scene as initial scene and put it to run state
		SetCurrentSceneInternal(SceneManager.GetActiveScene().name);
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	protected void Update () {
		// Update based on current state
		UpdateState();
	}

    //------------------------------------------------------------------//
    // STATIC PUBLIC METHODS											//
    //------------------------------------------------------------------//
    /// <summary>
    /// Switchs to the given scene, unless we're already on it.
    /// </summary>
    /// <param name="_nextSceneName">The name of the scene to go to.</param>
    /// <param name="_loadingScene">Optionally define a scene to be loaded between the current scene and the next one.</param>
    /// <param name="_forceScene">When <c>true</c> <c>_nextsceneName</c> is loaded even though it's already the current scene.</param>
    public static void SwitchScene(string _nextSceneName, string _loadingScene = "", bool _forceScene = false) {
		// If the target scene is different than the current one, store it as next scene
		if(currentScene != _nextSceneName) {
			// Store next scene, the load will begin on the next Update() call
			instance.m_nextScene = _nextSceneName;
			instance.m_loadingScene = _loadingScene;
		}
        else if (_forceScene) {
            instance.ChangeState(ESceneState.RESET);
        }
    }


	/// <summary>
	/// Specially for debugging, force the name of the current scene.
	/// Doesn't do any flow change, only sets internal vars.
	/// </summary>
	/// <param name="_sceneName">The name of the currently loaded scene.</param>
	public static void SetCurrentScene(string _sceneName) {
		instance.SetCurrentSceneInternal(_sceneName);
	}

	/// <summary>
	/// Internal version of SetCurrentScene to be called when the instance is not
	/// ready yet (during the Awake() function).
	/// </summary>
	/// <param name="_sceneName">The name of the currently loaded scene.</param>
	private void SetCurrentSceneInternal(string _sceneName) {
		m_prevScene = m_currentScene;
		m_currentScene = _sceneName;
		m_nextScene = _sceneName;
		m_sceneState = ESceneState.RUN;
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Changes the current scene state and dispatches a game event.
	/// </summary>
	/// <param name="_newState">The state to go to.</param>
	private void ChangeState(ESceneState _newState) {
		// Just store new state
		ESceneState oldState = m_sceneState;
		m_sceneState = _newState;

		// Perform actions based on new state
		switch(_newState) {
			// Reclaim some memory before loading a scene. 1-Frame state.
			case ESceneState.RESET: {
                // PreUnload event is dispatched so listeners can do stuff thar needs to be done before game objects in the current scene start being destroyed. This is typically useful
                // when the stuff to do when destroying a game object depends on another game object that could already be destroyed if that stuff was done in OnDestroy()
				if (m_prevScene != "") Messenger.Broadcast<string>(MessengerEvents.SCENE_PREUNLOAD, m_prevScene);               
                
                // Run a GC pass
                System.GC.Collect();
			} break;

			// Start loading the scene asynchronously. Run here any process you need to run before loading. 1-Frame state.
			case ESceneState.PRELOAD: {
                    HDCustomizerManager.instance.CheckAndApply();
			} break;

			// Loading the intermediate loading screen
			case ESceneState.LOADING_LOADING_SCENE: {
                // Trigger the load of the loading scene - this will unload all active scenes                
                m_loadTask = HDAddressablesManager.Instance.LoadSceneAsync(loadingScene);                
			} break;

			// Unload unused resources from the scene we're leaving.
			case ESceneState.UNLOADING: {
				// Trigger the unload of the assets belonging to previous scenes
				m_unloadTask = Resources.UnloadUnusedAssets();
			} break;

			// Stay here until the loading is done.
			case ESceneState.LOADING: {
				// Trigger the load of the new scene - this will unload all active scenes (including loading scene if any)
				m_loadTask = HDAddressablesManager.Instance.LoadDependenciesAndSceneAsync(nextScene);
			} break;

			// Unload the intermediate loading screen
			case ESceneState.UNLOADING_LOADING_SCENE: {
				// Trigger the unload of assets belonging to previous scenes (including loading scene if any)
				m_unloadTask = Resources.UnloadUnusedAssets();
			} break;

			// Run here any process required immediately after loading. 1-Frame state.
			case ESceneState.POSTLOAD: {
				// Update scene tracking
				m_currentScene = nextScene;
				m_loadingScene = "";

				// Dispatch event
				if(m_prevScene != "") Messenger.Broadcast<string>(MessengerEvents.SCENE_UNLOADED, m_prevScene);
				if(m_currentScene != "") Messenger.Broadcast<string>(MessengerEvents.SCENE_LOADED, m_currentScene);
			} break;

			// Just before running, force one last memory cleanup. Run here any process required before running the scene, e.g. activate user input, open welcome popups, etc. 1-Frame state.
			case ESceneState.READY: {
				// Run a GC pass
				// [AOC] WARNING!! If we have assets loaded in the scene that are currently unused but may be used later, DON'T do this here (comment the following line)
				System.GC.Collect();
			} break;

			// We stay here until a new scene change is requested.
			case ESceneState.RUN: {
				// Nothing to do
			} break;
		}

		// Dispatch event
		Messenger.Broadcast<ESceneState, ESceneState>(MessengerEvents.SCENE_STATE_CHANGED, oldState, m_sceneState);
	}

	/// <summary>
	/// Update based on current state
	/// </summary>
	private void UpdateState() {
		switch(m_sceneState) {
			// Reclaim some memory before loading a scene. 1-Frame state.
			case ESceneState.RESET: {
				ChangeState(ESceneState.PRELOAD);	// Immediately move to next state
			} break;

			// Start loading the scene asynchronously. Run here any process you need to run before loading. 1-Frame state.
			case ESceneState.PRELOAD: {
				// If we're using a loading scene, load it now. Otherwise load directly the next scene.
				if(loadingScene != "") {
					ChangeState(ESceneState.LOADING_LOADING_SCENE);
				} else {
					ChangeState(ESceneState.LOADING);
				}
			} break;

			// Loading the intermediate loading screen
			case ESceneState.LOADING_LOADING_SCENE: {
				// Have we finished loading?
				if(m_loadTask.isDone) {
					m_loadTask = null;
					ChangeState(ESceneState.UNLOADING);
				}
			} break;

				// Unload unused resources from the scene we're leaving.
			case ESceneState.UNLOADING: {
				// Have we finished cleaning up?
				if(m_unloadTask.isDone) {
					m_unloadTask = null;
					
					// If we're using a loading scene, it's time to load the next scene. Otherwise we're done!
					if(loadingScene != "") {
						ChangeState(ESceneState.LOADING);
					} else {
						ChangeState(ESceneState.POSTLOAD);
					}
				}
			} break;

			// Stay here until the loading is done.
			case ESceneState.LOADING: {
				// Have we finished loading?
				if(m_loadTask.isDone) {
					m_loadTask = null;

					// If we're using a loading scene, unload it now
					if(loadingScene != "") {
						ChangeState(ESceneState.UNLOADING_LOADING_SCENE);
					} else {
						ChangeState(ESceneState.UNLOADING);
					}
				}
			} break;

			// Unload the intermediate loading screen
			case ESceneState.UNLOADING_LOADING_SCENE: {
				// Have we finished cleaning up?
				if(m_unloadTask.isDone) {
					m_unloadTask = null;
					ChangeState(ESceneState.POSTLOAD);
				}
			} break;

			// Run here any process required immediately after loading. 1-Frame state.
			case ESceneState.POSTLOAD: {
				ChangeState(ESceneState.READY);	// Immediately move to next state
			} break;

			// Just before running, force one last memory cleanup. Run here any process required before running the scene, e.g. activate user input, open welcome popups, etc. 1-Frame state.
			case ESceneState.READY: {
				ChangeState(ESceneState.RUN);	// Immediately move to next state
			} break;

			// We stay here until a new scene change is requested.
			case ESceneState.RUN: {
				// Just wait for an external scene change request
				if(currentScene != nextScene) {
					// Update scene tracking
					m_prevScene = m_currentScene;
					
					// Start loading of the new scene
					ChangeState(ESceneState.RESET);
				}
			} break;
		}
	}
}

