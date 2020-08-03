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
	[SerializeField] protected GameObject m_happyHourButtonFx = null;

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

        // Subscribe to happy hour events
        Messenger.AddListener(MessengerEvents.HAPPY_HOUR_CHANGED, RefreshHappyHour);
    }

    private new void OnDestroy()
    {
        // Unsubscribe to events
        Messenger.RemoveListener(MessengerEvents.HAPPY_HOUR_CHANGED, RefreshHappyHour);
    }



    /// <summary>
    /// Component has been enabled.
    /// </summary>
    protected override void OnEnable() {

        base.OnEnable();

		// Refresh Happy Hour visuals immediately
		RefreshHappyHour();
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// https://docs.unity3d.com/ScriptReference/MonoBehaviour.InvokeRepeating.html
	/// </summary>
	public override void RefreshTimer() {
		// Let parent do its thing
		base.RefreshTimer();
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

        // Happy Hour visuals
        RefreshHappyHour();
    }


	/// <summary>
	/// Refresh Happy Hour visuals.
	/// </summary>
	/// <param name="_force">Force refresh or only if state has changed?</param>
    private void RefreshHappyHour() {
        
        // In case there is a happy hour active
        bool hhActive = false;
		if(OffersManager.happyHourManager.happyHour != null) {
			// Check whether Happy Hour applies to this pack or not
			HappyHourManager happyHour = OffersManager.happyHourManager;
			hhActive = happyHour.happyHour.IsActive() && happyHour.IsPackAffected(m_def);
		}

        // Add a shiny effect to the button
        if (m_happyHourButtonFx != null)
        {
            m_happyHourButtonFx.SetActive(hhActive);
        }


        // Apply to item slot
        if (m_itemSlotHC != null)
        {
            m_itemSlotHC.ApplyHappyHour(OffersManager.happyHourManager.happyHour);
        }


		// Hide the regular extra % text during HH
		if(m_bonusAmountText != null) {
			m_bonusAmountText.gameObject.SetActive(!hhActive);
		}
    }
}