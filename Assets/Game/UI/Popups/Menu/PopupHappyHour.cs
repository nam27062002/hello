// PopupHappyHour.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 17/09/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using TMPro;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupHappyHour : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float REFRESH_FREQUENCY = 1f; // Seconds

	new public const string PATH = "UI/Popups/Economy/PF_PopupHappyHour";

	public static string ACTION_CLOSE = "close";
	public static string ACTION_PURCHASE = "purchase";
	public static string ACTION_MORE_OFFERS = "more offers";


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	[SerializeField]
	private TextMeshProUGUI m_timeLeftText;
	[SerializeField]
	private TextMeshProUGUI m_descriptionText;
	[SerializeField]
	private TextMeshProUGUI m_extraGemsRateText;
	[SerializeField]
	private ShopHCPill m_offerToDisplay;

	[SerializeField]
	private GameObject m_regularBground;
	[SerializeField]
	private GameObject m_welcomeBackBground;

	// Internal
	private HappyHour m_happyHour;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		m_offerToDisplay.OnPurchaseSuccess.AddListener(OnPurchaseSucces);
	}

	/// <summary>
	/// Initialization.
	/// </summary>
	/// <param name="_lastPackDef">Definition of the pack that will be shown in the popup</param>
	public void Init(DefinitionNode _lastPackDef) {

		m_happyHour = OffersManager.happyHourManager.happyHour;

		// If the happy hour is currently active
		if(m_happyHour != null && m_happyHour.IsActive()) {
			// Time left set in the Update method

			// Convert offer rate to percentage (example: .5f to +50%) 
			float percentage = m_happyHour.extraGemsFactor * 100;
			string gemsPercentage = String.Format("{0}", Math.Round(percentage));

			// Show texts with offer rate
			m_descriptionText.text = LocalizationManager.SharedInstance.Localize("TID_HAPPY_HOUR_POPUP_MESSAGE", gemsPercentage);
			m_extraGemsRateText.text = LocalizationManager.SharedInstance.Localize("TID_SHOP_BONUS_AMOUNT", gemsPercentage + "%"); 

			// Show the PC offer in the popup
			if(_lastPackDef != null && m_offerToDisplay != null) {

				m_offerToDisplay.InitFromDef(_lastPackDef);
			}

			// The only way to know if this is activated by WB is looking at autostart. Not very elegant... but works
			bool welcomeBackHH = (m_happyHour.data.autoStart == false);

			// Show the proper background
			m_regularBground.SetActive(!welcomeBackHH);
			m_welcomeBackBground.SetActive(welcomeBackHH);

		} else {
			//Shouldnt happent, but just in case. If there is not happy hour active, close the popup.
			m_descriptionText.gameObject.SetActive(false);
			m_offerToDisplay.gameObject.SetActive(false);
			GetComponent<PopupController>().Close(true);
		}

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

		// We are showing the popup, so mark the pending popup flag as false
		OffersManager.happyHourManager.pendingPopup = false;

		// Refresh offer pill once per second
		InvokeRepeating("UpdatePeriodic", 0f, REFRESH_FREQUENCY);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Cancel periodic refresh of the offer pill
		CancelInvoke("UpdatePeriodic");
	}

	public void UpdatePeriodic() {
		// Refresh offers periodically for better performance
		Refresh();
	}

	/// <summary>
	/// Refresh visuals
	/// </summary>
	private void Refresh() {
		// Refresh the happy hour panel
		if(m_happyHour != null) {
			if(m_happyHour.IsActive()) {
				// Show time left in the proper format (1h 20m 30s)
				string timeLeft = TimeUtils.FormatTime(m_happyHour.TimeLeftSecs(), TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);
				m_timeLeftText.text = timeLeft;

			} else {
				// The happy hour offer is not longer active. Close the popup
				GetComponent<PopupController>().Close(true);
			}
		}

		// Refresh offer
		if(m_offerToDisplay != null) {
			m_offerToDisplay.RefreshTimer();
		}

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Open the PC shop.
	/// </summary>
	private void OpenPCShopPopup() {
		PopupController popup = PopupManager.LoadPopup(PopupShop.PATH);
		PopupShop shopPopup = popup.GetComponent<PopupShop>();

		// User is playing a run. Show only PC tab.
		shopPopup.Init(ShopController.Mode.PC_ONLY, "Happy_Hour_Popup");

		shopPopup.closeAfterPurchase = true;

		// Open the shop popup!
		popup.Open();

	}

	/// <summary>
	/// Track the player action when buys the offer
	/// </summary>
	public void TrackBuyButton() {
		// Track the action
		HDTrackingManager.Instance.Notify_CloseHappyHourPopup(m_offerToDisplay.GetIAPSku(), ACTION_PURCHASE);

	}

	/// <summary>
	/// Track the player action when clicks on more offers button
	/// </summary>
	public void TrackShopButton() {
		// Track the action
		HDTrackingManager.Instance.Notify_CloseHappyHourPopup(m_offerToDisplay.GetIAPSku(), ACTION_MORE_OFFERS);

	}

	/// <summary>
	/// Track the player action when closing the popup
	/// </summary>
	public void TrackCloseButton() {
		// Track the action
		HDTrackingManager.Instance.Notify_CloseHappyHourPopup(m_offerToDisplay.GetIAPSku(), ACTION_CLOSE);

	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The pack has been successfully purchased.
	/// </summary>
	/// <param name="_pill"></param>
	private void OnPurchaseSucces(IShopPill _pill) {
		// Track and close the popup
		TrackBuyButton();
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// Player clicks on the SHOP button
	/// </summary>
	public void OnShopButton() {
		// Close current popup
		GetComponent<PopupController>().Close(true);

		// Track this action
		TrackShopButton();

        // If the shop popup or shop scene are already open do nothing
        if (PopupManager.GetOpenPopup(PopupShop.PATH) != null
            || InstanceManager.menuSceneController.currentScreen == MenuScreen.SHOP)
        {
            return;
        }

        // Are we in the middle of a run at this moment?
        if (InstanceManager.gameSceneController != null)
        {
            // Show the shop only with gems packs
            OpenPCShopPopup();
            return;
        } 

        // So we are in the menu at this point. Go to the shop scene
        {
            InstanceManager.menuSceneController.GoToScreen(MenuScreen.SHOP);
            return;
        }

	}

}