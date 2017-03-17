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
	public TMPro.TextMeshProUGUI m_priceText;
	
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
		m_priceText.text = UIConstants.GetIconString(data.def.GetAsLong("unlockPriceCoins"), UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);
	}

	/// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlock() {
		// Unlock dragon
		DragonData data = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		long priceCoins = data.def.GetAsLong("unlockPriceCoins");
		if(UsersManager.currentUser.coins >= priceCoins) {
			UsersManager.currentUser.AddCoins(-priceCoins);
			data.Acquire();
			PersistenceManager.Save();
		} else {
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
            UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_SC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}
}
