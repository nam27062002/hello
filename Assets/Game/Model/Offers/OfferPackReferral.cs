// OfferPackReference.cs
// Hungry Dragon
// 
// Created by JOM on 09/06/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class OfferPackReferral: OfferPack {
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
	public OfferPackReferral() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPackReferral() {

	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // PARENT OVERRIDE														  //
    //------------------------------------------------------------------------//

    public override void InitFromDefinition(DefinitionNode _def)
    {
        base.InitFromDefinition(_def);

		// Sort the items based on the friends required
		m_items.Sort(OfferPackReferralReward.Compare);
    }

}