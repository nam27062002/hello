// LevelSelectionMissionSelector.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom implementation of a tab system for the missions in the level selection screen.
/// </summary>
public class LevelSelectionMissionSelector : TabSystem {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Reset all buttons
		for(int i = 0; i < m_tabButtons.Count; i++) {
			m_tabButtons[i].interactable = true;
		}

		// Re-select the current tab to make sure everything is properly initialized
		int screen = currentScreenIdx;
		if(screen == SCREEN_NONE) screen = (int)Mission.Difficulty.EASY;
		GoToScreen(SCREEN_NONE, NavigationScreen.AnimType.NONE);
		GoToScreen(screen, NavigationScreen.AnimType.AUTO);
	}

	//------------------------------------------------------------------------//
	// OVERRIDES															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Navigate to the target mission.
	/// </summary>
	/// <param name="_newScreen">The index of the new screen to go to. Use -1 for NONE.</param>
	/// <param name="_animType">Optionally force the direction of the animation.</param>
	override public void GoToScreen(int _newScreen, NavigationScreen.AnimType _animType) {
		// Hide current screen instantly
		if(currentScreen != null) currentScreen.Hide(NavigationScreen.AnimType.NONE);

		// Refresh new screen's content with the equivalent mission data
		NavigationScreen newScreen = GetScreen(_newScreen) as Tab;
		if(newScreen != null) {
			// The new tab should have the MissionPîll component
			MissionPill pill = newScreen.GetComponent<MissionPill>();
			if(pill != null) {
				// Buttons are sorted following the mission's difficulty order, so we can directly access the new mission
				Mission newMission = MissionManager.GetMission((Mission.Difficulty)_newScreen);
				pill.InitFromMission(newMission);
			}
		}

		// Let parent do the rest
		base.GoToScreen(_newScreen, _animType);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}