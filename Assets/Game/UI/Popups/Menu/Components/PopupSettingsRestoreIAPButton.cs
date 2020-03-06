// PopupSettingsRestoreIAPButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Button to restore non-consumable In-App Purchases bought in another device with
/// the same Apple ID account (i.e. VIP offer).
/// </summary>
public class PopupSettingsRestoreIAPButton : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal logic
	private bool m_isLoadingPopupOpen = false;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Disable ourselves if feature is not enabled
		this.gameObject.SetActive(FeatureSettingsManager.instance.IsRestoreIAPEnabled());
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The restore IAPs button has been pressed.
	/// </summary>
	public void OnRestoreIAPButton() {
		if(UsersManager.currentUser.removeAds.IsActive) {
			// The user already has the "remove ads" offer, so just show a message
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PURCHASES_ALREADY_RESTORED"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			return;
		}

		// Check if there's connection
		GameServerManager.SharedInstance.CheckConnection(delegate (FGOL.Server.Error connectionError) {
			if(connectionError == null) {
				// Success!

				// Call to the store to restore the purchases
				m_isLoadingPopupOpen = true;
				PersistenceFacade.Popups_OpenLoadingPopup();
				GameStoreManager.SharedInstance.RestorePurchases(OnRestorePurchasesCompleted);
			} else {
				// Failed to find an internet connection. Show a connection error message
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);

			}
		});
	}

	/// <summary>
	/// Callback of the restore purchases operation
	/// </summary>
	/// <param name="error">A non null value with the error information when an error occurred when restoring purchases. A null value when everything went ok</param>
	/// <param name="productIds"></param>
	private void OnRestorePurchasesCompleted(string error, List<string> productIds) {
		// The loading popup is still open!
		m_isLoadingPopupOpen = false;
		PersistenceFacade.Popups_CloseLoadingPopup();

		if(!string.IsNullOrEmpty(error)) {
			PersistenceFacade.Popups_OpenStoreErrorConnection(delegate ()
			{
				PopupSettingsSaveTab.Log("ERROR connecting to the store... ");
			});
			return;
		}

		List<Metagame.Reward> rewards = new List<Metagame.Reward>();
		int count = productIds.Count;
		for(int i = 0; i < count; i++) {
			UbiListUtils.AddRange(rewards, Metagame.Reward.GetRewardsFromIAP(productIds[i]), false, true);
		}

		count = UsersManager.currentUser.PushRewards(rewards);
		if(count > 0) {

			// Return to selection screen and show peding rewards
			// Close all open popups
			PopupManager.Clear(true);

			// Move to the rewards screen
			PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
			scr.StartFlow(false);   // No intro
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);

		} else {
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_NOTHING_TO_RESTORE"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}
}