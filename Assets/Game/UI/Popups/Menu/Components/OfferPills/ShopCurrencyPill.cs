// PopupShopCurrencyPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single pill in the shop for a currency pack.
/// </summary>
public class ShopCurrencyPill : ShopMonoRewardPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Currency Pill Specifics")]
	[SerializeField] protected Localizer m_bonusAmountText = null;	// [AOC] Unused as of 2.8, but keep it just in case

	// Internal
	protected UserProfile.Currency m_type = UserProfile.Currency.NONE;
	protected long m_amountApplied = 0;

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with a given pack's data.
	/// </summary>
	/// <param name="_pack">Pack.</param>
	public override void InitFromOfferPack(OfferPack _pack) {
		// Let parent do the hard work and do some extra initialization afterwards
		base.InitFromOfferPack(_pack);

		// If null, hide this pill and return
		this.gameObject.SetActive(m_def != null);
		if(m_def == null) return;

		// Init internal vars
		m_type = UserProfile.SkuToCurrency(m_def.Get("type"));

		// Bonus amount
        if (m_bonusAmountText != null)
        {
            float bonusAmount = m_def.GetAsFloat("bonusAmount");
            m_bonusAmountText.gameObject.SetActive(bonusAmount > 0f);
            m_bonusAmountText.Localize("TID_SHOP_BONUS_AMOUNT", StringUtils.MultiplierToPercentage(bonusAmount));   // 15% extra
        }
	}


    public void InitFromDef(DefinitionNode _def)
    {
        OfferPackCurrency offer = new OfferPackCurrency();
        offer.InitFromDefinition(_def);
        InitFromOfferPack(offer);
    }

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the info button mode for this pill's pack.
	/// </summary>
	/// <returns>The desired button mode.</returns>
	protected override InfoButtonMode GetInfoButtonMode() {
		// Never for currency pills
		return InfoButtonMode.NONE;
	}

	//------------------------------------------------------------------------//
	// IPopupShopPill IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Obtain the IAP sku as defined in the App Stores.
	/// </summary>
	/// <returns>The IAP sku corresponding to this shop pack. Empty if not an IAP.</returns>
	override public string GetIAPSku() {
		// Only for REAL money packs
		if(m_currency != UserProfile.Currency.REAL) return string.Empty;

		// In the case of currency packs, sku matches the one in the IAP
		if(m_def != null) {
			return m_def.sku;
		} else {
			return string.Empty;
		}
	}

	/// <summary>
	/// Get the tracking id for transactions performed by this shop pill
	/// </summary>
	/// <returns>The tracking identifier.</returns>
	override protected HDTrackingManager.EEconomyGroup GetTrackingId() {
		switch(m_type) {
			case UserProfile.Currency.HARD: {
				return HDTrackingManager.EEconomyGroup.SHOP_PC_PACK;
			} break;

			case UserProfile.Currency.SOFT: {
				return HDTrackingManager.EEconomyGroup.SHOP_COINS_PACK;
			} break;

			case UserProfile.Currency.KEYS: {
				return HDTrackingManager.EEconomyGroup.SHOP_KEYS_PACK;
			} break;
		}
		return HDTrackingManager.EEconomyGroup.SHOP_COINS_PACK;
	}

	/// <summary>
	/// Apply the shop pack to the current user!
	/// Invoked after a successful purchase.
	/// </summary>
	override protected void ApplyShopPack() {
		// [AOC] Rather than following the normal Offer Pack purchase flow (rewards screen, etc.), keep it simpler for currencies
		// Make sure we have a valid item to purchase!
		Metagame.Reward reward = null;
		if(m_pack != null && m_pack.items.Count > 0) {
			reward = m_pack.items[0].reward;
		}
		if(reward == null) {
			Debug.LogError(Colors.red.Tag("ERROR! Attempting to purchase currency pack without items defined. Skipping purchase."));
			return;
		}

		// Add amount
        switch (m_type) {
            case UserProfile.Currency.SOFT: {
				m_amountApplied = reward.amount;
                UsersManager.currentUser.EarnCurrency(UserProfile.Currency.SOFT, (ulong)m_amountApplied, true, HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE);
            } break;

            case UserProfile.Currency.HARD: {
				// Get the proper amount after applying the happy hour
				m_amountApplied = OffersManager.happyHourManager.happyHour.ApplyHappyHourExtra(reward.amount);

                // Add the amount to the player currencies
                UsersManager.currentUser.EarnCurrency(UserProfile.Currency.HARD, (ulong)m_amountApplied, true, HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE);

                // Force HH popup if the player is in the shop screen right now (only scene)
                bool forceHHPopup = InstanceManager.menuSceneController.currentScreen == MenuScreen.SHOP;
                
                // Broadcast this event, so the happy hour can be activated / extended
                Messenger.Broadcast<bool, string>(MessengerEvents.HC_PACK_ACQUIRED, forceHHPopup, m_def.sku);
            } break;

			case UserProfile.Currency.KEYS: {
				m_amountApplied = reward.amount;
                UsersManager.currentUser.EarnCurrency(UserProfile.Currency.KEYS, (ulong)m_amountApplied, true, HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE);
			} break;
		}

		// Save persistence
		PersistenceFacade.instance.Save_Request(true);
	}

	/// <summary>
	/// Shows the purchase success feedback.
	/// </summary>
	override protected void ShowPurchaseSuccessFeedback() {
        // Notify player
        UINotificationShop.CreateAndLaunch(
			UserProfile.SkuToCurrency(def.Get("type")),
			m_amountApplied, // Use this stored value, as the happy hour rate could have changed in the last frame
			Vector3.down * 150f, 
			this.GetComponentInParent<Canvas>().transform as RectTransform
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}