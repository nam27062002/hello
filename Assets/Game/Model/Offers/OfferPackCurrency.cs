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
	// OTHER METHODS														  //
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

        // Reset to default values
        Reset();

        // Store def
        m_def = _def;

        // Category
        m_shopCategory = m_def.GetAsString("shopCategory");

        // Order
        m_order = m_def.GetAsInt("order");

        // Offer Type
        m_type = StringToType(m_def.GetAsString("type"));

    }


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}