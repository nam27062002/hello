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
	public const string PROFILE_COINS_CHANGED = "StatsCoinsChanged";	// params: long iOldAmount, long iNewAmount
	public const string PROFILE_PC_CHANGED = "StatsPCChanged";			// params: long iOldAmount, long iNewAmount
	#endregion

	#region GAME LOGIC EVENTS ------------------------------------------------------------------------------------------
	// Game logic
	public const string GAME_STARTED = "GameStarted";	// no params
	public const string GAME_ENDED = "GameEnded";		// no params
	public const string SCORE_CHANGED = "ScoreChanged";	// params: long iOldAmount, long iNewAmount, GameEntity entity
	public const string SCORE_MULTIPLIER_CHANGED = "ScoreMultiplierChanged";	// params: ScoreMultiplier _oldMultiplier, ScoreMultiplier _newMultiplier
	public const string FURY_CHANGED = "FuryChanged";	// params: float fOldAmount, float fNewAmount
	public const string FURY_RUSH_TOGGLED = "FuryRushToggled";	// params: bool bActivated
	#endregion

	#region REWARD EVENTS ----------------------------------------------------------------------------------------------
	public const string REWARD_SCORE = "RewardScore";	// params: long iAmount, GameEntity entity
	public const string REWARD_COINS = "RewardCoins";	// params: long iAmount, GameEntity entity
	#endregion

	#region ENTITY EVENTS ----------------------------------------------------------------------------------------------
	public const string ENTITY_EATEN = "EntityEaten";	// params: GameEntity entity
	public const string ENTITY_BURNED = "EntityBurned";	// params: GameEntity entity
	#endregion

	#region PLAYER EVENTS ----------------------------------------------------------------------------------------------
	public const string PLAYER_IMPACT_RECEIVED = "PlayerImpactReceived";	// params: float fDamage, DamageDealer source
	public const string PLAYER_STARVING_TOGGLED = "PlayerStarvingToggled";	// params: bool bIsStarving
	#endregion

	#region COLLECTIBLE EVENTS -----------------------------------------------------------------------------------------
	public const string COLLECTIBLE_COLLECTED = "CollectibleCollected";		// params: Collectible collectible
	#endregion
}