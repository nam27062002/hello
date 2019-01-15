// ResultsScreenStepLeagueScore.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepLeagueScore : ResultsScreenSequenceStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Space]
	[SerializeField] private NumberTextAnimator m_scoreText = null;
	[SerializeField] private Localizer m_highScoreText = null;
	[Space]
	[SerializeField] private TweenSequence m_newHighScoreAnim = null;

	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// [AOC] Disabled for 1.16 until 1.18
		return true;

		// Always show for now
		return true;
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Start score number animation
		// [AOC] TODO!! Better sync with animation?
		m_scoreText.SetValue(0, m_controller.score);

		// Set high score text
		// Don't show if we have a new high score, the flag animation will cover it! Resolves issue HDK-616.
		// [AOC] TODO!!
		bool isNewBestScore = false;    // league.IsNewBestScore()
		long bestScore = 100;			// league.bestScore()
		if(isNewBestScore) {
			m_highScoreText.gameObject.SetActive(false);
		} else {
			m_highScoreText.gameObject.SetActive(true);
			m_highScoreText.Localize(
				m_highScoreText.tid, 
				StringUtils.FormatNumber(bestScore)
			);
		}

		// Hide new high score widget
		m_newHighScoreAnim.gameObject.SetActive(false);

        //TEMP DONOT COMMIT
        HDSeasonData season = HDLiveDataManager.league.season;
        season.SetScore(m_controller.score);
        season.currentLeague.leaderboard.RequestData();
        ///////////////////

	}

    /// <summary>
    /// Called when skip is triggered.
    /// </summary>
    override protected void OnSkip() {
		// Nothing to do for now
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Time to show the new high score feedback (if required).
	/// </summary>
	public void OnShowNewHighScore() {
		// Check whether we did a new high score and show the corresponding feedback
		// [AOC] TODO!!
		bool isNewBestScore = false;    // league.IsNewBestScore()
		if(isNewBestScore) {
			// Show widget and launch animation!
			m_newHighScoreAnim.gameObject.SetActive(true);
			m_newHighScoreAnim.Launch();
		}
	}
}