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
		/*PopupController popup = PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

		// In this particular case we want to allow several purchases in a row, so don't auto-close popup
		popup.GetComponent<PopupCurrencyShop>().closeAfterPurchase = false;*/

		// Currency popup / Resources flow disabled for now
		UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_COMING_SOON"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
	}
}
