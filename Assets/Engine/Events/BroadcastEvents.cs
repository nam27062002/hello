using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BroadcastEventType
{
    // Popups Management
    POPUP_CREATED,          // param: PopupManagementInfo
    POPUP_OPENED,           // param: PopupManagementInfo
    POPUP_CLOSED,           // param: PopupManagementInfo
    POPUP_DESTROYED,        // param: PopupManagementInfo
    
    // Egg management events
    EGG_STATE_CHANGED,          // params: Egg _egg, Egg.State _from, Egg.State _to
    // EGG_INCUBATION_STARTED,     // params: Egg _egg
    // EGG_INCUBATION_ENDED,       // params: Egg _egg
    // EGG_TAP,                    // params: EggController _egg, int _tapCount    // [AOC] Triggered when opening an egg
    // EGG_OPENED,                 // params: Egg _egg     // [AOC] Triggered when any egg is opened and its reward collected, whether it is the one in the incubator or one purchased from the shop

    
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
    UI_MAP_ZOOM_CHANGED,         // param: UIMapZoomChanged
    
    
    GAME_LEVEL_LOADED,
    GAME_AREA_ENTER,
    GAME_AREA_EXIT,
    GAME_ENDED,
    COUNT,
}

public class BroadcastEventInfo
{       
}

public class FuryRushToggled : BroadcastEventInfo
{
    public bool activated = false;
    public DragonBreathBehaviour.Type type = DragonBreathBehaviour.Type.None;
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