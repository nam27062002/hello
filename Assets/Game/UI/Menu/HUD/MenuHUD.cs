// MenuHUD.cs
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
/// Controller for the HUD in the main menu.
/// </summary>
public class MenuHUD : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Not much to do
	}

	/// <summary>
	/// Open the currency shop popup.
	/// </summary>
	public void OpenCurrencyShopPopup() {
		// Just do it
		PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);
	}

	/// <summary>
	/// Opens the missions popup.
	/// </summary>
	public void OpenMissionsPopup() {
		// Just do it
		PopupManager.OpenPopupInstant(PopupMissions.PATH);
	}
}
