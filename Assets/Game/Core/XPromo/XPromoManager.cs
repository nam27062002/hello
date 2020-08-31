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
using System.Linq;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class XPromoManager {
	//------------------------------------------------------------------------//
	// STRUCT															  //
	//------------------------------------------------------------------------//
    public enum Game {
        UNDEFINED,
        HD,
        HSE
    }

	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	public const string GAME_CODE_HD = "hd";
	public const string GAME_CODE_HSE = "hse";

	// Tracking constants
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

	// Set of local x-promo rewards as defined in content
	private List<XPromo.LocalReward> m_localRewards;

    // Queue with the rewards incoming from HSE. Will be given to the player when we have a chance (selection screen)
	private Queue<Metagame.Reward> m_pendingIncomingRewards;
    public Queue<Metagame.Reward> pendingIncomingRewards { get { return m_pendingIncomingRewards;  } }

	// Configuration
	private Boolean m_enabled;
	private DateTime m_startDate;
    private DateTime m_endDate;
	private int m_minRuns;
	private int m_cycleSize;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public XPromoManager() {

		// Subscribe to XPromo broadcast
		Messenger.AddListener(MessengerEvents.INCOMING_DEEPLINK_NOTIFICATION, OnDeepLinkNotification);

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~XPromoManager() {

		// Unsubscribe to XPromo broadcast
		Messenger.RemoveListener(MessengerEvents.INCOMING_DEEPLINK_NOTIFICATION, OnDeepLinkNotification);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

    // Empty all the sets
    public void Clear()
    {
		m_localRewards = new List<XPromo.LocalReward>();
		m_pendingIncomingRewards = new Queue<Metagame.Reward>();

		m_startDate = new DateTime();
		m_endDate = new DateTime();

	}

    /// <summary>
    /// Load all data from content tables
    /// </summary>
	public void InitFromDefinitions()
	{

        Clear();

		// Load settings
	    DefinitionNode settingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.XPROMO_SETTINGS, "xPromoSettings");
		m_enabled = settingsDef.GetAsBool("enabled");
		m_minRuns = settingsDef.GetAsInt("minRuns");
		m_cycleSize = settingsDef.GetAsInt("cycleSize");

		if (settingsDef.Has("startDate"))
		{
			m_startDate = TimeUtils.TimestampToDate(settingsDef.GetAsLong("startDate", 0), false);
		}

		if (settingsDef.Has("endDate"))
		{
			m_endDate = TimeUtils.TimestampToDate(settingsDef.GetAsLong("endDate", 0), false);
		}


		// Load local rewards
		List<DefinitionNode> localRwdDefinitions = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.LOCAL_REWARDS);
        foreach (DefinitionNode def in localRwdDefinitions)
        {
			XPromo.LocalReward localReward = XPromo.LocalReward.CreateLocalRewardFromDef(def);

			if (localReward != null)
				m_localRewards.Add(localReward);
        }

		// Check settings-rewards consistency, so the designers can know if they screwed up the content tables 
		List<XPromo.LocalReward> activeRewards = m_localRewards.Where<XPromo.LocalReward>(r => r.enabled == true).ToList();
        if (activeRewards.Count != m_cycleSize)
        {
			Debug.LogError("The number of xPromo active rewards doesn´t match the xPromo cycle length! Please, fix the content tables.");
        }


	}



	/// <summary>
	/// Check if there is any pending rewards coming from the promoted app (HSE) via deep link
	/// </summary>
	public void ProcessIncomingRewards()
	{
		// Get the rewards ids incoming in the deep link
		List<string> rewardsSku = new List<string>(); // = CaletyDynamicLinks.GetXPromoRewards();

		if (rewardsSku.Count == 0)
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
			Metagame.Reward reward = CreateRewardFromDef(rewardDef, Game.HSE);

			// Put this reward in the rewards queue
            if (reward != null)
				m_pendingIncomingRewards.Enqueue(reward);

            // This rewards will be given when the user enters the selection screen

		}


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
    /// <param name="_origin">The app that granted the reward</param>
	/// <returns>New reward created from the given definition.</returns>
	private static Metagame.Reward CreateRewardFromDef(DefinitionNode _def, Game _origin)
	{

		Metagame.Reward.Data rewardData = new Metagame.Reward.Data();
		rewardData.typeCode = _def.GetAsString("type");
		rewardData.amount = _def.GetAsLong("amount");
		rewardData.sku = _def.GetAsString("rewardSku");

		// Assign an economy group based on the xpromo reward origin
		HDTrackingManager.EEconomyGroup economyGroup;

        if (_origin == Game.HD) {
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