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
    // IMPORTANT: These keys should be declared PRIVATE when possible in order to make everyone use the accessors so we can prevent these cheats from being used in a PRODUCTION build
    // If that's not possible then use Prefs_GetXXXPlayer() and Prefs_SetXXXPlayer() methods instead of accessing to the prefs directly

	// Cheats
	private const string DRAGON_INVULNERABLE 	 		        = "DRAGON_INVULNERABLE";
    private const string DRAGON_INFINITE_FIRE 	 		        = "DRAGON_INFINITE_FIRE";
    private const string DRAGON_INFINITE_SUPER_FIRE 	 	    = "DRAGON_INFINITE_SUPER_FIRE";
    private const string DRAGON_INFINITE_BOOST  			    = "DRAGON_INFINITE_BOOST";
    private const string DRAGON_EAT			 			        = "DRAGON_EAT";
    private const string DRAGON_EAT_DISTANCE_POWER_UP 	        = "DRAGON_EAT_DISTANCE_POWER_UP";
    private const string DRAGON_SLOW_POWER_UP   			    = "DRAGON_SLOW_POWER_UP";

    public const string SHOW_XP_BAR			     			    = "SHOW_XP_BAR";
    public const string SHOW_COLLISIONS		    			    = "SHOW_COLLISIONS";
    public const string RESOLUTION_FACTOR                       = "RESOLUTION_FACTOR";
    public const string SHOW_SPEED						        = "SHOW_SPEED";
    private const string SHOW_ALL_COLLECTIBLES					= "SHOW_ALL_COLLECTIBLES";
	public const string SHOW_STATS								= "SHOW_STATS";
	public const string SHOW_FPS								= "SHOW_FPS";
	public const string SHOW_FPS_RECORDER						= "SHOW_FPS_RECORDER";

	public const string VERTICAL_ORIENTATION		 		    = "VERTICAL_ORIENTATION";
    private const string SIMULATED_SPECIAL_DEVICE				= "SIMULATED_SPECIAL_DEVICE";

    public const string INGAME_HUD						        = "INGAME_HUD";
    public const string INGAME_SPAWNERS			    		    = "INGAME_SPAWNERS";
    public const string INGAME_PARTICLES_FEEDBACK 		        = "INGAME_PARTICLES_FEEDBACK";
    private const string INGAME_PARTICLES_EATEN 			    = "INGAME_PARTICLES_EATEN";
    public const string INGAME_DRAGON_MOTION_SAFE               = "INGAME_DRAGON_MOTION_SAFE";
    private const string FOG_MANAGER   							= "FOG_MANAGER";
    public const string FOG_BLEND_TYPE   						= "FOG_BLEND_TYPE";

    public const string DPAD_MODE 						        = "DPAD_MODE";
    public const string DPAD_SMOOTH_FACTOR 		    		    = "DPAD_SMOOTH_FACTOR";
    public const string DPAD_THRESHOLD 			    		    = "DPAD_THRESHOLD";
    public const string DPAD_BREAK_TOLERANCE			        = "DPAD_BREAK_TOLERANCE";
    public const string DPAD_CLAMP_DOT 	    				    = "DPAD_CLAMP_DOT";

    public const string BOOST_PAD_FOLLOW				        = "BOOST_PAD_FOLLOW";
    public const string BOOST_AUTO_RESTART				        = "BOOST_AUTO_RESTART";         // Auto-restart boost if still holding and bar has refilled

    public const string TILT_CONTROL_DEBUG_UI					= "TILT_CONTROL_DEBUG_UI";

    public const string SHOW_MISSING_TIDS				        = "SHOW_MISSING_TIDS";
    public const string LOCALIZATION_DEBUG_MODE		    	    = "LOCALIZATION_DEBUG_MODE";

    private const string MENU_DISGUISES_AUTO_EQUIP		        = "MENU_DISGUISES_AUTO_EQUIP";
    private const string MENU_ENABLE_SHORTCUTS					= "MENU_ENABLE_SHORTCUTS";
    public const string MENU_CAMERA_TRANSITION_DURATION			= "MENU_CAMERA_TRANSITION_DURATION";
    public const string POPUP_AD_DURATION						= "POPUP_AD_DURATION";

    public const string MAP_ZOOM_SPEED							= "MAP_ZOOM_SPEED";
    private const string MAP_ZOOM_RESET							= "MAP_ZOOM_RESET";
    private const string MAP_POSITION_RESET						= "MAP_POSITION_RESET";

    private const string GLOBAL_EVENTS_DONT_CACHE_LEADERBOARD	= "GLOBAL_EVENTS_DONT_CACHE_LEADERBOARD";

    public const string SHOW_HIDDEN_PETS						= "SHOW_HIDDEN_PETS";
    public const string SHOW_SAFE_AREA	 					    = "SHOW_SAFE_AREA";

    // Special Dragon Cheats
    private const string USE_SPECIAL_DRAGON                      = "USE_SPECIAL_DRAGON";
    public const string SPECIAL_DRAGON_SKU                       = "SPECIAL_DRAGON_SKU";
    private const string SPECIAL_DRAGON_TIER                     = "SPECIAL_DRAGON_TIER";
    public const string SPECIAL_DRAGON_POWER_LEVEL               = "SPECIAL_DRAGON_POWER_LEVEL";
    public const string SPECIAL_DRAGON_HP_BOOST_LEVEL            = "SPECIAL_DRAGON_HP_BOOST_LEVEL";
    public const string SPECIAL_DRAGON_SPEED_BOOST_LEVEL         = "SPECIAL_DRAGON_SPEED_BOOST_LEVEL";
    public const string SPECIAL_DRAGON_ENERGY_BOOST_LEVEL        = "SPECIAL_DRAGON_ENERGY_BOOST_LEVEL";

    private const string SPECIAL_DATES                           = "SHOW_SPECIAL_DATES";

    public const string AB_AUTOMATIC_DOWNLOADER                 = "AB_AUTOMATIC_DOWNLOADER";
    public const string ADS_ENABLED                             = "ADS_ENABLED";

    // Camera settings Cheats
    public const string ALLOW_ZOOM_ON_BOOST                      = "ALLOW_ZOOM_ON_BOOST";
    public const string ALLOW_ZOOM_ON_FURY                       = "ALLOW_ZOOM_ON_FURY";
    public const string ALLOW_ZOOM_ON_SPACE                      = "ALLOW_ZOOM_ON_SPACE";
    public const string ALLOW_CAMERALOOK_ROTATION                = "ALLOW_CAMERALOOK_ROTATION";

    //------------------------------------------------------------------//
    // PROPERTIES														//
    //------------------------------------------------------------------//
    // Mainly shortcuts, all settings can be accessed using the Prefs class
    public static bool Prefs_GetBoolPlayer(string key, bool defaultValue = false) {
        return (FeatureSettingsManager.AreCheatsEnabled) ? Prefs.GetBoolPlayer(key, defaultValue) : defaultValue;
    }

    public static void Prefs_SetBoolPlayer(string key, bool value) {
        if (FeatureSettingsManager.AreCheatsEnabled) {
            Prefs.SetBoolPlayer(key, value);
        } 
    }

    private static void Prefs_ForceSetBoolPlayer(string key, bool value) {        
        Prefs.SetBoolPlayer(key, value);        
    }

    public static int Prefs_GetIntPlayer(string key, int defaultValue = 0) {
        return (FeatureSettingsManager.AreCheatsEnabled) ? Prefs.GetIntPlayer(key, defaultValue) : defaultValue;
    }

    public static void Prefs_SetIntPlayer(string key, int value) {
        if (FeatureSettingsManager.AreCheatsEnabled) {
            Prefs.SetIntPlayer(key, value);
        }
    }

    private static void Prefs_ForceSetIntPlayer(string key, int value) {
        Prefs.SetIntPlayer(key, value);
    }

    public static string Prefs_GetStringPlayer(string key, string defaultValue = "") {
        return (FeatureSettingsManager.AreCheatsEnabled) ? Prefs.GetStringPlayer(key, defaultValue) : defaultValue;
    }

    public static void Prefs_SetStringPlayer(string key, string value) {
        if (FeatureSettingsManager.AreCheatsEnabled) {
            Prefs.SetStringPlayer(key, value);
        }
    }

    private static void Prefs_ForceSetStringPlayer(string key, string value) {
        Prefs.SetStringPlayer(key, value);
    }

    public static float Prefs_GetFloatPlayer(string key, float defaultValue = 0) {
        return (FeatureSettingsManager.AreCheatsEnabled) ? Prefs.GetFloatPlayer(key, defaultValue) : defaultValue;
    }

    public static void Prefs_SetFloatPlayer(string key, float value) {
        if (FeatureSettingsManager.AreCheatsEnabled) {
            Prefs.SetFloatPlayer(key, value);
        }
    }

    private static void Prefs_ForceSetFloatPlayer(string key, float value) {
        Prefs.SetFloatPlayer(key, value);
    }

    public static bool areAdsEnabled {
        get { return Prefs_GetBoolPlayer(ADS_ENABLED, true); }
        set { Prefs_SetBoolPlayer(ADS_ENABLED, value); }
    }

    // Gameplay
    public static bool invulnerable { 
		get { return Prefs_GetBoolPlayer(DRAGON_INVULNERABLE, false); }
		set { Prefs_SetBoolPlayer(DRAGON_INVULNERABLE, value); }
	}

	public static bool infiniteFire { 
		get { return Prefs_GetBoolPlayer(DRAGON_INFINITE_FIRE, false); }
		set { Prefs_SetBoolPlayer(DRAGON_INFINITE_FIRE, value); }
	}

	public static bool infiniteSuperFire { 
		get { return Prefs_GetBoolPlayer(DRAGON_INFINITE_SUPER_FIRE, false); }
		set { Prefs_SetBoolPlayer(DRAGON_INFINITE_SUPER_FIRE, value); }
	}

	public static bool infiniteBoost {
		get { return Prefs_GetBoolPlayer(DRAGON_INFINITE_BOOST, false); }
		set { Prefs_SetBoolPlayer(DRAGON_INFINITE_BOOST, value); }
	}

	public static bool eat {
		get { return Prefs_GetBoolPlayer(DRAGON_EAT, false); }
		set { Prefs_SetBoolPlayer(DRAGON_EAT, value); }
	}

	public static bool slowPowerUp {
		get { return Prefs_GetBoolPlayer(DRAGON_SLOW_POWER_UP, false); }
		set { Prefs_SetBoolPlayer(DRAGON_SLOW_POWER_UP, value); }
	}

    public static bool allowCameraZoomOnBoost {
		get { return Prefs_GetBoolPlayer(ALLOW_ZOOM_ON_BOOST, true); }
		set { Prefs_SetBoolPlayer(ALLOW_ZOOM_ON_BOOST, value); }
	}

    public static bool allowCameraZoomOnFire {
		get { return Prefs_GetBoolPlayer(ALLOW_ZOOM_ON_FURY, true); }
		set { Prefs_SetBoolPlayer(ALLOW_ZOOM_ON_FURY, value); }
	}

    public static bool allowCameraZoomOnSpace {
		get { return Prefs_GetBoolPlayer(ALLOW_ZOOM_ON_SPACE, true); }
		set { Prefs_SetBoolPlayer(ALLOW_ZOOM_ON_SPACE, value); }
	}

    public static bool allowCameraRotation {
		get { return Prefs_GetBoolPlayer(ALLOW_CAMERALOOK_ROTATION, true); }
		set { Prefs_SetBoolPlayer(ALLOW_CAMERALOOK_ROTATION, value); }
	}

	static bool m_hitStopEnabled = true;
	public static bool hitStopEnabled{
		get { return m_hitStopEnabled; }
		set { m_hitStopEnabled = value;}
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

    static bool m_overrideSpawnPoint = false;
    public static bool overrideSpawnPoint {
        get { return m_overrideSpawnPoint; }
        set { m_overrideSpawnPoint = value; }
    }

    static string m_spawnArea = "area1";
    public static string spawnArea {
        get { return m_spawnArea; }
        set { m_spawnArea = value; }
    }

    static string m_spawnPoint = "v0";
    public static string spawnPoint {
        get { return m_spawnPoint; }
        set { m_spawnPoint = value; }
    }

    // Metagame cheats
    public static bool showHiddenPets {
		get { return Prefs_GetBoolPlayer(SHOW_HIDDEN_PETS, false); }
		set { Prefs_SetBoolPlayer(SHOW_HIDDEN_PETS, value); }
	}

    public static int fogBlendType {
        get { return Prefs_GetIntPlayer(FOG_BLEND_TYPE, 0); }
        set { Prefs_SetIntPlayer(FOG_BLEND_TYPE, value); }
    }

    public static bool ingameHud {
        get { return Prefs_GetBoolPlayer(INGAME_HUD, true); }
        set { Prefs_SetBoolPlayer(INGAME_HUD, value); }
    }

    public static bool ingameParticlesFeedback {
        get { return Prefs_GetBoolPlayer(INGAME_PARTICLES_FEEDBACK, true); }
        set { Prefs_SetBoolPlayer(INGAME_PARTICLES_FEEDBACK, value); }
    }

    public static bool ingameParticlesEaten {
        get { return Prefs_GetBoolPlayer(INGAME_PARTICLES_EATEN, true); }
        set { Prefs_SetBoolPlayer(INGAME_PARTICLES_EATEN, value); }
    }

    public static bool ingameSpawners
    {
        get { return Prefs_GetBoolPlayer(INGAME_SPAWNERS, true); }
        set { Prefs_SetBoolPlayer(INGAME_SPAWNERS, value); }
    }    

    public static bool ingameDragonMotionSafe {
        get { return Prefs_GetBoolPlayer(INGAME_DRAGON_MOTION_SAFE, true); }
        set { Prefs_SetBoolPlayer(INGAME_DRAGON_MOTION_SAFE, value); }
    }

    public static bool useSpecialDragon {
        get { return Prefs_GetBoolPlayer(USE_SPECIAL_DRAGON, false); }
        set { Prefs_SetBoolPlayer(USE_SPECIAL_DRAGON, value); }
    }

    public static bool showAllCollectibles {
        get { return Prefs_GetBoolPlayer(SHOW_ALL_COLLECTIBLES, false); }
        set { Prefs_SetBoolPlayer(SHOW_ALL_COLLECTIBLES, value); }
    }

    public static bool tiltControlDebugUI {
        get { return Prefs_GetBoolPlayer(TILT_CONTROL_DEBUG_UI, false); }
        set { Prefs_SetBoolPlayer(TILT_CONTROL_DEBUG_UI, value); }
    }

    public static bool isAutomaticDownloaderEnabled {
        get { return Prefs_GetBoolPlayer(AB_AUTOMATIC_DOWNLOADER, true); }
        set { Prefs_SetBoolPlayer(AB_AUTOMATIC_DOWNLOADER, value); }
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

	[SerializeField] private bool m_useDownloadablesMockHandlers = false;
	public static bool useDownloadablesMockHandlers {
		get { return instance.m_useDownloadablesMockHandlers; }
		set { instance.m_useDownloadablesMockHandlers = value; }
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

    public static bool popupAdDuration {
        get { return Prefs_GetBoolPlayer(POPUP_AD_DURATION, false); }
        set { Prefs_SetBoolPlayer(POPUP_AD_DURATION, value); }
    }

    public static bool verticalOrientation
    {
        get { return Prefs_GetBoolPlayer(VERTICAL_ORIENTATION, false); }
        set { Prefs_SetBoolPlayer(VERTICAL_ORIENTATION, value); }
    }    

    public static bool showSafeArea {
        get { return Prefs_GetBoolPlayer(SHOW_SAFE_AREA, false); }
        set { Prefs_SetBoolPlayer(SHOW_SAFE_AREA, value); }
    }

    public static int localizationDebugMode {
        get { return Prefs_GetIntPlayer(LOCALIZATION_DEBUG_MODE); }
        set { Prefs_SetIntPlayer(LOCALIZATION_DEBUG_MODE, value); }
    }   

    public static bool showMissingTids {
        get { return Prefs_GetBoolPlayer(SHOW_MISSING_TIDS, false); }
        set { Prefs_SetBoolPlayer(SHOW_MISSING_TIDS, value); }
    }

    // Special dragon cheats    
    public static int specialDragonTier {
        get { return Prefs_GetIntPlayer(SPECIAL_DRAGON_TIER, 0); }
        set { Prefs_SetIntPlayer(SPECIAL_DRAGON_TIER, value); }
    }

    public static int specialDragonPowerLevel {
        get { return Prefs_GetIntPlayer(SPECIAL_DRAGON_POWER_LEVEL); }
        set { Prefs_SetIntPlayer(SPECIAL_DRAGON_POWER_LEVEL, value); }
    }

    public static int specialDragonHpBoostLevel {
        get { return Prefs_GetIntPlayer(SPECIAL_DRAGON_HP_BOOST_LEVEL); }
        set { Prefs_SetIntPlayer(SPECIAL_DRAGON_HP_BOOST_LEVEL, value); }
    }

    public static int specialDragonSpeedBoostLevel {
        get { return Prefs_GetIntPlayer(SPECIAL_DRAGON_SPEED_BOOST_LEVEL); }
        set { Prefs_SetIntPlayer(SPECIAL_DRAGON_SPEED_BOOST_LEVEL, value); }
    }

    public static int specialDragonEnergyBoostLevel {
        get { return Prefs_GetIntPlayer(SPECIAL_DRAGON_ENERGY_BOOST_LEVEL); }
        set { Prefs_SetIntPlayer(SPECIAL_DRAGON_ENERGY_BOOST_LEVEL, value); }
    }
        
    public static bool specialDates {
        get { return Prefs_GetBoolPlayer(SPECIAL_DATES, false); }
        set { Prefs_SetBoolPlayer(SPECIAL_DATES, value); }
    }

    public static bool globalEventsDontCacheLeaderboard {
        get { return Prefs_GetBoolPlayer(GLOBAL_EVENTS_DONT_CACHE_LEADERBOARD, false); }
        set { Prefs_SetBoolPlayer(GLOBAL_EVENTS_DONT_CACHE_LEADERBOARD, value); }
    }    

    public static bool showSpeed {
        get { return Prefs_GetBoolPlayer(SHOW_SPEED); }
        set { Prefs_SetBoolPlayer(SHOW_SPEED, value); }
    }

    public static bool showXpBar {
        get { return Prefs_GetBoolPlayer(SHOW_XP_BAR); }
        set { Prefs_SetBoolPlayer(SHOW_XP_BAR, value); }
    }

	public static bool showStats {
		get { return Prefs_GetBoolPlayer(SHOW_STATS, UnityEngine.Debug.isDebugBuild); }
		set { Prefs_SetBoolPlayer(SHOW_STATS, value); }
	}

	public static bool showFps {
		get { return Prefs_GetBoolPlayer(SHOW_FPS, UnityEngine.Debug.isDebugBuild); }
		set { Prefs_SetBoolPlayer(SHOW_FPS, value); }
	}

	public static bool showFpsRecorder {
		get { return Prefs_GetBoolPlayer(SHOW_FPS_RECORDER); }
		set { Prefs_SetBoolPlayer(SHOW_FPS_RECORDER, value); }
	}

	public static bool menuDisguisesAutoEquip {
        get { return Prefs_GetBoolPlayer(MENU_DISGUISES_AUTO_EQUIP, true); }
        set { Prefs_SetBoolPlayer(MENU_DISGUISES_AUTO_EQUIP, value); }
    }    

    public static bool menuEnableShortcuts {
        get { return Prefs_GetBoolPlayer(MENU_ENABLE_SHORTCUTS); }
        set { Prefs_SetBoolPlayer(MENU_ENABLE_SHORTCUTS, value); }
    }    

    public static bool mapZoomReset {
        get { return Prefs_GetBoolPlayer(MAP_ZOOM_RESET, false); }
        set { Prefs_SetBoolPlayer(MAP_ZOOM_RESET, value); }
    }

    public static bool mapPositionReset {
        get { return Prefs_GetBoolPlayer(MAP_POSITION_RESET, true); }
        set { Prefs_SetBoolPlayer(MAP_POSITION_RESET, value); }
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
        Prefs_ForceSetBoolPlayer(key, Prefs_GetBoolPlayer(key, true));

        key = INGAME_SPAWNERS;        
		Prefs_ForceSetBoolPlayer(key, Prefs_GetBoolPlayer(key, true));        

		key = INGAME_PARTICLES_FEEDBACK;
        Prefs_ForceSetBoolPlayer(key, Prefs_GetBoolPlayer(key, true));

        key = INGAME_PARTICLES_EATEN;
        Prefs_ForceSetBoolPlayer(key, Prefs_GetBoolPlayer(key, true));

		key = FOG_MANAGER;
        Prefs_ForceSetBoolPlayer(key, Prefs_GetBoolPlayer(key, true));
       
        key = FOG_BLEND_TYPE;
        Prefs_ForceSetIntPlayer(key, Prefs_GetIntPlayer(key, 0));

		key = LOCALIZATION_DEBUG_MODE;
        Prefs_ForceSetIntPlayer(key, Prefs_GetIntPlayer(key, (int)LocalizationManager.DebugMode.NONE));        
    }
}