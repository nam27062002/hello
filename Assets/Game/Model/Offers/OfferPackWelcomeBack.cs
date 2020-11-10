// OfferPackWelcomeBack.cs
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
public class OfferPackWelcomeBack : OfferPack {
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
    public OfferPackWelcomeBack() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPackWelcomeBack() {

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
        
        // If welcome back offer is deactivated (can only be done via cheats panel), this offer expires
        if (oldState == State.ACTIVE && !WelcomeBackManager.instance.hasBeenActivated)
        {
            ChangeState(State.EXPIRED);
        }

        // Has state changed?
        return (oldState != m_state);
    }

    /// <summary>
    /// Checks parameters that could cause an activation.
    /// These are parameters that can only be triggered once:
    /// Start date, unlocked content, progression, etc.
    /// </summary>
    /// <returns>Whether this pack should be activated or not.</returns>
    public override bool CheckActivation()
    {
        // Welcome pack offers are only available if Welcome back has been triggered
        bool welcomeBackTriggered = WelcomeBackManager.instance.hasBeenActivated;
        
        // Check the rest of the  segmentation conditions
        bool activate =  base.CheckActivation() && welcomeBackTriggered;

        return activate;
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