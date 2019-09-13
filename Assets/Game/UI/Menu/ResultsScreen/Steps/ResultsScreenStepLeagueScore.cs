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

	private bool m_newHighScore = true;

	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {

        // Show only if the leagues are available
        return HDLiveDataManager.league.season.IsRunning() && 
               UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_LEAGUES_AT_RUN;

    }

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Start score number animation
		// [AOC] TODO!! Better sync with animation?
		m_scoreText.SetValue(0, m_controller.score);

        long leaderboardScore = 0;
        HDLeagueData leagueData = HDLiveDataManager.league.season.currentLeague;
        if (leagueData != null) {
            leaderboardScore = leagueData.leaderboard.playerScore;
        }

        // Set high score text
        // Don't show if we have a new high score, the flag animation will cover it! Resolves issue HDK-616.
        // Can't have a high score if the season is not running either!
        bool seasonRunning = HDLiveDataManager.league.season.IsRunning();
        bool isNewBestScore = m_controller.score > leaderboardScore;
		m_newHighScore = seasonRunning && isNewBestScore;
		if(isNewBestScore || !seasonRunning) {
			// Season not running or new high score: hide the previous high score text
			m_highScoreText.gameObject.SetActive(false);
		} else if(seasonRunning) {
			// Season running but we didn't do a high score: show the previous high score text
			m_highScoreText.gameObject.SetActive(true);
			m_highScoreText.Localize(
				m_highScoreText.tid, 
				StringUtils.FormatNumber(leaderboardScore)
			);
		}

		// Hide new high score widget
		m_newHighScoreAnim.gameObject.SetActive(false);
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
		if(!m_newHighScore) return;
        
		// Show widget and launch animation!
		m_newHighScoreAnim.gameObject.SetActive(true);
		m_newHighScoreAnim.Launch();
	}
}