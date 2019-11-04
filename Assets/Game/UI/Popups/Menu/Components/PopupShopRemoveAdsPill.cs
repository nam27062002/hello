// PopupShopRemoveAdsPill.cs
// Hungry Dragon
// 
// Created by Jose M. Olea
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;
using System.Text;

using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single pill in the shop for ads removal offer
/// </summary>
public class PopupShopRemoveAdsPill : PopupShopOffersPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Separator("Remove Ads Pill Specifics")]
	[SerializeField] private Button m_buyButton = null;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // PARENT OVERRIDES														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Apply the shop pack to the current user!
    /// Invoked after a successful purchase.
    /// </summary>
    override protected void ApplyShopPack()
    {
        // Activate the ads removal feature
        UsersManager.currentUser.removeAds.SetActive(true);

        // Saves persistence
        m_pack.Apply(); 

        // Close all open popups
        PopupManager.Clear(true);

    }

    /// <summary>
    /// Get the tracking id for transactions performed by this shop pill
    /// </summary>
    /// <returns>The tracking identifier.</returns>
    override protected HDTrackingManager.EEconomyGroup GetTrackingId() {
		return HDTrackingManager.EEconomyGroup.SHOP_REMOVE_ADS_PACK;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	
}