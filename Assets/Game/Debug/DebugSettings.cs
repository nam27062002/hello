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
	public const string DRAGON_INVULNERABLE 	 		        = "DRAGON_INVULNERABLE";
	public const string DRAGON_INFINITE_FIRE 	 		        = "DRAGON_INFINITE_FIRE";
	public const string DRAGON_INFINITE_SUPER_FIRE 	 	        = "DRAGON_INFINITE_SUPER_FIRE";
	public const string DRAGON_INFINITE_BOOST  			        = "DRAGON_INFINITE_BOOST";
	public const string DRAGON_EAT			 			        = "DRAGON_EAT";
	public const string DRAGON_EAT_DISTANCE_POWER_UP 	        = "DRAGON_EAT_DISTANCE_POWER_UP";
	public const string DRAGON_SLOW_POWER_UP   			        = "DRAGON_SLOW_POWER_UP";
    public const string DRAGON_BOOST_WITH_HARD_PUSH             = "DRAGON_BOOST_WITH_HARD_PUSH";
    public const string DRAGON_BOOST_WITH_HARD_PUSH_THRESHOLD   = "DRAGON_BOOST_WITH_HARD_PUSH_THRESHOLD";

    public const string SHOW_XP_BAR			 			        = "SHOW_XP_BAR";
	public const string SHOW_COLLISIONS					        = "SHOW_COLLISIONS";
	public const string SHOW_SPEED						        = "SHOW_SPEED";

	public const string NEW_CAMERA_SYSTEM		 		        = "NEW_CAMERA_SYSTEM";    

    public const string INGAME_HUD						        = "INGAME_HUD";
	public const string INGAME_SPAWNERS					        = "INGAME_SPAWNERS";	
	public const string INGAME_PARTICLES_FEEDBACK 		        = "INGAME_PARTICLES_FEEDBACK";
    public const string INGAME_PARTICLES_EATEN 			        = "INGAME_PARTICLES_EATEN";
	public const string FOG_MANAGER   							= "FOG_MANAGER";
	public const string FOG_BLEND_TYPE   						= "FOG_BLEND_TYPE";

	public const string DPAD_MODE 						        = "DPAD_MODE";
	public const string DPAD_SMOOTH_FACTOR 				        = "DPAD_SMOOTH_FACTOR";
	public const string DPAD_THRESHOLD 					        = "DPAD_THRESHOLD";
	public const string DPAD_BREAK_TOLERANCE			        = "DPAD_BREAK_TOLERANCE";
	public const string DPAD_CLAMP_DOT 					        = "DPAD_CLAMP_DOT";

	public const string SHOW_MISSING_TIDS				        = "SHOW_MISSING_TIDS";

	public const string MENU_DISGUISES_AUTO_EQUIP		        = "MENU_DISGUISES_AUTO_EQUIP";
	public const string MENU_ENABLE_SHORTCUTS					= "MENU_ENABLE_SHORTCUTS";

	public const string PLAY_TEST						        = "PLAY_TEST";

	public const string MAP_ZOOM_SPEED							= "MAP_ZOOM_SPEED";
	public const string MAP_ZOOM_RESET							= "MAP_ZOOM_RESET";
	public const string MAP_POSITION_RESET						= "MAP_POSITION_RESET";

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

	public static bool slowPowerUp {
		get { return Prefs.GetBoolPlayer(DRAGON_SLOW_POWER_UP, false); }
		set { Prefs.SetBoolPlayer(DRAGON_SLOW_POWER_UP, value); }
	}

	public static bool isPlayTest {
		get { return Prefs.GetBoolPlayer(PLAY_TEST, false); }
		set { Prefs.SetBoolPlayer(PLAY_TEST, value); }
	}

	static bool m_ignoreSpawnTime = false;
	public static bool ignoreSpawnTime{
		get { return m_ignoreSpawnTime; }
		set { m_ignoreSpawnTime = value;}
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

		key = INGAME_PARTICLES_FEEDBACK;
		Prefs.SetBoolPlayer(key, Prefs.GetBoolPlayer(key, true));

        key = INGAME_PARTICLES_EATEN;
		Prefs.SetBoolPlayer(key, Prefs.GetBoolPlayer(key, true));

        key = DRAGON_BOOST_WITH_HARD_PUSH;
		Prefs.SetBoolPlayer(key, Prefs.GetBoolPlayer(key, TouchControls.BOOST_WITH_HARD_PUSH_DEFAULT_ENABLED));

        key = DRAGON_BOOST_WITH_HARD_PUSH_THRESHOLD;
        Prefs.SetFloatPlayer(key, Prefs.GetFloatPlayer(key, TouchControls.BOOST_WITH_HARD_PUSH_DEFAULT_THRESHOLD));		        

		key = FOG_MANAGER;
		Prefs.SetBoolPlayer(key, Prefs.GetBoolPlayer(key, true));

		key = FOG_BLEND_TYPE;
		Prefs.SetIntPlayer(key, Prefs.GetIntPlayer(key, 0));
    }
}