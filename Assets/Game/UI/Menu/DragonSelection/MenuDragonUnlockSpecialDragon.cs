// MenuDragonUnlockGoldenFragments.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Unlock the selected special dragon.
/// </summary>
public class MenuDragonUnlockSpecialDragon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Localizer m_priceText = null;

	// Internal
	private MenuShowConditionally m_showConditions = null;

	// Transaction data
	private ResourcesFlow m_pcFlow = null;
	private ResourcesFlow m_gfFlow = null;
	private IDragonData m_transactionDragonData = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Required fields
		DebugUtils.Assert(m_priceText != null, "Required reference missing!");
		m_showConditions = transform.parent.GetComponent<MenuShowConditionally>();
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, Refresh);
		
		// Do a first refresh
		Refresh(InstanceManager.menuSceneController.selectedDragon);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, Refresh);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh with data from currently selected dragon.
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon</param>
	public void Refresh(string _sku) {
		// Get new dragon's data from the dragon manager
		IDragonData data = DragonManager.GetDragonData(_sku);
		if(data == null) return;

		// Update price
		m_priceText.Localize(
			m_priceText.tid, 
			StringUtils.FormatNumber(GetPrice(UserProfile.Currency.HARD)),
			StringUtils.FormatNumber(GetPrice(UserProfile.Currency.GOLDEN_FRAGMENTS))
		);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the price of unlocking the current selected dragon in the target currency.
	/// </summary>
	/// <returns>The price.</returns>
	/// <param name="_currency">Target currency.</param>
	private long GetPrice(UserProfile.Currency _currency) {
		// Choos currency
		string priceProperty = "";
		switch(_currency) {
			case UserProfile.Currency.GOLDEN_FRAGMENTS: priceProperty = "unlockPriceGF"; break;
			case UserProfile.Currency.HARD: priceProperty = "unlockPricePC"; break;
		}

		// Get price from the definition of the currently selected dragon.
		IDragonData dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		return dragonData.def.GetAsLong(priceProperty, 0);
	}

	/// <summary>
	/// Clear cached transaction data.
	/// </summary>
	private void ResetTransactionData() {
		m_transactionDragonData = null;
		m_pcFlow = null;
		m_gfFlow = null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlock() {
		// Make sure we can do it
		if(m_showConditions != null) {
			if(!m_showConditions.CheckDragon(InstanceManager.menuSceneController.selectedDragon)) {
				return;
			}
		}

		// Get price and start PC purchase flow
		m_transactionDragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		UserProfile.Currency currency = UserProfile.Currency.HARD;
		m_pcFlow = new ResourcesFlow("UNLOCK_SPECIAL_DRAGON_PC");
		m_pcFlow.OnSuccess.AddListener(OnPCTransactionSuccess);
		m_pcFlow.OnCancel.AddListener(OnTransactionCanceled);
		m_pcFlow.Begin(
			GetPrice(currency), 
			currency, 
			HDTrackingManager.EEconomyGroup.SPECIAL_DRAGON_UNLOCK, 
			m_transactionDragonData.def,
			false		// [AOC] DON'T PERFORM THE TRANSACTION! We will do it manually once both flows have been successful
		);
	}

	/// <summary>
	/// The ResourcesFlow for the PC transaction has succeeded.
	/// </summary>
	/// <param name="_flow">The flow that triggered the event.</param>
	private void OnPCTransactionSuccess(ResourcesFlow _flow) {
		// If this happens something went really wrong xD
		if(m_transactionDragonData == null) return;

		// Launch GF transaction
		// Get price and start GF purchase flow
		UserProfile.Currency currency = UserProfile.Currency.GOLDEN_FRAGMENTS;
		Debug.Log(Colors.lime.Tag("GF START"));
		m_gfFlow = new ResourcesFlow("UNLOCK_SPECIAL_DRAGON_GF");
		m_gfFlow.OnSuccess.AddListener(OnGFTransactionSuccess);
		m_gfFlow.OnCancel.AddListener(OnTransactionCanceled);
		m_gfFlow.Begin(
			GetPrice(currency),
			currency,
			HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON,
			m_transactionDragonData.def,
			false       // [AOC] DON'T PERFORM THE TRANSACTION! We will do it manually once both flows have been successful
		);
	}

	/// <summary>
	/// The ResourcesFlow for the PC transaction has succeeded.
	/// </summary>
	/// <param name="_flow">The flow that triggered the event.</param>
	private void OnGFTransactionSuccess(ResourcesFlow _flow) {
		// If this happens something went really wrong xD
		if(m_transactionDragonData == null) return;

		// Remove listeners (so they're not invoked again) and perform both transactions!
		m_pcFlow.OnSuccess.RemoveListener(OnPCTransactionSuccess);
		m_gfFlow.OnSuccess.RemoveListener(OnGFTransactionSuccess);
		m_pcFlow.DoTransaction();
		m_gfFlow.DoTransaction();

		// Unlock the dragon!
		m_transactionDragonData.Acquire();

		// TODO. Removed to fix HDK-5276
		// HDTrackingManager.Instance.Notify_DragonUnlocked(m_transactionDragonData.def.sku, m_transactionDragonData.GetOrder());

		// Show a nice animation!
		InstanceManager.menuSceneController.GetScreenData(MenuScreen.LAB_DRAGON_SELECTION).ui.GetComponent<LabDragonSelectionScreen>().LaunchAcquireAnim(m_transactionDragonData.def.sku);

		// Reset transaction cached data
		ResetTransactionData();
	}

	/// <summary>
	/// Any of the transactions has been canceled / failed.
	/// </summary>
	/// <param name="_flow">The flow that triggered the event.</param>
	private void OnTransactionCanceled(ResourcesFlow _flow) {
		// Reset transaction cached data
		ResetTransactionData();
	}
}
