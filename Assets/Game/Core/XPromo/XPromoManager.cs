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
using XPromo;
using FirebaseWrapper;

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

	// Deeplink params
	public const string XPROMO_REWARD_KEY = "rewardSku";

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

	// X-promo daily rewards cycle
	private XPromoCycle m_xPromoCycle;
	public XPromoCycle xPromoCycle
	{
		get
		{
            // Use m_xPromoCycle as initialization flag
			if (m_xPromoCycle == null)
			{
                // Initialize the manager
				Init();
			}

			return m_xPromoCycle;
		}
	}
    
	// Queue with the rewards incoming from HSE. Will be given to the player when we have a chance (selection screen)
	private Queue<Metagame.Reward> m_pendingIncomingRewards;
    public Queue<Metagame.Reward> pendingIncomingRewards { get { return m_pendingIncomingRewards;  } }

	

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public XPromoManager() {

		// Subscribe to XPromo broadcast
		Messenger.AddListener<Dictionary<string,string>>(MessengerEvents.INCOMING_DEEPLINK_NOTIFICATION, OnDeepLinkNotification);

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~XPromoManager() {

		// Unsubscribe to XPromo broadcast
		Messenger.RemoveListener<Dictionary<string, string>>(MessengerEvents.INCOMING_DEEPLINK_NOTIFICATION, OnDeepLinkNotification);
	}

    /// <summary>
    /// Initializes the XPromo manager from the content
    /// </summary>
    public void Init()
    {

		// Create a new cycle from the content data
		m_xPromoCycle = XPromoCycle.CreateXPromoCycleFromDefinitions();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

    public void Clear()
    {
		m_pendingIncomingRewards = new Queue<Metagame.Reward>();

        // Reset the xpromo cycle
		m_xPromoCycle.Clear();
	}



	/// <summary>
	/// Process the incoming reward from HSE
	/// </summary>
    /// <param name="_rewardSku">SKU of the incoming reward as defined in content</param>
	public void ProcessIncomingReward(string _rewardSku)
	{

		if (string.IsNullOrEmpty(_rewardSku)) 
			return; // No rewards

		// Find this reward in the content. We trust the SKU so we dont check ABGroup or origin.
		DefinitionNode rewardDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.XPROMO_REWARDS, "sku", _rewardSku);
       

        if (rewardDef == null)
        {
			// Reward not found. Process next reward (if any)
			Debug.LogError("Incoming reward with SKU " + _rewardSku + " is not defined in the content");
	    }

        // Create a reward from the content definition
		Metagame.Reward reward = CreateRewardFromDef(rewardDef, Game.HSE);

		// Put this reward in the rewards queue
        if (reward != null)
			m_pendingIncomingRewards.Enqueue(reward);

        // This rewards will be given when the user enters the selection screen
        

	}


	/// <summary>
	/// Send rewards to the promoted external app (HSE) via deeplink.
	/// This is the reciprocal counterpart of ProcessIncomingRewards()
	/// </summary>
	/// <param name="rewardsId"></param>
	public void SendRewardToHSE(LocalRewardHSE _reward)
    {
		

		// Send the reward with id = _reward.rewardSku
		Log("Reward with SKU='" + _reward.rewardSku + "' sent to HSE");



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
	public void OnDeepLinkNotification(Dictionary<string,string> _params)
	{

		// Check if this deep link notification contains a XPromo reward
        if (_params.ContainsKey(XPROMO_REWARD_KEY))
        {

			// Process the incoming rewards
			ProcessIncomingReward(_params[XPROMO_REWARD_KEY]);
		}


	}

    /// <summary>
    /// The content was updated (probably via customizer)
    /// </summary>
    public void OnContentUpdate()
    {
		// Update the rewards and cycle settings
		m_xPromoCycle.InitFromDefinitions();
	}

   


	//------------------------------------------------------------------------//
	// DEBUG CP 															  //
	//------------------------------------------------------------------------//


	public void OnResetProgression()
    {
		m_xPromoCycle.ResetProgression();

        //TODO: reset the incoming rewards collected from the user persistence
    }

    public void OnMoveIndexTo(int _newIndex)
    {
        m_xPromoCycle.totalNextRewardIdx = _newIndex;
    }

    public void OnSkipTimer()
    {
		m_xPromoCycle.nextRewardTimestamp = GameServerManager.GetEstimatedServerTime();
	}

	//------------------------------------------------------------------------//
	// DEBUG LOG															  //
	//------------------------------------------------------------------------//

	#region log
	private const string LOG_CHANNEL = "[XPromo]";
	public static void Log(string message)
	{
		ControlPanel.Log(LOG_CHANNEL + message, ControlPanel.ELogChannel.XPromo);
	}
	#endregion

}