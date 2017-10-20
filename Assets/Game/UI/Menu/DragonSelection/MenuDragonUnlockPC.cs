// MenuDragonUnlockPC.cs
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
/// Unlock the selected dragon using PC.
/// </summary>
public class MenuDragonUnlockPC : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	public Localizer m_priceText;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Required fields
		DebugUtils.Assert(m_priceText != null, "Required reference missing!");
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, Refresh);
		
		// Do a first refresh
		Refresh(InstanceManager.menuSceneController.selectedDragon);
	}
	
	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, Refresh);
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
		m_priceText.Localize(m_priceText.tid, StringUtils.FormatNumber(data.def.GetAsLong("unlockPricePC")));
	}

	/// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlock() 
	{
		// Get price and start purchase flow
		DragonData dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		ResourcesFlow purchaseFlow = new ResourcesFlow("UNLOCK_DRAGON_PC");
		purchaseFlow.OnSuccess.AddListener(
			(ResourcesFlow _flow) => {
				bool wasntLocked = dragonData.GetLockState() <= DragonData.LockState.LOCKED;
				// Just acquire target dragon!
				dragonData.Acquire();

				if ( wasntLocked && dragonData.def.sku.CompareTo( MenuSceneController.RATING_DRAGON ) == 0 ){
					MenuSceneController.CheckRatingFlow();
				}

                HDTrackingManager.Instance.Notify_DragonUnlocked(dragonData.def.sku, dragonData.GetOrder());

                // Show nice animation!
                InstanceManager.menuSceneController.GetScreen(MenuScreens.DRAGON_SELECTION).GetComponent<MenuDragonScreenController>().LaunchUnlockAnim(dragonData.def.sku, 0.2f, 0.1f, true);
			}
		);
		purchaseFlow.Begin(dragonData.def.GetAsLong("unlockPricePC"), UserProfile.Currency.HARD, HDTrackingManager.EEconomyGroup.UNLOCK_DRAGON, dragonData.def);
	}
}
