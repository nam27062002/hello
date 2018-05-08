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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private OfferItemSlot[] m_itemSlots = null;

	[Space]
	[SerializeField] private Localizer m_packNameText = null;
	[SerializeField] private Localizer m_discountText = null;
	[SerializeField] private Localizer m_remainingTimeText = null;
	[Space]
	[SerializeField] private Text m_priceText = null;
	[SerializeField] private TextMeshProUGUI m_previousPriceText = null;
	[SerializeField] private GameObject m_featuredHighlight = null;
	[Separator("Optional Decorations")]
	[SerializeField] private UIGradient m_backgroundGradient = null;
	[SerializeField] private UIGradient m_frameGradientLeft = null;
	[SerializeField] private UIGradient m_frameGradientRight = null;

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
		m_def = null;

		// If null, or pack is not a ctive, hide this pill and return
		if(m_pack == null || !m_pack.isActive) {
			this.gameObject.SetActive(false);
			return;
		}
		this.gameObject.SetActive(true);

		// Store def
		m_def = _pack.def;

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
		m_previousPrice = m_price/(1f - discount);

		// Init visuals
		OfferColorGradient gradientSetup = OfferItemPrefabs.GetGradient(discount);

		// Pack name
		m_packNameText.Localize(m_pack.def.GetAsString("tidName"));
		m_packNameText.text.enableVertexGradient = true;
		m_packNameText.text.colorGradient = Gradient4ToVertexGradient(gradientSetup.titleGradient);

		// Timer
		m_remainingTimeText.gameObject.SetActive(m_pack.isTimed);	// Don't show if offer is not timed
		RefreshTimer();

		// Discount
		m_discountText.text.colorGradient = Gradient4ToVertexGradient(gradientSetup.discountGradient);
		m_discountText.Localize(
			"TID_OFFER_DISCOUNT_PERCENTAGE",
			StringUtils.FormatNumber(discount * 100f, 0)
		);

		// Price
		m_priceText.text = GetLocalizedIAPPrice(m_price);

		// Original price
		// [AOC] This gets quite tricky. We will try to keep the format of the 
		//		 localized price (given by the store), but replacing the actual amount.
		// $150 150€ 150 €
		// [AOC] TODO!! Let's just put the formatted number for now
		m_previousPriceText.text = StringUtils.FormatNumber(m_previousPrice, 0);

		// Featured highlight
		m_featuredHighlight.SetActive(m_pack.featured);

		// Items
		for(int i = 0; i < m_itemSlots.Length; ++i) {
			// Skip if no slot (i.e. single item layouts)
			if(m_itemSlots[i] == null) continue;

			// If there are not enough item, hide the slot!
			if(i >= m_pack.items.Count) {
				m_itemSlots[i].InitFromItem(null);
			} else {
				m_itemSlots[i].InitFromItem(m_pack.items[i]);
			}
		}

		// Optional decorations
		if(m_backgroundGradient != null) {
			m_backgroundGradient.gradient.Set(gradientSetup.pillBackgroundGradient);
		}

		if(m_frameGradientLeft != null) {
			m_frameGradientLeft.gradient.Set(gradientSetup.pillFrameGradient);
		}

		if(m_frameGradientRight != null) {
			m_frameGradientRight.gradient.Set(gradientSetup.pillFrameGradient);
		}
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// https://docs.unity3d.com/ScriptReference/MonoBehaviour.InvokeRepeating.html
	/// </summary>
	public void RefreshTimer() {
		// Skip if no target offer or target offer is not timed
		if(m_pack == null) return;
		if(!m_pack.isTimed) return;

		// If pack is active, update text
		if(m_pack.isActive) {
			m_remainingTimeText.Localize(
				m_remainingTimeText.tid, 
				TimeUtils.FormatTime(
					System.Math.Max(0, m_pack.remainingTime.TotalSeconds), // Just in case, never go negative
					TimeUtils.EFormat.ABBREVIATIONS,
					4
				)
			);
		
		// If pack has expired, hide this pill
		} else {
			InitFromOfferPack(null);	// This will do it
		}
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
		// The pack will push all rewards to the reward stack
		m_pack.Apply();	// This already saves persistence

		// Close all open popups
		PopupManager.Clear(true);

		// Move to the rewards screen
		PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
		scr.StartFlow(false);	// No intro
		InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);
	}

	/// <summary>
	/// Shows the purchase success feedback.
	/// </summary>
	override protected void ShowPurchaseSuccessFeedback() {
		// [AOC] TODO!!
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Convert from Gradient4 to TMP's VertexGradient.
	/// </summary>
	/// <returns>TMP's VertexGradient matching input Gradient4.</returns>
	/// <param name="_gradient">Input Gradient4.</param>
	private VertexGradient Gradient4ToVertexGradient(Gradient4 _gradient) {
		return new VertexGradient(
			_gradient.topLeft,
			_gradient.topRight,
			_gradient.bottomLeft,
			_gradient.bottomRight
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}