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
    private PopupShopCurrencyPill m_offerToDisplay;

    // Internal
    private float m_timer;
    private HappyHourOffer m_happyHour;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    /// <param name="_lastOfferSku">Sku of the offer that will be shown in the popup</param>
    public void Init(string _lastOfferSku) {

        m_happyHour = OffersManager.instance.happyHour;

        // If the happy hour is currently active
        if (m_happyHour != null && m_happyHour.IsActive())
        {
            // Time left set in the Update method

            // Convert offer rate to percentage (example: .5f to +50%) 
            float percentage = m_happyHour.extraGemsFactor * 100;
            string gemsPercentage = String.Format("{0}", Math.Round(percentage));   

            // Show texts with offer rate
            m_descriptionText.text = LocalizationManager.SharedInstance.Localize("TID_HAPPY_HOUR_POPUP_MESSAGE", gemsPercentage);
            //m_extraGemsRateText.text = LocalizationManager.SharedInstance.Localize("TID_SHOP_BONUS_AMOUNT", gemsPercentage); 

            // Show the PC offer in the popup
            if (! string.IsNullOrEmpty(_lastOfferSku) && m_offerToDisplay != null)
            {
                DefinitionNode offerDef  = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, _lastOfferSku);

                if (offerDef != null)
                {
                    m_offerToDisplay.InitFromDef(offerDef);
                }
                
            }
        }

	}


	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

        if (m_happyHour != null)
        {
            // We are showing the popup, so mark the pending popup flag as false
            m_happyHour.pendingPopup = false;
        }

    }

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}


	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

    public void Update()
    {
        // Refresh offers periodically for better performance
        if (m_timer <= 0)
        {
            m_timer = 1f; // Refresh every second
            Refresh();
        }
        m_timer -= Time.deltaTime;
    }


    /// <summary>
    /// Refresh visuals
    /// </summary>
    private void Refresh()
    {

        // Refresh the happy hour panel
        if (m_happyHour != null)
        {
            if (m_happyHour.IsActive())
            {
                // Show time left in the proper format (1h 20m 30s)
                string timeLeft = TimeUtils.FormatTime(m_happyHour.TimeLeftSecs(), TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);
                m_timeLeftText.text = LocalizationManager.SharedInstance.Localize("TID_REFERRAL_DAYS_LEFT", timeLeft);

            }
            else
            {
                // The happy hour offer is not longer active. Close the popup
                GetComponent<PopupController>().Close(true);
            }
        }

    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Open the PC shop.
    /// </summary>
    private void OpenPCShopPopup()
    {
        PopupController popup = PopupManager.LoadPopup(PopupShop.PATH);
        PopupShop shopPopup = popup.GetComponent<PopupShop>();

        // Show the gems tab
        shopPopup.Init(PopupShop.Mode.DEFAULT, "Happy_Hour_Popup");
        shopPopup.closeAfterPurchase = true;

        // Open the shop popup!
        popup.Open();
  
    }

    /// <summary>
    /// Track the player action when buys the offer
    /// </summary>
    public void TrackBuyButton()
    {
        // Track the action
        HDTrackingManager.Instance.Notify_CloseHappyHourPopup(m_offerToDisplay.GetIAPSku(), ACTION_PURCHASE);

    }

    /// <summary>
    /// Track the player action when clicks on more offers button
    /// </summary>
    public void TrackShopButton()
    {
        // Track the action
        HDTrackingManager.Instance.Notify_CloseHappyHourPopup(m_offerToDisplay.GetIAPSku(), ACTION_MORE_OFFERS);

    }

    /// <summary>
    /// Track the player action when closing the popup
    /// </summary>
    public void TrackCloseButton()
    {
        // Track the action
        HDTrackingManager.Instance.Notify_CloseHappyHourPopup(m_offerToDisplay.GetIAPSku(), ACTION_CLOSE);

    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//



    /// <summary>
    /// Player clicks on the SHOP button
    /// </summary>
    public void OnShopButton ()
    {
        // Close current popup
        GetComponent<PopupController>().Close(true);

        // Track this action
        TrackShopButton();

        // If we are already in the shop do nothing
        if (PopupManager.GetOpenPopup(PopupShop.PATH) != null)
            return;

        // Otherwise open the shop in the gems tab
        OpenPCShopPopup();
    }

}