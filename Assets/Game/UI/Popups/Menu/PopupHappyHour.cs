// PopupHappyHour.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 17/09/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
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

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [SerializeField]
    private TextMeshProUGUI m_timeLeftText;
    [SerializeField]
    private TextMeshProUGUI m_descriptionText;
    [SerializeField]
    private TextMeshProUGUI m_extraGemsRateText;

    // Internal
    private float m_timer;
    private HappyHourOffer m_happyHour;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    public void Init() {

        m_happyHour = OffersManager.instance.happyHour;

        // If the happy hour is currently active
        if (m_happyHour != null && m_happyHour.IsActive())
        {
            // Time left set in the Update method

            // Convert offer rate to percentage (example: .5f to +50%) 
            string gemsPercentage = StringUtils.MultiplierToPercentage(m_happyHour.extraGemsFactor);

            // Show texts with offer rate
            m_descriptionText.text = LocalizationManager.SharedInstance.Localize("TID_HAPPY_HOUR_POPUP_MESSAGE", gemsPercentage);
            m_extraGemsRateText.text = LocalizationManager.SharedInstance.Localize("TID_SHOP_BONUS_AMOUNT", gemsPercentage); 
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

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}