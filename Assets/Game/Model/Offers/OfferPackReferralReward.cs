// OfferPackReferralReward.cs
// Hungry Dragon
// 
// Created by  on 09/06/2020.
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
public class OfferPackReferralReward:OfferPackItem {

    //------------------------------------------------------------------------//
    // CONSTANTS    														  //
    //------------------------------------------------------------------------//

    public static string TYPE_REFERRAL = "referral";


	//------------------------------------------------------------------------//
	// ENUM         														  //
	//------------------------------------------------------------------------//
	public enum State
    {
        NOT_AVAILABLE,
        READY_TO_CLAIM,
        CLAIMED
    }

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	private int m_friendsRequired;
	public int friendsRequired
    { get => m_friendsRequired; set => m_friendsRequired = value; }

	// The SKU of the referral reward (not the item)
	private string m_referralRewardSku;
	public string referralRewardSku
	{ get => m_referralRewardSku; set => m_referralRewardSku = value;  }

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public OfferPackReferralReward() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPackReferralReward() {

	}


    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

	/// <summary>
	/// Initialize from referral offer pack definition.
	/// </summary>
	/// <param name="_def">Definition node of the offer pack related.</param>
	/// <param name="_itemIdx">Index of the item within the pack (1..N)</param>
	/// <param name="_ecoGroup">Group ID used for tracking when the reward is collected.</param>
	public override void InitFromDefinition(DefinitionNode _def, int _itemIdx, HDTrackingManager.EEconomyGroup _ecoGroup) {
		// Aux vars
		string prefix = GetPrefix(_itemIdx);

		// Check definition
		if(_def == null) {
			Clear();
			return;
		}


		// If unknown type, clear data and return. Make sure the type is referral, otherwise get out.
		m_type = _def.GetAsString(prefix + "Type", OffersManager.settings.emptyValue);
        if( m_type == OffersManager.settings.emptyValue ||
            string.IsNullOrEmpty(m_type) || m_type != TYPE_REFERRAL )
        {
			Clear();
			return;
		}

		// Item Sku. If empty, clear data and return
		m_sku = _def.GetAsString(prefix + "Sku");
        if( m_sku == OffersManager.settings.emptyValue ||
            string.IsNullOrEmpty(m_sku) )
        {
			Clear();
			return;
		}

		// Ignore amount

		// We use the SKU to find the real reward in the referralRewards table
		DefinitionNode referralReward = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.REFERRAL_REWARDS, sku);

		// If the reward doesnt exist. Clear and return
		if (referralReward == null)
		{
			Clear();
			return;
		}

        // Init the reward
		InitFromRewardDefinition(referralReward, _ecoGroup);

	}


	/// <summary>
	/// Init the reward object from the reward definition
	/// </summary>
	/// <param name="_def">The referral reward definition</param>
	/// <param name="_ecoGroup">Group ID used for tracking when the reward is collected.</param>
	public void InitFromRewardDefinition (DefinitionNode _def, HDTrackingManager.EEconomyGroup _ecoGroup = HDTrackingManager.EEconomyGroup.UNKNOWN)
    {

		// Initialize the referral reward

		// Friends required
		m_friendsRequired = _def.GetAsInt("friends");

		// If unknown type, clear data and return
		m_type = _def.GetAsString("itemType", OffersManager.settings.emptyValue);
		if (m_type == OffersManager.settings.emptyValue || string.IsNullOrEmpty(m_type))
		{
			Clear();
			return;
		}

        // Referral Reward SKU
		m_referralRewardSku = _def.GetAsString("sku");

		// Item Sku
		m_sku = _def.GetAsString("itemSku");

		// Amount
		long amount = _def.GetAsLong("itemAmount", 1);

		Metagame.Reward.Data rewardData = new Metagame.Reward.Data();
		rewardData.typeCode = m_type;
		rewardData.sku = m_sku;
		rewardData.amount = amount;
		m_reward = Metagame.Reward.CreateFromData(
			rewardData,
			_ecoGroup,
			""
		);
	}

	//------------------------------------------------------------------------//
	// STATIC   															  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Function used to sort referral rewards based on the friends required
	/// </summary>
	/// <returns>The result of the comparison (-1, 0, 1).</returns>
	/// <param name="_item1">First item to compare.</param>
	/// <param name="_item2">Second item to compare.</param>
	public static int Compare(OfferPackReferralReward _item1, OfferPackReferralReward _item2)
	{
		// Depends on type
		return _item1.friendsRequired.CompareTo(_item2.friendsRequired);
	}
}