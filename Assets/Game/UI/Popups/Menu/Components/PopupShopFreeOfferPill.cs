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
public class PopupShopFreeOfferPill : PopupShopOffersPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Separator("Free Offer Pill Specifics")]
	[SerializeField] private Button m_watchAdButton = null;
    [SerializeField] private Localizer m_adButtonText = null;
    [SerializeField] private Button m_freeButton = null;
    [SerializeField] private Localizer m_freeButtonText = null;

	// Internal logic
	private bool m_isOnCooldown = false;
	private string m_defaultButtonTID = "";
    private bool removeAdsActive;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Backup some values
		m_defaultButtonTID = m_adButtonText.tid;
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

		// Perform a first refresh
		RefreshTimer();
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// https://docs.unity3d.com/ScriptReference/MonoBehaviour.InvokeRepeating.html
	/// </summary>
	public override void RefreshTimer() {
		// Call parent
		base.RefreshTimer();

		// Has state changed?
		TimeSpan remainingCooldown = OffersManager.freeOfferRemainingCooldown;
		bool isOnCooldown = remainingCooldown.TotalSeconds > 0;
		if(m_isOnCooldown != isOnCooldown) {
			// Enable/Disable button
			m_watchAdButton.interactable = !isOnCooldown;
            m_freeButton.interactable = !isOnCooldown;

			// If leaving cooldown, restore text
			if(!isOnCooldown) {
				m_adButtonText.Localize("TID_FREE_DAILY_REWARD_BUTTON");
                m_freeButtonText.Localize("TID_FREE_DAILY_REWARD_BUTTON");
            }
			
			// Save new state
			m_isOnCooldown = isOnCooldown;
		}

		// If on cooldown, refresh timer
		if(m_isOnCooldown) {
			// Set text
			m_adButtonText.Set(TimeUtils.FormatTime(remainingCooldown.TotalSeconds, TimeUtils.EFormat.DIGITS, 3));
            m_freeButtonText.Set(TimeUtils.FormatTime(remainingCooldown.TotalSeconds, TimeUtils.EFormat.DIGITS, 3));
        }

        // Show the proper buttons
        removeAdsActive = UsersManager.currentUser.removeAds.IsActive;
        m_freeButton.gameObject.SetActive(removeAdsActive);
        m_watchAdButton.gameObject.SetActive(!removeAdsActive);

    }

	/// <summary>
	/// Get the tracking id for transactions performed by this shop pill
	/// </summary>
	/// <returns>The tracking identifier.</returns>
	override protected HDTrackingManager.EEconomyGroup GetTrackingId() {
		return HDTrackingManager.EEconomyGroup.SHOP_AD_OFFER_PACK;
	}

	/// <summary>
	/// Apply the shop pack to the current user!
	/// Invoked after a successful purchase.
	/// </summary>
	override protected void ApplyShopPack() {
		// We are going to go to the rewards screen as any other offer, but we want
		// to open the shop again once all rewards are collected so the player
		// realizes the free offer is on cooldown and takes one more look at the
		// rest of offers

		// Let parent do the hard work
		base.ApplyShopPack();

		// Make sure we are on the menu
		if(InstanceManager.menuSceneController == null) return;

		// Tell the menu controller to open the shop after the rewards screen
		InstanceManager.menuSceneController.interstitialPopupsController.SetFlag(MenuInterstitialPopupsController.StateFlag.OPEN_OFFERS_SHOP, true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Parent has started the purchase logic.
	/// </summary>
	protected override void OnPurchaseStarted() {
		// Call parent
		base.OnPurchaseStarted();

        // If the player has remove ads offer, give him the reward directly
        if (UsersManager.currentUser.removeAds.IsActive)
        {
            OnVideoRewardCallback(true);
            return;
        }

		// Ignore if offline
		if(DeviceUtilsManager.SharedInstance.internetReachability == NetworkReachability.NotReachable) {
			// Show some feedback
			UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_AD_ERROR"),
				new Vector2(0.5f, 0.33f),
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);

			// Tell the pill purchase has failed
			EndPurchase(false);
			return;
		}

		// Show video ad!
		PopupAdBlocker.LaunchAd(true, GameAds.EAdPurpose.FREE_OFFER_PACK, OnVideoRewardCallback);
	}

	/// <summary>
	/// Ad visualization has finished.
	/// </summary>
	/// <param name="_success">Whether the ad has successfully been played.</param>
	private void OnVideoRewardCallback(bool _success) {
		// Tell the pill logic the result of the purchase
		EndPurchase(_success);
	}

	/// <summary>
	/// Parent has finished the purchase logic.
	/// </summary>
	/// <param name="_success">Has it been successful?</param>
	protected override void OnPurchaseFinished(bool _success) {
		// Call parent
		base.OnPurchaseFinished(_success);
	}
}