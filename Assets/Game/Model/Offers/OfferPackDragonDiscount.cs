// OfferPackDragonDiscount.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/06/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using SimpleJSON;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Extension of the offer pack for Dragon Discount packs.
/// </summary>
public class OfferPackDragonDiscount : OfferPack {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private bool m_modApplied = false;
	private ModEconomyDragonPrice m_mod = null;
	public ModEconomyDragonPrice mod {
		get { return m_mod; }
    }
	
    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public OfferPackDragonDiscount() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPackDragonDiscount() {
		// Clear mod
		ApplyMod(false);
		m_mod = null;
	}

    //------------------------------------------------------------------------//
    // PARENT OVERRIDES														  //
    //------------------------------------------------------------------------//
	/// <summary>
	/// Reset to default values.
	/// </summary>
    public override void Reset() {
		// Clear mod
		ApplyMod(false);
		m_mod = null;

		// Let parent do the rest
        base.Reset();
    }

    /// <summary>
    /// Initialize from definition.
    /// </summary>
    /// <param name="_def">Def.</param>
    public override void InitFromDefinition(DefinitionNode _def) {
		// Check definition
		if(_def == null) {
			return;
		}

		// Let the parent do the trick
		// [AOC] The definitions won't contain most of the values (since it's not
		// from the offerPackDefinitions table), but that should be ok - default
		// values will be used instead
		base.InitFromDefinition(_def);

		// Create a mod and initialize it with values from the offer definition
		// Translate values to a format that the economy mod can understand
		SimpleJSON.JSONClass json = new SimpleJSON.JSONClass();
		json.Add("param1", _def.GetAsString("dragonSku"));
		json.Add("param2", _def.GetAsFloat("discount") * -100f);	// Dragon price mod expects negative percentage value as discount
		json.Add("param3", _def.GetAsString("currency"));
		m_mod = new ModEconomyDragonPrice(json);
	}

	/// <summary>
	/// Update loop. Should be called periodically from the manager.
	/// Will look for pack's state changes.
	/// </summary>
	/// <returns>Whether the pack has change its state.</returns>
	public override bool UpdateState() {
		//OffersManager.LogPack(this, "UpdateState {0} | {1}", Colors.pink, def.sku, m_state);
		State oldState = m_state;

		// If a state change is forced, do it
		if(m_forcedStateChangePending) {
			m_forcedStateChangePending = false;
			ChangeState(m_forcedStateChange);
		} else {
			// Based on pack's state
			switch(m_state) {
				case State.PENDING_ACTIVATION: {
					// Do nothing, dragon discount packs can only be activated externally
				} break;

				case State.ACTIVE: {
					// Default behaviour:
					// Check for expiration
					if (CheckExpiration(true)) {
						ChangeState(State.EXPIRED);
					}

					// The pack might have gone out of segmentation range (i.e. currency balance). Check it!
					// [AOC] TODO!! We might wanna keep some packs until they expire even if initial segmentation is no longer valid
					else if (!CheckSegmentation()) {
						ChangeState(State.PENDING_ACTIVATION);
					}
				} break;

				case State.EXPIRED: {
					// Nothing to do (expired packs can't be reactivated)
				} break;
			}
		}

		// Has state changed?
		return (oldState != m_state);
	}

	/// <summary>
	/// Change the logic state of the pack.
	/// No validation is done.
	/// </summary>
	/// <param name="_newState">New state for the pack.</param>
	protected override void ChangeState(State _newState) {
		// Actions to be performed when leaving a state
		switch(m_state) {
			case State.ACTIVE: {
				// Leaving ACTIVE state, disable mod
				ApplyMod(false);
			} break;
		}

		// Let parent change the state
		base.ChangeState(_newState);

		// Actions to perform when entering a new state
		switch(m_state) {
			case State.ACTIVE: {
				// Entering ACTIVE state, apply mod
				ApplyMod(true);
			} break;
        }
	}

	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public override void Load(JSONClass _data) {
		// Store state before loading
		State oldState = m_state;

		// Call parent
        base.Load(_data);

		// If state changed, check whether the mod needs to be applied
		if(m_mod != null) {
			if(oldState != m_state) {
				// Entering ACTIVE state, apply mod
				if(m_state == State.ACTIVE) {
					ApplyMod(true);
				}
				
				// Leaving ACTIVE state, disable mod
				else if(oldState == State.ACTIVE) {
					ApplyMod(false);
				}
			}
		}
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
	/// Apply/Remove mod. Will check if the mod was already applied to avoid duplicating its effect.
	/// </summary>
	/// <param name="_apply">Whether to apply or remove.</param>
	private void ApplyMod(bool _apply) {
		// Nothing to do if mod not valid
		if(m_mod == null) return;

		// Apply or remove?
		if(_apply) {
			// Apply! Only if mod is not currently applied
			if(!m_modApplied) {
				m_mod.Apply();
				m_modApplied = true;
			}
        } else {
			// Remove! Only if mod is currently applied
			if(m_modApplied) {
				m_mod.Remove();
				m_modApplied = false;
            }
        }
    }
}