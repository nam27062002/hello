// PopupTutorialControls.cs
// 
// Created by Alger Ortín Castellví on 05/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Pause popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupTutorialControls : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Tutorial/PF_PopupTutorialControls";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_loadingInfo = null;
	[SerializeField] private ShowHideAnimator m_playButton = null;

	// References
	private Slider m_loadingBar = null;
	private GameSceneController m_sceneController = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get references
		m_loadingBar = m_loadingInfo.FindComponentRecursive<Slider>();
		m_sceneController = InstanceManager.GetSceneController<GameSceneController>();
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update progress bar
		m_loadingBar.normalizedValue = m_sceneController.levelLoadingProgress;

		// Show/Hide elements
		m_loadingInfo.Set(m_sceneController.levelLoadingProgress < 1f, true);
		m_playButton.Set(m_sceneController.levelLoadingProgress >= 1f, true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Open animation is about to start.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Prevent game from starting once loading has finished
		m_sceneController.startWhenLoaded = false;

		// Initialize popup
		m_loadingInfo.ForceShow(false);
		m_playButton.ForceHide(false);
	}

	/// <summary>
	/// Close animation has started.
	/// </summary>
	public void OnClosePreAnimation() {
		// Start playing!
		m_sceneController.startWhenLoaded = true;
	}
}
