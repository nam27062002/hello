// GameEvents.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

/// <summary>
/// Collection of events related to the game.
/// </summary>
public static class GameEvents {
	// Profile events
	public const string PROFILE_COINS_CHANGED = "PROFILE_COINS_CHANGED";	// params: long _oldAmount, long _newAmount
	public const string PROFILE_PC_CHANGED = "PROFILE_PC_CHANGED";			// params: long _oldAmount, long _newAmount
	
	// Game logic events
	public const string GAME_COUNTDOWN_STARTED = "GAME_COUNTDOWN_STARTED";		// no params
	public const string GAME_STARTED = "GAME_STARTED";		// no params
	public const string GAME_PAUSED = "GAME_PAUSED";		// params: bool _paused
	public const string GAME_ENDED = "GAME_ENDED";			// no params
	public const string REWARD_APPLIED = "REWARD_APPLIED";	// params: Reward _reward, Transform _entity
	public const string SCORE_MULTIPLIER_CHANGED = "SCORE_MULTIPLIER_CHANGED";	// params: ScoreMultiplier _oldMultiplier, ScoreMultiplier _newMultiplier
	public const string FURY_RUSH_TOGGLED = "FURY_RUSH_TOGGLED";	// params: bool _activated
	public const string HUNT_EVENT_TOGGLED = "HUNT_EVENT_TOGGLED";	// params: Transform _entityLocation, bool _activated

	// Entity events
	public const string ENTITY_EATEN = "ENTITY_EATEN";			// params: Transform _entity, Reward _reward
	public const string ENTITY_BURNED = "ENTITY_BURNED";		// params: Transform _entity, Reward _reward
	public const string ENTITY_DESTROYED = "ENTITY_DESTROYED";	// params: Transform _entity, Reward _reward

	// Player events
	public const string PLAYER_DAMAGE_RECEIVED = "PLAYER_DAMAGE_RECEIVED";		// params: float _damage, DamageDealer _source
	public const string PLAYER_STARVING_TOGGLED = "PLAYER_STARVING_TOGGLED";	// params: bool _isStarving
	public const string PLAYER_STATE_CHANGED = "PLAYER_STATE_CHANGED";			// params: DragonPlayer.EState _oldState, DragonPlayer.EState _newState
	public const string PLAYER_DIED = "PLAYER_DIED";							// no params

	// Collectible events
	public const string COLLECTIBLE_COLLECTED = "COLLECTIBLE_COLLECTED";		// params: Collectible _collectible

	// Dragon collection events
	public const string DRAGON_LEVEL_UP = "DRAGON_LEVEL_UP";					// params: DragonData _data
	public const string DRAGON_SKILL_UPGRADED = "DRAGON_SKILL_UPGRADED";		// params: DragonSkill _skill	// [AOC] TODO!! We might want to know whose dragon this skill belongs to - figure out how
}