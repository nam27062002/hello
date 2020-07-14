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

	// Cache
	private int[] m_milestones;

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
	/// Get the index of next reward in the progression (or already ready to claim)
	/// </summary>
	/// <param name="_totalReferrals">The amount of referrals</param>
	/// <returns>The next offer pack to claim</returns>
	public OfferPackReferralReward GetNextReward(int _totalReferrals)
	{
        // Check if there is a pending to claim reward
        if (UsersManager.currentUser.unlockedReferralRewards.Count > 0)
        {

            // Pending rewards always are the next reward despite of the amount of referrals 
			return UsersManager.currentUser.unlockedReferralRewards[0];

		}

		int maxFriendsAmount = ((OfferPackReferralReward)items[items.Count - 1]).friendsRequired;
		int referralsCapped = _totalReferrals;

		// Module operation in case the amount of friends has surpassed the last milestone
		if (_totalReferrals > maxFriendsAmount)
			referralsCapped = (_totalReferrals - 1) % maxFriendsAmount + 1;


		// Find the next reward in the progression
		int i = 0;
		while (i < m_milestones.Length - 1)
		{
			if (m_milestones[i] > referralsCapped)
			{
				return (OfferPackReferralReward) items[i];
			}
			i++;
		}
		return (OfferPackReferralReward) items[i];

	}

	/// <summary>
	/// Get the index of a given reward
	/// </summary>
    ///<param name="_reward">A reward belonging to this offer pack</param>
    ///<returns>The index of this reward in the pack. Returns 0 if is not found.</returns>
	public int GetRewardIndex(OfferPackReferralReward _reward)
	{
		for (int i=0; i<items.Count; i++)
        {
            if (items[i] == _reward )
            {
				return i;
            }
        }

		return 0;

	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDE														  //
	//------------------------------------------------------------------------//

	public override void InitFromDefinition(DefinitionNode _def)
    {
        base.InitFromDefinition(_def);

		// Sort the items based on the friends required
		m_items.Sort(OfferPackReferralReward.Compare);

        // Cache the milestones
		m_milestones = GetFriendsRequired();

	}

}