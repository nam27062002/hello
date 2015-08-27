﻿// GameEvents.cs
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
	public const string GAME_STARTED = "GAME_STARTED";		// no params
	public const string GAME_ENDED = "GAME_ENDED";			// no params
	public const string SCORE_CHANGED = "SCORE_CHANGED";	// params: long _oldAmount, long _newAmount, GameEntity _entity
	public const string SCORE_MULTIPLIER_CHANGED = "SCORE_MULTIPLIER_CHANGED";	// params: ScoreMultiplier _oldMultiplier, ScoreMultiplier _newMultiplier
	public const string FURY_CHANGED = "FURY_CHANGED";		// params: float _oldAmount, float _newAmount
	public const string FURY_RUSH_TOGGLED = "FURY_RUSH_TOGGLED";	// params: bool _activated
	public const string HUNT_EVENT_TOGGLED = "HUNT_EVENT_TOGGLED";	// params: Transform _entityLocation, bool _activated

	// Reward events
	public const string REWARD_SCORE = "REWARD_SCORE";	// params: long _amount, GameEntity _entity
	public const string REWARD_COINS = "REWARD_COINS";	// params: long _amount, GameEntity _entity

	// Entity events
	public const string ENTITY_EATEN = "ENTITY_EATEN";		// params: GameEntity _entity
	public const string ENTITY_BURNED = "ENTITY_BURNED";	// params: GameEntity _entity

	// Player events
	public const string PLAYER_DAMAGE_RECEIVED = "PLAYER_DAMAGE_RECEIVED";		// params: float _damage, DamageDealer _source
	public const string PLAYER_STARVING_TOGGLED = "PLAYER_STARVING_TOGGLED";	// params: bool _isStarving
	public const string PLAYER_STATE_CHANGED = "PLAYER_STATE_CHANGED";			// params: DragonPlayer.EState _oldState, DragonPlayer.EState _newState
	public const string PLAYER_DIED = "PLAYER_DIED";							// no params

	// Collectible events
	public const string COLLECTIBLE_COLLECTED = "COLLECTIBLE_COLLECTED";		// params: Collectible _collectible
}