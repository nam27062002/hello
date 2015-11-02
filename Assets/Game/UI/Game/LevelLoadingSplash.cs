// LevelLoadingSplash.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Splash screen to show while the level is loading.
/// </summary>
public class LevelLoadingSplash : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[SerializeField] private Slider m_progressBar = null;
	[SerializeField] private Text m_text = null;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private GameSceneController sceneController { get { return InstanceManager.sceneController as GameSceneController; }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required references
		DebugUtils.Assert(m_progressBar != null, "Required param!");
		DebugUtils.Assert(m_text != null, "Required param!");
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnGameLevelLoaded);
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void Update() {
		// Update progress
		m_progressBar.normalizedValue = sceneController.levelLoadingProgress;
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnGameLevelLoaded);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The game level has been loaded.
	/// </summary>
	private void OnGameLevelLoaded() {
		// Go out - using animation (if any)
		Animator anim = GetComponent<Animator>();
		if(anim != null) {
			anim.SetTrigger("out");
		} else {
			OnOutAnimFinished();
		}
	}

	/// <summary>
	/// The out animation has finished.
	/// </summary>
	private void OnOutAnimFinished() {
		//Destroy(gameObject);	// [AOC] For some reason this is problematic
		gameObject.SetActive(false);
	}
}