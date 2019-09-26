// PopupCurrencyShopPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// PREPROCESSOR																  //
//----------------------------------------------------------------------------//
#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Diagnostics;
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
	// ABSTRACT AND VIRTUAL METHODS											  //
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

	/// <summary>
	/// A purchase has been started.
	/// </summary>
	protected virtual void OnPurchaseStarted() {
		// To be implemented by heirs if needed
	}

	/// <summary>
	/// A purchase has finished.
	/// </summary>
	/// <param name="_success">Has it been successful?</param>
	protected virtual void OnPurchaseFinished(bool _success) {
		// To be implemented by heirs if needed
	}

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
		string localizedPrice = string.Empty;
		if(GameStoreManager.SharedInstance.IsReady()) {
			localizedPrice = GameStoreManager.SharedInstance.GetLocalisedPrice(GetIAPSku());
		}

#if DEBUG
		// If store was not initialized or iap can't be localized, use reference price as placeholder
		if(string.IsNullOrEmpty(localizedPrice)) {
			localizedPrice = "$" + StringUtils.FormatNumber(_referencePriceDollars, 2);
		}
#endif

		return localizedPrice;
	}

	/// <summary>
	/// Internal logic to start purchase.
	/// </summary>
	private void StartPurchase() {
		// Track
		m_transactionInProgress = true;

		// Track
		HDTrackingManager.Instance.Notify_StoreItemView(m_def.sku);

		// Notify heirs
		OnPurchaseStarted();
	}

	/// <summary>
	/// Internal logic to finalize a purchase.
	/// </summary>
	/// <param name="_success">Has it been successful?</param>
	private void EndPurchase(bool _success) {
		Log("EndPurchase. Success? " + _success);

		// Stop tracking
		m_transactionInProgress = false;

		// Successful?
		if(_success) {
			// Apply rewards
			ApplyShopPack();

			// Show feedback!
			ShowPurchaseSuccessFeedback();

			// Notify external listeners
			OnPurchaseSuccess.Invoke(this);
		} else {
			// Notify external listeners
			OnPurchaseError.Invoke(this);
		}

		// If real money, properly finalize IAP
		if(m_currency == UserProfile.Currency.REAL) {
			FinalizeIAP(_success);
		}

		// Notifiy heirs
		OnPurchaseFinished(_success);
	}

	/// <summary>
	/// Tell the pill whether to listen to GameStoreManager events or not.
	/// </summary>
	/// <param name="_track">Track purchases?</param>
	private void TrackIAPEvents(bool _track) {
		// Skip if same state
		if(_track == m_awaitingPurchaseConfirmation) {
			Log("Already watiting (or not)(" + m_awaitingPurchaseConfirmation + ") for purchase confirmation, skip subscribing to IAP events");
			return;
		}

		// Store new state
		m_awaitingPurchaseConfirmation = _track;

		// Update listeners
		if(_track) {
			Log("Subscribing to IAP events");
			Messenger.AddListener<string, string, SimpleJSON.JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, OnIAPSuccess);
			Messenger.AddListener<string>(MessengerEvents.PURCHASE_ERROR, OnIAPFailed);
			Messenger.AddListener<string>(MessengerEvents.PURCHASE_FAILED, OnIAPFailed);
			Messenger.AddListener<string>(MessengerEvents.PURCHASE_CANCELLED, OnIAPFailed);
		} else {
			Log("Unsubscribing from IAP events");
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
		Log("Finalize IAP: Success? " + _success);

		// Close loading popup
		if(m_loadingPopupController != null) {
			Log("Closing loading popup!!");
			m_loadingPopupController.Close(true);
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

		// Start internal logic
		StartPurchase();

		// Depends on currency
		switch(m_currency) {
			case UserProfile.Currency.HARD: {
					// Make sure we have enough and adjust new balance
					// Resources flow makes it easy for us!
					ResourcesFlow purchaseFlow = new ResourcesFlow(this.GetType().Name);
					purchaseFlow.confirmationPopupBehaviour = ResourcesFlow.ConfirmationPopupBehaviour.FORCE;  // [AOC] For currency packs always request confirmation (UMR compliance)
					purchaseFlow.OnFinished.AddListener(OnResourcesFlowFinished);
					purchaseFlow.Begin((long)m_price, UserProfile.Currency.HARD, GetTrackingId(), def);
				}
				break;

			case UserProfile.Currency.REAL: {
					// Do a first quick check on Internet connectivity
					Log("Quick connectivity check");
					if(DeviceUtilsManager.SharedInstance.internetReachability == NetworkReachability.NotReachable) {
						// We have no internet connectivity, finalize the IAP
						Log("No internet connectivity, finalize the IAP");
						EndPurchase(false);
						UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
					} else {
						// Start real money transaction flow!
						Log("Internet connectivity OK, start real money transaction flow!");

						// Open loading popup to block all the UI while the transaction is in progress
						Log("Opening Loading Popup!");
						m_loadingPopupController = PopupManager.PopupLoading_Open();

						// Check connection to the store
#if UNITY_EDITOR
					// [AOC] Editor override
					// Simulate some delay
					UbiBCN.CoroutineManager.DelayedCall(() => {
						OnConnectionCheckFinished(null);
					}, 3f);
#else
						GameServerManager.SharedInstance.CheckConnection(OnConnectionCheckFinished);
#endif
					}
				}
				break;

			default: {
					EndPurchase(false);
				}
				break;
		}
	}

	/// <summary>
	/// Connection to the store has been checked.
	/// </summary>
	void OnConnectionCheckFinished(FGOL.Server.Error _connectionError) {
		Log("OnConnectionCheckFinished: Error " + (_connectionError == null ? "NULL" : _connectionError.ToString()));

		if(_connectionError == null) {
			// No error! Proceed with the IAP flow
			Log("OnConnectionCheckFinished: No Error! Proceed with IAP flow");
			if(GameStoreManager.SharedInstance.IsInitializing()) {
				Log("Store not yet initialized. Waiting...");
				GameStoreManager.SharedInstance.WaitForInitialization(RequestIAP);
			} else {
				RequestIAP();
			}
		} else {
			// There was a connection error with the store, finalize the IAP
			Log("There was a connection error with the store, finalize the IAP");
			EndPurchase(false);
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void RequestIAP() {
		if(GameStoreManager.SharedInstance.CanMakePayment()) {
			// Player can perform the payment, continue with the IAP flow
			Log("Player can perform the payment, continue with the IAP flow");
			TrackIAPEvents(true);  // Start listening to GameStoreManager events
			GameStoreManager.SharedInstance.Buy(GetIAPSku());
		} else {
			// Player can't make payment, finalize the IAP
			Log("Player can't make payment, finalize the IAP");
			EndPurchase(false);

			string msg = null;
			float duration = -1f;

			{
#if UNITY_ANDROID
                msg = LocalizationManager.SharedInstance.Localize("TID_CHECK_PAYMENT_METHOD", LocalizationManager.SharedInstance.Localize("TID_PAYMENT_METHOD_GOOGLE"));                
                // Longer time is given to this feedback because the text is long
                duration = 4f;
#else
				msg = LocalizationManager.SharedInstance.Localize("TID_CANNOT_PAY");
#endif
			}

			UIFeedbackText feedbackText = UIFeedbackText.CreateAndLaunch(msg, new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			if(duration > 0) {
				feedbackText.duration = duration;
			}
		}
	}

	/// <summary>
	/// Real money transaction has succeeded.
	/// </summary>
	/// <param name="_sku">Sku of the purchased item.</param>
	private void OnIAPSuccess(string _sku, string _storeTransactionID, SimpleJSON.JSONNode _receipt) {
		Log("OnIAPSuccess! " + _sku);

		// Is it this one?
		if(_sku == GetIAPSku()) {
			Log("Applying shop pack and finalizing IAP flow");
			// Stop listening to GameStoreManager events
			TrackIAPEvents(false);

			// Finalize IAP flow
			EndPurchase(true);
		}
	}

	/// <summary>
	/// Real money transaction has failed.
	/// </summary>
	/// <param name="_sku">Sku of the item to be purchased.</param>
	private void OnIAPFailed(string _sku) {
		Log("OnIAPFailed! " + _sku);

		// Is it this one?
		if(_sku == GetIAPSku()) {
			// Stop tracking
			TrackIAPEvents(false);

			// Finalize IAP flow
			EndPurchase(false);
		}
	}

	/// <summary>
	/// Transaction has finished (for good or bad).
	/// </summary>
	/// <param name="_flow">Flow.</param>
	private void OnResourcesFlowFinished(ResourcesFlow _flow) {
		Log("OnResourcesFlowFinished! " + _flow.name);

		// Is it this one?
		if(_flow.itemDef.sku == def.sku) {
			// Use internal method to finalize the transaction
			Log("Finishing resources flow transaction " + _flow.name);
			EndPurchase(_flow.successful);
		}
	}

    //------------------------------------------------------------------------//
    // DEBUG METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Print something on the console / control panel log.
    /// </summary>
    /// <param name="_message">Message to be printed.</param>
    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    private void Log(string _message) {
		ControlPanel.Log("[ShopPill]" + _message, ControlPanel.ELogChannel.Store);
	}
}