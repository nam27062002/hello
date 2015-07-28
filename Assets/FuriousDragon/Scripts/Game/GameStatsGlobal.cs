// GameStatsGlobal.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Extension of the game stats to store long-term stats as well.
/// </summary>
public class GameStatsGlobal : GameStats {
	#region PROPERTIES -------------------------------------------------------------------------------------------------
	// [AOC] We want these to be consulted but never set from outside, so don't add a setter
	// How many high scores to store
	[SerializeField] private int _NUM_HIGH_SORES = 10;
	public int NUM_HIGH_SORES {
		get { return _NUM_HIGH_SORES; }
	}

	// High scores - will contain -1 if no record was stored
	private long[] mHighScores;
	public long[] highScores {
		get { return mHighScores; }
	}
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Pre-initialization.
	/// </summary>
	override protected void Awake() {
		// Initialize high scores
		mHighScores = new long[NUM_HIGH_SORES];

		// Call parent - afterwards, since it will call to Reset() and needs the high scores array to be created!
		base.Awake();
	}
	#endregion

	#region PUBLIC UTILS ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Reset stats to their default values.
	/// </summary>
	override public void Reset() {
		// Call parent
		base.Reset();

		// Clear highscores
		for(int i = 0; i < mHighScores.Length; i++) {
			mHighScores[i] = -1;
		}
	}

	/// <summary>
	/// Incorporate a stats set to this one.
	/// </summary>
	/// <param name="name="_stats">The stats to be joined to these.</param>
	public void JoinStats(GameStatsGlobal _stats) {
		// [AOC] TODO!!
	}
	#endregion

	#region CALLBACKS -------------------------------------------------------------------------------------------------
	/// <summary>
	/// Check whether the given score is a max score and store it in the case it is.
	/// </summary>
	/// <param name="_iScore">The score to be checked.</param>
	public void CheckMaxScore(long _iScore) {
		// [AOC] TODO!!
	}
	#endregion
}
#endregion