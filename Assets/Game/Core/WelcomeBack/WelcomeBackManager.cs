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
	private int m_minAbsentDays; // Amount of days the the player needs to be absent to get the WB


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

	}


    /// <summary>
	/// Checks if the player is elegible for the welcome back feature
	/// </summary>
	/// <returns>Returns true if the player has been X days without connecting to the game
	/// and didnt enjoy this welcome back feature before.</returns>
    public bool IsElegibleForWB()
	{
        // This player already enjoyed this feature
        if (m_welcomeBackTriggered)
		    return false;

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


    private void CreateSoloQuest()
    {

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
	public void LoadData(SimpleJSON.JSONNode _data)
	{
	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json.</returns>
	public SimpleJSON.JSONClass SaveData()
	{
		SimpleJSON.JSONClass data = null;

		return data;
	}
}