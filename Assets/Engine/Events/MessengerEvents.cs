// GameEvents.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//------------------------------------------------------------------------//
// CONSTANTS															  //
//------------------------------------------------------------------------//
/// <summary>
/// Collection of events related to the game.
/// Please keep the params documented!
/// </summary>
public enum MessengerEvents {
	// Game Scene Manager
	SCENE_STATE_CHANGED = 0,	// params: SceneManager.ESceneState _oldState, SceneManager.ESceneState _newState
    SCENE_PREUNLOAD,        // params: string _sceneName: The scene is about to be unloaded. Listeners to this event can be sure that all game object in the scene still exist
    SCENE_UNLOADED,			// params: string _sceneName
	SCENE_LOADED,			// params: string _sceneName

	// Popups Management
	// POPUP_CREATED,			// params: PopupController _popup
	// POPUP_OPENED,			// params: PopupController _popup
	// POPUP_CLOSED,			// params: PopupController _popup
	// POPUP_DESTROYED,		// params: PopupController _popup

	// Screen Navigation System
	// [AOC] Triggered at the start of the animation, parameters englobed in a custom class:
	NAVIGATION_SCREEN_CHANGED,		// params: NavigationScreenSystem.ScreenChangedEvent _eventData

	// Rules and localization
	// LANGUAGE_CHANGED,		// no params
	// FONT_CHANGE_STARTED,	// no params
	// FONT_CHANGE_FINISHED,	// no params
	DEFINITIONS_LOADED,		// no params

	// Tech
	GOOGLE_PLAY_STATE_UPDATE,// no params
	GOOGLE_PLAY_AUTH_FAILED,// no params
	GOOGLE_PLAY_AUTH_CANCELLED,// no params
	CONNECTION_RECOVERED,
	PERSISTENCE_SYNC_CHANGED,   // paramas: bool (whether or not local and cloud persistences are synced)
	APPLICATION_QUIT,

	// Store Transactions
	PURCHASE_SUCCESSFUL,	// string _productSku (TODO: _transactionData? _purchaseId?)
	PURCHASE_FAILED,		// string _productSku (TODO: _purchaseId?)
	PURCHASE_CANCELLED,		// string _productSku (TODO: _purchaseId?)
	PURCHASE_ERROR,			// string _productSku (TODO: _purchaseId?)
	PURCHASE_FINISHED,		// string _productSku
	PURCHASE_RECEIVED_PRODUCTS_AVAILABILITY,	// no params

	// UI Events
	UI_LOCK_INPUT,			// bool _lock

	// Debug and control panel events
	DEBUG_MENU_DRAGON_SELECTED,
	DEBUG_SIMULATION_FINISHED,
	DEBUG_UNLOCK_LEVELS,
	DEBUG_REFRESH_MISSION_INFO,	// no params
	CP_PREF_CHANGED,			// params _string _prefID
	CP_BOOL_CHANGED,			// params: string _prefID, bool _newValue
	CP_STRING_CHANGED,			// params: string _prefID, string _newValue
	CP_INT_CHANGED,				// params: string _prefID, int _newValue
	CP_FLOAT_CHANGED,			// params: string _prefID, float _newValue
	CP_ENUM_CHANGED,			// params: string _prefID, int _newValue (should be casted to target enum)
    CP_QUALITY_CHANGED,         // no params

	// Game Core events
	GAME_MODE_CHANGED,			// params: SceneController.Mode _oldMode, SceneController.Mode _newMode

	// Profile events
	PROFILE_CURRENCY_CHANGED,	// params: UserProfile.Currency _currency, long _oldAmount, long _newAmount
	// PROFILE_MAP_UNLOCKED,		// no params
	PROFILE_REWARD_PUSHED,		// params Metagame.Reward _reward
	PROFILE_REWARD_POPPED,		// params Metagame.Reward _reward
	TUTORIAL_STEP_TOGGLED,		// params: TutorialStep _step, bool _completed
	
	// Game logic events
	// GAME_LEVEL_LOADED,			// no params
	GAME_STARTED,				// no params
	GAME_COUNTDOWN_STARTED,		// no params
	GAME_COUNTDOWN_ENDED,		// no params
	// GAME_AREA_ENTER,			// no params 
	// GAME_AREA_EXIT,				// no params
	GAME_UPDATED,				// no params
	// GAME_PAUSED,				// params: bool _paused
	// GAME_ENDED,					// no params
	REWARD_APPLIED,				// params: Reward _reward, Transform _entity
	SCORE_MULTIPLIER_CHANGED,	// params: ScoreMultiplier _newMultiplier, int goldScoreMultiplier
	SCORE_MULTIPLIER_LOST,		// no params
    SCORE_MULTIPLIER_FORCE_UP,  // no params
	PREWARM_FURY_RUSH,			// params: DragonBreathBehaviour.Type _type, float duration
	// FURY_RUSH_TOGGLED,			// params: bool _activated, DragonBreathBehaviour.Type _type
	HUNT_EVENT_TOGGLED,			// params: Transform _entityLocation, bool _activated
	SLOW_MOTION_TOGGLED,		// params: bool _activated
	// BOOST_TOGGLED,				// params: bool _activated
	BOOST_SPACE,					// no params
	BIGGER_DRAGON_NEEDED,		// params: DragonTier _requiredTierSku (use COUNT for generic message), string _entitySku
	UNDERWATER_TOGGLED,			// params: bool _activated
    INTOSPACE_TOGGLED,          // params: bool _activated
    BREAK_OBJECT_BIGGER_DRAGON, // no params
	BREAK_OBJECT_NEED_TURBO,	// no params
	BREAK_OBJECT_SHALL_NOT_PASS,// no params
	BREAK_OBJECT_WITH_FIRE,		// no params
    BREAK_OBJECT_TO_OPEN,       // no params
    DARK_ZONE_TOGGLE,           // params: bool _enter / _exit, CandleEffectTrigger
	MISSION_ZONE,   			// params: bool _inside, ZoneTrigger _zone

