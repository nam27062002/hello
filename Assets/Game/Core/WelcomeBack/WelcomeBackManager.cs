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
        get => (m_timesActivated > 0);
    }


    private int m_timesActivated;
    public int timesActivated { get => m_timesActivated; }

    // Use only for trackin purposes
    private DateTime m_lastActivationTime;
    private bool m_enablePopup;
    private bool m_enableSoloQuest;
    private bool m_enablePassive;
    private bool m_enableTournamentPass;
    private bool m_enableHappyHour;
    private bool m_enableBoostedDailyLogin;
    private bool m_enableOffer;

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
    /// Try to activate WB in the client. Send a tracking event with the result.
    /// </summary>
    /// <returns>True if the WB has been succesfully activated in the client</returns>
    public bool TryActivation ()
    {
        if (m_def == null)
        {
            // WB not active in the content
            return false;
        }

        if (IsElegibleForWB())
        {

            if (Activate())
            {
                // Tracking (WB just activated!)
                TrackWelcomeBackStatus(true);
                return true;
            }

        }
        
        // Tracking (witout WB activation)
        TrackWelcomeBackStatus(false);

        return false;

    }


    /// <summary>
    /// Check if this player is elegible for the WB feature
    /// </summary>
    /// <returns>True if all the checks passed</returns>
    private bool IsElegibleForWB ()
    {

        Debug.Assert(def != null);


        // First of all check if the player finished the mininum required runs
        int minRuns = m_def.GetAsInt("minRuns");
        if (UsersManager.currentUser.gamesPlayed < minRuns)
        {
            Log("Won´t activate WB: The player didnt complete the required runs");
            return false;
        }


        // Check if the maximum WB activations were reached
        if (m_timesActivated >= m_def.GetAsInt("maxActivationsAllowed"))
        {
            Log("Won´t activate WB: The maximum amount of activations was reached");
            return false;
        }


        // Check the absent time, based on the last saved timestamp
        DateTime m_lastVisit = UsersManager.currentUser.saveTimestamp;
        if (m_lastVisit > GameServerManager.GetEstimatedServerTime())
        {
            // Someone is cheating
            Log("Won´t activate WB: Stored date cannot be greater than server time");
            return false;
        }

        TimeSpan timeAbsent = GameServerManager.GetEstimatedServerTime() - m_lastVisit;
        int minAbsentDays = m_def.GetAsInt("minAbsentDays");


        // Did this player spend enough days offline to get a WB?
        if (timeAbsent.TotalDays < minAbsentDays)
        {
            Log("The player needs to be absent at least " + minAbsentDays + " days to trigger WB, " +
                "he was out only " + timeAbsent.TotalDays);
            return false;
        }


        // All tests passed. 
        Log("The player is elegible for welcome back!");

        return true;
    }


    /// <summary>
	/// The welcome back feature becomes active. Enables all the benefits depending on the player profile.
	/// </summary>
    /// <returns>True if the activation was succesful</returns>
    public bool Activate()
    {

        m_perksDef = FindPerksForThisPlayer();

        if (m_perksDef == null)
        {
            // No configuration find for this player
            Debug.LogError("No WB perks config was found for this player. This shouldn't happen!");

            return false;
        }


        // Check all the perks one by one:
        m_enablePopup = perksDef.GetAsBool(ENABLE_POPUP);
        m_enableSoloQuest = perksDef.GetAsBool(ENABLE_SOLO_QUEST);
        m_enablePassive = perksDef.GetAsBool(ENABLE_PASSIVE);
        m_enableTournamentPass = perksDef.GetAsBool(ENABLE_TOURNAMENT_PASS);
        m_enableHappyHour = perksDef.GetAsBool(ENABLE_HAPPY_HOUR);
        m_enableBoostedDailyLogin = perksDef.GetAsBool(ENABLE_BOOSTED_DAILY_LOGIN);
        m_enableOffer = perksDef.GetAsBool(ENABLE_OFFER);

        if (m_enablePopup)
            EnablePopup();  

        if (m_enableSoloQuest)
            CreateSoloQuest();       

        if (m_enablePassive)
            ActivatePassiveEvent();

        if (m_enableTournamentPass)
            ActivateTournamentPass();

        if (m_enableHappyHour)
            ActivateHappyHour();

        if (m_enableBoostedDailyLogin)
            CreateBoostedSevenDayLogin();

        if (m_enableOffer)
            EnableSpecialOffer();

        m_timesActivated++;

        m_lastActivationTime = GameServerManager.GetEstimatedServerTime();

        string perks = String.Format(
            "Popup = {0}, " +
            "Solo Quest = {1}, " +
            "Passive = {2}, " +
            "Tournament pass = {3}, " +
            "Happy Hour = {4}, " +
            "Boosted daily reward = {5}, " +
            "Special offer = {6}",
            m_enablePopup, m_enableSoloQuest, m_enablePassive, m_enableTournamentPass, m_enableHappyHour, m_enableBoostedDailyLogin, m_enableOffer);

        Log("Welcome back activated. " + perks);


        return true;

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

        m_timesActivated = 0;

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

        if (cluster == null )
        {
            // By the time WB is triggered, the player should already been assigned to a cluster
            Debug.LogWarning("This player is not assigned to any cluster yet!");
            return null;

        }

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

        // Let all the UI involved that something changed
        Messenger.Broadcast(MessengerEvents.LIVE_EVENT_STATES_UPDATED);


    }
    
    /// <summary>
    /// Finalize the free pass tournament perk
    /// </summary>
    private void EndTournamentPass()
    {
        // Reset the expiration date to invalidate the free pass ticket.
        m_tournamentPassExpirationTimestamp = new DateTime();

        // Let all the UI involved that something changed
        Messenger.Broadcast(MessengerEvents.LIVE_EVENT_STATES_UPDATED);
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
		       

        string key = "timesActivated";
        if (_data.ContainsKey(key))
        {
            m_timesActivated = PersistenceUtils.SafeParse<int>(_data[key]);
        }

        key = "hasOffer";
        if (_data.ContainsKey(key))
        {
            m_hasSpecialOffer = PersistenceUtils.SafeParse<bool>(_data[key]);
        }

        key = "popupWaiting";
        if (_data.ContainsKey(key))
        {
            m_isPopupWaiting = PersistenceUtils.SafeParse<bool>(_data[key]);
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
        key = "passive";
        if ( _data.ContainsKey(key) )
        {
            HDLocalPassiveEventManager passive = new HDLocalPassiveEventManager();
            passive.ParseJson(_data[key]);
            HDLiveDataManager.instance.localPassive = passive;
        }
        
        // Load free tournament pass data
        key = "tourPassExp";
        if ( _data.ContainsKey(key) )
        {
            m_tournamentPassExpirationTimestamp = PersistenceUtils.SafeParse<DateTime>(_data[key]);
        }
        
        // Load boosted daily rewards
        key = "boostedDailyRwds";
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
        
        data.Add("timesActivated", PersistenceUtils.SafeToString(m_timesActivated));

        data.Add("hasOffer", PersistenceUtils.SafeToString(m_hasSpecialOffer));

        // In case the player closes the app before seeing the popup, we want to show it the next time he comes back
        data.Add("popupWaiting", PersistenceUtils.SafeToString(m_isPopupWaiting));

        // If there is an active SoloQuest, save it
        if (HDLiveDataManager.instance.soloQuest.EventExists())
		{
			data.Add("soloQuest", HDLiveDataManager.instance.soloQuest.ToJson());
		}
		
        // Save local passive events
        if (HDLiveDataManager.instance.localPassive.EventExists())
        {
            data.Add("passive", HDLiveDataManager.instance.localPassive.ToJson());
        }
        
        // Save free tournament pass
        data.Add("tourPassExp", PersistenceUtils.SafeToString(m_tournamentPassExpirationTimestamp));

        // Save boosted daily rewarsd
        if (m_boostedDailyRewards != null)
        {
            data.Add("boostedDailyRwds", m_boostedDailyRewards.SaveData());
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

    }

    /// <summary>
    /// End all the benefits granted by the welcome back feature
    /// </summary>
    public void OnForceEnd()
	{
        // Disable the WB in the client
		Deactivate();
    }


    //------------------------------------------------------------------------//
    // TRACKING         													  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Send the tracking event
    /// </summary>
    /// <param name="_triggered">True if the welcome back has been triggered in this round</param>
    private void TrackWelcomeBackStatus(bool _triggered)
    {

        string clusterId = ClusteringManager.Instance.GetClusterId();
        int minAbsentDays = def.GetAsInt("minAbsentDays");

        string perks = "";
        if (_triggered)
        {
            // Only send the perks list if we just triggered WB
            perks = String.Format(
                    "Popup = {0}, " +
                    "Solo Quest = {1}, " +
                    "Passive = {2}, " +
                    "Tournament pass = {3}, " +
                    "Happy Hour = {4}, " +
                    "Boosted daily reward = {5}, " +
                    "Special offer = {6}",
                    m_enablePopup, m_enableSoloQuest, m_enablePassive, m_enableTournamentPass, m_enableHappyHour, m_enableBoostedDailyLogin, m_enableOffer);
        }

        // Generate a unique ID for this precise welcome back
        string welcomeBackId = "WB_" + m_lastActivationTime.ToString("ddMMyyHHmmss");

        HDTrackingManager.Instance.Notify_WelcomeBackStatus(clusterId, minAbsentDays, perks, _triggered, welcomeBackId);

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