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
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	private DateTime m_lastVisit;
	private bool m_welcomeBackTriggered;

	// WB configuration
	private bool enabled = true;
	private int m_minAbsentDays; // Amount of days the the player needs to be absent to get the WB

	
	// Keep the definition at hand
	private DefinitionNode m_def;

    public DefinitionNode def => m_def;
    
    // Free tournament
    private DateTime m_freeTournamentExpirationTimestamp;

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

        // Load the config params
        m_minAbsentDays = def.GetAsInt("minAbsentDays");

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
        if (m_welcomeBackTriggered)
		    return false;

		m_lastVisit = UsersManager.currentUser.saveTimestamp;

        // This player didnt spend enough days offline to get a WB
		if (GameServerManager.GetEstimatedServerTime() < m_lastVisit.AddDays(m_minAbsentDays))
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

		// Profile specific perks:
		bool nonPayer = true;
        if ( nonPayer)
		{
			// Activate boosted seven day login
			CreateBoostedSevenDayLogin();

			// Show non payer offer in the shop
			CreateNonPayerOffer();

		} else
		{
			// Enable Happy Hour
			ActivateHappyHour();

            // Dragon progression specifics
			bool playerOwnsLatestDragon = true;
            if (playerOwnsLatestDragon)
			{
				// Show special Gatcha offer
				CreateSpecialGatchaOffer();
			}
            else
			{
				// Show Latest dragon offer
				CreateLatestDragonOffer();
			}
		}

		// Register WB
		m_welcomeBackTriggered = true;
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
    }

    /// <summary>
    /// Check if the free tournament pass is active
    /// </summary>
    /// <returns>True if active</returns>
    public bool IsFreeTournamentPassActive()
    {
        return m_freeTournamentExpirationTimestamp > GameServerManager.GetEstimatedServerTime();
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
		
		// Load solo quest in the liveDataManager
		string key = "soloQuest";
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
        
        
		
	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json.</returns>
	public SimpleJSON.JSONClass ToJson()
	{
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

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
		m_welcomeBackTriggered = false;
		
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