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
public class ResultsScreenStepScore : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private NumberTextAnimator m_scoreText = null;
	[SerializeField] private Localizer m_highScoreText = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_tapToContinue = null;
	[SerializeField] private TweenSequence m_sequence;
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
	override protected void DoInit() {
		// Notify when sequence is finished
		m_sequence.OnFinished.AddListener(() => OnFinished.Invoke());
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Start score number animation
		// [AOC] TODO!! Better sync with animation?
		m_scoreText.SetValue(0, m_controller.score);

		// Set high score text
		m_highScoreText.Localize(m_highScoreText.tid, StringUtils.FormatNumber(m_controller.highScore));

		// Hide new high score widget
		m_newHighScoreAnim.gameObject.SetActive(false);

		// Hide tap to continue
		m_tapToContinue.ForceHide(false);

		// Launch sequence!
		m_sequence.Launch();
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
		}
	}

	/// <summary>
	/// The tap to continue button has been pressed.
	/// </summary>
	public void OnTapToContinue()                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        {
		// Only if enabled! (to prevent spamming)
		// [AOC] Reuse visibility state to control whether tap to continue is enabled or not)
		if(!m_tapToContinue.visible) return;

		// Hide tap to continue to prevent spamming
		m_tapToContinue.Hide();

		// Launch end sequence
		m_sequence.Play();
	}
}