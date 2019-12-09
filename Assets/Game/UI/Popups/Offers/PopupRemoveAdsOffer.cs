// PopupRemoveAdsOffer.cs
// Hungry Dragon
// 
// Created by Jose Maria Olea
// Copyright (c) 2019 Ubisoft. All rights reserved.

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
public class PopupRemoveAdsOffer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupRemoveAdsOffer";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private PopupShopOffersPill m_singleItemLayoutPill = null;

	// Internal
	private OfferPack m_pack = null;

	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		m_singleItemLayoutPill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		m_singleItemLayoutPill.OnPurchaseSuccess.RemoveListener(OnPurchaseSuccessful);
	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initializes the popup with the remove ads offer active.
    /// If no one is active, close the popup.
    /// </summary>
    public void Init()
    {
        // Find any active remove ads offer (shouldnt be more than 1, but just in case of crazy AB tests)
        OfferPack pack =  OffersManager.activeOffers.Find(p => p.type == OfferPack.Type.REMOVE_ADS);

        if (pack == null)
        {
            // There are no remove ads active offers
            GetComponent<PopupController>().Close(true);
            return;
        }

        OfferPackRemoveAds removeAds = (OfferPackRemoveAds)pack;

        // Initialize the popup with the offer pack data
        InitFromOfferPack(removeAds);

    }


    /// <summary>
    /// Initialize the popup with a given pack's data.
    /// </summary>
    /// <param name="_pack">Pack.</param>
    public void InitFromOfferPack(OfferPack _pack) {
		// Store pack
		m_pack = _pack;

        // Clear both pills
        m_singleItemLayoutPill.InitFromOfferPack(null);

		// Don't do anything else if pack is null (shouldn't happen though :s)
		if(m_pack == null) return;

        // Initialize target pill with offer pack
        m_singleItemLayoutPill.InitFromOfferPack(m_pack);
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