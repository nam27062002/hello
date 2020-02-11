// PopupShopHCPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization of the currency pill for HC packs.
/// </summary>
public class ShopHCPill : ShopCurrencyPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("HC Pill Specifics")]
	[SerializeField] protected GameObject m_happyHourBg = null;

    // Public
    private bool m_happyHourActive = false;
	public bool happyHourActive {
		get { return m_happyHourActive; }
	}

	// Internal
	OfferItemSlotHC m_itemSlotHC = null;    // Shortcut to already casted slot

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake() {
		// Store casted reference to item slot
		if(m_offerItemSlot != null) {
			m_itemSlotHC = m_offerItemSlot as OfferItemSlotHC;
		}
		Debug.Assert(m_itemSlotHC != null, "This pill type should have a OfferItemSlotHC as item slot.");
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	public void OnEnable() {
		// Refresh Happy Hour visuals immediately
		RefreshHappyHour(true);
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// https://docs.unity3d.com/ScriptReference/MonoBehaviour.InvokeRepeating.html
	/// </summary>
	public override void RefreshTimer() {
		// Let parent do its thing
		base.RefreshTimer();

		// Refresh Happy Hour visuals periodically for better performance
		RefreshHappyHour(false);
	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize the pill with a given pack's data.
    /// </summary>
    /// <param name="_pack">Pack.</param>
    public override void InitFromOfferPack(OfferPack _pack)
    {
        // Let parent do the hard work and do some extra initialization afterwards
        base.InitFromOfferPack(_pack);

        // Nothing to do if pack is not valid
        if (m_def == null) return;

        // Hide all the happy hour elements
        if (m_happyHourBg != null)
            m_happyHourBg.SetActive(false);

        // Happy Hour visuals
        RefreshHappyHour(false);
    }


	/// <summary>
	/// Refresh Happy Hour visuals.
	/// </summary>
	/// <param name="_force">Force refresh or only if state has changed?</param>
    private void RefreshHappyHour(bool _force) {
		// In case there is a happy hour active
		bool hhActive = false;
		if(OffersManager.happyHourManager.happyHour != null) {
			// Check whether Happy Hour applies to this pack or not
			HappyHourManager happyHour = OffersManager.happyHourManager;
			hhActive = happyHour.happyHour.IsActive() && happyHour.IsPackAffected(m_def);
		}

		// Do we need to refresh visuals?
		if(hhActive != m_happyHourActive || _force) {
			// Store new state
			m_happyHourActive = hhActive;

			// Show a nice purple bground
			if(m_happyHourBg != null) {
				m_happyHourBg.SetActive(hhActive);
			}

			// Apply to item slot
			if(m_itemSlotHC != null) {
				m_itemSlotHC.ApplyHappyHour(hhActive ? OffersManager.happyHourManager.happyHour : null);
			}

			// Hide the regular extra % text during HH
			if(m_bonusAmountText != null) {
				m_bonusAmountText.gameObject.SetActive(!hhActive);
			}
		}
    }
}