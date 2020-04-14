// OfferPackRemoveAds.cs
// Hungry Dragon
// 
// Created by Jose M. Olea
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Extension of the offer pack for the Remove ads offer.

/// </summary>
public class OfferPackRemoveAds : OfferPack {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public OfferPackRemoveAds() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPackRemoveAds() {

	}

    //------------------------------------------------------------------------//
    // PARENT OVERRIDES														  //
    //------------------------------------------------------------------------//
    /// <summary>
	/// Update loop. Should be called periodically from the manager.
	/// Will look for pack's state changes.
	/// </summary>
	/// <returns>Whether the pack has change its state.</returns>
	public override bool UpdateState()
    {
        OffersManager.LogPack(this, "UpdateState {0} | {1}", Colors.pink, def.sku, m_state);
		State oldState = m_state;

		// Let parent do its thing
		base.UpdateState();

        // If the user already has the offer, disable it
        if (UsersManager.currentUser.removeAds.IsActive)
        {
            ChangeState(State.EXPIRED);
        }

        // Has state changed?
        return (oldState != m_state);
    }

    /// <summary>
    /// Apply this pack to current user.
    /// </summary>
    public override void Apply()
    {

        // Do nothing. The remove ads featured is applied at a reward level now in RewardRemoveAds.cs
                
        base.Apply();
    }

    //------------------------------------------------------------------------//
    // CUSTOM METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Immediately mark this pack as active.
    /// No checks will be performed!
    /// </summary>
    public void Activate() {
		ChangeState(State.ACTIVE);
	}


}