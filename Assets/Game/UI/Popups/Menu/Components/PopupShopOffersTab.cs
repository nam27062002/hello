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
		// Nothing if not enabled
		if(!this.isActiveAndEnabled) return;

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

		// Re-create pills
		RefreshOfferPills();
	}

	/// <summary>
	/// Refresh the offer pills, adding new active offers and removing expired ones.
	/// Reuses existing pills and creates new ones if needed.
	/// </summary>
	private void RefreshOfferPills() {
		// Make sure manager is updated
		OffersManager.instance.Refresh();

		// Get list of active offer packs and create a pill for each one
		// List should already be properly sorted
		List<OfferPack> activeOffers = OffersManager.activeOffers;
		PopupShopOffersPill pill = null;
		for(int i = 0; i < activeOffers.Count; ++i) {
			// Do we need a new pill for this offer?
			if(i >= m_pills.Count) {
				// Create new instance and store it
				GameObject newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
				pill = newPillObj.GetComponent<PopupShopOffersPill>();
				m_pills.Add(pill);
			} else {
				// Reuse existing pill
				pill = m_pills[i] as PopupShopOffersPill;
			}

			// Initialize pill
			pill.gameObject.SetActive(true);
			pill.InitFromOfferPack(activeOffers[i]);
		}

		// Hide unused pills (if any)
		for(int i = activeOffers.Count; i < m_pills.Count; ++i) {
			pill.gameObject.SetActive(false);
		}

		// Reset scroll list position
		m_scrollList.horizontalNormalizedPosition = 0f;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The tab is about to be displayed.
	/// </summary>
	public void OnShow() {
		// Refresh pills list
		RefreshOfferPills();

		// React to offers being reloaded while tab is active
		Messenger.AddListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
	}

	/// <summary>
	/// The tab has been hidden.
	/// </summary>
	public void OnHide() {
		Messenger.RemoveListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
	}

	/// <summary>
	/// Offers have been reloaded.
	/// </summary>
	private void OnOffersReloaded() {
		// Refresh pills list
		RefreshOfferPills();
	}
}