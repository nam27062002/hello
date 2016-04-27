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
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] private Slider m_progressBar = null;
	private GameSceneController m_sceneController = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required references
		DebugUtils.Assert(m_progressBar != null, "Required param!");
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		m_sceneController = InstanceManager.GetSceneController<GameSceneController>();
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnGameLevelLoaded);

		// Show!
		GetComponent<ShowHideAnimator>().ForceShow(false);
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void Update() {
		// Update progress
		m_progressBar.normalizedValue = m_sceneController.levelLoadingProgress;
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
		// Hide!
		GetComponent<ShowHideAnimator>().ForceHide();
	}
}