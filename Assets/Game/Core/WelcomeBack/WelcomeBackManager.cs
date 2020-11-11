// WelcomeBackManager.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 22/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// The Welcome Back feature triggers a bunch of perks for the players that are coming back
/// after some time without playing the game. This class will manage all the logic related to
/// triggering this benefits.
/// </summary>
[Serializable]
public class WelcomeBackManager : Singleton<WelcomeBackManager>
{
	//------------------------------------------------------------------------//
	// ENUM        															  //
	//------------------------------------------------------------------------//

    
    //------------------------------------------------------------------------//
    // CONST       															  //
    //------------------------------------------------------------------------//

    // Perks
    public const string ENABLE_POPUP = "enablePopup"; 
    public const string ENABLE_HAPPY_HOUR = "enableHappyHour";
    public const string ENABLE_OFFER = "enableOffer";
    public const string ENABLE_BOOSTED_DAILY_LOGIN = "enableBoostedDailyLogin";
    public const string ENABLE_PASSIVE = "enablePassive";
    public const string ENABLE_SOLO_QUEST = "enableSoloQuest";
    public const string ENABLE_TOURNAMENT_PASS = "enableTournamentPass";

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    private DateTime m_lastActivationTime;
    public DateTime lastActivationTime
    {
        get => m_lastActivationTime;
    }


    private bool m_isPopupWaiting = false;
    public bool isPopupWaiting
    {
        get => m_isPopupWaiting;
        set => m_isPopupWaiting = value;
    }


    // WB configuration
	private bool enabled = true;


    // Keep the definition at hand
	private DefinitionNode m_def;
    public DefinitionNode def => m_def;

    private DefinitionNode m_perksDef;
    public DefinitionNode perksDef => m_perksDef;

    private List<DefinitionNode> m_perksDefs;

    

    // Free tournament
    private DateTime m_tournamentPassExpirationTimestamp;
    
    // Boosted daily reward
    private BoostedDailyRewardsSequence m_boostedDailyRewards;
    public BoostedDailyRewardsSequence boostedDailyRewards
    {
        get => m_boostedDailyRewards;
    }
    
    // Special offers
    private bool m_hasSpecialOffer = false;
    public bool hasSpecialOffer => m_hasSpecialOffer;


    // The multiplier applied to the reward if watching an ad
    public int boostedDailyRewardAdMultiplier
    {
        get
        {
            return m_def.GetAsInt("boostedDailyRewardsAdMultiplier");
        }
    }

    public bool hasBeenActivated
    {
        get => (m_lastActivationTime != null && m_lastActivationTime > DateTime.MinValue);
    }



    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public WelcomeBackManager()
	{

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~WelcomeBackManager()
	{

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Load all the configuration from the content
	/// </summary>
	public void InitFromDefinitions()
	{
		 
		// Read the settings from content
        List<DefinitionNode> defs =
            DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.WELCOME_BACK, 
                "enabled", "true");

        if (defs.Count > 0)
        {
            // Take the first enabled ocurrence
            m_def = defs[0];
        }

        // Read the perks definitions
        m_perksDefs =
            DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.WELCOME_BACK_PERKS,
            "enabled", "true");

    }


    /// <summary>
    /// Try to activate WB in the client. Takes the time this WB was activated in the server and compares it
    /// with the last time it was activated in the client. If the times differ, activates
    /// the feature in the client. If the time is same, it means that we are receiveing data from an already
    /// activated WB, so we can ignore it.
    /// </summary>
    /// <param name="_activationTime">Time when the WB was activated in the server</param>
    /// <returns>True if the WB has been succesfully activated in the client</returns>
    public bool TryActivation ( DateTime _activationTime )
    {

        if (m_lastActivationTime == _activationTime)
        {
            // The welcome back sent by the server is already active in the client. Do nothing
            return false;
          
        } else
        {
            // Otherwise the WB has been properly triggered
            Activate();
            m_lastActivationTime = _activationTime;

            return true;
        }

    }



