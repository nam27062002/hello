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
public class WelcomeBackManager : SingletonInstance<WelcomeBackManager>
{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

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
	/// Checks if the player is elegible for this welcome back feature
	/// </summary>
	/// <returns>Returns true if the player has been X days without connecting to the game
	/// and didnt enjoy this welcome back feature before.</returns>
    public bool CheckPlayerComingBack()
	{
		return false;
	}


    /// <summary>
	/// The welcome back feature becomes active. Enables all the benefits depending on the player profile.
	/// </summary>
    public void Activate()
	{
		// Create Solo Quest

		// Create Passive Event

		// Activate free tournament entrance

        // Profile specific perks:
		bool nonPayer = true;
        if ( nonPayer)
		{
            // Activate boosted seven day login

            // Show non payer offer in the shop
		} else
		{
			// Enable Happy Hour

            // Dragon progression specifics
			bool playerOwnsLatestDragon = true;
            if (playerOwnsLatestDragon)
			{
                // Show special Gatcha offer
			}
            else
			{
                // Show Latest dragon offer
			}
		}
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