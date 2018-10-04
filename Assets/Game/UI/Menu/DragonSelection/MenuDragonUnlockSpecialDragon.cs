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
	[List(UserProfile.Currency.GOLDEN_FRAGMENTS, UserProfile.Currency.HARD)]
	[SerializeField] private UserProfile.Currency m_currency = UserProfile.Currency.GOLDEN_FRAGMENTS;
	[SerializeField] private Localizer m_priceText = null;

	// Internal
	private MenuShowConditionally m_showConditions;

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
		m_priceText.Localize(m_priceText.tid, StringUtils.FormatNumber(GetPrice()));
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the price of unlocking the current selected dragon in the target currency.
	/// </summary>
	/// <returns>The price.</returns>
	private long GetPrice() {
		// Choos currency
		string priceProperty = "";
		switch(m_currency) {
			case UserProfile.Currency.GOLDEN_FRAGMENTS: priceProperty = "unlockPriceGoldenFragments"; break;
			case UserProfile.Currency.HARD: priceProperty = "unlockPricePC"; break;
		}

		// Get price from the definition of the currently selected dragon.
		IDragonData dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		return dragonData.def.GetAsLong(priceProperty, 0);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlock() {
		// Make sure we can do it
		if(!m_showConditions.CheckDragon(InstanceManager.menuSceneController.selectedDragon)) {
			return;
		}

		// Get price and start purchase flow
		IDragonData dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		ResourcesFlow purchaseFlow = new ResourcesFlow("UNLOCK_SPECIAL_DRAGON");

		purchaseFlow.OnSuccess.AddListener(
			(ResourcesFlow _flow) => {
				bool wasntLocked = dragonData.GetLockState() <= IDragonData.LockState.LOCKED;

				// Just acquire target dragon!
				dragonData.Acquire();

				// If unlocking the required dragon for the rating popup, mark the popup as it can be displayed
				if(wasntLocked && dragonData.def.sku.CompareTo(MenuInterstitialPopupsController.RATING_DRAGON) == 0) {
					Prefs.SetBoolPlayer(Prefs.RATE_CHECK_DRAGON, true);
				}

				// [AOC] TODO!! Special tracking event for special dragons?
                HDTrackingManager.Instance.Notify_DragonUnlocked(dragonData.def.sku, dragonData.GetOrder());

                // Show a nice animation!
				InstanceManager.menuSceneController.GetScreenData(MenuScreen.LAB_DRAGON_SELECTION).ui.GetComponent<LabDragonSelectionScreen>().LaunchAcquireAnim(dragonData.def.sku);
			}
		);

		purchaseFlow.Begin(
			GetPrice(), 
			m_currency, 
			HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON, 
			dragonData.def
		);
	}
}
