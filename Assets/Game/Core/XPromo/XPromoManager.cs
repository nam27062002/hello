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
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class takes care of all the stuff related with the xPromo rewards system.
/// Manages the send of deeplinks related with local rewards and the recepction of incoming rewards from HSE.
/// </summary>
[Serializable]
public class XPromoManager: Singleton<XPromoManager> {
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

    // List of collected incoming rewards. So we dont give them twice.
	private List<String> m_incomingRewardsCollected;

	// HSE dynamic short links
	private Dictionary<string, string> m_dynamicShortLinks;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor. This method is called in the game initalization, so keep it light.
	/// </summary>
	public XPromoManager() {

		// Subscribe to XPromo broadcast
		Messenger.AddListener<Dictionary<string,string>>(MessengerEvents.INCOMING_DEEPLINK_NOTIFICATION, OnDeepLinkNotification);

		m_pendingIncomingRewards = new Queue<Metagame.Reward>();

		m_incomingRewardsCollected = new List<string>();

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

		// Load the dynamic shortlinks
		m_dynamicShortLinks = LoadShortLinksFromAsset( );

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

    public void Clear()
    {
		m_pendingIncomingRewards.Clear();

		m_incomingRewardsCollected.Clear();

		// Reset the xpromo cycle
		xPromoCycle.Clear();
	}


	/// <summary>
	/// Send rewards to the promoted external app (HSE) via deeplink.
	/// This is the reciprocal counterpart of ProcessIncomingRewards()
	/// </summary>
	/// <param name="rewardsId"></param>
	public void SendRewardToHSE(LocalRewardHSE _reward)
    {

        // Make some safety checks 
        if (!m_dynamicShortLinks.ContainsKey(_reward.rewardSku))
        {
            // The requested reward shortlink is not defined
			Debug.LogError("There is no HSE shortlink defined for the reward with sku=" + _reward.rewardSku);
			return;
        }

		string url = m_dynamicShortLinks[_reward.rewardSku];

        if (string.IsNullOrEmpty(url))
        {
			// The url was left empty in the scriptable object
			Debug.LogError("The HSE shortlink url is empty.");
			return;
		}


		// Send the reward with id = _reward.rewardSku
		Log("Reward with SKU='" + _reward.rewardSku + "' sent to HSE");

		// All good! open the HSE app
		Application.OpenURL(url);

    }
	

    /// <summary>
    /// Load all the HSE reward short links that are stored in a scriptable object
    /// it will ignore the rewards not belonging to the player's AB group
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> LoadShortLinksFromAsset ()
    {

        // Load the scriptable object containing all the links
		XPromoDynamicLinksCollection scriptableObj = Resources.Load<XPromoDynamicLinksCollection>("XPromo/XPromoDynamicLinks");
        List< XPromoDynamicLinksCollection.XPromoRewardShortLink> links = scriptableObj.xPromoShortLinks;

		Dictionary<string, string> result = new Dictionary<string, string>();

        // Interate all the shortlinks defined
        foreach (XPromoDynamicLinksCollection.XPromoRewardShortLink rewardLink in links)
        {

			// If the player is not in this AB group, ignore this entry
			if (rewardLink.abGroup.ToString().ToLower() == m_xPromoCycle.aBGroup.ToString().ToLower())
            {
                // Use the reward sku as key in the links dictionary
				result.Add(rewardLink.rewardSKU, rewardLink.url);
            }
			
        }

		return result;

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
		Metagame.Reward reward = CreateRewardFromDef(rewardDef);

		// check that this reward has not been already collected
		if (m_incomingRewardsCollected.Contains(reward.sku))
		{
            // Nice try ¬¬
			return;
        }

		// Put this reward in the rewards queue
		// This rewards will be given when the user enters the selection screen
		if (reward != null)
			m_pendingIncomingRewards.Enqueue(reward);


		// At this poing treat the reward as collected
		m_incomingRewardsCollected.Add(reward.sku);

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
		xPromoCycle.InitFromDefinitions();
	}


    /// <summary>
    /// Reset all the rewards progression. Used for debugging purposes.
    /// </summary>
    private void ResetProgression()
    {

		// Forget all the incoming rewards received
		m_pendingIncomingRewards.Clear();

		// Go back to the first reward
		m_xPromoCycle.totalNextRewardIdx = 0;

		// Reset the timestamp, so the first reward can be collected.
		m_xPromoCycle.nextRewardTimestamp = DateTime.MinValue;

	}

	//------------------------------------------------------------------------//
	// STATIC   															  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Creates a new Metagame.Reward initialized with the data in the given Definition from rewards table.
	/// </summary>
	/// <param name="_def">Definition from xPromo rewards table.</param>
	/// <param name="_origin">The app that granted the reward</param>
	/// <returns>New reward created from the given definition.</returns>
	private static Metagame.Reward CreateRewardFromDef(DefinitionNode _def)
	{

		Metagame.Reward.Data rewardData = new Metagame.Reward.Data();
		rewardData.typeCode = _def.GetAsString("type");
		rewardData.amount = _def.GetAsLong("amount");
		rewardData.sku = _def.GetAsString("rewardSku");

		// Assign an economy group based on the xpromo reward origin
		HDTrackingManager.EEconomyGroup economyGroup;

		Game origin = XPromoManager.GameStringToEnum(_def.GetAsString("origin"));

		if (origin == Game.HD)
		{
			economyGroup = HDTrackingManager.EEconomyGroup.REWARD_XPROMO_LOCAL;
		}
		else
		{
			economyGroup = HDTrackingManager.EEconomyGroup.REWARD_XPROMO_INCOMING;
		}

		// Construct the reward
		return Metagame.Reward.CreateFromData(rewardData, economyGroup, DEFAULT_SOURCE);


	}

	/// <summary>
	/// String to enum conversion for Game code
	/// </summary>
	/// <param name="_input"></param>
	/// <returns></returns>
	public static Game GameStringToEnum (string _input)
    {
        switch (_input)
        {

			case GAME_CODE_HD: return Game.HD; 
			case GAME_CODE_HSE: return Game.HSE; 
			default: return Game.UNDEFINED;
                
        }
    }

	//------------------------------------------------------------------------//
	// PERSISTENCE METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Constructor from json data.
	/// </summary>
	/// <param name="_data">Data to be parsed.</param>
	public void LoadData(SimpleJSON.JSONNode _data)
	{

		// Reset any existing data
		Clear();

		// Load collected incoming rewards
        string key = "xPromoCollectedRewards";
        if ( _data.ContainsKey(key) ) {

			// Rewards skus are stored as an array 
			JSONArray arrayData = _data[key].AsArray;
			for (int j = 0; j < arrayData.Count; ++j)
			{
				m_incomingRewardsCollected.Add(arrayData[j]);
			}
		
		}


		// Load XPromo Cycle specifics:
		// Current reward index
		if (_data.ContainsKey("xPromoNextRewardIdx"))
		{
			m_xPromoCycle.totalNextRewardIdx = PersistenceUtils.SafeParse<int>(_data["xPromoNextRewardIdx"]);
		}
        
		// Collect timestamp
		if (_data.ContainsKey("xPromoNextRewardTimestamp"))
		{
			m_xPromoCycle.nextRewardTimestamp = PersistenceUtils.SafeParse<DateTime>(_data["xPromoNextRewardTimestamp"]);
		}

	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json. Can be null if sequence has never been generated.</returns>
	public SimpleJSON.JSONClass SaveData()
	{

		// Create a new json data object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Save collected incoming rewards skus
		JSONArray arrayData = new JSONArray();
		foreach (string rewardSku in m_incomingRewardsCollected)
		{
			arrayData.Add(rewardSku);
		}
        data.Add("xPromoCollectedRewards", arrayData);


		// Save XPromo Cycle specifics:
		if (xPromoCycle != null)

			// Current reward index
			data.Add("xPromoNextRewardIdx", PersistenceUtils.SafeToString(xPromoCycle.totalNextRewardIdx));

			// Collect timestamp
			data.Add("xPromoNextRewardTimestamp", PersistenceUtils.SafeToString(xPromoCycle.nextRewardTimestamp));


		// Done!
		return data;
	}


	//------------------------------------------------------------------------//
	// DEBUG CP 															  //
	//------------------------------------------------------------------------//


	public void OnResetProgression()
    {

		ResetProgression();
        
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