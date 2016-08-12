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
	public static readonly string DRAGON_INFINITE_SUPER_FIRE 	 = "DRAGON_INFINITE_SUPER_FIRE";
	public static readonly string DRAGON_INFINITE_BOOST  = "DRAGON_INFINITE_BOOST";
	public static readonly string DRAGON_EAT			 = "DRAGON_EAT";
	public static readonly string DRAGON_DIVE			 = "DRAGON_DIVE";
	public static readonly string DRAGON_EAT_DISTANCE_POWER_UP = "DRAGON_EAT_DISTANCE_POWER_UP";
	public static readonly string DRAGON_SLOW_POWER_UP   = "DRAGON_SLOW_POWER_UP";
	public static readonly string NEW_CAMERA_SYSTEM      = "NEW_CAMERA_SYSTEM";

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Mainly shortcuts, all settings can be accessed using the Prefs class
	// Gameplay
	public static bool invulnerable { 
		get { return Prefs.GetBoolPlayer(DRAGON_INVULNERABLE, false); }
		set { Prefs.SetBoolPlayer(DRAGON_INVULNERABLE, value); }
	}

	public static bool infiniteFire { 
		get { return Prefs.GetBoolPlayer(DRAGON_INFINITE_FIRE, false); }
		set { Prefs.SetBoolPlayer(DRAGON_INFINITE_FIRE, value); }
	}

	public static bool infiniteSuperFire { 
		get { return Prefs.GetBoolPlayer(DRAGON_INFINITE_SUPER_FIRE, false); }
		set { Prefs.SetBoolPlayer(DRAGON_INFINITE_SUPER_FIRE, value); }
	}

	public static bool infiniteBoost {
		get { return Prefs.GetBoolPlayer(DRAGON_INFINITE_BOOST, false); }
		set { Prefs.SetBoolPlayer(DRAGON_INFINITE_BOOST, value); }
	}

	public static bool eat {
		get { return Prefs.GetBoolPlayer(DRAGON_EAT, false); }
		set { Prefs.SetBoolPlayer(DRAGON_EAT, value); }
	}

	public static bool dive {
		get { return Prefs.GetBoolPlayer(DRAGON_DIVE, false); }
		set { Prefs.SetBoolPlayer(DRAGON_DIVE, value); }
	}

	public static bool eatDistancePowerUp {
		get { return Prefs.GetBoolPlayer(DRAGON_EAT_DISTANCE_POWER_UP, false); }
		set { Prefs.SetBoolPlayer(DRAGON_EAT_DISTANCE_POWER_UP, value); }
	}

	public static bool slowPowerUp {
		get { return Prefs.GetBoolPlayer(DRAGON_SLOW_POWER_UP, false); }
		set { Prefs.SetBoolPlayer(DRAGON_SLOW_POWER_UP, value); }
	}

	public static bool newCameraSystem{
		get { return Prefs.GetBoolPlayer(NEW_CAMERA_SYSTEM, true); }	// New camera system by default
		set { Prefs.SetBoolPlayer(NEW_CAMERA_SYSTEM, value); }
	}
}