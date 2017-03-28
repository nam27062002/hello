// PopupMissionsIngame.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Temp popup to show active missions during the game.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupPause : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/PF_PopupPause";

	public enum Tabs {
		MISSIONS,
		MAP,
		OPTIONS,

		COUNT
	}

	// Shortcut to tabs system
	private TabSystem m_tabs = null;
	private TabSystem tabs {
		get {
			if(m_tabs == null) {
				m_tabs = GetComponent<TabSystem>();
			}
			return m_tabs;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// This popup won't be destroyed during the whole game, but we want to destroy it upon game ending
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnd);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe to external events
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnd);
	}

	/// <summary>
	/// Go to a specific tab in the popup.
	/// </summary>
	/// <param name="_tab">Target tab.</param>
	public void GoToTab(Tabs _tab) {
		// Is the popup open?
		PopupController popup = GetComponent<PopupController>();
		if(popup.isOpen) {
			// Go to target screen
			tabs.GoToScreen((int)_tab);
		} else {
			// Set as initial screen (in case tab system wasn't already started)
			tabs.SetInitialScreen((int)_tab);

			// Go to target screen without animation
			tabs.GoToScreen((int)_tab, NavigationScreen.AnimType.NONE);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Open animation is about to start.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Pause the game
		GameSceneController gameController = InstanceManager.gameSceneController;
		if(gameController != null) {
			gameController.PauseGame(true);
		}

		// Hide the tabs during the first run (tutorial)
		if(UsersManager.currentUser.gamesPlayed < 1 && SceneManager.GetActiveScene().name != "SC_Popups") {
			// Get the tab system component
			if(tabs != null) {
				// Set options tab as initial screen
				tabs.SetInitialScreen((int)Tabs.OPTIONS);
				//tabs.GoToScreen((int)Tabs.OPTIONS, NavigationScreen.AnimType.NONE);

				// Hide all buttons
				for(int i = 0; i < tabs.m_tabButtons.Count; i++) {
					tabs.m_tabButtons[i].gameObject.SetActive(false);
				}
			}
		}

		// [AOC] TEMP!!
		// Hide all buttons (testing HUD buttons)
		for(int i = 0; i < tabs.m_tabButtons.Count; i++) {
			tabs.m_tabButtons[i].gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Close animation has finished.
	/// </summary>
	public void OnClosePostAnimation() {
		// Resume game
		if(InstanceManager.gameSceneController != null) {
			InstanceManager.gameSceneController.PauseGame(false);
		}
	}

	/// <summary>
	/// The game has eneded.
	/// </summary>
	private void OnGameEnd() {
		// Destroy this popup
		GameObject.Destroy(this.gameObject);
	}
}
