// PopupCurrencyShopPill.cs
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
/// Single pill in the currency shop.
/// </summary>
public class PopupShopOffersPill : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Parametrized event
	public class CurrencyShopPillEvent : UnityEvent<PopupShopOffersPill> { }

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

	[Space]
	[SerializeField] private GameObject m_bestValueObj = null;

	// Public
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private UserProfile.Currency m_type = UserProfile.Currency.NONE;
	public UserProfile.Currency type {
		get { return m_type; }
	}

	// Events
	public CurrencyShopPillEvent OnPurchaseSuccess = new CurrencyShopPillEvent();
	public CurrencyShopPillEvent OnPurchaseError = new CurrencyShopPillEvent();

	// Internal
	private UserProfile.Currency m_currency = UserProfile.Currency.REAL;
	private float m_price = 0f;
	private ResourceRequest m_iconLoadTask = null;

	private FGOL.Server.Error m_checkConnectionError;
	private PopupController m_loadingPopupController;
	private bool m_awaitingPurchaseConfirmation = false;

	private static int s_loadingTaskPriority = -1;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

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
		m_iconLoadTask.priority = s_loadingTaskPriority;

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

		// Price
		// Figure out currency first
		// Special case for real money
		m_currency = UserProfile.SkuToCurrency(m_def.Get("priceType"));
		m_price = m_def.GetAsFloat("price");
		if(m_currency == UserProfile.Currency.REAL) {
			// Price is localized by the store api, if available
			if(GameStoreManager.SharedInstance.IsReady()) {
				string localizedPrice =  GameStoreManager.SharedInstance.GetLocalisedPrice(m_def.sku);
				m_priceButtons.SetAmount(localizedPrice, m_currency);
			} else {
				m_priceButtons.SetAmount("$" + StringUtils.FormatNumber(m_price, 2), m_currency);
			}
		} else {
			m_priceButtons.SetAmount(m_price, m_currency);
		}
	}

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	public static void ApplyShopPack( DefinitionNode def )
	{	
		UserProfile.Currency type = UserProfile.SkuToCurrency(def.Get("type"));
		// Add amount
		// [AOC] Could be joined in a single instruction for all types, but keep it split in case we need some extra processing (i.e. tracking!)
		switch(type) {
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

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The buy button has been pressed.
	/// </summary>
	public void OnBuyButton() {
		// Ignore if not properly initialized
		if(m_def == null) return;

		// Depends on currency
		switch(m_currency) {
			case UserProfile.Currency.HARD: {
				// What are we buying?
				string resourcesFlowName = "";
				HDTrackingManager.EEconomyGroup trackingId = HDTrackingManager.EEconomyGroup.SHOP_COINS_PACK;
				switch(m_type) {
					case UserProfile.Currency.SOFT: {
						resourcesFlowName = "SHOP_COINS_PACK";
						trackingId = HDTrackingManager.EEconomyGroup.SHOP_COINS_PACK;
					} break;

					case UserProfile.Currency.KEYS: {
						resourcesFlowName = "SHOP_KEYS_PACK";
						trackingId = HDTrackingManager.EEconomyGroup.SHOP_KEYS_PACK;
					} break;
				}

				// Make sure we have enough and adjust new balance
				// Resources flow makes it easy for us!
				ResourcesFlow purchaseFlow = new ResourcesFlow(resourcesFlowName);
				purchaseFlow.OnSuccess.AddListener(
					(ResourcesFlow _flow) => {
						ApplyShopPack(_flow.itemDef);

						// Trigger message
						OnPurchaseSuccessEnd(_flow.itemDef);
					}
				);
				purchaseFlow.Begin((long)m_price, UserProfile.Currency.HARD, trackingId,  m_def);
			} break;

			case UserProfile.Currency.REAL: {
                    // HACK to fix HDK-524 quickly:
                    // There's an issue with PopupController that prevents OnClosePostAnimation listener from being called when a popup is closed immediately after being opened
                    // So far we just avoid that situation
                    if (Application.internetReachability == NetworkReachability.NotReachable) {
                        OnPurchaseError.Invoke(this);
                        UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
                    } else {
                        // Start real money transaction flow
                        m_loadingPopupController = PopupManager.PopupLoading_Open();
                        m_loadingPopupController.OnClosePostAnimation.AddListener(OnConnectionCheck);
                        GameServerManager.SharedInstance.CheckConnection(delegate (FGOL.Server.Error connectionError)
                        {
                            m_checkConnectionError = connectionError;
                    #if UNITY_EDITOR
                            m_checkConnectionError = null;
                    #endif
                            m_loadingPopupController.Close(true);
                        });
                    }
			} break;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	void OnConnectionCheck()
	{
		if ( m_checkConnectionError == null )
		{
			if ( GameStoreManager.SharedInstance.CanMakePayment() )
			{
				TrackPurchaseResult(true);
				GameStoreManager.SharedInstance.Buy( m_def.sku );
			}	
			else
			{
				OnPurchaseError.Invoke(this);
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_CANNOT_PAY"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			}
		}
		else
		{
			OnPurchaseError.Invoke(this);
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

	/// <summary>
	/// Tell the pill whether to track purchases or not.
	/// </summary>
	/// <param name="_track">Track purchases?.</param>
	private void TrackPurchaseResult(bool _track) {
		// Skip if same state
		if(_track == m_awaitingPurchaseConfirmation) return;

		// Store new state
		m_awaitingPurchaseConfirmation = _track;

		// Update listeners
		if(_track) {
			Messenger.AddListener<string, string, SimpleJSON.JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, OnPurchaseSuccessful);
			Messenger.AddListener<string>(MessengerEvents.PURCHASE_ERROR, OnPurchaseFailed);
			Messenger.AddListener<string>(MessengerEvents.PURCHASE_FAILED, OnPurchaseFailed);
		} else {
			Messenger.RemoveListener<string, string, SimpleJSON.JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, OnPurchaseSuccessful);
			Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_ERROR, OnPurchaseFailed);
			Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_FAILED, OnPurchaseFailed);
		}
	}

	/// <summary>
	/// Real money transaction has succeeded.
	/// </summary>
	/// <param name="_sku">Sku of the purchased item.</param>
	private void OnPurchaseSuccessful(string _sku, string _storeTransactionID, SimpleJSON.JSONNode _receipt) {
		// Is it this one?
		if(_sku == m_def.sku) {
			// Stop tracking
			TrackPurchaseResult(false);

			OnPurchaseSuccessEnd(m_def);
		}
	}

	private void OnPurchaseSuccessEnd(DefinitionNode _def) {
		// Notify player
		UINotificationShop.CreateAndLaunch(
			UserProfile.SkuToCurrency(_def.Get("type")), 
			_def.GetAsInt("amount"), 
			Vector3.down * 150f, 
			this.GetComponentInParent<Canvas>().transform as RectTransform
		);

		// Notify listeners
		OnPurchaseSuccess.Invoke(this);
	}

	/// <summary>
	/// Real money transaction has failed.
	/// </summary>
	/// <param name="_sku">Sku of the item to be purchased.</param>
	private void OnPurchaseFailed(string _sku) {
		// Is it this one?
		if(_sku == m_def.sku) {
			// Stop tracking
			TrackPurchaseResult(false);

			// Notify listeners
			OnPurchaseError.Invoke(this);
		}
	}
}