    // Entity events
    ENTITY_EATEN,				// params: IEntity _entity, Reward _reward
	ENTITY_BURNED,              // params: IEntity _entity, Reward _reward
    ENTITY_DESTROYED,           // params: IEntity _entity, Reward _reward
    BLOCKER_DESTROYED,			// no params
	FLOCK_EATEN,                // params: IEntity _entity, Reward _reward
    STAR_COMBO,
	ENTITY_ESCAPED,             // params: IEntity _entity

    // Player events
    PLAYER_DAMAGE_RECEIVED,		// params: float _damage, DamageType _type, Transform _source
	PLAYER_LOST_SHIELD,			// params: DamageType _type, Transform _originTransform
	PLAYER_HEALTH_MODIFIER_CHANGED,	// params: DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier
	PLAYER_STATE_CHANGED,		// params: DragonPlayer.EState _oldState, DragonPlayer.EState _newState
	PLAYER_KO,					// params: DamageType _type, Transform _source
	PLAYER_DIED,				// no params
	PLAYER_PET_PRE_FREE_REVIVE,// no params
	PLAYER_FREE_REVIVE,			// no params
    PLAYER_MUMMY_REVIVE,        // no params
    PLAYER_REVIVE,				// no params
	PLAYER_LEAVING_AREA,		// no params
	PLAYER_ENTERING_AREA,		// no params
	PLAYER_ASK_PETS_EATING,		// DragonMotion.PetsEatingTest. If one pets is still eating should put the attribute to true to let know the dragon it still cannot change area

	// Collectible events
	CHEST_COLLECTED,			// params: Chest _chest
	CHESTS_RESET,				// no params
	CHESTS_PROCESSED,			// no params					// ChestManager has processed the chests and given the rewards
	EGG_COLLECTED,				// params: CollectibleEgg _egg
	EGG_COLLECTED_FAIL,			// params: CollectibleEgg _egg
	LETTER_COLLECTED,			// params: Reward _r
	EARLY_ALL_HUNGRY_LETTERS_COLLECTED, 	//
	START_ALL_HUNGRY_LETTERS_COLLECTED,			//
	ALL_HUNGRY_LETTERS_COLLECTED,			//
	ANNIVERSARY_CAKE_SLICE_EATEN,
    ANNIVERSARY_LAUNCH_ANIMATION,
    ANNIVERSARY_START_BDAY_MODE,

	SUPER_SIZE_TOGGLE,			// params: bool _activated
	TICKET_COLLECTED,				// no params
	TICKET_COLLECTED_FAIL,			// no params

	// Dragon collection events
	DRAGON_TEASED,				// params: DragonData _data
	DRAGON_ACQUIRED,			// params: DragonData _data
	DRAGON_LEVEL_UP,			// params: DragonData _data

	// Metagame events
	SKIN_ACQUIRED,			// params: string _skinSku
	PET_ACQUIRED,			// params: string _petSku

	// Menu events
	MENU_DRAGON_SELECTED,		 // params: string _selectedDragonSku	// [AOC] Triggered when the dragon hovered in the dragon selection screen changes
	MENU_DRAGON_CONFIRMED,		 // params: string _confirmedDragonSku	// [AOC] Triggered when the dragon hovered on the menu is valid to be used in gameplay (UserProfile.currentDragon updated)
	MENU_DRAGON_DISGUISE_CHANGE, // params: string _dragonSku
	MENU_DRAGON_PET_CHANGE,		 // params: string _dragonSku, int _slotIdx, string _newPetSku

	MENU_SCREEN_TRANSITION_REQUESTED,	// params: MenuScreen _from, MenuScreen _to
	MENU_SCREEN_TRANSITION_START,		// params: MenuScreen _from, MenuScreen _to
	MENU_SCREEN_TRANSITION_END,			// params: MenuScreen _from, MenuScreen _to
	MENU_CAMERA_TRANSITION_START,		// params: MenuScreen _from, MenuScreen _to, bool _usingPath

