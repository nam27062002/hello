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

	}

	/// <summary>
	/// Get the info button mode for this pill's pack.
	/// </summary>
	/// <returns>The desired button mode.</returns>
	protected override InfoButtonMode GetInfoButtonMode() {
		// Only for Eggs
		InfoButtonMode mode = InfoButtonMode.NONE; // None by default with rotationals
		if(m_pack != null && m_pack.items.Count > 0 && m_pack.items[0] != null) {
			// Info button mode depends on pack's item type
			switch(m_pack.items[0].type) {
				case Metagame.RewardEgg.TYPE_CODE: {
					mode = InfoButtonMode.TOOLTIP;
				} break;
			}
		}

		return mode;
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// https://docs.unity3d.com/ScriptReference/MonoBehaviour.InvokeRepeating.html
	/// </summary>
	public override void RefreshTimer() {
		// Call parent
		base.RefreshTimer();

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

		// If we are in the shop tell the menu controller to open the shop after the rewards screen
		if (InstanceManager.menuSceneController.currentScreen == MenuScreen.SHOP)
		{
			InstanceManager.menuSceneController.interstitialPopupsController.SetFlag(MenuInterstitialPopupsController.StateFlag.OPEN_SHOP, true);
		}
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