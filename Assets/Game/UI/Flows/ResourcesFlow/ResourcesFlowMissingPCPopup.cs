// ResourcesFlowMissingPCPopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// PREPROCESSOR																  //
//----------------------------------------------------------------------------//
#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar popup to the ResourcesFlow.
/// </summary>
public class ResourcesFlowMissingPCPopup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupMissingPC";
	private const float REFRESH_FREQUENCY = 1f; // Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed Members
	[SerializeField] private ShopCurrencyPill m_recommendedPackPill = null;

	// Events
	[System.NonSerialized] public UnityEvent OnRecommendedPackPurchased = new UnityEvent();
    [System.NonSerialized] public UnityEvent OnGoToShop = new UnityEvent();
    [System.NonSerialized] public UnityEvent OnCancel = new UnityEvent();

	// Happy hour banner
	[Space]
    [Header("Happy hour")]
	[SerializeField] private GameObject m_happyHourPanel;
    [SerializeField] private TextMeshProUGUI m_happyHourBadgeText;
    [SerializeField] private TextMeshProUGUI m_happyHourTimer;
    [SerializeField] private GameObject m_happyHourBackground;

    // Internal
    private HappyHour m_happyHour; // cached object

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		Log("Subscribing to OnPurchaseSuccess event " + m_recommendedPackPill.GetIAPSku());
		m_recommendedPackPill.OnPurchaseSuccess.AddListener(OnPillPurchaseSuccess);

		// Refresh happy hour once per second
		InvokeRepeating("UpdatePeriodic", 0f, REFRESH_FREQUENCY);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		Log("Unsubscribing from OnPurchaseSuccess event " + m_recommendedPackPill.GetIAPSku());
		m_recommendedPackPill.OnPurchaseSuccess.RemoveListener(OnPillPurchaseSuccess);

		// Cancel periodic refresh of the happy hour
		CancelInvoke("UpdatePeriodic");
	}

	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	public void UpdatePeriodic() {
		// Refresh happy hour periodically for better performance
		RefreshHappyHour();

		// Refresh the pill
		if(m_recommendedPackPill != null) {
			m_recommendedPackPill.RefreshTimer();
		}
	}


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given data.
	/// </summary>
	/// <param name="_coinsToBuy">Amount of coins to buy.</param>
	/// <param name="_pricePC">PC price of the coins to buy.</param>
	public void Init(DefinitionNode _recommendedPackDef) {
		// Initialize recommended pack
		m_recommendedPackPill.InitFromDef(_recommendedPackDef);

		// Cache happy hour offer
		m_happyHour = OffersManager.happyHourManager.happyHour;
	}

	/// <summary>
	/// Refresh the Happy Hour visuals
	/// </summary>
	private void RefreshHappyHour() {
		// Refresh the happy hour panel
		if(m_happyHour != null)  {

            // If show the happy hour panel only if the offer is active        
            if (m_happyHourPanel != null)
            {
                m_happyHourPanel.SetActive(m_happyHour.IsActive());
            }

            if (m_happyHourBackground != null)
            {
                m_happyHourBackground.SetActive(m_happyHour.IsActive());
            }

            if (m_happyHour.IsActive()) {

                // Convert offer rate to percentage (example: .5f to +50%) 
                float percentage = m_happyHour.extraGemsFactor * 100;
                string gemsPercentage = String.Format("{0}", Math.Round(percentage));

                // Show badge with extra rate
                string badgeText = LocalizationManager.SharedInstance.Localize("TID_SHOP_BONUS_AMOUNT", gemsPercentage + "%");
                m_happyHourBadgeText.text = badgeText;

                // Show time left in the proper format (1h 20m 30s)
                string timeLeft = TimeUtils.FormatTime(m_happyHour.TimeLeftSecs(), TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);
				if(m_happyHourTimer != null) {
					m_happyHourTimer.text = timeLeft;
				}

			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// More offers button has been pressed.
	/// </summary>
	public void OnMoreOffersButton() {
		// Managed externally
		OnGoToShop.Invoke();
	}

	/// <summary>
	/// Cancel button has been pressed.
	/// </summary>
	public void OnCancelButton() {
		// Managed externally
		OnCancel.Invoke();
	}

	/// <summary>
	/// The purchase flow on the pill has sucessfully ended.
	/// </summary>
	/// <param name="_pill">The pill that triggered the event</param>
	private void OnPillPurchaseSuccess(IShopPill _pill) {
		// Notify listeners
		Log("OnPillPurchaseSuccess! " + _pill.GetIAPSku());
		OnRecommendedPackPurchased.Invoke();
	}

	//------------------------------------------------------------------------//
	// DEBUG METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Print something on the console / control panel log.
	/// </summary>
	/// <param name="_message">Message to be printed.</param>
#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
	[Conditional("FALSE")]
#endif
	private void Log(string _message) {
		ControlPanel.Log("[MissingPCPopup]" + _message, ControlPanel.ELogChannel.Store);
	}
}