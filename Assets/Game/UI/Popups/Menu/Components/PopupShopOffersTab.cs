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
using TMPro;

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
	[SerializeField] private TextMeshProUGUI m_textOffersEmpty;
	[SerializeField] private TextMeshProUGUI m_offersCount;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		InvokeRepeating("PeriodicRefresh", 0f, REFRESH_FREQUENCY);

		// React to offers being reloaded while tab is active
		Messenger.AddListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
		Messenger.AddListener(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
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

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
		Messenger.RemoveListener(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
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
	/// <param name="_refreshManager">Force a refresh on the manager?</param>
	private void RefreshOfferPills() {
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
			pill = m_pills[i] as PopupShopOffersPill;
			pill.InitFromOfferPack(null);	// This will do it
		}

		// Reset scroll list position
		m_scrollList.horizontalNormalizedPosition = 0f;

		// Update texts
		m_textOffersEmpty.gameObject.SetActive(activeOffers.Count == 0);
		m_offersCount.text = OffersManager.activeOffers.Count.ToString();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The tab is about to be displayed.
	/// </summary>
	public void OnTabShow() {
		// Refresh pills list
		RefreshOfferPills();

		// Tracking
		for(int i = 0; i < m_pills.Count; ++i) {
			// Skip unused pills (expired packs)
			if(m_pills[i].def == null) continue;
            // The experiment name is used as offer name        
            HDTrackingManager.Instance.Notify_OfferShown(true, m_pills[i].GetIAPSku(), HDCustomizerManager.instance.GetExperimentNameForDef(m_pills[i].def), m_pills[i].def.GetAsString("type"));
        }
	}

	/// <summary>
	/// The tab has been hidden.
	/// </summary>
	public void OnTabHide() {
		
	}

	/// <summary>
	/// Offers have been reloaded.
	/// </summary>
	private void OnOffersReloaded() {
		// Ignore if not active
		if(!this.isActiveAndEnabled) return;

		// Refresh pills list
		RefreshOfferPills();
	}

	/// <summary>
	/// Offers list has changed.
	/// </summary>
	private void OnOffersChanged() {
		// Ignore if not active
		if(!this.isActiveAndEnabled) return;

		// Refresh pills list
		RefreshOfferPills();
	}
}