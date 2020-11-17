// PopupWelcomeBack.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 23/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//

using System;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup Welcome Back. It is shown when the feature is activated.
/// </summary>

[RequireComponent(typeof(PopupController))]
public class PopupWelcomeBack : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//
    public const string PATH = "UI/Popups/Menu/PF_PopupWelcomeBack";
    
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
    [SerializeField] private GameObject m_benefitsWithOffer;
    [SerializeField] private GameObject m_benefitsWithoutOffer;
	
	//------------------------------------------------------------------------//
	//  METHODS														          //
	//------------------------------------------------------------------------//


    public void OnEnable()
    {
  
        // Welcome back has a special offer for some type of players, not for all
        bool showSpecialOffer = WelcomeBackManager.instance.hasSpecialOffer;
        
        // Show the proper perk icons in the popup
        m_benefitsWithOffer.SetActive(showSpecialOffer);
        m_benefitsWithoutOffer.SetActive(!showSpecialOffer);
        
        // Dont show it again
        WelcomeBackManager.instance.isPopupWaiting = false;

        // Send some tracking
        HDTrackingManager.Instance.Notify_InfoPopup("Menu/PF_PopupWelcomeBack", "automatic");
    }

    //------------------------------------------------------------------------//
    //  CALLBACK													          //
    //------------------------------------------------------------------------//
    
    /// <summary>
    /// The dismiss button has been pressed.
    /// </summary>
    public void OnDismissButton() {
        // Just close the poopup
        GetComponent<PopupController>().Close(true);
    }
    
    
}