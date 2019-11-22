// ResultsScreenStepScore.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

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
public class ResultsScreenStepScore : ResultsScreenSequenceStep {
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
		if(m_controller.isHighScore) {
			m_highScoreText.gameObject.SetActive(false);
		} else {
			m_highScoreText.gameObject.SetActive(true);
			m_highScoreText.Localize(m_highScoreText.tid, StringUtils.FormatNumber(m_controller.highScore));
		}

		// Hide new high score widget
		m_newHighScoreAnim.gameObject.SetActive(false);

		// Only allow skipping if it's not a high score
		m_skipAllowed = !m_controller.isHighScore;
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
		if(m_controller.isHighScore) {
			// Show widget and launch animation!
			m_newHighScoreAnim.gameObject.SetActive(true);
			m_newHighScoreAnim.Launch();
		} else {
			// No high score, show "tap to skip" message
			m_tapToContinue.ForceShow();
		}
	}

	/// <summary>
	/// Pause sequence if we got a new high score!
	/// </summary>
	public void OnPauseIfHighScore() {
		// Pause the sequence if we got a new high score
		if(m_controller.isHighScore) {
			// Pause sequence
			m_sequence.Pause();

			// Show "tap to continue" message
			m_tapToContinue.ForceShow();
		}
	}

	/// <summary>
	/// Do the summary line for this step. Connect in the sequence.
	/// </summary>
	public void DoSummary() {
		m_controller.summary.ShowScore(m_controller.score);
	}

	/// <summary>
	/// Show the run duration in the summary
	/// </summary>
	override public void ShowSummary() {
		// Show time group
		m_controller.summary.ShowTime(m_controller.time, false);

		// Call parent
		base.ShowSummary();
	}

	/// <summary>
	/// Share high score button has been pressed.
	/// </summary>
	public void OnShareButton() {
		IDragonData dragonData = null;
		if(SceneController.mode == SceneController.Mode.TOURNAMENT) {
			dragonData = HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData;
		} else {
			dragonData = DragonManager.CurrentDragon;
		}

		// Initialize and show the corresponding share screen
		ShareScreenHighScore shareScreen = ShareScreensManager.GetShareScreen("high_score") as ShareScreenHighScore;
		shareScreen.Init(
			"high_score",
			SceneController.GetMainCameraForCurrentScene(),
			m_controller.score,
			dragonData
		);

		shareScreen.TakePicture();
	}
}