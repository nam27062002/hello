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
		SceneController.SetMode(SceneController.Mode.DEFAULT);
        HDLiveDataManager.instance.SwitchToQuest();
        // Depends on the tutorial status
        if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN)) {
            // Go straight to game
            OnStartGameButton();
        } else {
            // Go to the dragon default screen defined in the inspector
            OnNavigationButton();
        }

        // Save flag to not display play screen again
        GameVars.playScreenShown = true;

        HDTrackingManager.Instance.Notify_Calety_Funnel_Load(FunnelData_Load.Steps._04_click_play);
	}
}
