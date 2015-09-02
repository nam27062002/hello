// SceneManager.cs
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
/// Generic class to properly manage transitions through scenes.
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class SceneManager : Singleton<SceneManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum ESceneState {
		RESET,		// Reclaim some memory before loading a scene. 1-Frame state.
		PRELOAD,	// Start loading the scene asynchronously. Run here any process you need to run before loading. 1-Frame state.
		LOAD,		// Stay here until the loading is done.
		UNLOAD,		// Unload unused resources from the scene we just loaded.
		POSTLOAD,	// Run here any process required immediately after loading. 1-Frame state.
		READY,		// Just before running, force one last memory cleanup. Run here any process required before running the scene, e.g. activate user input, open welcome popups, etc. 1-Frame state.
		RUN,		// We stay here until a new scene change is requested.
		COUNT
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Scene tracking
	private string m_prevScene;
	public static string prevScene { get { return instance.m_prevScene; }}

	private string m_currentScene;
	public static string currentScene { get { return instance.m_currentScene; }}

	private string m_nextScene;
	public static string nextScene { get { return instance.m_nextScene; }}

	private ESceneState m_sceneState;
	public static ESceneState sceneState { get { return instance.m_sceneState; }}

	// Loading tech
	private AsyncOperation m_unloadTask;
	private AsyncOperation m_loadTask;

	// Scene update delegates
	protected delegate void SceneUpdateDelegate();
	protected SceneUpdateDelegate[] m_updateDelegates;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// The percentage of loading completed [0..1]
	public static float loadProgress {
		get {
			// Only makes sense during loading state
			if(sceneState < ESceneState.LOAD) {
				return 0;
			} else if(sceneState == ESceneState.LOAD) {
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
	override protected void Awake () {
		// Call parent
		base.Awake();

		// Setup the array of update delegates
		m_updateDelegates = new SceneUpdateDelegate[(int)ESceneState.COUNT];
		m_updateDelegates[(int)ESceneState.RESET] = UpdateReset;
		m_updateDelegates[(int)ESceneState.PRELOAD] = UpdatePreload;
		m_updateDelegates[(int)ESceneState.LOAD] = UpdateLoad;
		m_updateDelegates[(int)ESceneState.UNLOAD] = UpdateUnload;
		m_updateDelegates[(int)ESceneState.POSTLOAD] = UpdatePostload;
		m_updateDelegates[(int)ESceneState.READY] = UpdateReady;
		m_updateDelegates[(int)ESceneState.RUN] = UpdateRun;

		// [AOC] Pick current scene as initial scene and put it to run state
		SetCurrentSceneInternal(Application.loadedLevelName);
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	override protected void Update () {
		// Call parent
		base.Update();

		// Invoke delegate for the current scene state
		if(m_updateDelegates[(int)sceneState] != null) {
			m_updateDelegates[(int)sceneState]();
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Clean up the update delegates
		if(m_updateDelegates != null) {
			for(int i = 0; i < (int)ESceneState.COUNT; i++) {
				m_updateDelegates[i] = null;
			}
			m_updateDelegates = null;
		}

		// Call parent
		base.OnDestroy();
	}

	//------------------------------------------------------------------//
	// STATIC PUBLIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Switchs to the given scene, unless we're already on it.
	/// </summary>
	/// <param name="_nextSceneName">The name of the scene to go to.</param>
	public static void SwitchScene(string _nextSceneName) {
		// If the target scene is different than the current one, store it as next scene
		if(currentScene != _nextSceneName) {
			// Store next scene, the load will begin on the next Update() call
			instance.m_nextScene = _nextSceneName;
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

		// Dispatch event
		Messenger.Broadcast<ESceneState, ESceneState>(EngineEvents.SCENE_STATE_CHANGED, oldState, m_sceneState);
	}

	//------------------------------------------------------------------//
	// UPDATE DELEGATES													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update delegate for the RESET scene state.
	/// </summary>
	private void UpdateReset() {
		// Just run a GC pass
		System.GC.Collect();
		ChangeState(ESceneState.PRELOAD);
	}

	/// <summary>
	/// Update delegate for the PRELOAD scene state.
	/// </summary>
	private void UpdatePreload() {
		// Trigger the load of the new scene
		m_loadTask = Application.LoadLevelAsync(nextScene);
		ChangeState(ESceneState.LOAD);
	}

	/// <summary>
	/// Update delegate for the LOAD scene state.
	/// </summary>
	private void UpdateLoad() {
		// Have we finished loading?
		if(m_loadTask.isDone) {
			m_loadTask = null;
			ChangeState(ESceneState.UNLOAD);
		}
	}

	/// <summary>
	/// Update delegate for the UNLOAD scene state.
	/// </summary>
	private void UpdateUnload() {
		// Are we already cleaning up?
		if(m_unloadTask == null) {
			m_unloadTask = Resources.UnloadUnusedAssets();
		} else {
			// Have we finished cleaning up?
			if(m_unloadTask.isDone) {
				m_unloadTask = null;
				ChangeState(ESceneState.POSTLOAD);
			}
		}
	}

	/// <summary>
	/// Update delegate for the POSTLOAD scene state.
	/// </summary>
	private void UpdatePostload() {
		// Update scene tracking
		m_prevScene = m_currentScene;
		m_currentScene = m_nextScene;
		ChangeState(ESceneState.READY);
	}

	/// <summary>
	/// Update delegate for the READY scene state.
	/// </summary>
	private void UpdateReady() {
		// Run a GC pass
		// [AOC] WARNING!! If we have assets loaded in the scene that are currentlu unused but may be used later, DON'T do this here (comment the following line)
		System.GC.Collect();
		ChangeState(ESceneState.RUN);
	}

	/// <summary>
	/// Update delegate for the RUN scene state.
	/// </summary>
	private void UpdateRun() {
		// Just wait for an external scene change request
		if(currentScene != nextScene) {
			ChangeState(ESceneState.RESET);
		}
	}
}

