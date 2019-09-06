using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// KEEP EVENTS ORDER AS IT IS TO AVOID EDITOR CHANGING VALUES
public enum BroadcastEventType
{
    /////// DO NOT ADD NEW EVENTS OF MODIFY ///////
    // Popups Management
    POPUP_CREATED,          // param: PopupManagementInfo
    POPUP_OPENED,           // param: PopupManagementInfo
    POPUP_CLOSED,           // param: PopupManagementInfo
    POPUP_DESTROYED,        // param: PopupManagementInfo
    
    // Egg management events
    EGG_STATE_CHANGED,          // params: Egg _egg, Egg.State _from, Egg.State _to
    
    // Rules and localization
    LANGUAGE_CHANGED,       // no params
    FONT_CHANGE_STARTED,    // no params
    FONT_CHANGE_FINISHED,   // no params

    // Profile events
    PROFILE_MAP_UNLOCKED,       // no params

    // power up events
    APPLY_ENTITY_POWERUPS,      // no params
    
    FURY_RUSH_TOGGLED,          // param: FuryRushToggled
    
    // UI events
    UI_MAP_ZOOM_CHANGED,        // param: UIMapZoomChanged
    UI_MAP_EXPIRED,				// no params

    GAME_LEVEL_LOADED,
    GAME_AREA_ENTER,
    GAME_AREA_EXIT,
    GAME_ENDED,
    
    BOOST_TOGGLED,              // params: ToggleParam
    SPECIAL_POWER_TOGGLED,      // params: ToggleParam

	// Debug
	DEBUG_REFRESH_DAILY_REWARDS,    // no params
    
    SHIELD_HIT,
    
    POOL_MANAGER_READY,
    GAME_PAUSED,                // params: ToggleParam

	SEASON_CHANGED,				// params: oldSeasonSku, newSeasonSku

    /////// NEW EVENTS HERE!!! ///////

    // EGG_INCUBATION_STARTED,     // params: Egg _egg
    // EGG_INCUBATION_ENDED,       // params: Egg _egg
    // EGG_TAP,                    // params: EggController _egg, int _tapCount    // [AOC] Triggered when opening an egg
    // EGG_OPENED,                 // params: Egg _egg     // [AOC] Triggered when any egg is opened and its reward collected, whether it is the one in the incubator or one purchased from the shop


    COUNT,
}

public class BroadcastEventInfo
{       
}

public class FuryRushToggled : BroadcastEventInfo
{
    public bool activated = false;
    public DragonBreathBehaviour.Type type = DragonBreathBehaviour.Type.None;
    public FireColorSetupManager.FireColorType color = FireColorSetupManager.FireColorType.RED;
}

public class UIMapZoomChanged : BroadcastEventInfo
{
    public float zoomFactor = 1;
}

public class PopupManagementInfo : BroadcastEventInfo
{
    public PopupController popupController = null;
}

public class EggStateChanged : BroadcastEventInfo
{
    public Egg egg = null;
    public Egg.State from = Egg.State.COLLECTED;
    public Egg.State to = Egg.State.COLLECTED;
}

public class ToggleParam : BroadcastEventInfo
{
    public bool value = false;
}

public class ShieldHit : BroadcastEventInfo
{
    public float value = 0;
    public bool broken = false;
    public bool bigHit = false;
}

public class SeasonChangedEventInfo : BroadcastEventInfo {
	public string oldSeasonSku = "";
	public string newSeasonSku = "";
}