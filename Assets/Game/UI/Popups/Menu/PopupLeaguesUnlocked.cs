// PopupLeaguesUnlocked.cs
// 
// Created by Jose Maria Olea. 6/8/19
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tiers info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupLeaguesUnlocked : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupLeaguesUnlocked";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References



    // Internal 
    private MenuScreen m_sourceScreen;

    //------------------------------------------------------------------------//
    // METHODS																  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Check whether the popup must be triggered considering the current profile.
    /// </summary>
    /// <returns>Must the popup be displayed?</returns>
    public static bool Check() {

		// Don't if already displayed
		if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.LEAGUES_INFO)) {
			return false;
		}

        // Check the player has completed the proper runs
        if (UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_LEAGUES_AT_RUN)
        {
            return false;
        }
        
        // Don't if a dragon of the required tier is not yet owned
        if (DragonManager.biggestOwnedDragon.tier < HDLiveDataManager.league.GetMinimumTierToShowLeagues() ) {
			return false;
		}

		// All checks passed! Popup must be displayed
		return true;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//


	/// <summary>
	/// Initialize this popup with different variations depending on where it's triggered.
	/// </summary>
	/// <param name="_sourceScreen">The screen that triggered this popup.</param>
	public void Init(MenuScreen _sourceScreen) {
		// Store params
		m_sourceScreen = _sourceScreen;
	}

	/// <summary>
	/// The popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		
		// Mark tutorial as completed
		UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.LEAGUES_INFO, true);

	}

	/// <summary>
	/// The popup is about to be closed.
	/// </summary>
	public void OnClosePreAnimation() {
		
	}

	/// <summary>
	/// Dismiss button has been pressed.
	/// </summary>
	public void OnGoToLeaguesButton() {
		// Close the popup
		GetComponent<PopupController>().Close(true);

		// Go to the leagues if not already there
		if(m_sourceScreen != MenuScreen.LEAGUES) {
			// Clear any other queued poppup
			PopupManager.ClearQueue();

			// Go to the lab main screen after a short delay
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
                    // Tracking!
                    string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupLeaguesUnlocked.PATH);
                    HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");

                    // Change mode
                    HDLiveDataManager.instance.SwitchToLeague();

                    // Go to leagues Screen
                    InstanceManager.menuSceneController.GoToScreen(MenuScreen.LEAGUES);
                }, 0.25f
			);
		}
	} 
}
