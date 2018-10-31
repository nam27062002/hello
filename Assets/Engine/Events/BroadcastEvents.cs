using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BroadcastEventType
{

    // power up events
    APPLY_ENTITY_POWERUPS,      // no params
    
    GAME_LEVEL_LOADED,
    GAME_AREA_ENTER,
    GAME_AREA_EXIT,
    GAME_ENDED,
    COUNT,
}

public class BroadcastEventInfo
{       
}

