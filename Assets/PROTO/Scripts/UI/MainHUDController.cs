// MainHUDController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Controls the main HUD behaviour.
/// </summary>
[Serializable]
public class MainHUDController : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------

	#endregion

	#region EXPOSED MEMBERS --------------------------------------------------------------------------------------------
	// [AOC] Initialize all these from the inspector :-)
	[Header("Visual Elements")]
	[SerializeField] private Slider lifeBar;
	[SerializeField] private Slider energyBar;
	[SerializeField] private NumberTextAnimator scoreLabel;
	[SerializeField] private Text timeLabel;
	[SerializeField] private NumberTextAnimator coinsLabel;
	[SerializeField] private Text collectiblesLabel;

	[Header("Fury Rush")]
	[SerializeField] private Slider furyBar;
	[SerializeField] private Animator furyRushAnimator;
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private long mLastDisplayedTime = -1;
	private Animator mAnimator = null;
	private DragonPlayer mPlayerStats;
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Awake() {
		// Make sure everything is properly defined in the inspector
		DebugUtils.Assert(lifeBar != null, "Required component not defined");
		DebugUtils.Assert(energyBar != null, "Required component not defined");
		DebugUtils.Assert(furyBar != null, "Required component not defined");
		DebugUtils.Assert(furyRushAnimator != null, "Required component not defined");
		DebugUtils.Assert(scoreLabel != null, "Required component not defined");
		DebugUtils.Assert(timeLabel != null, "Required component not defined");
		DebugUtils.Assert(coinsLabel != null, "Required component not defined");
		DebugUtils.Assert(collectiblesLabel != null, "Required component not defined");

		// Get reference to required components
		mAnimator = GetComponent<Animator>();
		DebugUtils.Assert(mAnimator != null, "Required component not defined");

		// Subscribe to external events
		Messenger.AddListener(GameEvents_OLD.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(GameEvents_OLD.GAME_ENDED, OnGameEnded);
		Messenger.AddListener<long, long>(GameEvents_OLD.SCORE_CHANGED, OnScoreChanged);
		Messenger.AddListener<long, long>(GameEvents_OLD.PROFILE_COINS_CHANGED, OnCoinsChanged);
		Messenger.AddListener<bool>(GameEvents_OLD.PLAYER_STARVING_TOGGLED, OnStarvingToggled);
		Messenger.AddListener<bool>(GameEvents_OLD.FURY_RUSH_TOGGLED, OnFuryRushToggled);
		Messenger.AddListener<Collectible>(GameEvents_OLD.COLLECTIBLE_COLLECTED, OnCollectibleCollected);
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update() {

		if (mPlayerStats == null) {
			mPlayerStats = GameObject.Find ("Player").GetComponent<DragonPlayer>();
		}

		// Update life bar
		lifeBar.value = mPlayerStats.life;

		// Update energy bar
		energyBar.value = mPlayerStats.energy;

		// Update fury bar
		furyBar.value = mPlayerStats.fury;

		// Update time - only if changed by more than 1 second
		long currentTime = (long)App.Instance.gameLogic.elapsedSeconds;
		if(mLastDisplayedTime != currentTime) {
			// Format to MM:SS
			long iMins = currentTime/60;
			long iSecs = currentTime - (iMins * 60);
			timeLabel.text = String.Format("{0:00}:{1:00}", iMins, iSecs);
			mLastDisplayedTime = currentTime;
		}
	}

	/// <summary>
	/// Called right before destroying the object.
	/// </summary>
	void OnDestroy() {
		// Nothing to do for now
		Messenger.RemoveListener(GameEvents_OLD.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(GameEvents_OLD.GAME_ENDED, OnGameEnded);
		Messenger.RemoveListener<long, long>(GameEvents_OLD.SCORE_CHANGED, OnScoreChanged);
		Messenger.RemoveListener<long, long>(GameEvents_OLD.PROFILE_COINS_CHANGED, OnCoinsChanged);
		Messenger.RemoveListener<bool>(GameEvents_OLD.FURY_RUSH_TOGGLED, OnFuryRushToggled);
		Messenger.RemoveListener<bool>(GameEvents_OLD.PLAYER_STARVING_TOGGLED, OnStarvingToggled);
		Messenger.RemoveListener<Collectible>(GameEvents_OLD.COLLECTIBLE_COLLECTED, OnCollectibleCollected);
	}
	#endregion

	#region CALLBACKS ------------------------------------------------------------------------------------------------
	/// <summary>
	/// The game has started, initialize everything.
	/// </summary>
	private void OnGameStarted() {

		if (mPlayerStats == null)
			mPlayerStats = GameObject.Find ("Player").GetComponent<DragonPlayer>();

		// Set max values
		lifeBar.maxValue = mPlayerStats.maxLife;
		energyBar.maxValue = mPlayerStats.maxEnergy;
		furyBar.maxValue = mPlayerStats.maxFury;
		
		// Init score and coins
		OnScoreChanged(0, App.Instance.gameLogic.score);
		OnCoinsChanged(0, App.Instance.gameLogic.score);
		OnCollectibleCollected(null);
		
		// Make sure fury rush is not active
		OnFuryRushToggled(false);
	}

	/// <summary>
	/// The game has ended.
	/// </summary>
	private void OnGameEnded() {
		// Fade out
		mAnimator.SetTrigger("FadeOut");
	}

	/// <summary>
	/// The score has been changed.
	/// </summary>
	/// <param name="_iOldAmount">Old score.</param>
	/// <param name="_iNewAmount">New score.</param>
	private void OnScoreChanged(long _iOldAmount, long _iNewAmount) {
		// Update text animator
		scoreLabel.SetValue((int)_iNewAmount);
	}

	/// <summary>
	/// The coins balance has been changed.
	/// </summary>
	/// <param name="_iOldAmount">Old coins balance.</param>
	/// <param name="_iNewAmount">New coins balance.</param>
	private void OnCoinsChanged(long _iOldAmount, long _iNewAmount) {
		// Update text animator
		coinsLabel.SetValue((int)_iNewAmount);
	}

	/// <summary>
	/// Triggered when 
	/// </summary>
	/// <param name="_bIsStarving">If set to <c>true</c> _b is starving.</param>
	private void OnStarvingToggled(bool _bIsStarving) {
		mAnimator.SetBool("IsStarving", _bIsStarving);
	}

	/// <summary>
	/// Triggered whenever the fury rush is toggled.
	/// </summary>
	/// <param name="_bActivated">Whether the fury rush started or ended.</param>
	private void OnFuryRushToggled(bool _bActivated) {
		// Notify animator's
		furyRushAnimator.SetBool("IsFuryRush", _bActivated);

		// Let's add some fun
		if(_bActivated) {
			Text furyRushTxt = furyRushAnimator.gameObject.FindSubObject("Fury Rush Label").GetComponent<Text>();
			string[] funnyStrings = {
				"UNLEASHED!",
				"ON FIRE!",
				"BURN'EM ALL!",
				"FIRE RUSH!",
				"FIRE FRENZY!"
			};
			furyRushTxt.text = funnyStrings[UnityEngine.Random.Range(0, funnyStrings.Length)];
		}
	}

	/// <summary>
	/// A collectible has been collected.
	/// </summary>
	/// <param name="_collectible">The collected item.</param>
	private void OnCollectibleCollected(Collectible _collectible) {
		// Get data from game stats
		int iCollected = 0;
		int iTotal = 0;
		foreach(Collectible c in App.Instance.gameStats.collectibles) {
			iTotal++;
			if(c.IsCollected()) {
				iCollected++;
			}
		}

		// Update label
		collectiblesLabel.text = String.Format("{0}/{1}", iCollected, iTotal);
	}
	#endregion
}
#endregion