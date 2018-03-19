// PopupShopOffersPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;

using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single pill in the shop for an offer pack.
/// </summary>
public class PopupShopOffersPill : IPopupShopPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float TIMER_REFRESH_INTERVAL = 1;	// Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private OfferItemUI[] m_itemsPreview = null;

	[Space]
	[SerializeField] private Localizer m_packNameText = null;
	[SerializeField] private Localizer m_discountText = null;
	[SerializeField] private Localizer m_remainingTimeText = null;
	[Space]
	[SerializeField] private Text m_priceText = null;
	[SerializeField] private TextMeshProUGUI m_previousPriceText = null;

	// Public
	private OfferPack m_pack = null;
	public OfferPack pack {
		get { return m_pack; }
	}

	// Internal
	private float m_previousPrice = 0f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with a given pack's data.
	/// </summary>
	/// <param name="_pack">Pack.</param>
	public void InitFromOfferPack(OfferPack _pack) {
		// Store new pack
		m_pack = _pack;

		// If null, or pack can't be displayed, hide this pill and return
		if(m_pack == null || !m_pack.CanBeDisplayed()) {
			this.gameObject.SetActive(false);
			return;
		}

		// Pricing
		m_currency = UserProfile.Currency.REAL;	// For now offer packs are only bought wtih real money!
		m_price = m_def.GetAsFloat("refPrice");
		if(GameStoreManager.SharedInstance.IsReady()) {
			StoreManager.StoreProduct productInfo = GameStoreManager.SharedInstance.GetStoreProduct(GetIAPSku());
			if(productInfo != null) {
				// Price is localized by the store api, if available
				m_price = productInfo.m_fLocalisedPriceValue;
			}
		}

		// Compute price before applying the discount
		float discount = m_pack.def.GetAsFloat("discount");
		m_previousPrice = m_price/discount;

		// Init visuals
		// Pack name
		m_packNameText.Localize(m_pack.def.GetAsString("tidName"));

		// Timer
		RefreshTimer();

		// Discount
		m_discountText.Localize(
			"TID_OFFER_DISCOUNT_PERCENTAGE",
			StringUtils.FormatNumber(m_pack.def.GetAsFloat("discount") * 100f, 0)
		);

		// Price
		m_priceText.text = GetLocalizedIAPPrice(m_price);

		// Original price
		// [AOC] This gets quite tricky. We will try to keep the format of the 
		//		 localized price (given by the store), but replacing the actual amount.
		// $150 150€ 150 €
		// [AOC] TODO!! Let's just put the formatted number for now
		m_previousPriceText.text = StringUtils.FormatNumber(m_previousPrice, 0);

		// Items
		for(int i = 0; i < m_itemsPreview.Length; ++i) {
			// If there are not enough item, hide the slot!
			if(i >= m_pack.items.Count) {
				m_itemsPreview[i].InitFromItem(null);
			} else {
				m_itemsPreview[i].InitFromItem(m_pack.items[i]);
			}
		}
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// https://docs.unity3d.com/ScriptReference/MonoBehaviour.InvokeRepeating.html
	/// </summary>
	public void RefreshTimer() {
		// Skip if no target offer
		if(m_pack == null) return;

		// Don't show timer if offer is not timed
		m_remainingTimeText.gameObject.SetActive(m_pack.isTimed);

		// Update text
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
		m_remainingTimeText.Localize(
			m_remainingTimeText.tid, 
			TimeUtils.FormatTime(
				System.Math.Max(0, (m_pack.endDate - serverTime).TotalSeconds), // Just in case, never go negative
				TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
				4
			)
		);
	}

	//------------------------------------------------------------------------//
	// IPopupShopPill IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Obtain the IAP sku as defined in the App Stores.
	/// </summary>
	/// <returns>The IAP sku corresponding to this shop pack. Empty if not an IAP.</returns>
	override protected string GetIAPSku() {
		// Only for REAL money packs
		if(m_currency != UserProfile.Currency.REAL) return string.Empty;
		return m_def.GetAsString("iapSku");
	}

	/// <summary>
	/// Get the tracking id for transactions performed by this shop pill
	/// </summary>
	/// <returns>The tracking identifier.</returns>
	override protected HDTrackingManager.EEconomyGroup GetTrackingId() {
		return HDTrackingManager.EEconomyGroup.SHOP_OFFER_PACK;
	}

	/// <summary>
	/// Apply the shop pack to the current user!
	/// Invoked after a successful purchase.
	/// </summary>
	override protected void ApplyShopPack() {
		// [AOC] TODO!!

		// Save persistence
		PersistenceFacade.instance.Save_Request(true);
	}

	/// <summary>
	/// Shows the purchase success feedback.
	/// </summary>
	override protected void ShowPurchaseSuccessFeedback() {
		// [AOC] TODO!!
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}