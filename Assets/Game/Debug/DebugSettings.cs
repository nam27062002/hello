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
	public const string DRAGON_INVULNERABLE 	 		= "DRAGON_INVULNERABLE";
	public const string DRAGON_INFINITE_FIRE 	 		= "DRAGON_INFINITE_FIRE";
	public const string DRAGON_INFINITE_SUPER_FIRE 	 	= "DRAGON_INFINITE_SUPER_FIRE";
	public const string DRAGON_INFINITE_BOOST  			= "DRAGON_INFINITE_BOOST";
	public const string DRAGON_EAT			 			= "DRAGON_EAT";
	public const string DRAGON_DIVE			 			= "DRAGON_DIVE";
	public const string DRAGON_EAT_DISTANCE_POWER_UP 	= "DRAGON_EAT_DISTANCE_POWER_UP";
	public const string DRAGON_SLOW_POWER_UP   			= "DRAGON_SLOW_POWER_UP";

	public const string SHOW_XP_BAR			 			= "SHOW_XP_BAR";
	public const string SHOW_COLLISIONS					= "SHOW_COLLISIONS";

	public const string NEW_CAMERA_SYSTEM		 		= "NEW_CAMERA_SYSTEM";

	public const string RESULTS_SCREEN_CHEST_TEST_MODE 	= "RESULTS_SCREEN_CHEST_TEST_MODE";
	public const string RESULTS_SCREEN_EGG_TEST_MODE 	= "RESULTS_SCREEN_EGG_TEST_MODE";

	public const string INGAME_HUD						= "INGAME_HUD";
	public const string INGAME_SPAWNERS					= "INGAME_SPAWNERS";
	public const string INGAME_GLOW						= "INGAME_GLOW";

	public static readonly string DPAD_MODE 			= "DPAD_MODE";
	public static readonly string DPAD_SMOOTH_FACTOR 	= "DPAD_SMOOTH_FACTOR";
	public static readonly string DPAD_THRESHOLD 		= "DPAD_THRESHOLD";
	public static readonly string DPAD_CLAMP_DOT 		= "DPAD_CLAMP_DOT";

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

	public static ResultsSceneSetup.ChestTestMode resultsChestTestMode {
		get { return (ResultsSceneSetup.ChestTestMode)Prefs.GetIntPlayer(RESULTS_SCREEN_CHEST_TEST_MODE, (int)ResultsSceneSetup.ChestTestMode.NONE); }
		set { Prefs.SetIntPlayer(RESULTS_SCREEN_CHEST_TEST_MODE, (int)value); }
	}

	public static ResultsSceneSetup.EggTestMode resultsEggTestMode {
		get { return (ResultsSceneSetup.EggTestMode)Prefs.GetIntPlayer(RESULTS_SCREEN_EGG_TEST_MODE, (int)ResultsSceneSetup.EggTestMode.NONE); }
		set { Prefs.SetIntPlayer(RESULTS_SCREEN_EGG_TEST_MODE, (int)value); }
	}

    //------------------------------------------------------------------//
    // METHODS															//
    //------------------------------------------------------------------//
    public static void Init() {
        // Properties that need to be positive by default should be initialized here
        string key = INGAME_HUD;        
        Prefs.SetBoolPlayer(key, Prefs.GetBoolPlayer(key, true));

        key = INGAME_SPAWNERS;        
		Prefs.SetBoolPlayer(key, Prefs.GetBoolPlayer(key, true));

        key = INGAME_GLOW;
		Prefs.SetBoolPlayer(key, Prefs.GetBoolPlayer(key, true));
    }
}