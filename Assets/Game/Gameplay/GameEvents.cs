﻿// GameEvents.cs
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
	FURY_RUSH_TOGGLED,			// params: bool _activated
	HUNT_EVENT_TOGGLED,			// params: Transform _entityLocation, bool _activated

	// Entity events
	ENTITY_EATEN,		// params: Transform _entity, Reward _reward
	ENTITY_BURNED,		// params: Transform _entity, Reward _reward
	ENTITY_DESTROYED,	// params: Transform _entity, Reward _reward

	// Player events
	PLAYER_DAMAGE_RECEIVED,		// params: float _damage, Transform _source
	PLAYER_STARVING_TOGGLED,	// params: bool _isStarving
	PLAYER_STATE_CHANGED,		// params: DragonPlayer.EState _oldState, DragonPlayer.EState _newState
	PLAYER_DIED,				// no params

	// Collectible events
	COLLECTIBLE_COLLECTED,		// params: Collectible _collectible

	// Dragon collection events
	DRAGON_ACQUIRED,			// params: DragonData _data
	DRAGON_LEVEL_UP,			// params: DragonData _data
	DRAGON_SKILL_UPGRADED,		// params: DragonSkill _skill	// [AOC] TODO!! We might want to know whose dragon this skill belongs to - figure out how

	// Menu events
	MENU_DRAGON_SELECTED,	// params: string _selectedDragonSku
	MENU_LEVEL_SELECTED,	// params: string _selectedLevelSku

	// Mission events
	MISSION_COMPLETED,			// params: Mission _mission
	MISSION_REMOVED,			// params: Mission _newMission
	MISSION_SKIPPED,			// params: Mission _mission
	MISSION_UNLOCKED,			// params: Mission _mission
	MISSION_COOLDOWN_FINISHED,	// params: Mission _mission
	MISSION_STATE_CHANGED		// params: Mission _mission, Mission.State _oldState, Mission.State _newState
}