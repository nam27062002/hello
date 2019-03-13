// PopupFeaturedOffer.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the Featured Offer popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupFeaturedOffer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupFeaturedOffer";
	private const float REFRESH_FREQUENCY = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private PopupShopOffersPill m_basicLayoutPill = null;
	[SerializeField] private PopupShopOffersPill m_featuredLayoutPill = null;
	[SerializeField] private PopupShopOffersPill m_singleItemLayoutPill = null;

	// Internal
	private OfferPack m_pack = null;
	private PopupShopOffersPill m_activePill = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		m_basicLayoutPill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);
		m_featuredLayoutPill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);
		m_singleItemLayoutPill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		InvokeRepeating("PeriodicRefresh", 0f, REFRESH_FREQUENCY);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		m_basicLayoutPill.OnPurchaseSuccess.RemoveListener(OnPurchaseSuccessful);
		m_featuredLayoutPill.OnPurchaseSuccess.RemoveListener(OnPurchaseSuccessful);
		m_singleItemLayoutPill.OnPurchaseSuccess.RemoveListener(OnPurchaseSuccessful);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with a given pack's data.
	/// </summary>
	/// <param name="_pack">Pack.</param>
	public void InitFromOfferPack(OfferPack _pack) {
		// Store pack
		m_pack = _pack;

		// Clear both pills
		m_basicLayoutPill.InitFromOfferPack(null);
		m_featuredLayoutPill.InitFromOfferPack(null);
		m_singleItemLayoutPill.InitFromOfferPack(null);

		// Don't do anything else if pack is null (shouldn't happen though :s)
		if(m_pack == null) return;

		// Select target layout, based on whether the pack has a featured item or not
		m_activePill = m_basicLayoutPill;
		if(m_pack.items.Count == 1) {
			// Single item layout
			m_activePill = m_singleItemLayoutPill;
		} else {
			// Featured layout?
			for(int i = 0; i < m_pack.items.Count; ++i) {
				if(m_pack.items[i].featured) {
					m_activePill = m_featuredLayoutPill;
					break;
				}
			}
		}

		// Initialize target pill with offer pack
		m_activePill.InitFromOfferPack(m_pack);
	}

	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	private void PeriodicRefresh() {
		// Nothing if not enabled
		if(!this.isActiveAndEnabled) return;

		// Propagate to active pill
		if(m_activePill != null) m_activePill.RefreshTimer();

		// If invalid pack or pack has expired, close popup
		if(m_pack == null || !m_pack.isActive) {
			GetComponent<PopupController>().Close(true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The shop button has been pressed.
	/// </summary>
	public void OnShopButton() {
		// Close this popup
		GetComponent<PopupController>().Close(true);

		// Open shop popup
		PopupController shopPopup = PopupManager.LoadPopup(PopupShop.PATH);
		shopPopup.GetComponent<PopupShop>().Init(PopupShop.Mode.OFFERS_FIRST, "Featured_Offer");
		shopPopup.Open();
	}

	/// <summary
	/// Successful purchase.
	/// </summary>
	/// <param name="_pill">The pill that triggered the event</param>
	private void OnPurchaseSuccessful(IPopupShopPill _pill) {
		// Close popup
		GetComponent<PopupController>().Close(true);
	}
}