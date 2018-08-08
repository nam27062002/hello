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
public abstract class IPopupShopPill : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Parametrized event
	public class ShopPillEvent : UnityEvent<IPopupShopPill> { }

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Public
	protected DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	// Events
	public ShopPillEvent OnPurchaseSuccess = new ShopPillEvent();
	public ShopPillEvent OnPurchaseError = new ShopPillEvent();

	// Internal
	protected UserProfile.Currency m_currency = UserProfile.Currency.REAL;
	protected float m_price = 0f;

	private FGOL.Server.Error m_checkConnectionError;
	private PopupController m_loadingPopupController;
	private bool m_awaitingPurchaseConfirmation = false;

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Obtain the IAP sku as defined in the App Stores.
	/// </summary>
	/// <returns>The IAP sku corresponding to this shop pack. Empty if not an IAP.</returns>
	public abstract string GetIAPSku();
	
	/// <summary>
	/// Get the tracking id for transactions performed by this shop pill
	/// </summary>
	/// <returns>The tracking identifier.</returns>
	protected abstract HDTrackingManager.EEconomyGroup GetTrackingId();
	
	/// <summary>
	/// Apply the shop pack to the current user!
	/// Invoked after a successful purchase.
	/// </summary>
	protected abstract void ApplyShopPack();

	/// <summary>
	/// Shows the purchase success feedback.
	/// </summary>
	protected abstract void ShowPurchaseSuccessFeedback();

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the localized IAP price, as returned from the App Store.
	/// If the App Store is not reachable or the product is not found, return the given reference price instead.
	/// </summary>
	/// <returns>The localized IAP price.</returns>
	/// <param name="_referencePriceDollars">The price to be used if the App Store can't be reached or can't find the requested product.</param>
	protected string GetLocalizedIAPPrice(float _referencePriceDollars) {
		// Price is localized by the store api, if available
		if(GameStoreManager.SharedInstance.IsReady()) {
			return GameStoreManager.SharedInstance.GetLocalisedPrice(GetIAPSku());
		} else {
			return "$" + StringUtils.FormatNumber(_referencePriceDollars, 2);
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
			Messenger.AddListener<string, string, SimpleJSON.JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, OnIAPSuccess);
			Messenger.AddListener<string>(MessengerEvents.PURCHASE_ERROR, OnIAPFailed);
			Messenger.AddListener<string>(MessengerEvents.PURCHASE_FAILED, OnIAPFailed);
		} else {
			Messenger.RemoveListener<string, string, SimpleJSON.JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, OnIAPSuccess);
			Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_ERROR, OnIAPFailed);
			Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_FAILED, OnIAPFailed);
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
		if(def == null) return;

		// Depends on currency
		switch(m_currency) {
			case UserProfile.Currency.HARD: {
				// Make sure we have enough and adjust new balance
				// Resources flow makes it easy for us!
				ResourcesFlow purchaseFlow = new ResourcesFlow(this.GetType().Name);
				purchaseFlow.OnSuccess.AddListener(OnResourcesFlowSuccess);
				purchaseFlow.Begin((long)m_price, UserProfile.Currency.HARD, GetTrackingId(), def);
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
	void OnConnectionCheck() {
		if(m_checkConnectionError == null) {
			if(GameStoreManager.SharedInstance.CanMakePayment()) {
				TrackPurchaseResult(true);
				GameStoreManager.SharedInstance.Buy(GetIAPSku());
			} else {
				OnPurchaseError.Invoke(this);
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_CANNOT_PAY"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			}
		} else {
			OnPurchaseError.Invoke(this);
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

	/// <summary>
	/// Real money transaction has succeeded.
	/// </summary>
	/// <param name="_sku">Sku of the purchased item.</param>
	private void OnIAPSuccess(string _sku, string _storeTransactionID, SimpleJSON.JSONNode _receipt) {
		// Is it this one?
		if(_sku == GetIAPSku()) {
			// Apply rewards
			ApplyShopPack();

			// Stop tracking
			TrackPurchaseResult(false);

			// Notify external listeners
			OnPurchaseSuccess.Invoke(this);

			// Show feedback!
			ShowPurchaseSuccessFeedback();
		}
	}

	/// <summary>
	/// Real money transaction has failed.
	/// </summary>
	/// <param name="_sku">Sku of the item to be purchased.</param>
	private void OnIAPFailed(string _sku) {
		// Is it this one?
		if(_sku == GetIAPSku()) {
			// Stop tracking
			TrackPurchaseResult(false);

			// Notify external listeners
			OnPurchaseError.Invoke(this);
		}
	}

	/// <summary>
	/// PC Transaction has succeeded.
	/// </summary>
	/// <param name="_flow">Flow.</param>
	private void OnResourcesFlowSuccess(ResourcesFlow _flow) {
		// Is it this one?
		if(_flow.itemDef.sku == def.sku) {
			// Apply rewards
			ApplyShopPack();

			// Notify external listeners
			OnPurchaseSuccess.Invoke(this);

			// Show feedback!
			ShowPurchaseSuccessFeedback();
		}
	}
}