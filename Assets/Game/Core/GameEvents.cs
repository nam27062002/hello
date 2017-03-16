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
public enum GameEvents {
	// Debug and control panel events
	DEBUG_MENU_DRAGON_SELECTED = EngineEvents.END,
	DEBUG_SIMULATION_FINISHED,
	DEBUG_UNLOCK_LEVELS,
	CP_PREF_CHANGED,			// params _string _prefID
	CP_BOOL_CHANGED,			// params: string _prefID, bool _newValue
	CP_STRING_CHANGED,			// params: string _prefID, string _newValue
	CP_INT_CHANGED,				// params: string _prefID, int _newValue
	CP_FLOAT_CHANGED,			// params: string _prefID, float _newValue
	CP_ENUM_CHANGED,			// params: string _prefID, int _newValue (should be casted to target enum)
    CP_QUALITY_CHANGED,         // no params

	// Profile events
	PROFILE_COINS_CHANGED,		// params: long _oldAmount, long _newAmount
	PROFILE_PC_CHANGED,			// params: long _oldAmount, long _newAmount
	PROFILE_CURRENCY_CHANGED,	// params: UserProfile.Currency _currency, long _oldAmount, long _newAmount
	PROFILE_MAP_UPGRADED,		// params: int mapLevel
	
	// Game logic events
	GAME_LEVEL_LOADED,			// no params
	GAME_STARTED,				// no params
	GAME_COUNTDOWN_STARTED,		// no params
	GAME_COUNTDOWN_ENDED,		// no params
	GAME_PAUSED,				// params: bool _paused
	GAME_ENDED,					// no params
	REWARD_APPLIED,				// params: Reward _reward, Transform _entity
	SCORE_MULTIPLIER_CHANGED,	// params: ScoreMultiplier _newMultiplier, int goldScoreMultiplier
	SCORE_MULTIPLIER_LOST,		// no params
	FURY_RUSH_TOGGLED,			// params: bool _activated, DragonBreathBehaviour.Type _type
	HUNT_EVENT_TOGGLED,			// params: Transform _entityLocation, bool _activated
	SLOW_MOTION_TOGGLED,		// params: bool _activated
	BOOST_TOGGLED,				// params: bool _activated
	DRUNK_TOGGLED,				// params: bool _isDrunk
	BIGGER_DRAGON_NEEDED,		// params: DragonTier _requiredTierSku (use COUNT for generic message), string _entitySku
	UNDERWATER_TOGGLED,			// params: bool _activated
	BREAK_OBJECT_BIGGER_DRAGON, // no params
	BREAK_OBJECT_NEED_TURBO,	// no params

	// Entity events
	ENTITY_EATEN,				// params: Transform _entity, Reward _reward
	ENTITY_BURNED,				// params: Transform _entity, Reward _reward
	ENTITY_DESTROYED,			// params: Transform _entity, Reward _reward
	FLOCK_EATEN,				// params: Transform _entity, Reward _reward
	ENTITY_ESCAPED,				// params: Transform _entity

	// Player events
	PLAYER_DAMAGE_RECEIVED,		// params: float _damage, DamageType _type, Transform _source
	PLAYER_LOST_SHIELD,			// params: DamageType _type, Transform _originTransform
	PLAYER_HEALTH_MODIFIER_CHANGED,	// params: DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier
	PLAYER_STATE_CHANGED,		// params: DragonPlayer.EState _oldState, DragonPlayer.EState _newState
	PLAYER_KO,					// params: DamageType
	PLAYER_DIED,				// no params
	PLAYER_PET_PRE_FREE_REVIVE,			// no params
	PLAYER_FREE_REVIVE,			// no params
	PLAYER_REVIVE,				// no params

	// Collectible events
	CHEST_COLLECTED,			// params: Chest _chest
	CHESTS_RESET,				// no params
	CHESTS_PROCESSED,			// no params					// ChestManager has processed the chests and given the rewards
	EGG_COLLECTED,				// params: CollectibleEgg _egg
	EGG_COLLECTED_FAIL,			// params: CollectibleEgg _egg
	LETTER_COLLECTED,			// params: Reward _r
	EARLY_ALL_HUNGRY_LETTERS_COLLECTED, 	//
	ALL_HUNGRY_LETTERS_COLLECTED,			//
	SUPER_SIZE_TOGGLE,			// params: bool _activated

	// Dragon collection events
	DRAGON_ACQUIRED,			// params: DragonData _data
	DRAGON_LEVEL_UP,			// params: DragonData _data

	// Menu events
	MENU_DRAGON_SELECTED,		 // params: string _selectedDragonSku	// [AOC] Triggered when the dragon hovered in the dragon selection screen changes
	MENU_DRAGON_CONFIRMED,		 // params: string _confirmedDragonSku	// [AOC] Triggered when the dragon hovered on the menu is valid to be used in gameplay (UserProfile.currentDragon updated)
	MENU_DRAGON_DISGUISE_CHANGE, // params: string _dragonSku
	MENU_DRAGON_PET_CHANGE,		 // params: string _dragonSku, int _slotIdx, string _newPetSku

	// Mission events
	MISSION_COMPLETED,			// params: Mission _mission
	MISSION_REMOVED,			// params: Mission _newMission
	MISSION_SKIPPED,			// params: Mission _mission
	MISSION_UNLOCKED,			// params: Mission _mission
	MISSION_COOLDOWN_FINISHED,	// params: Mission _mission
	MISSION_STATE_CHANGED,		// params: Mission _mission, Mission.State _oldState, Mission.State _newState

	SURVIVAL_BONUS_ACHIEVED,	// no params

	// Egg management events
	EGG_STATE_CHANGED,			// params: Egg _egg, Egg.State _from, Egg.State _to
	EGG_INCUBATION_STARTED,		// params: Egg _egg
	EGG_INCUBATION_ENDED,		// params: Egg _egg
	EGG_TAP,					// params: EggController _egg, int _tapCount	// [AOC] Triggered when opening an egg
	EGG_OPENED,					// params: Egg _egg		// [AOC] Triggered when any egg is opened and its reward collected, whether it is the one in the incubator or one purchased from the shop

	// GameServerManager events
	LOGGED,						// params: bool	

	// Social Platform Manager Events
	SOCIAL_LOGGED,				// params: bool	
	
	// UI events
	UI_INGAME_PC_FEEDBACK_END,		// no params
	UI_TOGGLE_CURRENCY_COUNTERS,	// bool _show

	// Camera events
	CAMERA_INTRO_DONE,			// no params
	CAMERA_SHAKE,				// params: float _duration, float _intensity

	// power up events
	APPLY_ENTITY_POWERUPS,		// no params

	APPLICATION_QUIT,

    // Device events
    DEVICE_RESOLUTION_CHANGED,  // params: Vector2 _newResolution
    DEVICE_ORIENTATION_CHANGED  // params: DeviceOrientation _newOrientation
}

//------------------------------------------------------------------------//
// CLASSES																  //
// For events requiring 4 or more parameters, where we can't use the	  //
// templated Messenger methods.											  //
//------------------------------------------------------------------------//