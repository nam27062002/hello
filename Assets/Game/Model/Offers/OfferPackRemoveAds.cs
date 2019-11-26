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

        // Based on pack's state
        State oldState = m_state;
        switch (m_state)
        {
            case State.PENDING_ACTIVATION:
                {
                    // Check for activation
                    if (CheckActivation() && CheckSegmentation())
                    {
                        ChangeState(State.ACTIVE);

                        // Just in case, check for expiration immediately after
                        if (CheckExpiration(true))
                        {
                            ChangeState(State.EXPIRED);
                        }
                    }

                    // Packs expiring before ever being activated (i.e. dragon not owned, excluded countries, etc.)
                    else if (CheckExpiration(true))
                    {
                        ChangeState(State.EXPIRED);
                    }
                }
                break;

            case State.ACTIVE:
                {
                    // Check for expiration
                    if (CheckExpiration(true))
                    {
                        ChangeState(State.EXPIRED);
                    }

                    // The pack might have gone out of segmentation range (i.e. currency balance). Check it!
                    // [AOC] TODO!! We might wanna keep some packs until they expire even if initial segmentation is no longer valid
                    else if (!CheckSegmentation())
                    {
                        ChangeState(State.PENDING_ACTIVATION);
                    }
                }
                break;

            case State.EXPIRED:
                {
                    // Nothing to do (expired packs can't be reactivated)
                }
                break;
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