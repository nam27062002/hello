// MenuPlayButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for the big play button in the menu.
/// </summary>
public class MenuPlayButton : MenuNavigationButton {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// Depends on the tutorial status
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_PLAY_SCREEN)) {
			// Advance tutorial step
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.FIRST_PLAY_SCREEN);
			PersistenceManager.Save();

			// Go straight to game
			OnStartGameButton();
		} else {
			// Go to the dragon default screen defined in the inspector
			OnNavigationButton();
		}
	}
}
