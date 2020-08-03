// PopupInfoEgg.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Eggs info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupInfoBabyPets : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoBabyPets";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

    /// <summary>
    /// Open the popup and mark this tutorial step as completed
    /// </summary>
    public static void ShowPopup ()
    {
		// Load the popup
		PopupController popup = PopupManager.LoadPopup(PopupInfoBabyPets.PATH);
		PopupInfoBabyPets popupPets = popup.GetComponent<PopupInfoBabyPets>();

		// Show the popup
		PopupManager.EnqueuePopup(popup);

		// Mark this tutorial step as completed
		UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.BABY_PETS_INFO);
	}
}
