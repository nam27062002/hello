// CurrencyFlow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class encapsulates the whole logic of purchasing anything in the game with
/// either SC or PC. Manages the check of required currencies, computes the required
/// PC to top up price and opens the PC shop if needed.
/// If the flow is completed, performs the currency transaction on the profile and
/// invokes the corresponding Callback to notify its result so item transaction can
/// be completed.
/// </summary>
public class ResourcesFlow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const long PC_CONFIRMATION_POPUP_THRESHOLD = 20;	// Show confirmation popup for PC purchases bigger than this threshold

	// Custom events
	public class ResourcesFlowEvent : UnityEvent<ResourcesFlow> { };
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	private DefinitionNode m_itemDef = null;	// Optional, item to be purchased
	public DefinitionNode itemDef {
		get { return m_itemDef; }
		set { m_itemDef = value; }
	}

	// Base prices
	private long m_priceCoins = 0;
	private long m_pricePC = 0;
	
	// Missing resources
	private long m_missingCoins = 0;
	private long m_missingPC = 0;

	// Actual prices considering current resources - computed by the currency flow
	private long m_finalPriceCoins = 0;
	private long m_finalPricePC = 0;

	// Popups
	private List<PopupController> m_popups = null;

	// Events
	public ResourcesFlowEvent OnSuccess = new ResourcesFlowEvent();
	public ResourcesFlowEvent OnCancel = new ResourcesFlowEvent();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public ResourcesFlow() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	~ResourcesFlow() {

	}

	/// <summary>
	/// Start the flow.
	/// Prices should have been set.
	/// </summary>
	public void Begin() {
		// Aux vars
		bool hasEnoughCoins = HasEnoughCoins();
		bool hasEnoughPC = HasEnoughPC();

		// If a resource has failed, decide what to do
		if(!hasEnoughCoins) {

		} else if(!hasEnoughPC) {

		// Everything ok! Do the transaction
		} else {
			// Show confirmation popup for big PC purchases
			if(m_pricePC >= PC_CONFIRMATION_POPUP_THRESHOLD) {
				// Show confirmation popup
				// [AOC] TODO!!
			} else {
				// Everything ok!
				DoTransaction();
			}
		}
	}

	/// <summary>
	/// Perform the transaction! Should only be called once when all the checks
	/// have been passed.
	/// </summary>
	private void DoTransaction() {
		// [AOC] TODO!! Tracking

		// Coins transaction
		if(m_finalPriceCoins > 0) {
			UsersManager.currentUser.AddCoins(-m_finalPriceCoins);
		}

		// PC Transaction
		if(m_finalPricePC > 0) {
			UsersManager.currentUser.AddPC(-m_finalPricePC);
		}

		// Notify!
		OnSuccess.Invoke(this);

		// Close any open popups
		Close();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether the current user has enough coins to start the transaction.
	/// Price must have been set.
	/// Initializes the m_missingCoins variable.
	/// </summary>
	/// <returns>Whether the current user has enough coins to start the transaction.</returns>
	public bool HasEnoughCoins() {
		// Check coins
		if(m_priceCoins > UsersManager.currentUser.coins) {
			m_missingCoins = m_priceCoins - UsersManager.currentUser.coins;
			return false;
		}
		return true;
	}

	/// <summary>
	/// Check whether the current user has enough PC to start the transaction.
	/// Price must have been set.
	/// Initializes the m_missingPC variable.
	/// </summary>
	/// <returns>Whether the current user has enough PC to start the transaction.</returns>
	public bool HasEnoughPC() {
		// Check coins
		if(m_pricePC > UsersManager.currentUser.pc) {
			m_missingPC = m_pricePC - UsersManager.currentUser.pc;
			return false;
		}
		return true;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Close any popups/UI opened by this flow.
	/// </summary>
	private void Close() {
		// Close any open popup
		for(int i = 0; i < m_popups.Count; i++) {
			m_popups[i].Close(false);	// Don't destroy, let's reuse popups from flow to flow! (popup manager will take care of that)
		}
		m_popups.Clear();
	}

	/// <summary>
	/// Cancel the flow.
	/// </summary>
	private void Cancel() {
		// Notify!
		OnCancel.Invoke(this);

		// Close flow
		Close();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}