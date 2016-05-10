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
	public static readonly string PATH = "UI/Popups/Pause/PF_PopupPause";

	public enum Tabs {
		MISSIONS,
		MAP,
		OPTIONS,

		COUNT
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Open animation is about to start.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Pause the game
		GameSceneController gameController = InstanceManager.GetSceneController<GameSceneController>();
		if(gameController != null) {
			gameController.PauseGame(true);
		}

		// Hide the tabs during the first run (tutorial)
		if(UserProfile.gamesPlayed < 1) {
			// Get the tab system component
			TabSystem tabs = GetComponent<TabSystem>();
			if(tabs != null) {
				// Set options tab as initial screen
				tabs.initialScreen = tabs.GetScreen((int)Tabs.OPTIONS);
				//tabs.GoToScreen((int)Tabs.OPTIONS, NavigationScreen.AnimType.NONE);

				// Hide all buttons
				for(int i = 0; i < tabs.m_tabButtons.Count; i++) {
					tabs.m_tabButtons[i].gameObject.SetActive(false);
				}
			}
		}
	}

	/// <summary>
	/// Close animation has finished.
	/// </summary>
	public void OnClosePostAnimation() {
		// Resume game
		GameSceneController gameController = InstanceManager.GetSceneController<GameSceneController>();
		if(gameController != null) {
			gameController.PauseGame(false);
		}
	}
}
