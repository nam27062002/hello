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
public class PopupShopReferral : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupShopReferral";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShopReferralPill m_referralPill = null;

	// Buttons
	[SerializeField] private GameObject m_buttonInvite;
	[SerializeField] private GameObject m_buttonInviteMore;
	[SerializeField] private GameObject m_buttonClaim;

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
		//m_singleItemLayoutPill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		//m_singleItemLayoutPill.OnPurchaseSuccess.RemoveListener(OnPurchaseSuccessful);
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
        OfferPack pack =  OffersManager.activeOffers.Find(p => p.type == OfferPack.Type.REFERRAL);

        if (pack == null)
        {
            // There are no remove ads active offers
            GetComponent<PopupController>().Close(true);
            return;
        }


        // Initialize the popup with the offer pack data
        InitFromOfferPack(pack);

    }


    /// <summary>
    /// Initialize the popup with a given pack's data.
    /// </summary>
    /// <param name="_pack">Pack.</param>
    public void InitFromOfferPack(OfferPack _pack) {
		// Store pack
		m_pack = _pack;

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
    /// The user has pressed the INVITE button
    /// </summary>
    public void OnInviteButtonPressed ()
    {

    }


    /// <summary>
    /// The user has pressed the CLAIM reward button
    /// </summary>
	public void OnClaimButtonPressed ()
	{

	}

}