// GameLogic.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Aux class to represent the score multipliers.
/// </summary>
[Serializable]
public class ScoreMultiplier {
	public float multiplier = 1;
	public int requiredKillStreak = 1;	// Both eat and burn count - TODO!! Solve the burn+eat counting twice
	public List<UIFeedbackMessage> feedbackMessages = new List<UIFeedbackMessage>();
}

/// <summary>
/// Main game control logic.
/// </summary>
public class GameLogic : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------
	public enum EStates {
		INIT,
		COUNTDOWN,
		RUNNING,
		PAUSED,
		FINISHED
	}
	#endregion

	#region EXPOSED CONFIGURATION --------------------------------------------------------------------------------------
	// [AOC] We want these to be consulted but never set from outside (except the inspector), so don't add a setter
	[SerializeField] private float _FURY_RUSH_THRESHOLD = 150;	// When to trigger the fury rush! Scale equivalent to score
	public float FURY_RUSH_THRESHOLD {
		get { return _FURY_RUSH_THRESHOLD; }
	}

	[SerializeField] private float _FURY_RUSH_DURATION = 15;	// Seconds
	public float FURY_RUSH_DURATION {
		get { return _FURY_RUSH_DURATION; }
	}

	[SerializeField] private float _SCORE_MULTIPLIER_DURATION = 15;	// Time before ending the current killing streak, seconds
	public float SCORE_MULTIPLIER_DURATION {
		get { return _SCORE_MULTIPLIER_DURATION; }
	}

	[SerializeField] private ScoreMultiplier[] SCORE_MULTIPLIERS;
	#endregion

	#region PROPERTIES -------------------------------------------------------------------------------------------------
	// [AOC] We want these to be consulted but never set from outside, so don't add a setter
	// Score
	private long mScore;
	public long score { 
		get { return mScore; }
	}

	// Score multiplier
	private int mScoreMultiplierIdx;
	public float scoreMultiplier {
		get { return SCORE_MULTIPLIERS[mScoreMultiplierIdx].multiplier; }
	}
	
	// Fury/Gold Rush/Rage
	private float mFury;
	public float fury {
		get { return mFury; }
	}

	// Time
	private float mElapsedSeconds = 0;
	public float elapsedSeconds {
		get { return mElapsedSeconds; }
	}

	// Logic state
	private EStates mState = EStates.INIT;
	public EStates state {
		get { return mState; }
	}

	// Reference to player
	private DragonPlayer mPlayer = null;
	public DragonPlayer player {
		get { return mPlayer; }
	}
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	float mFuryRushTimer = -1;	// Timer of the fury rush
	float mTimer = -1;	// Misc use
	float mScoreMultiplierTimer = -1;	// Time to end the current killing streak
	int mScoreMultiplierStreak = 0;		// Amount of consecutive eaten/burnt entities without taking damage
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Pre-initialization.
	/// </summary>
	void Awake() {
		// [PAC] HACK!! Small hack to load the dragon even if we don't come from menu
		if(state == EStates.INIT 
		   && Application.loadedLevelName == FlowManager.GetSceneName(FlowManager.EScenes.GAME)) {
			//StartGame();
		}
	}

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Nothing to do for now

		// [AOC] HACK!! Small hack to be able to start the game directly from the game scene
		if(state == EStates.INIT 
		&& !Application.loadedLevelName.Equals(FlowManager.GetSceneName(FlowManager.EScenes.MAIN_MENU))) {
			StartGame();
		}
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update() {
		// Different actions based on current state
		switch(mState) {
			// During countdown, let's just wait
			case EStates.COUNTDOWN: {
				// [AOC] TODO!! Show some feedback
				mTimer -= Time.deltaTime;
				if(mTimer <= 0) {
					ChangeState(EStates.RUNNING);
				}
			} break;

			case EStates.RUNNING: {
				// Update time
				mElapsedSeconds += Time.deltaTime;

				// Update fury rush
				if(mFuryRushTimer > 0) {
					// Update fury amount (decrease the same proportion as elapsed time)
					float fDelta = Time.deltaTime/FURY_RUSH_DURATION;
					AddFury(-(fDelta * FURY_RUSH_THRESHOLD));

					// Update timer
					mFuryRushTimer -= Time.deltaTime;
					if(mFuryRushTimer <= 0) {
						// Fury rush has ended!!
						// Reset fury
						// [AOC] TODO!! HSE Starts with the fury meter filled to some percentage based on shark type (and maybe some other params)
						mFury = 0;

						// Dispatch game event
						Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
					}
				}

				// Update score multiplier
				if(mScoreMultiplierTimer > 0) {
					// Update timer
					mScoreMultiplierTimer -= Time.deltaTime;
				
					// If timer has ended, end multiplier streak
					if(mScoreMultiplierTimer <= 0) {
						SetScoreMultiplier(0);
					}
				}
			} break;
		}
	}

	/// <summary>
	/// Called right before destroying the object.
	/// </summary>
	void OnDestroy() {
		// Nothing to do for now
	}
	#endregion

	#region FLOW CONTROL ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Start a new game. All temp game stats will be reset.
	/// </summary>
	public void StartGame() {
		// Load the dragon
		LoadDragon();

		// Reset timer
		mElapsedSeconds = 0;

		// Reset fury meter
		mFury = 0;
		mFuryRushTimer = -1;

		// Reset score
		mScore = 0;
		SetScoreMultiplier(0);

		// Reset game stats
		App.Instance.gameStats.OnGameStart();

		// Dispatch game event
		Messenger.Broadcast(GameEvents.GAME_STARTED);

		// Change state
		ChangeState(EStates.COUNTDOWN);
	}

	/// <summary>
	/// End the current game. Wont reset the stats so they can be used.
	/// </summary>
	public void EndGame() {
		// Change state
		ChangeState(EStates.FINISHED);

		// Stop fury rush (if active)
		if(mFuryRushTimer > 0) {
			mFuryRushTimer = -1;
			Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
		}

		// Update global stats
		// [AOC] TODO!!
		App.Instance.gameStatsGlobal.JoinStats(App.Instance.gameStats);
		//App.Instance.gameStatsGlobal.CheckMaxScore(mScore);

		// Dispatch game event
		Messenger.Broadcast(GameEvents.GAME_ENDED);
	}

	/// <summary>
	/// Pause/resume
	/// </summary>
	/// <param name="_bPause">Whether to pause the game or resume it.</param>
	public void PauseGame(bool _bPause) {
		// Only when playing
		if(state == EStates.PAUSED || state == EStates.RUNNING) {
			if(_bPause) {
				ChangeState(EStates.PAUSED);
			} else {
				ChangeState(EStates.RUNNING);
			}
		}
	}

	/// <summary>
	/// Is the game currently paused? If the game hasn't started yet, it's considered paused as well.
	/// </summary>
	/// <returns><c>true</c> if game is currently paused; otherwise, <c>false</c>.</returns>
	public bool IsGamePaused() {
		return (state == EStates.PAUSED);
	}
	#endregion

	#region PUBLIC UTILS -----------------------------------------------------------------------------------------------
	/// <summary>
	/// Add coins.
	/// </summary>
	/// <param name="_iAmount">Amount to add. Negative to subtract.</param>
	/// <param name="_entity">Optionally define the entity that triggered this score. Leave <c>null</c> if none.</param> 
	private void AddScore(long _iAmount, GameEntity _entity = null) {
		// Skip checks for now
		// Compute new value and dispatch event
		mScore += _iAmount;
		Messenger.Broadcast<long, long>(GameEvents.SCORE_CHANGED, mScore - _iAmount, mScore);
		Messenger.Broadcast<long, GameEntity>(GameEvents.REWARD_SCORE, _iAmount, _entity);

		// If the score comes from an entity, add fury as well!
		if(_entity != null) {
			AddFury(_entity.GetFuryReward());
		}
	}

	/// <summary>
	/// Add fury to the meter. Wont do anything during fury rush.
	/// </summary>
	/// <param name="_fAmount">Amount to add. Negative to subtract.</param>
	private void AddFury(float _fAmount) {
		// Don't add fury during fury rush
		if(mFuryRushTimer > 0 && _fAmount >= 0) return;

		// Compute new value and dispatch event
		mFury += _fAmount;
		Messenger.Broadcast<float, float>(GameEvents.FURY_CHANGED, mFury - _fAmount, mFury);

		// If we've reached the threshold, start fury rush!!
		if(mFury >= FURY_RUSH_THRESHOLD) {
			// Cap to avoid triggering twice
			mFury = FURY_RUSH_THRESHOLD;

			// Start timer and dispatch game event
			mFuryRushTimer = FURY_RUSH_DURATION;
			Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);
		}
	}

	/// <summary>
	/// Whether the fury rush is active at this moment.
	/// </summary>
	/// <returns><c>true</c> if this fury rush is active; otherwise, <c>false</c>.</returns>
	public bool IsFuryRushActive() {
		return (mFuryRushTimer > 0);
	}

	/// <summary>
	/// Forces the fury rush. For debug purposes only.
	/// </summary>
	/// <param name="_bActive">Whether to activate or de-activate fury rush.</param>
	public void ForceFuryRush(bool _bActive) {
		bool bWasActive = IsFuryRushActive();
		if(_bActive) {
			mFury = FURY_RUSH_THRESHOLD;
			mFuryRushTimer = FURY_RUSH_DURATION;
			if(!bWasActive) {
				// Dispatch game event
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);
			}
		} else {
			mFury = 0;
			mFuryRushTimer = 0;
			if(bWasActive) {
				// Dispatch game event
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
			}
		}
	}
	#endregion

	#region INTERNAL UTILS ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Changes current logic state. Will be ignored if the state is the same we already are in.
	/// </summary>
	/// <param name="_eNewState">The new state to go to.</param>
	private void ChangeState(EStates _eNewState) {
		// Ignore if already in this state
		if(mState == _eNewState) return;

		// Actions to perform when leaving the current state
		switch(mState) {
			case EStates.RUNNING: {
				// Unsubscribe from external events
				Messenger.RemoveListener<GameEntity>(GameEvents.ENTITY_EATEN, OnEntityEaten);
				Messenger.RemoveListener<GameEntity>(GameEvents.ENTITY_BURNED, OnEntityBurned);
				Messenger.RemoveListener<float, DamageDealer>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnPlayerDamage);
			} break;
		}

		// Actions to perform when entering the new state
		switch(_eNewState) {
			case EStates.COUNTDOWN: {
				// Start countdown timer
				mTimer = 1f;	// [AOC] TODO!! Do it properly
			} break;

			case EStates.RUNNING: {
				// Subscribe to external events
				Messenger.AddListener<GameEntity>(GameEvents.ENTITY_EATEN, OnEntityEaten);
				Messenger.AddListener<GameEntity>(GameEvents.ENTITY_BURNED, OnEntityBurned);
				Messenger.AddListener<float, DamageDealer>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnPlayerDamage);
			} break;

			case EStates.PAUSED: {
				// Ignore if not running
				if(mState != EStates.RUNNING) return;
			} break;
		}

		// Store new state
		mState = _eNewState;
	}

	/// <summary>
	/// Load and instantiate a dragon prefab based on current game settings.
	/// </summary>
	private void LoadDragon(){
		// Destroy any previously created player
		GameObject debugDragon = GameObject.Find ("Player");
		if (debugDragon != null)
			DestroyImmediate( debugDragon);

		// Create the Dragon
		UnityEngine.Object dragon = Resources.Load ("Dragons/"+GameSettings.dragonType);
		GameObject dragonObj = (GameObject)UnityEngine.Object.Instantiate(dragon);
		dragonObj.name = "Player";
		dragonObj.transform.position = GameObject.Find ("PlayerSpawn").transform.position;

		// Store reference to the dragon for global fast access
		mPlayer = dragonObj.GetComponent<DragonPlayer>();
	}

	/// <summary>
	/// Define the new score multiplier.
	/// </summary>
	/// <param name="_iMultiplierIdx">The index of the new multiplier in the SCORE_MULTIPLIERS array.</param>
	private void SetScoreMultiplier(int _iMultiplierIdx) {
		// Make sure given index is valid
		if(_iMultiplierIdx < 0 || _iMultiplierIdx >= SCORE_MULTIPLIERS.Length) return;

		// Reset everything when going to 0
		if(_iMultiplierIdx == 0) {
			mScoreMultiplierTimer = -1;
			mScoreMultiplierStreak = 0;
		}

		// Dispatch game event (only if actually changing)
		if(_iMultiplierIdx != mScoreMultiplierIdx) {
			Messenger.Broadcast<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, SCORE_MULTIPLIERS[mScoreMultiplierIdx], SCORE_MULTIPLIERS[_iMultiplierIdx]);
		}

		// Store new multiplier value
		mScoreMultiplierIdx = _iMultiplierIdx;
	}

	/// <summary>
	/// Update the score multiplier with a new kill (eat/burn).
	/// </summary>
	private void UpdateScoreMultiplier() {
		// Update current streak
		mScoreMultiplierStreak++;

		// Reset timer
		mScoreMultiplierTimer = SCORE_MULTIPLIER_DURATION;

		// Check if we've reached next threshold
		if(mScoreMultiplierIdx < SCORE_MULTIPLIERS.Length - 1 
		&& mScoreMultiplierStreak >= SCORE_MULTIPLIERS[mScoreMultiplierIdx + 1].requiredKillStreak) {
			// Yes!! Change current multiplier
			SetScoreMultiplier(mScoreMultiplierIdx + 1);
		}
	}
	#endregion

	#region CALLBACKS ---------------------------------------------------------------------------------------------------
	/// <summary>
	/// An entity has been eaten, give appropriate rewards.
	/// </summary>
	/// <param name="_entity">The entity that has been eaten.</param>
	private void OnEntityEaten(GameEntity _entity) {
		// Update game stats
		App.Instance.gameStats.OnEntityEaten(_entity);

		// Give score reward
		AddScore(_entity.GetScoreReward(), _entity);

		// Give coins reward
		long iRewardCoins = _entity.GetCoinsReward();
		if(iRewardCoins > 0) {
			App.Instance.userData.AddCoins(iRewardCoins);
			Messenger.Broadcast<long, GameEntity>(GameEvents.REWARD_COINS, iRewardCoins, _entity);
		}

		// Update score multiplier
		UpdateScoreMultiplier();
	}

	/// <summary>
	/// An entity has been eaten, give appropriate rewards.
	/// </summary>
	/// <param name="_entity">The entity that has been eaten.</param>
	private void OnEntityBurned(GameEntity _entity) {
		// Update game stats
		App.Instance.gameStats.OnEntityBurned(_entity);
		
		// Give rewards if required
		FlamableBehaviour flamable = _entity.GetComponent<FlamableBehaviour>();	// Should always have one
		if(flamable.giveRewardsOnBurn) {
			// Score
			AddScore(_entity.GetScoreReward(), _entity);

			// Coins
			long iRewardCoins = _entity.GetCoinsReward();
			if(iRewardCoins > 0) {
				App.Instance.userData.AddCoins(iRewardCoins);
				Messenger.Broadcast<long, GameEntity>(GameEvents.REWARD_COINS, iRewardCoins, _entity);
			}

			// Update score multiplier
			UpdateScoreMultiplier();
		}
	}

	/// <summary>
	/// The player has received damage from an entity.
	/// </summary>
	/// <param name="_fDamage">The amaount of damage dealt.</param>
	/// <param name="_source">The object that dealt the damage.</param>
	void OnPlayerDamage(float _fDamage, DamageDealer _source) {
		// End any active streak
		SetScoreMultiplier(0);
	}
	#endregion
}
#endregion