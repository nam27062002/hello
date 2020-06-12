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

    /// <summary>
    /// Returns an array with the friends required for each milestone
    /// </summary>
    /// <returns></returns>
    public int[] GetFriendsRequired()
    {
		int[] friends = new int[items.Count];

		int i = 0;
        foreach (OfferPackItem it in items)
        {
			friends[i] = ((OfferPackReferralReward)it).friendsRequired;
			i++;
        }

		return friends;
    }

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