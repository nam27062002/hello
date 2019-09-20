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

    [Space]
    [Header("Happy Hour")]
    [SerializeField] private GameObject m_purpleBground;
    [SerializeField] private TextMeshProUGUI m_amountBeforeOffer;
    [SerializeField] private GameObject m_discountBadge;
    [SerializeField] private Localizer m_discountBadgeText;

    // Public
    private UserProfile.Currency m_type = UserProfile.Currency.NONE;
	public UserProfile.Currency type {
		get { return m_type; }
	}

	// Internal
	private ResourceRequest m_iconLoadTask = null;
	private static int s_loadingTaskPriority = 0;

	private bool m_waitingForPrice = false;
    private bool happyHourActive = false;
    private int amountApplied; // Keep a record of the currency amount bought

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
				if(s_loadingTaskPriority < 0) s_loadingTaskPriority = 0;	// Can't be negative!
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

        Refresh();

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

        // Hide all the happy hour elements
        if (m_purpleBground != null && m_discountBadge != null)
        {
            m_purpleBground.SetActive(false);
            m_discountBadge.SetActive(false);
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

    public void Refresh()
    {
        // In case is gem pack
        if (m_type == UserProfile.Currency.HARD)
        {

            // In case there is a happy hour active
            if (OffersManager.instance.happyHour != null && OffersManager.instance.happyHour.IsActive())
            {
                happyHourActive = true;

                if (m_amountBeforeOffer != null)
                {
                    // Show amount before the happy hour offer
                    m_amountBeforeOffer.gameObject.SetActive(true);
                    m_amountBeforeOffer.text = UIConstants.GetIconString(m_def.GetAsInt("amount"), m_type, UIConstants.IconAlignment.LEFT);
                }

                // Show a nice purple bground
                if (m_purpleBground != null)
                {
                    m_purpleBground.SetActive(true);
                }

                // Hide the regular extra % text
                m_bonusAmountText.gameObject.SetActive(false);

                // Instead of that, show it in a cool badge
                float bonusAmount = OffersManager.instance.happyHour.extraGemsFactor;
                m_discountBadge.SetActive(bonusAmount > 0f);
                m_discountBadgeText.Localize("TID_SHOP_BONUS_AMOUNT", StringUtils.MultiplierToPercentage(bonusAmount));	// 15% extra

                // Set total amount of gems
                float newAmount = m_def.GetAsFloat("amount") * (1f + bonusAmount);
                m_amountText.text = UIConstants.GetIconString((int)newAmount, m_type, UIConstants.IconAlignment.LEFT);

            }
            else
            {
                if (happyHourActive)
                {
                    // Only enter here once, when the happy hour finishes
                    happyHourActive = false;

                    // Restore the original offer values
                    InitFromDef(m_def);
                }
            }
        }
    }


    /// <summary>
    /// Check if there is a happy hour offer active and apply the extra amount
    /// </summary>
    private int ApplyHappyHourExtra (int _amount)
    {
        // Is there a happy hour offer?
        HappyHourOffer happyHour = OffersManager.instance.happyHour;
        if (happyHour.IsActive())
        {
            // Apply the extra gems factor
            return Mathf.RoundToInt((_amount) * (1 + happyHour.extraGemsFactor));
        }

        return _amount;
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
        switch (m_type) {
            case UserProfile.Currency.SOFT: {
                    amountApplied =  def.GetAsInt("amount");
                    UsersManager.currentUser.EarnCurrency(UserProfile.Currency.SOFT, (ulong)amountApplied, true, HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE);
            } break;

            case UserProfile.Currency.HARD: {

                    // Get the proper amount after applying the happy hour
                    amountApplied = ApplyHappyHourExtra(def.GetAsInt("amount"));

                    // Add the amount to the player currencies
                    UsersManager.currentUser.EarnCurrency(UserProfile.Currency.HARD, (ulong) amountApplied, true, HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE);

                    // Force popup if the player is in the shop screen right now
                    bool forcePopup = (PopupManager.GetOpenPopup(PopupShop.PATH) != null);
                    string offerSku = m_def.sku;

                    // Broadcast this event, so the happy hour can be activated / extended
                    Messenger.Broadcast<bool, string>(MessengerEvents.HC_PACK_ACQUIRED, forcePopup, offerSku);

                } break;

			case UserProfile.Currency.KEYS: {
                    amountApplied = def.GetAsInt("amount");
                    UsersManager.currentUser.EarnCurrency(UserProfile.Currency.KEYS, (ulong)amountApplied, true, HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE);
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
            amountApplied, // Use this stored value, as the happy hour rate could have changed in the last frame
			Vector3.down * 150f, 
			this.GetComponentInParent<Canvas>().transform as RectTransform
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}