// PopupShopTab.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tab in the currency shop!
/// </summary>
public class PopupShopOffersTab : IPopupShopTab {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float REFRESH_FREQUENCY = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		InvokeRepeating("PeriodicRefresh", 0f, REFRESH_FREQUENCY);
	}

	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	private void PeriodicRefresh() {
		// Propagate to pills
		for(int i = 0; i < m_pills.Count; ++i) {
			(m_pills[i] as PopupShopOffersPill).RefreshTimer();
		}
	}

	//------------------------------------------------------------------------//
	// IPopupShopTab IMPLEMENTATION											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this tab and instantiate required pills.
	/// </summary>
	override public void Init() {
		// Clear current pills
		Clear();

		// Get list of active offer packs and create a pill for each one
		// List should already be properly sorted
		List<OfferPack> activeOffers = OffersManager.activeOffers;
		for(int i = 0; i < activeOffers.Count; ++i) {
			// Create new instance and initialize it
			GameObject newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
			PopupShopOffersPill newPill = newPillObj.GetComponent<PopupShopOffersPill>();
			newPill.InitFromOfferPack(activeOffers[i]);

			// Store to local collection for further use
			m_pills.Add(newPill);
		}

		// Reset scroll list position
		m_scrollList.horizontalNormalizedPosition = 0f;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}