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

	private PopupController m_loadingPopupController;
	private bool m_awaitingPurchaseConfirmation = false;

	private bool m_transactionInProgress = false;

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
	/// Tell the pill whether to listen to GameStoreManager events or not.
	/// </summary>
	/// <param name="_track">Track purchases?</param>
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
			Messenger.AddListener<string>(MessengerEvents.PURCHASE_CANCELLED, OnIAPFailed);
		} else {
			Messenger.RemoveListener<string, string, SimpleJSON.JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, OnIAPSuccess);
			Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_ERROR, OnIAPFailed);
			Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_FAILED, OnIAPFailed);
			Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_CANCELLED, OnIAPFailed);
		}
	}

	/// <summary>
	/// Perform all required logic when the IAP flow has finished.
	/// </summary>
	/// <param name="_success">Has the IAP been successful?</param>
	private void FinalizeIAP(bool _success) {
		// Close loading popup
		if(m_loadingPopupController != null) {
			m_loadingPopupController.Close(true);
		}

		// Reset internal flag
		m_transactionInProgress = false;

		// Notify external listeners
		if(_success) {
			OnPurchaseSuccess.Invoke(this);
		} else {
			OnPurchaseError.Invoke(this);
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

		// Ignore if a transaction is already in progress (prevent spamming)
		// Resolves issue HDK-2589 and others
		if(m_transactionInProgress) return;

		// Depends on currency
		switch(m_currency) {
			case UserProfile.Currency.HARD: {
				m_transactionInProgress = true;

				// Make sure we have enough and adjust new balance
				// Resources flow makes it easy for us!
				ResourcesFlow purchaseFlow = new ResourcesFlow(this.GetType().Name);
				purchaseFlow.forceConfirmation = true;	// [AOC] For currency packs always request confirmation (UMR compliance)
				purchaseFlow.OnSuccess.AddListener(OnResourcesFlowSuccess);
				purchaseFlow.OnFinished.AddListener(OnResourcesFlowFinished);
				purchaseFlow.Begin((long)m_price, UserProfile.Currency.HARD, GetTrackingId(), def);
			} break;

			case UserProfile.Currency.REAL: {
				// Do a first quick check on Internet connectivity
                if(Application.internetReachability == NetworkReachability.NotReachable) {
					// We have no internet connectivity, finalize the IAP
                    FinalizeIAP(false);
                    UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
                } else {
					// Start real money transaction flow!
					// Init internal flag to prevent spamming
					m_transactionInProgress = true;

					// Open loading popup to block all the UI while the transaction is in progress
                    m_loadingPopupController = PopupManager.PopupLoading_Open();

					// Check connection to the store
					GameServerManager.SharedInstance.CheckConnection(OnConnectionCheckFinished);
                }
			} break;
		}
	}
    
	/// <summary>
	/// Connection to the store has been checked.
	/// </summary>
	void OnConnectionCheckFinished(FGOL.Server.Error _connectionError) {
		// [AOC] Editor override
	#if UNITY_EDITOR
		_connectionError = null;
	#endif

		if(_connectionError == null) {
			// No error! Proceed with the IAP flow
			if(GameStoreManager.SharedInstance.CanMakePayment()) {
				// Player can perform the payment, continue with the IAP flow
				TrackPurchaseResult(true);	// Start listening to GameStoreManager events
				GameStoreManager.SharedInstance.Buy(GetIAPSku());
			} else {
				// Player can't make payment, finalize the IAP
				FinalizeIAP(false);

#if UNITY_ANDROID
                string msg = LocalizationManager.SharedInstance.Localize("TID_CHECK_PAYMENT_METHOD", LocalizationManager.SharedInstance.Localize("TID_PAYMENT_METHOD_GOOGLE"));
                UIFeedbackText feedbackText = UIFeedbackText.CreateAndLaunch(msg, new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);           
                // Longer time is given to this feedback because the text is long
                feedbackText.duration = 4f;
#else
                UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_CANNOT_PAY"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);           
#endif

            }
        } else {
			// There was a connection error with the store, finalize the IAP
			FinalizeIAP(false);
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

			// Stop listening to GameStoreManager events
			TrackPurchaseResult(false);

			// Finalize IAP flow
			FinalizeIAP(true);

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

			// Finalize IAP flow
			FinalizeIAP(false);
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

	/// <summary>
	/// Transaction has finished (for good or bad).
	/// </summary>
	/// <param name="_flow">Flow.</param>
	private void OnResourcesFlowFinished(ResourcesFlow _flow) {
		// Is it this one?
		if(_flow.itemDef.sku == def.sku) {
			// Update control vars
			m_transactionInProgress = false;
		}
	}
}