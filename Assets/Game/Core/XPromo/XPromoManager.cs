// XPromoManager.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 26/08/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class XPromoManager {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Tracking constants
	private const HDTrackingManager.EEconomyGroup ECONOMY_GROUP = HDTrackingManager.EEconomyGroup.XPROMO;
	private const string DEFAULT_SOURCE = "";


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	// Singleton instance
	private static XPromoManager m_instance = null;
	public static XPromoManager instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new XPromoManager();
			}
			return m_instance;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public XPromoManager() {

		// Subscribe to XPromo broadcast
		Messenger.AddListener(MessengerEvents.DEEPLINK_NOTIFICATION, OnDeepLinkNotification);

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~XPromoManager() {

		// Unsubscribe to XPromo broadcast
		Messenger.RemoveListener(MessengerEvents.DEEPLINK_NOTIFICATION, OnDeepLinkNotification);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//



	/// <summary>
	/// Check if there is any pending rewards coming from the promoted app via deep link
	/// </summary>
	public void ProcessIncomingRewards()
	{
        // Get the rewards ids incoming in the deep link
		string[] rewardsSku = CaletyDynamicLinks.GetXPromoRewards();

		if (rewardsSku.Length == 0)
			return; // No rewards

        // Process all incoming rewards
        foreach (string rewardSku in rewardsSku)
        {

			// Find this reward in the content
			DefinitionNode rewardDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.INCOMING_REWARDS, "sku", rewardSku);

            if (rewardDef == null)
            {
				// Reward not found. Process next reward (if any)
				Debug.LogError("Incoming reward with SKU " + rewardSku + " is not defined in the content");
				continue;
            }

            // Create a reward from the content definition
			Metagame.Reward reward = CreateRewardFromDef(rewardDef, false);

			// Put this reward in the rewards queue
            if (reward != null)
			    UsersManager.currentUser.PushReward(reward);

		}

		// Shows a different reward popup depending if this is the
		// first time the player opens the game (welcome popup)
		// or if the game was already being played by this user (reward popup).


	}


	/// <summary>
	/// Send rewards to the promoted external app (HSE).
	/// This is the reciprocal counterpart of ProcessIncomingRewards()
	/// </summary>
	/// <param name="rewardsId"></param>
	public void SendRewards(string [] rewardsId)
    {

    }

	/// <summary>
	/// Creates a new Metagame.Reward initialized with the data in the given Definition from rewards table.
	/// </summary>
	/// <param name="_def">Definition from localRewards or incomingRewards table.</param>
    /// <param name="_localReward">True if the reward origin is HD, false if the origin is HSE</param>
	/// <returns>New reward created from the given definition.</returns>
	private static Metagame.Reward CreateRewardFromDef(DefinitionNode _def, bool _localReward)
	{

		Metagame.Reward.Data rewardData = new Metagame.Reward.Data();
		rewardData.typeCode = _def.GetAsString("type");
		rewardData.amount = _def.GetAsLong("amount");
		rewardData.sku = _def.GetAsString("rewardSku");

		// Assign an economy group based on the xpromo reward origin
		HDTrackingManager.EEconomyGroup economyGroup;

        if (_localReward) {
			economyGroup = HDTrackingManager.EEconomyGroup.REWARD_XPROMO_LOCAL;
		} else { 
			economyGroup = HDTrackingManager.EEconomyGroup.REWARD_XPROMO_INCOMING;
		}

        // Construct the reward
		return Metagame.Reward.CreateFromData(rewardData, economyGroup, DEFAULT_SOURCE);
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// A new deeplink notification was registered
	/// </summary>
	public void OnDeepLinkNotification()
	{

		// Check if this deep link notification contains a XPromo reward

        // Process the incoming rewards
		ProcessIncomingRewards();
	}

}