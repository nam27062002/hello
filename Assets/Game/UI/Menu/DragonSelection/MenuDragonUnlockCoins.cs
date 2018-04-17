// MenuDragonUnlockCoins.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Unlock the selected dragon using coins.
/// </summary>
public class MenuDragonUnlockCoins : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	public Localizer m_priceText;
	private MenuShowConditionally m_showConditions;
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
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

	/// <summary>
	/// Refresh with data from currently selected dragon.
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon</param>
	public void Refresh(string _sku) {
		// Get new dragon's data from the dragon manager
		DragonData data = DragonManager.GetDragonData(_sku);
		if(data == null) return;

		// Update price
		m_priceText.Localize(m_priceText.tid, StringUtils.FormatNumber(data.def.GetAsLong("unlockPriceCoins")));
	}

	/// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlock() {
		if (m_showConditions.CheckDragon(InstanceManager.menuSceneController.selectedDragon)){
			// Get price and start purchase flow
			DragonData dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
			ResourcesFlow purchaseFlow = new ResourcesFlow("UNLOCK_DRAGON_COINS");
			purchaseFlow.OnSuccess.AddListener(
				(ResourcesFlow _flow) => {
					bool wasntLocked = dragonData.GetLockState() <= DragonData.LockState.LOCKED;

					// Just acquire target dragon!
					dragonData.Acquire();

					// If unlocking the required dragon for the rating popup, mark the popup as it can be displayed
					if(wasntLocked && dragonData.def.sku.CompareTo(MenuInterstitialPopupsController.RATING_DRAGON) == 0) {
						Prefs.SetBoolPlayer(Prefs.RATE_CHECK_DRAGON, true);
					}

	                HDTrackingManager.Instance.Notify_DragonUnlocked(dragonData.def.sku, dragonData.GetOrder());

	                // Show a nice animation!
					InstanceManager.menuSceneController.GetScreenData(MenuScreen.DRAGON_SELECTION).ui.GetComponent<MenuDragonScreenController>().LaunchAcquireAnim(dragonData.def.sku);
				}
			);
			purchaseFlow.Begin(dragonData.def.GetAsLong("unlockPriceCoins"), UserProfile.Currency.SOFT, HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON, dragonData.def);
		}
	}
}
