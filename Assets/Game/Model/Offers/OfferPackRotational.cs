// OfferPackRotational.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Extension of the offer pack for Rotational packs.
/// </summary>
public class OfferPackRotational : OfferPack {
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
	public OfferPackRotational() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPackRotational() {

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
		// Based on pack's state
		State oldState = m_state;
		switch(m_state) {
			case State.PENDING_ACTIVATION: {
				// Do nothing, rotational packs can only be activated externally
			} break;

			case State.ACTIVE: {
				// Check for expiration by time
				if(CheckExpirationByTime()) {
					// Go back to pending activation state so the pack can be selected again
					ChangeState(State.PENDING_ACTIVATION);
				}

				// However, if it has expired for any other reason, go to the expired state
				else if(CheckExpiration(false)) {
					ChangeState(State.EXPIRED);
				}
			} break;

			case State.EXPIRED: {
				// Nothing to do (expired packs can't be reactivated)
			} break;
		}

		// Has state changed?
		return (oldState != m_state);
	}
}