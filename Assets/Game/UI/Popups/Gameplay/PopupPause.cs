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

public class PopupPause : PopupPauseBase {
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
	/// Go to a specific tab in the popup.
	/// </summary>
	/// <param name="_tab">Target tab.</param>
	public void GoToTab(Tabs _tab) {
		// Is the popup open?
		if(m_popup.isOpen) {
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
	override public void OnOpenPreAnimation() {
		// Call parent
		base.OnOpenPreAnimation();

		// Hide the tabs during the first run (tutorial)
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN) && SceneManager.GetActiveScene().name != "SC_Popups") {
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
}
