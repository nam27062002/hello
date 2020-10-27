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
    public enum PlayerType
    {
        NON_PAYER,
        PAYER_WITHOUT_LAST_DRAGON,
        PAYER_WITH_LAST_DRAGON
    }
    
    //------------------------------------------------------------------------//
    // CONST       															  //
    //------------------------------------------------------------------------//
    public const string ALL = "all";
    public const string PAYER = "payer";
    public const string NON_PAYER = "non_payer";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	private DateTime m_lastVisit;
    private PlayerType m_playerType;
    private bool m_active;


	// WB configuration
	private bool enabled = true;



    // Keep the definition at hand
	private DefinitionNode m_def;
    public DefinitionNode def => m_def;
    
    // Free tournament
    private DateTime m_freeTournamentExpirationTimestamp;
    
    // Boosted daily reward
    private BoostedDailyRewardsSequence m_boostedDailyRewards;
    public BoostedDailyRewardsSequence boostedDailyRewards
    {
        get => m_boostedDailyRewards;
    }
    
    // The multiplier applied to the reward if watching an ad
    public int boostedDailyRewardAdMultiplier
    {
        get
        {
            return m_def.GetAsInt("boostedDailyLoginAdMultiplier");
        }
    }

    
    // Welcome back becomes active when triggered and never expires. Only can be deactivated via cheats panel.
    public bool active
    {
        get { return m_active; }
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



    }


    /// <summary>
	/// Checks if the player is elegible for the welcome back feature
	/// </summary>
	/// <returns>Returns true if the player has been X days without connecting to the game
	/// and didnt enjoy this welcome back feature before.</returns>
    public bool IsElegibleForWB()
    {
	    // The feature is disabled from the content
	    if (!enabled)
		    return false;
		
        // This player already enjoyed this feature
        if (m_active)
		    return false;

		m_lastVisit = UsersManager.currentUser.saveTimestamp;
        
        // Amount of days the the player needs to be absent to get the WB
        int minAbsentDays = def.GetAsInt("minAbsentDays");

        // This player didnt spend enough days offline to get a WB
		if (GameServerManager.GetEstimatedServerTime() < m_lastVisit.AddDays(minAbsentDays))
			return false;

        // All checks passed
		return true;
	}


    /// <summary>
	/// The welcome back feature becomes active. Enables all the benefits depending on the player profile.
	/// </summary>
    public void Activate()
    {

		// Create Solo Quest
		CreateSoloQuest();

		// Activate Passive Event
		ActivatePassiveEvent();

		// Activate free tournament entrance
		ActivateFreePassTournament();

        // Which type of players can enjoy the boosted 7 day login?
        string boostedDailyRewardsPlayerType = m_def.GetAsString("boostedDailyRewardsPlayerType", "all");

		// Profile specific perks:
        if ( m_playerType == PlayerType.NON_PAYER)
        {
            // Show non payer offer in the shop
			CreateNonPayerOffer();
            
            if (boostedDailyRewardsPlayerType == ALL || boostedDailyRewardsPlayerType == NON_PAYER)
            {
                // Activate boosted seven day login
                CreateBoostedSevenDayLogin();
            }

		} 
        else if (m_playerType == PlayerType.PAYER_WITHOUT_LAST_DRAGON)
        {
            // Enable Happy Hour
            ActivateHappyHour();
            
            // Show Latest dragon offer
            CreateLatestDragonOffer();
            
            if (boostedDailyRewardsPlayerType == ALL || boostedDailyRewardsPlayerType == PAYER)
            {
                // Activate boosted seven day login
                CreateBoostedSevenDayLogin();
            }
			
        }
        else if (m_playerType == PlayerType.PAYER_WITH_LAST_DRAGON)
		{
			// Enable Happy Hour
			ActivateHappyHour();
    
            // Show special Gatcha offer
			CreateSpecialGatchaOffer();
            
            if (boostedDailyRewardsPlayerType == ALL || boostedDailyRewardsPlayerType == NON_PAYER)
            {
                // Activate boosted seven day login
                CreateBoostedSevenDayLogin();
            }
        }
        

		// Register WB
		m_active = true;
	}

    /// <summary>
    /// End all the welcome back perks
    /// We use it for testing purposes
    /// </summary>
    public void Deactivate()
    {
	    EndSoloQuest();
        EndPassiveEvent();
        EndFreePassTournament();
        EndBoostedSevenDayLogin();

        m_active = false;
    }

    /// <summary>
    /// Check if the "free" tournament pass is active. Notice that the entrance fee could be free or not.
    /// </summary>
    /// <returns>True if active</returns>
    public bool IsFreeTournamentPassActive()
    {
        return m_freeTournamentExpirationTimestamp > GameServerManager.GetEstimatedServerTime();
    }
    
    /// <summary>
    /// Check if the "free" tournament entrance fee is really free, or just cheaper
    /// </summary>
    /// <returns></returns>
    public bool IsFreeTournamentReallyFree()
    {
        return GetFreeTournamentPassPrice() == 0;
    }

    /// <summary>
    /// Returns the price of the "free" pass for tournaments. 
    /// </summary>
    /// <returns>Returns zero if free</returns>
    public int GetFreeTournamentPassPrice()
    {
        return m_def.GetAsInt("freeTournamentPassPrice");
    }

    
    /// <summary>
    /// The currency type defined in the content
    /// </summary>
    /// <returns>Returns the type of currency of the tournament pass</returns>
    public UserProfile.Currency GetFreeTournamentCurrency()
    {
        // This sku could be "-" in case of a free pass
        string sku = m_def.GetAsString("freeTournamentPassCurrency");
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
    private void ActivateFreePassTournament()
    {
        // Calculate the free pass expiration date
        int freeTournamentDurationHours = m_def.GetAsInt("freeTournamentDurationHours");
        DateTime now = GameServerManager.GetEstimatedServerTime();
        
        m_freeTournamentExpirationTimestamp = now.AddHours(freeTournamentDurationHours);
        
    }
    
    /// <summary>
    /// Finalize the free pass tournament perk
    /// </summary>
    private void EndFreePassTournament()
    {
        // Reset the expiration date to invalidate the free pass ticket.
        m_freeTournamentExpirationTimestamp = new DateTime();
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

    private void CreateNonPayerOffer()
    {

    }

    private void ActivateHappyHour()
    {

    }

    private void CreateLatestDragonOffer ()
    {

    }

    private void CreateSpecialGatchaOffer()
    {

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
		
        // Is active?
        string key = "active";
        if ( _data.ContainsKey(key) )
        {
            m_active = PersistenceUtils.SafeParse<bool>(_data[key]);
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
        key = "freeTournamentExpiration";
        if ( _data.ContainsKey(key) )
        {
            m_freeTournamentExpirationTimestamp = TimeUtils.TimestampToDate(PersistenceUtils.SafeParse<long>(_data["freeTournamentExpiration"]));
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
        
        // Welcome back state
        data.Add("active", m_active);

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
        data.Add("freeTournamentExpiration", PersistenceUtils.SafeToString(TimeUtils.DateToTimestamp( m_freeTournamentExpirationTimestamp )));

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
		m_active = false;
		
		Activate();
	}

	/// <summary>
	/// End all the benefits granted by the welcome back feature
	/// </summary>
	public void OnForceEnd()
	{
		Deactivate();
	}

}