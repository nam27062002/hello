// ShopBasePill.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 06/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class ShopBasePill : IShopPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	protected enum InfoButtonMode {
		NONE,
		POPUP,
		TOOLTIP
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Space]
	[Header("Offer pill basics")]
	[SerializeField] protected Localizer m_packNameText = null;
	[SerializeField] protected Localizer m_remainingTimeText = null;
	[SerializeField] protected GameObject m_infoButton = null;

    [Space]
    [SerializeField] protected GameObject m_discountObj = null;
    [SerializeField] protected Localizer m_discountText = null;

    [Space]
	[SerializeField] private GameObject m_bannerObj = null;
	[SerializeField] private Localizer m_bannerText = null;

	[Space]
	[Header("Price Buttons")]
	[SerializeField] protected MultiCurrencyButton m_priceButtonGroup = null;
	[SerializeField] protected GameObject m_loadingPricePlaceholder = null;

	// Public
	protected OfferPack m_pack = null;
	public OfferPack pack {
		get { return m_pack; }
	}

	// Internal
	protected float m_discount = 0f;
	protected float m_previousPrice = 0f;
	protected bool m_waitingForPrice = false;
	protected StringBuilder m_sb = new StringBuilder();

	private ScrollRect scrollRect;

	protected InfoButtonMode m_infoButtonMode = InfoButtonMode.NONE;
	protected ShopTooltip m_tooltip = null;

	// Used to delay some initialization avoiding coroutines
	protected List<OfferPackItem> m_itemsToSet = new List<OfferPackItem>();
	protected List<OfferItemSlot> m_slotsToSet = new List<OfferItemSlot>();
	protected List<OfferItemSlot> m_activeSlots = new List<OfferItemSlot>();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	public void Start() {
		scrollRect = GetComponentInParent<ScrollRect>();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	protected virtual void Update() {
		// Delayed Initialization to avoid weird behaviours
		if(m_itemsToSet.Count > 0) {
			int l = m_itemsToSet.Count;
			for(int i = 0; i < l; i++) {
				m_slotsToSet[i].InitFromItem(m_itemsToSet[i], pack.order);
				m_activeSlots.Add(m_slotsToSet[i]);
			}
			m_itemsToSet.Clear();
			m_slotsToSet.Clear();
		}

		// Waiting for price?
		if(m_waitingForPrice) {
			// Store initialized?
			if(GameStoreManager.SharedInstance.IsReady()) {
				// Yes! Refresh prices
				RefreshPrice();
			}
		}

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected void OnDestroy() {
		// If a tooltip was created, destroy it as well
		if(m_tooltip != null) {
			Destroy(m_tooltip);
			m_tooltip = null;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with a given pack's data.
	/// </summary>
	/// <param name="_pack">Pack.</param>
	public override void InitFromOfferPack(OfferPack _pack) {
		// Clear collections - will be filled by derived classes
		m_itemsToSet.Clear();
		m_slotsToSet.Clear();
		m_activeSlots.Clear();

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
		m_currency = _pack.currency; // Since v.2.2 offers can be paid in different currencies

        // Discount
        m_discount = m_pack.def.GetAsFloat("discount", 0f);
		bool validDiscount = m_discount > 0f;
		if(validDiscount) {
			m_discount = Mathf.Clamp(m_discount, 0.01f, 0.99f); // [AOC] Just to be sure input discount is valid
		}

		// Pack name
		if(m_packNameText != null) {
			m_packNameText.Localize(m_pack.def.GetAsString("tidName"));
		}

		// Timer
		if(m_remainingTimeText != null) {
			m_remainingTimeText.gameObject.SetActive(m_pack.isTimed);   // Don't show if offer is not timed
			RefreshTimer();
		}

        // Discount
        // Don't show if no discount is applied
        if (m_discountObj != null)
        {
            m_discountObj.SetActive(validDiscount);
        }

        if (m_discountText != null)
        {
            m_discountText.gameObject.SetActive(validDiscount);
            if (validDiscount)
            {
                m_discountText.Localize(
                    "TID_OFFER_DISCOUNT_PERCENTAGE",
                    StringUtils.FormatNumber(m_discount * 100f, 0)
                );
            }
        }

        // Banner
        if (m_bannerObj != null) {
			if(m_def.GetAsBool("bestValue", false)) {
				m_bannerObj.SetActive(true);
				if(m_bannerText != null) m_bannerText.Localize("TID_SHOP_BEST_VALUE");
			} else if(m_def.GetAsBool("mostPopular", false)) {
				m_bannerObj.SetActive(true);
				if(m_bannerText != null) m_bannerText.Localize("TID_SHOP_MOST_POPULAR");
			} else {
				m_bannerObj.SetActive(false);
			}
		}

		// Info button - depends on offer type
		m_infoButtonMode = GetInfoButtonMode();
		if(m_infoButton != null) {
			m_infoButton.SetActive(m_infoButtonMode != InfoButtonMode.NONE);
		}

		// Price texts
		RefreshPrice();
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// https://docs.unity3d.com/ScriptReference/MonoBehaviour.InvokeRepeating.html
	/// </summary>
	public override void RefreshTimer() {
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
					2
				))
				.Append("</nobr>")
				.ToString()
			);

			// If pack has expired, hide this pill
		} else {
			InitFromOfferPack(null);    // This will do it
		}
	}

	/// <summary>
	/// Refresh all price-related texts.
	/// </summary>
	protected virtual void RefreshPrice() {
		// If localized prices haven't been received from the store yet, wait for it
		bool storeReady = GameStoreManager.SharedInstance.IsReady();
		if(m_currency == UserProfile.Currency.REAL) {
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

			// If store is ready, initialize textfields
			if(storeReady) {
				// Price Text
				string localizedPrice = GetLocalizedIAPPrice(m_price);

				// Original price
				string localizedPreviousPrice = null;

				// Don't show if there is no valid discount
				if(validDiscount) {
					// [AOC] This gets quite tricky. We will try to keep the format of the 
					//		 localized price (given by the store), but replacing the actual amount.
					// Supported cases: "$150" "150?" "$ 150" "150 ?"
					localizedPreviousPrice = StringUtils.FormatNumber(m_previousPrice, 2);
					string currencySymbol = (productInfo != null) ? productInfo.m_strCurrencySymbol : "$";

					// a) "$150"
					if(localizedPrice.StartsWith(currencySymbol, StringComparison.InvariantCultureIgnoreCase)) {
						localizedPreviousPrice = currencySymbol + localizedPreviousPrice;
					}

					// b) "$ 150"
					else if(localizedPrice.StartsWith(currencySymbol + " ", StringComparison.InvariantCultureIgnoreCase)) {
						localizedPreviousPrice = currencySymbol + " " + localizedPreviousPrice;
					}

					// c) "150?"
					else if(localizedPrice.EndsWith(currencySymbol, StringComparison.InvariantCultureIgnoreCase)) {
						localizedPreviousPrice = localizedPreviousPrice + currencySymbol;
					}

					// d) "150 ?"
					else if(localizedPrice.EndsWith(" " + currencySymbol, StringComparison.InvariantCultureIgnoreCase)) {
						localizedPreviousPrice = localizedPreviousPrice + " " + currencySymbol;
					}

					// e) Anything else
					else {
						// Show just the formatted number - nothing to do
					}

				}

				// Show the proper currency button
				if(m_priceButtonGroup != null) {
					m_priceButtonGroup.SetAmount(localizedPrice, m_currency, localizedPreviousPrice);

				}
			}

		} else if(m_currency == UserProfile.Currency.HARD || m_currency == UserProfile.Currency.SOFT) {
			// Loading placeholder
			if(m_loadingPricePlaceholder != null) m_loadingPricePlaceholder.gameObject.SetActive(false);

			// Round the price, just in case. It shouldnt have decimals for this currency.
			m_price = Mathf.RoundToInt(m_def.GetAsFloat("refPrice"));

			// Show the proper currency button
			if(m_priceButtonGroup != null) {
				m_priceButtonGroup.SetAmount(m_price, m_currency);
			}
		}

		// Show buttons?
		if(m_priceButtonGroup != null) {
			m_priceButtonGroup.gameObject.SetActive(storeReady);
		}
	}

	/// <summary>
	/// Open the extended info popup for this pill.
	/// </summary>
	/// <param name="_trackInfoPopupEvent">Whether to send tracking event or not for the custom.player.infopopup event.</param>
	protected virtual void OpenInfoPopup(bool _trackInfoPopupEvent) {
		// Nothing to do if not initialized
		if(m_pack == null) return;

		// Tooltip or popup?
		switch(m_infoButtonMode) {
			// Popup
			case InfoButtonMode.POPUP: {
				// Open offer info popup
				PopupController popup = PopupManager.LoadPopup(PopupFeaturedOffer.PATH);
				PopupFeaturedOffer offerPopup = popup.GetComponent<PopupFeaturedOffer>();
				offerPopup.InitFromOfferPack(m_pack);
				popup.Open();

				// Send tracking event
				if(_trackInfoPopupEvent) {
					string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupFeaturedOffer.PATH);
					TrackInfoPopup(popupName);
				}
			} break;

			// Tooltip
			case InfoButtonMode.TOOLTIP: {
				// Need at least 1 active slot to work
				if(m_activeSlots.Count < 1) return;

				// If tooltip instance was not yet created, do it now!
				if(m_tooltip == null) {
					ShopTooltip prefab = Resources.Load<ShopTooltip>(ShopSettings.shopTooltipPath);
					Debug.Assert(prefab != null, "Couldn't find the prefab for the Shop Tooltip! " + ShopSettings.shopTooltipPath);
					m_tooltip = Instantiate<ShopTooltip>(prefab, this.GetComponentInParent<Canvas>().transform);
					m_tooltip.gameObject.SetActive(false);
				}

				// Just in case
				if(m_tooltip == null) return;

				// Prevent spamming
				if(m_tooltip.isActiveAndEnabled) return;

				// Initialize tooltip content
				// Use the first item preview to initialize the tooltip's content
				m_activeSlots[0].preview.InitTooltip(m_tooltip.tooltip);

				// Initialize tooltip position
				m_tooltip.Init(
					m_infoButton.transform as RectTransform,
					new Vector2(20f, 0f)	// [AOC] HARDCODED!!
				);

				// Open the tooltip
				m_tooltip.Show();

				// Send tracking event
				if(_trackInfoPopupEvent) {
					string popupName = System.IO.Path.GetFileNameWithoutExtension(ShopSettings.shopTooltipPath);
					TrackInfoPopup(popupName);
				}
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATES													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the info button mode for this pill's pack.
	/// </summary>
	/// <returns>The desired button mode.</returns>
	protected virtual InfoButtonMode GetInfoButtonMode() {
		// Popup by default
		return InfoButtonMode.POPUP;
	}

	//------------------------------------------------------------------------//
	// TRACKING																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Send tracking event or not for the custom.player.infopopup event.
	/// </summary>
	/// <param name="_popupName">The name of the popup being opened.</param>
	protected void TrackInfoPopup(string _popupName) {
		HDTrackingManager.Instance.Notify_InfoPopup(_popupName, "offer_shop");
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
		m_pack.Apply(); // This already saves persistence

		// Close all open popups
		PopupManager.Clear(true);

		// Move to the rewards screen
		PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
		scr.StartFlow(false);   // No intro
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
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Info button has been pressed.
	/// Don't call it if the pack type doesn't require info popup.
	/// Can be override by heirs, default behaviour is opening the offer info popup.
	/// </summary>
	public virtual void OnInfoButton() {
		OpenInfoPopup(true);

		// [AOC] TODO!! More tracking
	}

	/// <summary>
	/// The whole pill has been hit.
	/// </summary>
	public virtual void OnPillHit() {
		OpenInfoPopup(true);

		// [AOC] TODO!! More tracking
	}
}