// OffersManagerSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/12/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Exposed setup for the OffersManager and OfferPack classes.
/// </summary>
public class OffersManagerSettings {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string SKU = "offerSettings";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Content
	public DefinitionNode def = null;

	// General Settings
	public float refreshFrequency = 1f;

	// Rotational Offers
	public int rotationalActiveOffers = 1;
	public int rotationalHistorySize = 1;

	// Free Daily Offer
	public int freeHistorySize = 2;
	public int freeCooldownMinutes = 360;

	// Offer Pack Settings
	public string emptyValue = "-"; // Value in content representing the default value

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the settings object.
	/// </summary>
	public void InitFromDefinitions() {
		// Make sure content manager is ready
		Debug.Assert(ContentManager.ready, "ERROR: Trying to initialize offer manager settings but content is not ready yet!");

		// Gather definition
		def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, SKU);

		// Cache data
		refreshFrequency = def.GetAsFloat("refreshFrequency", refreshFrequency);

		rotationalActiveOffers = def.GetAsInt("rotationalActiveOffers", rotationalActiveOffers);
		rotationalHistorySize = def.GetAsInt("rotationalHistorySize", rotationalHistorySize);

		freeHistorySize = def.GetAsInt("freeHistorySize", freeHistorySize);
		freeCooldownMinutes = def.GetAsInt("freeCooldownMinutes", freeCooldownMinutes);

		emptyValue = def.GetAsString("emptyValue", emptyValue);
	}
}