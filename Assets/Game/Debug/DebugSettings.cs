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
public class DebugSettings : SingletonScriptableObject<DebugSettings> {
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

    public const string SHOW_XP_BAR			 			        = "SHOW_XP_BAR";
	public const string SHOW_COLLISIONS					        = "SHOW_COLLISIONS";
    public const string RESOLUTION_FACTOR                       = "RESOLUTION_FACTOR";
    public const string SHOW_SPEED						        = "SHOW_SPEED";
	public const string SHOW_ALL_COLLECTIBLES					= "SHOW_ALL_COLLECTIBLES";

	public const string VERTICAL_ORIENTATION		 		    = "VERTICAL_ORIENTATION";
	public const string SIMULATED_SPECIAL_DEVICE				= "SIMULATED_SPECIAL_DEVICE";

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

	public const string BOOST_PAD_FOLLOW				        = "BOOST_PAD_FOLLOW";
	public const string BOOST_AUTO_RESTART				        = "BOOST_AUTO_RESTART";			// Auto-restart boost if still holding and bar has refilled

	public const string TILT_CONTROL_DEBUG_UI					= "TILT_CONTROL_DEBUG_UI";

	public const string SHOW_MISSING_TIDS				        = "SHOW_MISSING_TIDS";
	public const string LOCALIZATION_DEBUG_MODE			        = "LOCALIZATION_DEBUG_MODE";

	public const string MENU_DISGUISES_AUTO_EQUIP		        = "MENU_DISGUISES_AUTO_EQUIP";
	public const string MENU_ENABLE_SHORTCUTS					= "MENU_ENABLE_SHORTCUTS";
	public const string MENU_CAMERA_TRANSITION_DURATION			= "MENU_CAMERA_TRANSITION_DURATION";
	public const string POPUP_AD_DURATION						= "POPUP_AD_DURATION";

	public const string PLAY_TEST						        = "PLAY_TEST";

	public const string MAP_ZOOM_SPEED							= "MAP_ZOOM_SPEED";
	public const string MAP_ZOOM_RESET							= "MAP_ZOOM_RESET";
	public const string MAP_POSITION_RESET						= "MAP_POSITION_RESET";

	public const string GLOBAL_EVENTS_DONT_CACHE_LEADERBOARD	= "GLOBAL_EVENTS_DONT_CACHE_LEADERBOARD";

	public const string SHOW_HIDDEN_PETS						= "SHOW_HIDDEN_PETS";
	public const string SHOW_SAFE_AREA	 					    = "SHOW_SAFE_AREA";

    // Special Dragon Cheats
    public const string USE_SPECIAL_DRAGON                      = "USE_SPECIAL_DRAGON";
    public const string SPECIAL_DRAGON_SKU                      = "SPECIAL_DRAGON_SKU";
    public const string SPECIAL_DRAGON_TIER                     = "SPECIAL_DRAGON_TIER";
    public const string SPECIAL_DRAGON_POWER_LEVEL              = "SPECIAL_DRAGON_POWER_LEVEL";
    public const string SPECIAL_DRAGON_HP_BOOST_LEVEL           = "SPECIAL_DRAGON_HP_BOOST_LEVEL";
    public const string SPECIAL_DRAGON_SPEED_BOOST_LEVEL        = "SPECIAL_DRAGON_SPEED_BOOST_LEVEL";
    public const string SPECIAL_DRAGON_ENERGY_BOOST_LEVEL       = "SPECIAL_DRAGON_ENERGY_BOOST_LEVEL";
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

	// Spawners cheats
	static bool m_ignoreSpawnTime = false;
	public static bool ignoreSpawnTime{
		get { return m_ignoreSpawnTime; }
		set { m_ignoreSpawnTime = value;}
	}

	static bool m_spawnChance0 = false;
	public static bool spawnChance0{
		get { return m_spawnChance0; }
		set { m_spawnChance0 = value;}
	}

	static bool m_spawnChance100 = false;
	public static bool spawnChance100{
		get { return m_spawnChance100; }
		set { m_spawnChance100 = value;}
	}

	// Metagame cheats
	public static bool showHiddenPets {
		get { return Prefs.GetBoolPlayer(SHOW_HIDDEN_PETS, false); }
		set { Prefs.SetBoolPlayer(SHOW_HIDDEN_PETS, value); }
	}

	// Server debugging tools
	// Only in editor!
	[Separator("Server Debug Tools")]
	[SerializeField] private bool m_useDebugServer = false;
	public static bool useDebugServer {
		get { return instance.m_useDebugServer; }
		set { instance.m_useDebugServer = value; }
	}

	[SerializeField] private Range m_debugServerDelayRange = new Range(0f, 0f);		// Simulate a delay on the server response. A random value will be taken for each call.
	public static Range serverDelayRange {
		get { return instance.m_debugServerDelayRange; }
		set { instance.m_debugServerDelayRange = value; }
	}

	[SerializeField] private bool m_useLiveEventsDebugCalls = false;
	public static bool useLiveEventsDebugCalls {
		get { return instance.m_useLiveEventsDebugCalls; }
		set { instance.m_useLiveEventsDebugCalls = value; }
	}

	// UI settings
	[Separator("UI Debug Tools")]
	[SerializeField] private bool m_useUnitySafeArea = false;
	public static bool useUnitySafeArea {
		get { return instance.m_useUnitySafeArea; }
	}

	[SerializeField] private bool m_useDebugSafeArea = false;

	[Comment("- iPhone X: 132, 63, 2172, 1062")]
	[SerializeField] private Rect m_debugSafeArea = new Rect();
	public static Rect debugSafeArea {
		get {
			if(instance.m_useDebugSafeArea) {
				return instance.m_debugSafeArea;
			} else {
				return new Rect(0f, 0f, Screen.width, Screen.height);
			}
		}
	}

	[HideEnumValues(false, true)]
	[SerializeField] private UIConstants.SpecialDevice m_simulatedSpecialDevice = UIConstants.SpecialDevice.NONE;
	public static UIConstants.SpecialDevice simulatedSpecialDevice {
		get {
			// Only in editor
#if UNITY_EDITOR
			if(instance.m_useDebugSafeArea) {
				return instance.m_simulatedSpecialDevice;
			} else {
				return UIConstants.SpecialDevice.NONE;
			}
#else
			return UIConstants.SpecialDevice.NONE;
#endif
		}
		set { instance.m_simulatedSpecialDevice = value; }
	}

    //------------------------------------------------------------------//
    // METHODS															//
    //------------------------------------------------------------------//
	/// <summary>
	/// Initialization. To be called at the start of the application.
	/// </summary>
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

		key = FOG_MANAGER;
		Prefs.SetBoolPlayer(key, Prefs.GetBoolPlayer(key, true));

		key = FOG_BLEND_TYPE;
		Prefs.SetIntPlayer(key, Prefs.GetIntPlayer(key, 0));

		key = LOCALIZATION_DEBUG_MODE;
		Prefs.SetIntPlayer(key, Prefs.GetIntPlayer(key, (int)LocalizationManager.DebugMode.NONE));
    }
}