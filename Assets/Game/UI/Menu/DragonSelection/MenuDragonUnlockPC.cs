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
	public Text m_priceText;
	
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
		Refresh(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon);
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

		// Update price
		m_priceText.text = StringUtils.FormatNumber(data.def.GetAsLong("unlockPricePC"));
	}

	/// <summary>
	/// The unlock button has been pressed.
	/// </summary>
	public void OnUnlock() {
		// Unlock dragon
		DragonData data = DragonManager.GetDragonData(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon);
		long pricePC = data.def.GetAsLong("unlockPricePC");
		if(UserProfile.pc >= pricePC) {
			UserProfile.AddPC(-pricePC);
			data.Acquire();
			PersistenceManager.Save();
		} else {
			PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);
		}
	}
}
