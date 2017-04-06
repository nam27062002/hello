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

	// Internal
	private UserProfile.Currency m_currency = UserProfile.Currency.REAL;
	private float m_price = 0f;

	private FGOL.Server.Error m_checkConnectionError;
	private PopupController m_loadingPopupController;

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
		m_bestValueObj.SetActive(m_def.GetAsBool("bestValue", false));

		// Price
		// Figure out currency first
		m_price = m_def.GetAsFloat("priceDollars");		// [AOC] TODO!! Price should be provided by the store api
		if(m_price > 0) {	// Real money prevails over HC
			// [AOC] TODO!! Price should be localized by the store api
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
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The buy button has been pressed.
	/// </summary>
	public void OnBuyButton() {
		// Ignore if not properly initialized
		if(m_def == null) return;

		// If currency is PC, make sure we have enough and adjust new balance
		if(m_currency == UserProfile.Currency.HARD) {
			long pricePC = (long)m_price;
			if(UsersManager.currentUser.pc >= pricePC) {
				UsersManager.currentUser.AddPC(-pricePC);

				ApplyShopPack( m_def );
				// [AOC] TODO!! Notify game - typically this is done by the store manager, do it properly
				Messenger.Broadcast<string>(EngineEvents.PURCHASE_SUCCESSFUL, m_def.sku);
			} else {
				// Show feedback and interrupt transaction
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
				return;
			}
		}
		else if ( m_currency == UserProfile.Currency.REAL )
		{
			m_loadingPopupController = PopupManager.PopupLoading_Open();
			m_loadingPopupController.OnClosePostAnimation.AddListener( OnConnectionCheck );
			Authenticator.Instance.CheckConnection(delegate (FGOL.Server.Error connectionError)
			{
				m_checkConnectionError = connectionError;
#if UNITY_EDITOR
					m_checkConnectionError = null;
#endif
				m_loadingPopupController.Close(true);
			});
		}
	}

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


	private static UserProfile.Currency GetCurrencyType( DefinitionNode def )
	{
		UserProfile.Currency type = UserProfile.Currency.NONE;
		switch(def.Get("type")) {
			case "sc": type = UserProfile.Currency.SOFT; break;
			case "hc": type = UserProfile.Currency.HARD; break;
		}
		return type;
	}

	void OnConnectionCheck()
	{
		if ( m_checkConnectionError == null )
		{
			if ( GameStoreManager.SharedInstance.CanMakePayment() )
			{
				GameStoreManager.SharedInstance.Buy( m_def.sku );
			}	
			else
			{
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_CANNOT_PAY"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			}
		}
		else
		{
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}
}