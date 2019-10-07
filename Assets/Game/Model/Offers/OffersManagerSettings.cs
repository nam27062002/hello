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
public class OffersManagerSettings : ScriptableObject {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "Singletons/OffersManagerSettings";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Offers Manager Settings
	[Header("General Settings")]
	[Tooltip("Seconds")]
	public float refreshFrequency = 1f;

	[Header("Rotational Offers")]
	public int rotationalActiveOffers = 1;
	public int rotationalHistorySize = 1;

	[Header("Free Daily Offer")]
	public int freeHistorySize = 2;

	[Header("Offer Pack Settings")]
	[Tooltip("Value in content representing the default value")]
	public string emptyValue = "-";
}