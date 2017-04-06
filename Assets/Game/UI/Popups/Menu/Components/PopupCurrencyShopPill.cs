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
public class PopupCurrencyShopPill : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Parametrized event
	public class CurrencyShopPillEvent : UnityEvent<PopupCurrencyShopPill> { }

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Image m_icon = null;

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

	private FGOL.Server.Error m_checkConnectionError;
	private PopupController m_loadingPopupController;
	private bool m_awaitingPurchaseConfirmation = false;

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
		m_type = GetCurrencyType( m_def );

		// Init visuals
		// Icon
		m_icon.sprite = Resources.Load<Sprite>(UIConstants.SHOP_ICONS_PATH + _def.Get("icon"));

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
		m_price = m_def.GetAsFloat("priceDollars");		// [AOC] TODO!! Price should be provided by the store api
		if(m_price > 0) {	// Real money prevails over HC
			// Price should be localized by the store api
			m_currency = UserProfile.Currency.REAL;
			if (GameStoreManager.SharedInstance.IsReady())
			{
				string localizedPrice =  GameStoreManager.SharedInstance.GetLocalisedPrice( m_def.sku);
				m_priceButtons.SetAmount( localizedPrice, UserProfile.Currency.REAL);
			}
			else
			{
				m_priceButtons.SetAmount("$" + StringUtils.FormatNumber(m_price, 2), UserProfile.Currency.REAL);
			}

		} else {
			m_currency = UserProfile.Currency.HARD;
			m_price = m_def.GetAsFloat("priceHC");
			m_priceButtons.SetAmount(m_price, m_currency);
		}
	}

	//------------------------------------------------------------------------//
	// STATIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="def"></param>
	public static void ApplyShopPack( DefinitionNode def )
	{	
		UserProfile.Currency type = GetCurrencyType(def);
		// Add amount
		switch(type) {
			case UserProfile.Currency.SOFT: {
				UsersManager.currentUser.AddCoins(def.GetAsLong("amount"));
			} break;

			case UserProfile.Currency.HARD: {
				UsersManager.currentUser.AddPC(def.GetAsLong("amount"));
			} break;
		}

		// Save persistence
		PersistenceManager.Save(true);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	/// <param name="def"></param>
	private static UserProfile.Currency GetCurrencyType( DefinitionNode def )
	{
		UserProfile.Currency type = UserProfile.Currency.NONE;
		switch(def.Get("type")) {
			case "sc": type = UserProfile.Currency.SOFT; break;
			case "hc": type = UserProfile.Currency.HARD; break;
		}
		return type;
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
				// Make sure we have enough and adjust new balance
				// Resources flow makes it easy for us!
				ResourcesFlow purchaseFlow = new ResourcesFlow("SHOP_COINS_PACK");
				purchaseFlow.OnSuccess.AddListener(
					(ResourcesFlow _flow) => {
						ApplyShopPack(_flow.itemDef);

						// Trigger message
						OnPurchaseSuccess.Invoke(this);
					}
				);
				purchaseFlow.Begin((long)m_price, UserProfile.Currency.HARD, m_def);

				// Without resources flow:
				/*long pricePC = (long)m_price;
				if(UsersManager.currentUser.pc >= pricePC) {
					UsersManager.currentUser.AddPC(-pricePC);
					ApplyShopPack( m_def );

					// Trigger message
					OnPurchaseSuccess.Invoke(this);
				} else {
					// Show feedback
					UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
				}*/
			} break;

			case UserProfile.Currency.REAL: {
				// Start real money transaction flow
				m_loadingPopupController = PopupManager.PopupLoading_Open();
				m_loadingPopupController.OnClosePostAnimation.AddListener( OnConnectionCheck );
				Authenticator.Instance.CheckConnection(delegate (FGOL.Server.Error connectionError)
					{
						m_checkConnectionError = connectionError;
						m_loadingPopupController.Close(true);
					});
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
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
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
			Messenger.AddListener<string>(EngineEvents.PURCHASE_SUCCESSFUL, OnPurchaseSuccessful);
			Messenger.AddListener<string>(EngineEvents.PURCHASE_ERROR, OnPurchaseFailed);
			Messenger.AddListener<string>(EngineEvents.PURCHASE_FAILED, OnPurchaseFailed);
		} else {
			Messenger.RemoveListener<string>(EngineEvents.PURCHASE_SUCCESSFUL, OnPurchaseSuccessful);
			Messenger.RemoveListener<string>(EngineEvents.PURCHASE_ERROR, OnPurchaseFailed);
			Messenger.RemoveListener<string>(EngineEvents.PURCHASE_FAILED, OnPurchaseFailed);
		}
	}

	/// <summary>
	/// Real money transaction has succeeded.
	/// </summary>
	/// <param name="_sku">Sku of the purchased item.</param>
	private void OnPurchaseSuccessful(string _sku) {
		// Is it this one?
		if(_sku == m_def.sku) {
			// Stop tracking
			TrackPurchaseResult(false);

			// Notify listeners
			OnPurchaseSuccess.Invoke(this);
		}
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