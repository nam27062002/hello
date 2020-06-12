// PopupShopFreeOfferPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/10/2019.
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
/// Single pill in the shop for a FREE offer pack.
/// </summary>
public class ShopReferralPill : ShopMonoRewardPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Separator("Referral install Pill Specifics")]
	[SerializeField] private FriendCounter m_friendCounter;

	[SerializeField] private Button m_buttonInvite = null;
	[SerializeField] private Button m_buttonClaim = null;

    [SerializeField] private Button m_pillHitArea = null;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with a given pack's data.
	/// </summary>
	/// <param name="_pack">Pack.</param>
	public override void InitFromOfferPack(OfferPack _pack) {
		// Call parent
		base.InitFromOfferPack(_pack);

		if (m_friendCounter != null)
			m_friendCounter.InitFromOfferPack( (OfferPackReferral) _pack);

	}


	/// <summary>
	/// Get the tracking id for transactions performed by this shop pill
	/// </summary>
	/// <returns>The tracking identifier.</returns>
	override protected HDTrackingManager.EEconomyGroup GetTrackingId() {
        // TODO: define the proper tracking id
		return HDTrackingManager.EEconomyGroup.SHOP_AD_OFFER_PACK;
	}


	/// <summary>
	/// Open the extended info popup for this pill.
	/// </summary>
	/// <param name="_trackInfoPopupEvent">Whether to send tracking event or not for the custom.player.infopopup event.</param>
	protected override void OpenInfoPopup(bool _trackInfoPopupEvent)
	{
		// Override parent to open the RemoveAdsOffer Popup instead
		// Load the popup
		PopupController popup = PopupManager.LoadPopup(PopupShopReferral.PATH);
		PopupShopReferral popupReferralInstall = popup.GetComponent<PopupShopReferral>();

		// Initialize it with the remove ad offer (if exists)
		popupReferralInstall.InitFromOfferPack(pack);

		// Show the popup
		popup.Open();

		// If defined, send tracking event
		if (_trackInfoPopupEvent)
		{
			string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupShopReferral.PATH);
			TrackInfoPopup(popupName);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}