    /// <summary>
	/// The welcome back feature becomes active. Enables all the benefits depending on the player profile.
	/// </summary>
    public void Activate()
    {

        m_perksDef = FindPerksForThisPlayer();

        if (m_perksDef == null)
        {
            // No configuration find for this player
            return;
        }


        // Check all the perks one by one:

        if (perksDef.GetAsBool(ENABLE_POPUP))
            EnablePopup();  

        if (perksDef.GetAsBool(ENABLE_SOLO_QUEST))
            CreateSoloQuest();       

        if (perksDef.GetAsBool(ENABLE_PASSIVE))
            ActivatePassiveEvent();

        if (perksDef.GetAsBool(ENABLE_TOURNAMENT_PASS))
            ActivateTournamentPass();

        if (perksDef.GetAsBool(ENABLE_HAPPY_HOUR))
            ActivateHappyHour();

        if (perksDef.GetAsBool(ENABLE_BOOSTED_DAILY_LOGIN))
            CreateBoostedSevenDayLogin();

        if (perksDef.GetAsBool(ENABLE_OFFER))
            EnableSpecialOffer();


        Log("Welcome back succesfully activated");

    }

    /// <summary>
    /// End all the welcome back perks
    /// We use it for testing purposes
    /// </summary>
    public void Deactivate()
    {
	    EndSoloQuest();
        EndPassiveEvent();
        EndTournamentPass();
        EndBoostedSevenDayLogin();
        EndHappyHour();
        DisableSpecialOffer();
        DisablePopup();

        m_lastActivationTime = DateTime.MinValue;

    }


    /// <summary>
    /// Check if the tournament pass is active. Notice that the entrance fee could be free or not.
    /// </summary>
    /// <returns>True if active</returns>
    public bool IsTournamentPassActive()
    {
        return m_tournamentPassExpirationTimestamp > GameServerManager.GetEstimatedServerTime();
    }

    
    /// <summary>
    /// Check if the tournament entrance fee is really free, or just cheaper
    /// </summary>
    /// <returns></returns>
    public bool IsTournamentPassFree()
    {
        return GetTournamentPassPrice() == 0;
    }

    /// <summary>
    /// Returns the price of the "free" pass for tournaments. 
    /// </summary>
    /// <returns>Returns zero if free</returns>
    public int GetTournamentPassPrice()
    {
        return m_def.GetAsInt("tournamentPassPrice");
    }

    
    /// <summary>
    /// The currency type defined in the content
    /// </summary>
    /// <returns>Returns the type of currency of the tournament pass</returns>
    public UserProfile.Currency GetTournamentPassCurrency()
    {
        // This sku could be "-" in case of a free pass
        string sku = m_def.GetAsString("tournamentPassCurrency");
        UserProfile.Currency currency = UserProfile.SkuToCurrency(sku);

        return currency;
    }

    /// <summary>
    /// Check if the boosted daily reward is active.
    /// </summary>
    /// <returns></returns>
    public bool IsBoostedDailyRewardActive()
    {
        if (m_boostedDailyRewards == null)
        {
            // have the boosted daily reward been initialized? 
            return false;
        }

        bool finalRewardCollected = m_boostedDailyRewards.totalRewardIdx >= m_boostedDailyRewards.rewards.Length;
        if (finalRewardCollected)
        {
            // If the 7 rewards have been collected, the boosted daily login is not longer active
            return false;
        }

        return true;
    }
    
    
    /// <summary>
    /// Find the perks associated to this player based on his cluster, and on the dragons he owns
    /// </summary>
    private DefinitionNode FindPerksForThisPlayer()
    {
        if (m_perksDefs.Count == 0)
        {
            Debug.LogError("The perks are not defined in the content!");
            return null;
        }

        // Cluster is needed to get the proper perks
        string cluster = ClusteringManager.Instance.GetClusterId();


        foreach (DefinitionNode candidate in m_perksDefs)
        {

            //Check clusters
            List<string> clusters = candidate.GetAsList<string>("cluster");

            if ( ! clusters.Contains(cluster) )
            {
                // Doesnt belong to this group. Try next
                continue;
            }


            // Check dragons ownership
            string ownedDragon = candidate.GetAsString("ownedDragon");
            string notOwnedDragon = candidate.GetAsString("notOwnedDragon");

            // In case the designer used the generick sku "last_dragon_progression", replace it with the proper dragon
            ownedDragon = ownedDragon.Replace(OfferPack.DRAGON_LAST_PROGRESSION, DragonManager.lastClassicDragon.sku);
            notOwnedDragon = notOwnedDragon.Replace(OfferPack.DRAGON_LAST_PROGRESSION, DragonManager.lastClassicDragon.sku);

            if (! string.IsNullOrEmpty(ownedDragon) && ! DragonManager.IsDragonOwned(ownedDragon))
            {
                // Doesnt own the required dragon. Try next
                continue;
            }

            if (!string.IsNullOrEmpty(notOwnedDragon) && DragonManager.IsDragonOwned(notOwnedDragon))
            {
                // Owns the forbidden dragon. Try next
                continue;
            }


            // At this point we have a candidate! Stop searching
            return candidate;

        }

        // We finish without finding a candidate. It shouldnt happen. Designers should fix the content!
        Debug.LogError("There is no perks definition suitable for the current player! He doesnt belong to any group!");
        return null;

    }


