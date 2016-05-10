// GameEvents.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

/// <summary>
/// Collection of events related to the game.
/// Please keep the params documented!
/// </summary>
public enum GameEvents {
	// Debug events
	DEBUG_MENU_DRAGON_SELECTED = EngineEvents.END,
	DEBUG_SIMULATION_FINISHED,
	DEBUG_UNLOCK_LEVELS,

	// Profile events
	PROFILE_COINS_CHANGED,		// params: long _oldAmount, long _newAmount
	PROFILE_PC_CHANGED,			// params: long _oldAmount, long _newAmount
	
	// Game logic events
	GAME_LEVEL_LOADED,			// no params
	GAME_STARTED,				// no params
	GAME_COUNTDOWN_STARTED,		// no params
	GAME_COUNTDOWN_ENDED,		// no params
	GAME_PAUSED,				// params: bool _paused
	GAME_ENDED,					// no params
	REWARD_APPLIED,				// params: Reward _reward, Transform _entity
	SCORE_MULTIPLIER_CHANGED,	// params: ScoreMultiplier _oldMultiplier, ScoreMultiplier _newMultiplier
	SCORE_MULTIPLIER_LOST,		// no params
	FURY_RUSH_TOGGLED,			// params: bool _activated, DragonBreathBehaviour.Type _type
	HUNT_EVENT_TOGGLED,			// params: Transform _entityLocation, bool _activated
	SLOW_MOTION_TOGGLED,		// params: bool _activated
	BOOST_TOGGLED,				// params: bool _activated
	BIGGER_DRAGON_NEEDED,		// params: DragonTier _requiredTierSku (use COUNT for generic message)

	// Entity events
	ENTITY_EATEN,		// params: Transform _entity, Reward _reward
	ENTITY_BURNED,		// params: Transform _entity, Reward _reward
	ENTITY_DESTROYED,	// params: Transform _entity, Reward _reward
	FLOCK_EATEN,		// params: Transform _entity, Reward _reward
	ENTITY_ESCAPED,		// params: Transform _entity

	// Player events
	PLAYER_DAMAGE_RECEIVED,		// params: float _damage, Transform _source
	PLAYER_STARVING_TOGGLED,	// params: bool _isStarving
	PLAYER_CRITICAL_TOGGLED,	// params: bool _isCritical
	PLAYER_STATE_CHANGED,		// params: DragonPlayer.EState _oldState, DragonPlayer.EState _newState
	PLAYER_KO,					// no params
	PLAYER_DIED,				// no params
	PLAYER_FREE_REVIVE,			// no params
	PLAYER_CURSED,				// no params

	// Collectible events
	COLLECTIBLE_COLLECTED,		// params: Collectible _collectible
	CHEST_COLLECTED,			// params: Chest _chest

	// Dragon collection events
	DRAGON_ACQUIRED,			// params: DragonData _data
	DRAGON_LEVEL_UP,			// params: DragonData _data
	DRAGON_SKILL_UPGRADED,		// params: DragonSkill _skill	// [AOC] TODO!! We might want to know whose dragon this skill belongs to - figure out how

	// Menu events
	MENU_DRAGON_SELECTED,		 // params: string _selectedDragonSku	// [AOC] Triggered when the dragon hovered in the dragon selection screen changes
	MENU_DRAGON_CONFIRMED,		 // params: string _confirmedDragonSku	// [AOC] Triggered when the dragon hovered on the menu is valid to be used in gameplay (UserProfile.currentDragon updated)
	MENU_DRAGON_DISGUISE_CHANGE, // params: string _dragonSku
	MENU_LEVEL_SELECTED,		 // params: string _selectedLevelSku

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
	EGG_ADDED_TO_INVENTORY,		// params: Egg _egg, int _slotIdx
	EGG_INCUBATION_STARTED,		// params: Egg _egg
	EGG_INCUBATION_ENDED,		// params: Egg _egg
	EGG_COLLECTED,				// params: Egg _egg				// [AOC] Triggered when any egg is collected, whether it is the one in the incubator or one purchased from the shop
	EGG_INCUBATOR_CLEARED,		// no params					// [AOC] Triggered when the egg in the incubator is collected. Use this whenever possible rather than EGG_COLLECTED
	EGG_DRAG_STARTED,			// params: EggController _egg
	EGG_DRAG_ENDED				// params: EggController _egg
}