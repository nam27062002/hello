// GameEvents.cs
// TS
// 
// Created by Alger Ortín Castellví on 16/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

/// <summary>
/// Collection of events related to the game.
/// </summary>
public static class GameEvents {
	#region PROFILE EVENTS ---------------------------------------------------------------------------------------------
	public const string PROFILE_COINS_CHANGED = "PROFILE_COINS_CHANGED";	// params: long iOldAmount, long iNewAmount
	public const string PROFILE_PC_CHANGED = "PROFILE_PC_CHANGED";			// params: long iOldAmount, long iNewAmount
	#endregion

	#region GAME LOGIC EVENTS ------------------------------------------------------------------------------------------
	// Game logic
	public const string GAME_STARTED = "GAME_STARTED";		// no params
	public const string GAME_ENDED = "GAME_ENDED";			// no params
	public const string SCORE_CHANGED = "SCORE_CHANGED";	// params: long iOldAmount, long iNewAmount, GameEntity entity
	public const string SCORE_MULTIPLIER_CHANGED = "SCORE_MULTIPLIER_CHANGED";	// params: ScoreMultiplier _oldMultiplier, ScoreMultiplier _newMultiplier
	public const string FURY_CHANGED = "FURY_CHANGED";		// params: float fOldAmount, float fNewAmount
	public const string FURY_RUSH_TOGGLED = "FURY_RUSH_TOGGLED";	// params: bool bActivated
	#endregion

	#region REWARD EVENTS ----------------------------------------------------------------------------------------------
	public const string REWARD_SCORE = "REWARD_SCORE";	// params: long iAmount, GameEntity entity
	public const string REWARD_COINS = "REWARD_COINS";	// params: long iAmount, GameEntity entity
	#endregion

	#region ENTITY EVENTS ----------------------------------------------------------------------------------------------
	public const string ENTITY_EATEN = "ENTITY_EATEN";		// params: GameEntity entity
	public const string ENTITY_BURNED = "ENTITY_BURNED";	// params: GameEntity entity
	#endregion

	#region PLAYER EVENTS ----------------------------------------------------------------------------------------------
	public const string PLAYER_DAMAGE_RECEIVED = "PLAYER_DAMAGE_RECEIVED";		// params: float fDamage, DamageDealer source
	public const string PLAYER_STARVING_TOGGLED = "PLAYER_STARVING_TOGGLED";	// params: bool bIsStarving
	public const string PLAYER_STATE_CHANGED = "PLAYER_STATE_CHANGED";			// params: DragonPlayer.EState _oldState, DragonPlayer.EState _newState
	#endregion

	#region COLLECTIBLE EVENTS -----------------------------------------------------------------------------------------
	public const string COLLECTIBLE_COLLECTED = "COLLECTIBLE_COLLECTED";		// params: Collectible collectible
	#endregion
}