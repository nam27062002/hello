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
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed Members
	[SerializeField] private PopupShopCurrencyPill m_recommendedPackPill = null;

	// Events
	public UnityEvent OnRecommendedPackPurchased = new UnityEvent();
	public UnityEvent OnGoToShop = new UnityEvent();
	public UnityEvent OnCancel = new UnityEvent();

    // Happy hour banner
    [Space]
    [SerializeField] private GameObject m_happyHourPanel;
    [SerializeField] private TextMeshProUGUI m_happyHourTimer;

    // Internal
    private float m_timer = 0;
    private HappyHourOffer m_happyHour; // cached object

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Component has been enabled.
    /// </summary>
    private void OnEnable() {
		Log("Subscribing to OnPurchaseSuccess event " + m_recommendedPackPill.GetIAPSku());
		m_recommendedPackPill.OnPurchaseSuccess.AddListener(OnPillPurchaseSuccess);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		Log("Unsubscribing from OnPurchaseSuccess event " + m_recommendedPackPill.GetIAPSku());
		m_recommendedPackPill.OnPurchaseSuccess.RemoveListener(OnPillPurchaseSuccess);
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
        m_happyHour = OffersManager.instance.happyHour;

    }

    /// <summary>
    /// Refresh the visual elements of the popup
    /// </summary>
    private void Refresh()
    {

        // Refresh the happy hour panel
        if (m_happyHour != null)
        {
            // If show the happy hour panel only if the offer is active        
            m_happyHourPanel.SetActive(m_happyHour.IsActive());

            if (m_happyHour.IsActive())
            {
                // Show time left in the proper format (1h 20m 30s)
                string timeLeft = TimeUtils.FormatTime(m_happyHour.TimeLeftSecs(), TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);
                m_happyHourTimer.text = LocalizationManager.SharedInstance.Localize("TID_REFERRAL_DAYS_LEFT", timeLeft);

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
	private void OnPillPurchaseSuccess(IPopupShopPill _pill) {
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