    /// <summary>
    /// Initialize the solo quest perk
    /// </summary>
    private void CreateSoloQuest()
    {
	    // Initialize the solo quest
		HDLiveDataManager.instance.soloQuest.StartQuest();
    }

    /// <summary>
    /// Finalize the solo quest perk
    /// </summary>
    private void EndSoloQuest()
    {
	    HDLiveDataManager.instance.soloQuest.DestroyQuest();
    }

    /// <summary>
    /// Enable the passive event perk
    /// </summary>
	private void ActivatePassiveEvent()
    {
        HDLiveDataManager.instance.localPassive.StartPassiveEvent();
    }

    /// <summary>
    /// Finalize the passive event perk
    /// </summary>
    private void EndPassiveEvent()
    {
        HDLiveDataManager.instance.localPassive.DestroyPassiveEvent();
    }

    /// <summary>
    /// Activate the free pass tournament perk
    /// </summary>
    private void ActivateTournamentPass()
    {
        // Calculate the free pass expiration date
        int durationHours = m_def.GetAsInt("tournamentPassDurationHours");
        DateTime now = GameServerManager.GetEstimatedServerTime();
        
        m_tournamentPassExpirationTimestamp = now.AddHours(durationHours);
        
    }
    
    /// <summary>
    /// Finalize the free pass tournament perk
    /// </summary>
    private void EndTournamentPass()
    {
        // Reset the expiration date to invalidate the free pass ticket.
        m_tournamentPassExpirationTimestamp = new DateTime();
    }


    private void CreateBoostedSevenDayLogin()
    {
        m_boostedDailyRewards = new BoostedDailyRewardsSequence();
        
        //Initialize from content
        m_boostedDailyRewards.Generate();
    }


    private void EndBoostedSevenDayLogin()
    {
        m_boostedDailyRewards = null;
    }



    private void ActivateHappyHour()
    {
        string sku = def.GetAsString("happyHourSku");
        if (string.IsNullOrEmpty(sku) || sku == "-")
        {
            // Happy hour feature is disabled from content
            return;
        }
        
        // Force the activation of this HH config
        OffersManager.happyHourManager.ForceStart(sku);

    }

    
    private void EndHappyHour()
    {
        string sku = def.GetAsString("happyHourSku");
        if (string.IsNullOrEmpty(sku) || sku == "-")
        {
            // Happy hour feature is disabled from content
            return;
        }
        
        // Force the stop of this HH config
        OffersManager.happyHourManager.ForceStop(sku);

    }


    private void EnableSpecialOffer ()
    {
        // Nothing to do, this offer will be automatically activated
        // by the offersManager
        
        // Make sure we show this perk in the popup
        m_hasSpecialOffer = true;
    }


    private void DisableSpecialOffer ()
    {
        // Nothing to do. Offers manager will expire WB offers automatically

        m_hasSpecialOffer = false;
    }


