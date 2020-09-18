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

	// Incoming rewards received from HSE via deep link waiting to be processed
	// Will be given to the player when we have a chance in the selection screen.
	private Queue<String> m_incomingRewardsToProcess;


	// List of already collected incoming rewards. So we dont give them twice.
	private List<String> m_collectedIncomingRewards;


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



		m_incomingRewardsToProcess = new Queue<string>();

		m_collectedIncomingRewards = new List<string>();

		// Debug
		//m_incomingRewardsToProcess.Enqueue( "reward_hse_hd_1a" );

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

		m_collectedIncomingRewards.Clear();

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
        if (!m_dynamicShortLinks.ContainsKey(_reward.sku))
        {
            // The requested reward shortlink is not defined
			Debug.LogError("There is no HSE shortlink defined for the reward with sku=" + _reward.sku);
			return;
        }

		string url = m_dynamicShortLinks[_reward.sku];

        if (string.IsNullOrEmpty(url))
        {
			// The url was left empty in the scriptable object
			Debug.LogError("The HSE shortlink url is empty.");
			return;
		}


		// Send the reward with id = _reward.rewardSku
		Log("Reward with SKU='" + _reward.sku + "' sent to HSE");
		Log("Opening URL " + url );

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
    /// Returns true if there is valid incoming reward waiting to be collected
    /// </summary>
    public bool IsIncomingRewardWaiting()
    {
		// First of all process the incoming rewards queue to discard invalid/already collected SKUs
		DiscardInvalidIncomingRewards();

		return m_incomingRewardsToProcess.Count > 0;
    }


	/// <summary>
	/// Checks if there is any incoming reward waiting to be processed. Clean all the non valid/already collected
    /// rewards in the incoming rewards queue.
	/// </summary>

	public void DiscardInvalidIncomingRewards()
	{
		// No incoming rewards waiting
		if (m_incomingRewardsToProcess.Count == 0)
			return;


		// Create a temp queue to keep the valid incoming rewards
		Queue<string> validRewards = new Queue<string>();


		while (m_incomingRewardsToProcess.Count > 0)
		{
            // Take the first SKU in the queue
            string sku = m_incomingRewardsToProcess.Dequeue();

			// This reward was already collected. Nice try
			if (m_collectedIncomingRewards.Contains(sku))
			{
                // Continue with the next reward
				continue;
			}

			// Find this reward in the content. Note that ABGroup param is not affecting the receiving end.
			DefinitionNode rewardDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.XPROMO_REWARDS, "sku", sku);

			// Just in case the content is wrong. Shouldnt happen.
			Debug.Assert(rewardDef.GetAsString("origin") == GAME_CODE_HSE, "This sku doesnt belong to an incoming reward!");

			if (rewardDef == null)
			{
				// Reward not found. 
				Debug.LogError("Incoming reward with SKU " + m_incomingRewardsToProcess + " is not defined in the content");

				// Continue with the next reward
				continue;
			}

			// Create a reward from the content definition
			Metagame.Reward reward = CreateRewardFromDef(rewardDef);

			// Put this reward in the queue
			// This rewards will be given when the user enters the selection screen
			if (reward == null)
			{
				Debug.LogError("Incoming reward with SKU " + m_incomingRewardsToProcess + " contains an invalid reward");

				// Continue with the next reward
				continue;
			}

			// All checks passed. The incoming reward is valid candidate.
			validRewards.Enqueue(sku);

		}

		// Replace the old queue with new one that contains only the valid rewards
		m_incomingRewardsToProcess = validRewards;

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

			// Store the incoming rewards SKU. Treat it later.
			m_incomingRewardsToProcess.Enqueue(_params[XPROMO_REWARD_KEY]);
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

		// Go back to the first reward
		m_xPromoCycle.totalNextRewardIdx = 0;

		// Reset the timestamp, so the first reward can be collected.
		m_xPromoCycle.nextRewardTimestamp = DateTime.MinValue;

		// Reset all the incoming rewards
		m_collectedIncomingRewards.Clear();

	}


	/// <summary>
	/// The user collected the incoming rewards, so put them all in the pending reward queue
	/// </summary>
	public void OnCollectAllIncomingRewards()
	{
		// First of all process the incoming rewards queue to discard invalid/already collected SKUs
		DiscardInvalidIncomingRewards();

		// From now no more checks are needed, as all the elements in the queue are valid

		while (m_incomingRewardsToProcess.Count > 0)
		{
    		string sku = m_incomingRewardsToProcess.Dequeue();

			// Find this reward in the content. Note that ABGroup param is not affecting the receiving end.
			DefinitionNode rewardDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.XPROMO_REWARDS, "sku", sku);

			// Create a reward from the content definition
			Metagame.Reward reward = CreateRewardFromDef(rewardDef);

            // Just in case
			if (reward != null)
			{
				// All checks passed. Put it in the pending rewards queue
				UsersManager.currentUser.PushReward(reward);
			}

			// At this point consider the reward as collected
			m_collectedIncomingRewards.Add(sku);

		}

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

	/// <summary>
	/// Is the Hungry Shark Evolution game installed?
	/// </summary>
	/// <returns>Whether Hungry Shark Evolution is installed in this device.</returns>
	public static bool IsHungrySharkGameInstalled()
	{
		bool ret = false;
#if UNITY_EDITOR
		ret = true;
#elif UNITY_ANDROID
        ret = PlatformUtils.Instance.ApplicationExists("com.fgol.HungrySharkEvolution");
#elif UNITY_IOS
		ret = PlatformUtils.Instance.ApplicationExists("hungrysharkevolution://");
#endif
		return ret;
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
				m_collectedIncomingRewards.Add(arrayData[j]);
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
		foreach (string rewardSku in m_collectedIncomingRewards)
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