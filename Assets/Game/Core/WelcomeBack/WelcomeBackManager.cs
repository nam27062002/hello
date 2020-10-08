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

	
	// Benefit definitions from content
	DefinitionNode m_soloQuestDef;
    

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
    }

    private void CreateSoloQuest()
    {
	    // Initialize the solo quest
		HDLiveDataManager.instance.soloQuest.StartQuest();
    }

    private void EndSoloQuest()
    {
	    HDLiveDataManager.instance.soloQuest.DestroyQuest();
    }

	private void ActivatePassiveEvent()
    {

    }

    private void ActivateFreePassTournament()
    {

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