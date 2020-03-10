// PopupShopOfferPack.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the Offer Pack Popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupShopOfferPack : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupShopOfferPack";
	private const float REFRESH_FREQUENCY = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShopMultiRewardPill m_rootPill = null;

	// Internal
	private OfferPack m_pack = null;
	private int m_initializationPending = -1;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		m_rootPill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);
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
		m_rootPill.OnPurchaseSuccess.RemoveListener(OnPurchaseSuccessful);
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

		// Clear the pill
		m_rootPill.InitFromOfferPack(null);

		// Don't do anything else if pack is null (shouldn't happen though :s)
		if(m_pack == null) return;

		// Initialize pill with target offer pack
		// Delay until popup is ready
		m_initializationPending = 2;	// Delat some frames until popup is ready
		//m_rootPill.InitFromOfferPack(m_pack);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Initialize visuals if needed
		if(m_initializationPending > 0) {
			m_initializationPending--;
			if(m_initializationPending <= 0) {
				m_rootPill.InitFromOfferPack(m_pack);
				m_initializationPending = -1;
			}
		}
	}

	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	private void PeriodicRefresh() {
		// Nothing if not enabled
		if(!this.isActiveAndEnabled) return;

		// Propagate to active pill
		m_rootPill.RefreshTimer();

		// If invalid pack or pack has expired, close popup
		if(m_pack == null || !m_pack.isActive) {
			GetComponent<PopupController>().Close(true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup has been opened.
	/// </summary>
	public void OnShowPostAnimation() {
		// Update pack's view tracking
		m_pack.NotifyPopupDisplayed();
	}

	/// <summary>
	/// The shop button has been pressed.
	/// </summary>
	public void OnShopButton() {
		// Close this popup
		GetComponent<PopupController>().Close(true);

		// Open shop popup - unless already open
		if(PopupManager.GetOpenPopup(PopupShop.PATH) == null) {
			PopupController shopPopup = PopupManager.LoadPopup(PopupShop.PATH);
			shopPopup.GetComponent<PopupShop>().Init(ShopController.Mode.DEFAULT, "Featured_Offer");
			shopPopup.Open();
		}
	}

	/// <summary
	/// Successful purchase.
	/// </summary>
	/// <param name="_pill">The pill that triggered the event</param>
	private void OnPurchaseSuccessful(IShopPill _pill) {
		// Close popup
		GetComponent<PopupController>().Close(true);
	}
}