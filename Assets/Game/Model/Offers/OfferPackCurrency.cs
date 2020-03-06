// OfferPackItem.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Globalization;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single item within an offer pack.
/// </summary>
[Serializable]
public class OfferPackCurrency : OfferPack {
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
	public OfferPackCurrency() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPackCurrency() {

	}

	/// <summary>
	/// Clear the item.
	/// </summary>
	public void Clear() {

	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
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

		// Create a single item which will be the purchased currency
		OfferPackItem item = new OfferPackItem();
		HDTrackingManager.EEconomyGroup ecoGroup = HDTrackingManager.EEconomyGroup.SHOP_OFFER_PACK;
		switch(m_type) {
			case Type.SC:	ecoGroup = HDTrackingManager.EEconomyGroup.SHOP_COINS_PACK;	break;
			case Type.HC:	ecoGroup = HDTrackingManager.EEconomyGroup.SHOP_PC_PACK;	break;
		}
		item.InitAsCurrency(
			TypeToString(m_type),
			_def.GetAsLong("amount"),
			true,
			ecoGroup
		);

		// If a reward wasn't generated, the item is either not properly defined or the pack doesn't have this item, don't store it
		if(item.reward != null) {
			m_items.Add(item);
		}

		// Currency packs are always active
		ChangeState(State.ACTIVE);
	}

	/// <summary>
	/// Update loop. Should be called periodically from the manager.
	/// Will look for pack's state changes.
	/// </summary>
	/// <returns>Whether the pack has change its state.</returns>
	public override bool UpdateState() {
		// Currency packs are always active
		return false;
	}

	/// <summary>
	/// In the particular case of the offers, we only need to persist them in specific cases.
	/// </summary>
	/// <returns>Whether the offer should be persisted or not.</returns>
	public override bool ShouldBePersisted() {
		// Currency packs should never be persisted
		return false;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}