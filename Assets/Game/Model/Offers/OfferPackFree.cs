// OfferPackFree.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/10/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Extension of the offer pack for Free packs.
/// https://mdc-web-tomcat17.ubisoft.org/confluence/pages/viewpage.action?pageId=568926409
/// </summary>
public class OfferPackFree : OfferPack {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
	// Internal
    private bool m_hasBeenApplied = false;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public OfferPackFree() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPackFree() {

	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update loop. Should be called periodically from the manager.
	/// Will look for pack's state changes.
	/// </summary>
	/// <returns>Whether the pack has change its state.</returns>
	public override bool UpdateState() {
		OffersManager.LogPack("UpdateState {0} | {1}", Colors.pink, def.sku, m_state);

		// Based on pack's state
		State oldState = m_state;
		switch(m_state) {
			case State.PENDING_ACTIVATION: {
				// Do nothing, free packs can only be activated externally
			} break;

			case State.ACTIVE: {
				// Check for expiration by time
				if(CheckExpirationByTime()) {
					// Go back to pending activation state so the pack can be selected again
					ChangeState(State.PENDING_ACTIVATION);
				}

                // However, if it has expired for any other reason (i.e. Purchase Limit), go to the expired state or it's markes as ready to expire (typically because the user has just purchased it)
                else if(CheckExpiration(false)) { 
                    ChangeState(State.EXPIRED);
				}

				// Finally, if the pack hasn't expired by time nor conditions, but it has just been applied, put it back to pending activation state so it can be selected again
				else if(m_hasBeenApplied) {
					m_hasBeenApplied = false;
					ChangeState(State.PENDING_ACTIVATION);
				}
			} break;

			case State.EXPIRED: {
				// Nothing to do (expired packs can't be reactivated)
			} break;
		}

		// Has state changed?
		return (oldState != m_state);
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

	/// <summary>
	/// Apply this pack to current user.
	/// </summary>
    public override void Apply() {
        // A free offer has to disappear right after the user purchases it. (FIX for https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-3612)
        // It's marked as applied so it will be removed next time OffersManager updates this offer
		m_hasBeenApplied = true;

		// Reset free offer cooldown timer
		OffersManager.RestartFreeOfferCooldown();

		// Parent will do the rest
        base.Apply();
    }
}