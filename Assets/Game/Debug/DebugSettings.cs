// DebugSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global setup of the game.
/// </summary>
public static class DebugSettings {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Use the Prefs class to access their values
	// Cheats
	public static readonly string DRAGON_INVULNERABLE 	 = "DRAGON_INVULNERABLE";
	public static readonly string DRAGON_INFINITE_FIRE 	 = "DRAGON_INFINITE_FIRE";
	public static readonly string DRAGON_INFINITE_BOOST  = "DRAGON_INFINITE_BOOST";
	public static readonly string DRAGON_EAT			 = "DRAGON_EAT";
	public static readonly string DRAGON_DIVE			 = "DRAGON_DIVE";
	public static readonly string DRAGON_EAT_DISTANCE_POWER_UP = "DRAGON_EAT_DISTANCE_POWER_UP";

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Mainly shortcuts, all settings can be accessed using the Prefs class
	// Gameplay
	public static bool invulnerable { 
		get { return Prefs.GetBool(DRAGON_INVULNERABLE, false); }
		set { Prefs.SetBool(DRAGON_INVULNERABLE, value); }
	}

	public static bool infiniteFire { 
		get { return Prefs.GetBool(DRAGON_INFINITE_FIRE, false); }
		set { Prefs.SetBool(DRAGON_INFINITE_FIRE, value); }
	}

	public static bool infiniteBoost {
		get { return Prefs.GetBool(DRAGON_INFINITE_BOOST, false); }
		set { Prefs.SetBool(DRAGON_INFINITE_BOOST, value); }
	}

	public static bool eat {
		get { return Prefs.GetBool(DRAGON_EAT, false); }
		set { Prefs.SetBool(DRAGON_EAT, value); }
	}

	public static bool dive {
		get { return Prefs.GetBool(DRAGON_DIVE, false); }
		set { Prefs.SetBool(DRAGON_DIVE, value); }
	}

	public static bool eatDistancePowerUp {
		get { return Prefs.GetBool(DRAGON_EAT_DISTANCE_POWER_UP, false); }
		set { Prefs.SetBool(DRAGON_EAT_DISTANCE_POWER_UP, value); }
	}
}