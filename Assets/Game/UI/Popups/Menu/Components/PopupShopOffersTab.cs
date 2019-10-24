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
	[Space]
	[SerializeField] private GameObject m_freeOfferPillPrefab = null;

	// Internal
	private List<PopupShopOffersPill> m_normalOfferPills = new List<PopupShopOffersPill>();
	private PopupShopFreeOfferPill m_freeOfferPill = null;

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
	/// Clear the pills.
	/// </summary>
	public override void Clear() {
		// Clear local collections
		m_pills.Clear();
		m_freeOfferPill = null;
		
		// Call parent
		base.Clear();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the offer pills, adding new active offers and removing expired ones.
	/// Reuses existing pills and creates new ones if needed.
	/// </summary>
	private void RefreshOfferPills() {
		// Get list of active offer packs and create a pill for each one
		// List should already be properly sorted
		List<OfferPack> activeOffers = OffersManager.activeOffers;
		List<IPopupShopPill> sortedPills = new List<IPopupShopPill>();
		PopupShopOffersPill pill = null;
		for(int i = 0; i < activeOffers.Count; ++i) {
			// Do we need a new pill for this offer?
			// Depends on pill type
			switch(activeOffers[i].type) {
				// Free offer
				case OfferPack.Type.FREE: {
					if(m_freeOfferPill == null) {
						// Create new instance and store it
						pill = InstantiatePill(m_freeOfferPillPrefab);
						m_freeOfferPill = pill as PopupShopFreeOfferPill;
					} else {
						// Reuse existing pill
						pill = m_freeOfferPill;
					}
				} break;

				// Rest of offer types
				default: {
					if(i >= m_normalOfferPills.Count) {
						// Create new instance and store it
						pill = InstantiatePill(m_pillPrefab);
						m_normalOfferPills.Add(pill);
					} else {
						// Reuse existing pill
						pill = m_normalOfferPills[i];
					}
				} break;
			}

			// Initialize pill
			pill.gameObject.SetActive(true);
			pill.InitFromOfferPack(activeOffers[i]);

			// Change its order in the hierarchy
			pill.transform.SetSiblingIndex(i);

			// Store it sorted
			sortedPills.Add(pill);

			// Removed from unsorted pills list
			m_pills.Remove(pill);
		}

		// Hide unused pills (if any) and add them to the end of the sorted list
		for(int i = 0; i < m_pills.Count; ++i) {
			pill = m_pills[i] as PopupShopOffersPill;
			pill.InitFromOfferPack(null);   // This will do it
			sortedPills.Add(pill);
		}

		// Replace pills list by the sorted one
		m_pills = sortedPills;

		// Reset scroll list position
		m_scrollList.horizontalNormalizedPosition = 0f;

		// Update texts
		m_textOffersEmpty.gameObject.SetActive(activeOffers.Count == 0);
		m_offersCount.text = OffersManager.activeOffers.Count.ToString();

		// Notify listeners
		OnPillListChanged.Invoke(this);
	}

	/// <summary>
	/// Create a new instance of the given pill prefab.
	/// The new instance will be added to the <c>m_pills</c> list.
	/// </summary>
	/// <param name="_prefab">Prefab of the pill to be instantiated.</param>
	/// <returns>The new pill instance.</returns>
	private PopupShopOffersPill InstantiatePill(GameObject _prefab) {
		// Instantiate the given prefab within the scrolllist content
		GameObject newPillObj = Instantiate<GameObject>(_prefab, m_scrollList.content, false);

		// Get the pill component, add it to the pills list and return
		PopupShopOffersPill pill = newPillObj.GetComponent<PopupShopOffersPill>();
		m_pills.Add(pill);
		return pill;
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
            string offerName = OffersManager.GenerateTrackingOfferName(m_pills[i].def);
            // The experiment name is used as offer name        
            HDTrackingManager.Instance.Notify_OfferShown(true, m_pills[i].GetIAPSku(), offerName, m_pills[i].def.GetAsString("type"));
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