	// Mission events
	MISSION_COMPLETED,			// params: Mission _mission
	MISSION_REMOVED,			// params: Mission _newMission
	MISSION_SKIPPED,			// params: Mission _mission
	MISSION_UNLOCKED,			// params: Mission _mission
	MISSION_COOLDOWN_FINISHED,	// params: Mission _mission
	MISSION_STATE_CHANGED,		// params: Mission _mission, Mission.State _oldState, Mission.State _newState

	SURVIVAL_BONUS_ACHIEVED,	// no params

	// Egg management events
	// EGG_STATE_CHANGED,			// params: Egg _egg, Egg.State _from, Egg.State _to
	EGG_INCUBATION_STARTED,		// params: Egg _egg
	EGG_INCUBATION_ENDED,		// params: Egg _egg
	EGG_TAP,					// params: EggController _egg, int _tapCount	// [AOC] Triggered when opening an egg
	EGG_OPENED,					// params: Egg _egg		// [AOC] Triggered when any egg is opened and its reward collected, whether it is the one in the incubator or one purchased from the shop

	// GameServerManager events
	LOGGED,						// params: bool	

    // Merge
    MERGE_SUCCEEDED,
    MERGE_FAILED,
    MERGE_SHOW_POPUP_NEEDED,    // params: CaletyConstants.PopupMergeType eType, JSONNode kLocalAccount, JSONNode kCloudAccount

    // Social Platform Manager Events
    SOCIAL_LOGGED,				// params: bool	

	// UI events
	UI_INGAME_PC_FEEDBACK_END,		// no params
	UI_TOGGLE_CURRENCY_COUNTERS,	// params: bool _show
	// UI_MAP_ZOOM_CHANGED,			// params: float _zoomFactor (percentage relative to initial zoom level (0.5x, 1x, 2x, etc, the smaller the closer)
	UI_MAP_CENTER_TO_DRAGON,		// Request centering the map to the dragon! params: float _scrollSpeed (use <= 0 for instant)

	// Camera events
	CAMERA_INTRO_DONE,			// no params
	CAMERA_SHAKE,				// params: float _duration, float _intensity


    // Device events
    DEVICE_RESOLUTION_CHANGED, 	// params: Vector2 _newResolution
    DEVICE_ORIENTATION_CHANGED, // params: DeviceOrientation _newOrientation

    // Settigns events
	GAME_SETTING_TOGGLED,		// params: string settingId, bool _toggled
    TILT_CONTROL_CALIBRATE,		// no params, use to force a tilt calibration (only in-game)
	TILT_CONTROL_SENSITIVITY_CHANGED,	// params: float _sensitivity

	// Global events events (xD)
	GLOBAL_EVENT_CUSTOMIZER_ERROR,
	GLOBAL_EVENT_CUSTOMIZER_NO_EVENTS,

	GLOBAL_EVENT_UPDATED,			// params: _requestType, the manager notifies that has received new data from the server related to the current event. Triggers with all the request types: event definition, event state, leaderboard...
	GLOBAL_EVENT_DATA_UPDATED,
	GLOBAL_EVENT_STATE_UPDATED,
	GLOBAL_EVENT_LEADERBOARD_UPDATED,
	GLOBAL_EVENT_SCORE_REGISTERED,	// params: bool _sucess, the manager notifies whether a contribution has been successfully registered to the server or not

	// Shop/Offers events
	OFFERS_RELOADED,	// no params
	OFFERS_CHANGED,		// no params
	OFFER_APPLIED,		// OfferPack _pack
    HC_PACK_ACQUIRED,  // a HC pack was bought by the player. PARAMS: bool _showPopup: opens the happy hour popup immediately, string offerSku: the purchased offer sku

	// Live Events
	LIVE_EVENT_STATES_UPDATED,
	LIVE_EVENT_NEW_DEFINITION,
	LIVE_EVENT_REWARDS_RECEIVED,
	LIVE_EVENT_FINISHED,
	TOURNAMENT_LEADERBOARD,
	TOURNAMENT_SCORE_SENT,
	TOURNAMENT_ENTRANCE,
	QUEST_SCORE_UPDATED,
	QUEST_SCORE_SENT,
	TIMES_UP,
	TARGET_REACHED,

	// Lab/Special Dragons
	//SPECIAL_DRAGON_STAT_UPGRADED,	// params: DragonDataSpecial _dragonData, DragonDataSpecial.Stat _stat
	SPECIAL_DRAGON_POWER_UPGRADED,	// params: DragonDataSpecial _dragonData
	//SPECIAL_DRAGON_TIER_UPGRADED,	// params: DragonDataSpecial _dragonData
    SPECIAL_DRAGON_LEVEL_UPGRADED,	// params: DragonDataSpecial _dragonData

    // Modifiers
    MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED,  // params: IDragonData

	COUNT
}

//------------------------------------------------------------------------//
// CLASSES																  //
// For events requiring 4 or more parameters, where we can't use the	  //
// templated Messenger methods.											  //
//------------------------------------------------------------------------//