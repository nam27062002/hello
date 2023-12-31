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
public class ShopRemoveAdsPill : ShopMonoRewardPill {
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

        // Applies offer and saves persistence
        m_pack.Apply(); 

        // Close all open popups
        PopupManager.Clear(true);

        // Move to the rewards screen
        PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
        scr.StartFlow(false);   // No intro
        InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);

        // If we are in the shop tell the menu controller to open the shop after the rewards screen
        if (InstanceManager.menuSceneController.currentScreen == MenuScreen.SHOP)
        {
            InstanceManager.menuSceneController.interstitialPopupsController.SetFlag(MenuInterstitialPopupsController.StateFlag.OPEN_SHOP, true);
        }
    }

    /// <summary>
    /// Get the tracking id for transactions performed by this shop pill
    /// </summary>
    /// <returns>The tracking identifier.</returns>
    override protected HDTrackingManager.EEconomyGroup GetTrackingId() {
		return HDTrackingManager.EEconomyGroup.SHOP_REMOVE_ADS_PACK;
	}


    /// <summary>
	/// Initialize the pill with a given pack's data.
	/// </summary>
	/// <param name="_pack">Pack.</param>
	public override void InitFromOfferPack(OfferPack _pack)
    {
        // Store new pack
        m_pack = _pack;
        m_def = null;

        // If null, or pack is not a ctive, hide this pill and return
        if (m_pack == null || !m_pack.isActive)
        {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);

        // Store def and some vars
        m_def = _pack.def;
        m_currency = _pack.currency; // Since v.2.2 offers can be paid in different currencies

        // Discount
        m_discount = m_pack.def.GetAsFloat("discount", 0f);
        bool validDiscount = m_discount > 0f;
        if (validDiscount)
        {
            m_discount = Mathf.Clamp(m_discount, 0.01f, 0.99f); // [AOC] Just to be sure input discount is valid
        }

        // Pack name
        if (m_packNameText != null)
        {
            m_packNameText.Localize(m_pack.def.GetAsString("tidName"));
        }

        // Price texts
        RefreshPrice();
    }

	/// <summary>
	/// Open the extended info popup for this pill.
	/// </summary>
	/// <param name="_trackInfoPopupEvent">Whether to send tracking event or not for the custom.player.infopopup event.</param>
	protected override void OpenInfoPopup(bool _trackInfoPopupEvent) {
		// Override parent to open the RemoveAdsOffer Popup instead
		// Load the popup
		PopupController popup = PopupManager.LoadPopup(PopupRemoveAdsOffer.PATH);
		PopupRemoveAdsOffer popupRemoveAdsOffer = popup.GetComponent<PopupRemoveAdsOffer>();

		// Initialize it with the remove ad offer (if exists)
		popupRemoveAdsOffer.Init();

		// Show the popup
		popup.Open();

		// If defined, send tracking event
		if(_trackInfoPopupEvent) {
			string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupRemoveAdsOffer.PATH);
			TrackInfoPopup(popupName);
		}
	}
}