    private void EnablePopup()
    {
        // Show the welcome back popup when possible
        m_isPopupWaiting = true;
    }


    private void DisablePopup()
    {
        // Probably the popup has already been displayed.
        // But just in case we are in time...
        m_isPopupWaiting = false;
    }
    
	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//


	/// <summary>
	/// Constructor from json data.
	/// </summary>
	/// <param name="_data">Data to be parsed.</param>
	public void ParseJson(SimpleJSON.JSONNode _data)
	{
		
        
        string key = "lastActivationTime";
        if ( _data.ContainsKey(key) )
        {
            m_lastActivationTime = TimeUtils.TimestampToDate(PersistenceUtils.SafeParse<long>(_data["lastActivationTime"]));
        }
        
		// Load solo quest in the liveDataManager
		key = "soloQuest";
		if ( _data.ContainsKey(key) )
		{
			HDSoloQuestManager soloQuest = new HDSoloQuestManager();
			soloQuest.ParseJson(_data[key]);
			HDLiveDataManager.instance.soloQuest = soloQuest;
		}
        
        // Load passive events in the liveDataManager
        key = "localPassive";
        if ( _data.ContainsKey(key) )
        {
            HDLocalPassiveEventManager passive = new HDLocalPassiveEventManager();
            passive.ParseJson(_data[key]);
            HDLiveDataManager.instance.localPassive = passive;
        }
        
        // Load free tournament pass data
        key = "tournamentPassExpiration";
        if ( _data.ContainsKey(key) )
        {
            m_tournamentPassExpirationTimestamp = TimeUtils.TimestampToDate(PersistenceUtils.SafeParse<long>(_data["tournamentPassExpiration"]));
        }
        
        // Load boosted daily rewards
        key = "boostedDailyRewards";
        if ( _data.ContainsKey(key) )
        {
            m_boostedDailyRewards = new BoostedDailyRewardsSequence();
            m_boostedDailyRewards.LoadData(_data[key]);
            
        }
        
        
		
	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json.</returns>
	public SimpleJSON.JSONClass ToJson()
	{
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
        
        // Activation time
        data.Add("lastActivationTime", PersistenceUtils.SafeToString(TimeUtils.DateToTimestamp(m_lastActivationTime)));

        // If there is an active SoloQuest, save it
		if (HDLiveDataManager.instance.soloQuest.EventExists())
		{
			data.Add("soloQuest", HDLiveDataManager.instance.soloQuest.ToJson());
		}
		
        // Save local passive events
        if (HDLiveDataManager.instance.localPassive.EventExists())
        {
            data.Add("localPassive", HDLiveDataManager.instance.localPassive.ToJson());
        }
        
        // Save free tournament pass
        data.Add("tournamentPassExpiration", PersistenceUtils.SafeToString(TimeUtils.DateToTimestamp( m_tournamentPassExpirationTimestamp )));

        // Save boosted daily rewarsd
        if (m_boostedDailyRewards != null)
        {
            data.Add("boostedDailyRewards", m_boostedDailyRewards.SaveData());
        }

        return data;
	}
	
	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Forces the activation of Welcome back feature
	/// </summary>
	public void OnForceStart()
	{
        

        Activate();

        // In debug mode, activation time is now!
        m_lastActivationTime = GameServerManager.GetEstimatedServerTime();

        // TODO: Call the server to Start
    }

    /// <summary>
    /// End all the benefits granted by the welcome back feature
    /// </summary>
    public void OnForceEnd()
	{
		Deactivate();

        m_lastActivationTime = DateTime.MinValue;

        // TODO: Call the server to Stop
	}

    //------------------------------------------------------------------------//
    // DEBUG LOG															  //
    //------------------------------------------------------------------------//

    #region log
    private const string LOG_CHANNEL = "[WelcomeBack]";
    public static void Log(string message)
    {
        ControlPanel.Log(LOG_CHANNEL + message, ControlPanel.ELogChannel.WelcomeBack);
    }
    #endregion

}