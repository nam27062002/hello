// PopupShopCurrencyPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

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
public class PopupShopCurrencyPill : IPopupShopPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private RectTransform m_iconContainer = null;

	[Space]
	[SerializeField] private TextMeshProUGUI m_amountText = null;
	[SerializeField] private Localizer m_bonusAmountText = null;

	[Space]
	[SerializeField] private MultiCurrencyButton m_priceButtons = null;
	[SerializeField] private LoadingDots m_loadingPricePlaceholder = null;

	[Space]
	[SerializeField] private GameObject m_bestValueObj = null;

	// Public
	private UserProfile.Currency m_type = UserProfile.Currency.NONE;
	public UserProfile.Currency type {
		get { return m_type; }
	}

	// Internal
	private ResourceRequest m_iconLoadTask = null;
	private static int s_loadingTaskPriority = -1;

	private bool m_waitingForPrice = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Check if image has finished loading
		if(m_iconLoadTask != null) {
			if(m_iconLoadTask.isDone) {
				// Instantiate icon
				GameObject.Instantiate(m_iconLoadTask.asset, m_iconContainer, false);

				// Clear loading task
				m_iconLoadTask = null;
				s_loadingTaskPriority--;
			}
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

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this pill with the given def data.
	/// </summary>
	/// <param name="_def">Definition of the currency package.</param>
	public void InitFromDef(DefinitionNode _def) {
		// Store new definition
		m_def = _def;

		// If null, hide this pill and return
		this.gameObject.SetActive(_def != null);
		if(_def == null) return;

		// Init internal vars
		m_type = UserProfile.SkuToCurrency( m_def.Get("type") );

		// Init visuals
		// Icon
		// Destroy any existing icon
		m_iconContainer.DestroyAllChildren(false);
		int loadingTaskPriority = s_loadingTaskPriority;
		if(m_iconLoadTask != null) {
			loadingTaskPriority = m_iconLoadTask.priority;
		} else {
			s_loadingTaskPriority++;
		}
		m_iconLoadTask = Resources.LoadAsync<GameObject>(UIConstants.SHOP_ICONS_PATH + _def.Get("icon"));
		m_iconLoadTask.priority = loadingTaskPriority;

		// Amount
		m_amountText.text = UIConstants.GetIconString(m_def.GetAsInt("amount"), m_type, UIConstants.IconAlignment.LEFT);

		// Bonus amount
		float bonusAmount = m_def.GetAsFloat("bonusAmount");
		m_bonusAmountText.gameObject.SetActive(bonusAmount > 0f);
		m_bonusAmountText.Localize("TID_SHOP_BONUS_AMOUNT", StringUtils.MultiplierToPercentage(bonusAmount));	// 15% extra

		// Best value
		if(m_bestValueObj != null) {
			m_bestValueObj.SetActive(m_def.GetAsBool("bestValue", false));
		}

		// Price and currency
		m_price = m_def.GetAsFloat("price");
		m_currency = UserProfile.SkuToCurrency(m_def.Get("priceType"));
		RefreshPrice();
	}

	/// <summary>
	/// Initialize price tags.
	/// </summary>
	private void RefreshPrice() {
		// Special case for real money
		if(m_currency == UserProfile.Currency.REAL) {
			// If localized prices haven't been received from the store yet, wait for it
			bool storeReady = GameStoreManager.SharedInstance.IsReady();

			// Buttons
			if(m_priceButtons != null) {
				m_priceButtons.gameObject.SetActive(storeReady);
				if(storeReady) {
					m_priceButtons.SetAmount(GetLocalizedIAPPrice(m_price), m_currency);
				}
			}

			// Loading placeholder
			if(m_loadingPricePlaceholder != null) m_loadingPricePlaceholder.gameObject.SetActive(!storeReady);

			// Internal flag
			m_waitingForPrice = !storeReady;
		} else {
			m_priceButtons.SetAmount(m_price, m_currency);
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
		// Add amount
		// [AOC] Could be joined in a single instruction for all types, but keep it split in case we need some extra processing (i.e. tracking!)
		switch(m_type) {
			case UserProfile.Currency.SOFT: {
				UsersManager.currentUser.EarnCurrency(UserProfile.Currency.SOFT, (ulong)def.GetAsLong("amount"), true, HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE);
			} break;

			case UserProfile.Currency.HARD: {
				UsersManager.currentUser.EarnCurrency(UserProfile.Currency.HARD, (ulong)def.GetAsLong("amount"), true, HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE);
			} break;

			case UserProfile.Currency.KEYS: {
				UsersManager.currentUser.EarnCurrency(UserProfile.Currency.KEYS, (ulong)def.GetAsLong("amount"), true, HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE);
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
			def.GetAsInt("amount"), 
			Vector3.down * 150f, 
			this.GetComponentInParent<Canvas>().transform as RectTransform
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}