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


	/// <summary>
	/// Get the index of the current or next reward defined in the referral rewards progression
	/// </summary>
	/// <param name="_totalReferrals">The amount of referrals</param>
	/// <returns>The index of the next/current reward in this pack</returns>
	public int GetNextRewardIndex(int _totalReferrals)
	{
		int maxFriendsAmount = ((OfferPackReferralReward)items[items.Count - 1]).friendsRequired;
		int referralsCapped = _totalReferrals;

		// Module operation in case the amount of friends has surpassed the last milestone
		if (_totalReferrals > maxFriendsAmount)
			referralsCapped = (_totalReferrals - 1) % maxFriendsAmount + 1;

		int[] milestones = GetFriendsRequired();

		int i = 0;

        // Find the current (or next) reward in the progression
        while (i < milestones.Length - 1)
        {
            if (milestones[i] >= referralsCapped)
            {
				return i;
            }
			i++;
        }
		return i;

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