// GameEvents.cs
// TS
// 
// Created by Alger Ortín Castellví on 16/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

/// <summary>
/// Collection of events related to the game.
/// </summary>
public enum GameEvents_OLD {
	#region PROFILE EVENTS ---------------------------------------------------------------------------------------------
	PROFILE_COINS_CHANGED = EngineEvents.END,	// params: long iOldAmount, long iNewAmount
	PROFILE_PC_CHANGED,			// params: long iOldAmount, long iNewAmount
	#endregion

	#region GAME LOGIC EVENTS ------------------------------------------------------------------------------------------
	// Game logic
	GAME_STARTED,		// no params
	GAME_ENDED,			// no params
	SCORE_CHANGED,	// params: long iOldAmount, long iNewAmount, GameEntity entity
	SCORE_MULTIPLIER_CHANGED,	// params: ScoreMultiplier _oldMultiplier, ScoreMultiplier _newMultiplier
	FURY_CHANGED,		// params: float fOldAmount, float fNewAmount
	FURY_RUSH_TOGGLED,	// params: bool bActivated
	#endregion

	#region REWARD EVENTS ----------------------------------------------------------------------------------------------
	REWARD_SCORE,	// params: long iAmount, GameEntity entity
	REWARD_COINS,	// params: long iAmount, GameEntity entity
	#endregion

	#region ENTITY EVENTS ----------------------------------------------------------------------------------------------
	ENTITY_EATEN,		// params: GameEntity entity
	ENTITY_BURNED,	// params: GameEntity entity
	#endregion

	#region PLAYER EVENTS ----------------------------------------------------------------------------------------------
	PLAYER_DAMAGE_RECEIVED,		// params: float fDamage, DamageDealer source
	PLAYER_STARVING_TOGGLED,	// params: bool bIsStarving
	PLAYER_STATE_CHANGED,			// params: DragonPlayer.EState _oldState, DragonPlayer.EState _newState
	#endregion

	#region COLLECTIBLE EVENTS -----------------------------------------------------------------------------------------
	COLLECTIBLE_COLLECTED		// params: Collectible collectible
	#endregion
}