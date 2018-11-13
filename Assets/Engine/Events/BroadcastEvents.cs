using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BroadcastEventType
{
    // Rules and localization
    LANGUAGE_CHANGED,       // no params
    FONT_CHANGE_STARTED,    // no params
    FONT_CHANGE_FINISHED,   // no params

    // Profile events
    PROFILE_MAP_UNLOCKED,       // no params

    // power up events
    APPLY_ENTITY_POWERUPS,      // no params
    
    FURY_RUSH_TOGGLED,          // params: FuryRushToggled
    
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
