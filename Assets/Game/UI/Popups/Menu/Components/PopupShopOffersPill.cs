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
using System.Text;

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
	[SerializeField] private Text m_previousPriceText = null;
	[SerializeField] private GameObject m_featuredHighlight = null;

	[Space]
	[SerializeField] private GameObject m_priceButtonGroup = null;
	[SerializeField] private GameObject m_loadingPricePlaceholder = null;

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
	private float m_discount = 0f;
	private float m_previousPrice = 0f;
	private bool m_waitingForPrice = false;
	private StringBuilder m_sb = new StringBuilder();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Waiting for price?
		if(m_waitingForPrice) {
			// Store initialized?
			if(GameStoreManager.SharedInstance.IsReady()) {
				// Yes! Refresh prices
				RefreshPrice();
			}
		}
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

		// Store def and some vars
		m_def = _pack.def;
		m_currency = UserProfile.Currency.REAL; // For now offer packs are only bought wtih real money!

		// Discount
		m_discount = m_pack.def.GetAsFloat("discount", 0f);
		bool validDiscount = m_discount > 0f;
		if(validDiscount) {
			m_discount = Mathf.Clamp(m_discount, 0.01f, 0.99f); // [AOC] Just to be sure input discount is valid
		}
		
		// Init visuals
		OfferColorGradient gradientSetup = null;
		if(validDiscount) {
			gradientSetup = OfferItemPrefabs.GetGradient(m_discount);
		} else {
			gradientSetup = OfferItemPrefabs.GetGradient(0.9f);	// [AOC] Hardcoded!! Use high-discount colors
		}

		// Pack name
		if(m_packNameText != null) {
			m_packNameText.Localize(m_pack.def.GetAsString("tidName"));
			m_packNameText.text.enableVertexGradient = true;
			m_packNameText.text.colorGradient = Gradient4ToVertexGradient(gradientSetup.titleGradient);
		}

		// Timer
		if(m_remainingTimeText != null) {
			m_remainingTimeText.gameObject.SetActive(m_pack.isTimed);   // Don't show if offer is not timed
			RefreshTimer();
		}

		// Discount
		// Don't show if no discount is applied
		if(m_discountText != null) {
			m_discountText.gameObject.SetActive(validDiscount);
			if(validDiscount) {
				m_discountText.text.colorGradient = Gradient4ToVertexGradient(gradientSetup.discountGradient);
				m_discountText.Localize(
					"TID_OFFER_DISCOUNT_PERCENTAGE",
					StringUtils.FormatNumber(m_discount * 100f, 0)
				);
			}
		}

		// Featured highlight
		if(m_featuredHighlight != null) {
			m_featuredHighlight.SetActive(m_pack.featured);
		}

		// Items
		for(int i = 0; i < m_itemSlots.Length; ++i) {
			// Skip if no slot (i.e. single item layouts)
			OfferItemSlot slot = m_itemSlots[i];
			if(slot == null) continue;

			// Start hidden and initialize after some delay
			// [AOC] We do this because initializing the slots at the same time that the popup is being instantiated results in weird behaviours
			slot.InitFromItem(null);
			if(i < m_pack.items.Count) {
				OfferPackItem item = m_pack.items[i];
				UbiBCN.CoroutineManager.DelayedCallByFrames(() => {
					slot.InitFromItem(item);
				}, 1);
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

		// Price texts
		RefreshPrice();
	}

	/// <summary>
	/// Refresh all price-related texts.
	/// </summary>
	private void RefreshPrice() {
		// If localized prices haven't been received from the store yet, wait for it
		bool storeReady = GameStoreManager.SharedInstance.IsReady();

		// Loading placeholder
		if(m_loadingPricePlaceholder != null) m_loadingPricePlaceholder.gameObject.SetActive(!storeReady);

		// Internal flag
		m_waitingForPrice = !storeReady;

		// Initialize price
		// Get localized price
		m_price = m_def.GetAsFloat("refPrice");
		StoreManager.StoreProduct productInfo = null;
		if(GameStoreManager.SharedInstance.IsReady()) {
			productInfo = GameStoreManager.SharedInstance.GetStoreProduct(GetIAPSku());
			if(productInfo != null) {
				// Price is localized by the store api, if available
				m_price = productInfo.m_fLocalisedPriceValue;
			}
		}

		// Compute previous price
		bool validDiscount = m_discount > 0f;
		if(validDiscount) {
			m_previousPrice = m_price / (1f - m_discount);

			// [AOC] Beautify original price so it's more credible
			// 		 Put the same decimal part as the actual price
			m_previousPrice = Mathf.Floor(m_previousPrice) + (m_price - Mathf.Floor(m_price));
		} else {
			m_previousPrice = m_price;
		}

#if DEBUG && false
		Debug.Log(Colors.yellow.Tag(
			"Valid Discount: " + validDiscount + "\n"
			+ "Previous Price: " + m_previousPrice + "\n"
			+ "Price: " + m_price + "\n"
			+ "Store Ready: " + storeReady
		));
#endif

		// Buttons
		if(m_priceButtonGroup != null) {
			// Show?
			m_priceButtonGroup.gameObject.SetActive(storeReady);

			// If store is ready, initialize textfields
			if(storeReady) {
				// Price Text
				string localizedPrice = GetLocalizedIAPPrice(m_price);
				if(m_priceText != null) {
					m_priceText.text = localizedPrice;
				}

				// Original price
				if(m_previousPriceText != null) {
					// Don't show if there is no valid discount
					m_previousPriceText.gameObject.SetActive(validDiscount);
					if(validDiscount) {
						// [AOC] This gets quite tricky. We will try to keep the format of the 
						//		 localized price (given by the store), but replacing the actual amount.
						// Supported cases: "$150" "150€" "$ 150" "150 €"
						string localizedPreviousPrice = StringUtils.FormatNumber(m_previousPrice, 2);
						string currencySymbol = (productInfo != null) ? productInfo.m_strCurrencySymbol : "$";

						// a) "$150"
						if(localizedPrice.StartsWith(currencySymbol, StringComparison.InvariantCultureIgnoreCase)) {
							localizedPreviousPrice = currencySymbol + localizedPreviousPrice;
						}

						// b) "$ 150"
						else if(localizedPrice.StartsWith(currencySymbol + " ", StringComparison.InvariantCultureIgnoreCase)) {
							localizedPreviousPrice = currencySymbol + " " + localizedPreviousPrice;
						}

						// c) "150€"
						else if(localizedPrice.EndsWith(currencySymbol, StringComparison.InvariantCultureIgnoreCase)) {
							localizedPreviousPrice = localizedPreviousPrice + currencySymbol;
						}

						// d) "150 €"
						else if(localizedPrice.EndsWith(" " + currencySymbol, StringComparison.InvariantCultureIgnoreCase)) {
							localizedPreviousPrice = localizedPreviousPrice + " " + currencySymbol;
						}

						// e) Anything else
						else {
							// Show just the formatted number - nothing to do
						}

						// Done! Set text
						m_previousPriceText.text = localizedPreviousPrice;
					}
				}
			}
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
		if(m_remainingTimeText == null) return;

		// If pack is active, update text
		if(m_pack.isActive) {
			m_sb.Length = 0;
			m_remainingTimeText.Localize(
				m_remainingTimeText.tid, 
				m_sb.Append("<nobr>")
				.Append(TimeUtils.FormatTime(
					System.Math.Max(0, m_pack.remainingTime.TotalSeconds), // Just in case, never go negative
					TimeUtils.EFormat.ABBREVIATIONS,
					4
				))
				.Append("</nobr>")
				.ToString()
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

	/// <summary>
	/// A purchase has been started.
	/// </summary>
	protected override void OnPurchaseStarted() {
		// Prevent offers from expiring
		OffersManager.autoRefreshEnabled = false;
	}

	/// <summary>
	/// A purchase has finished.
	/// </summary>
	/// <param name="_success">Has it been successful?</param>
	protected override void OnPurchaseFinished(bool _success) {
		// Restore offers auto-refresh
		OffersManager.autoRefreshEnabled = true;
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