// ResultsScreenStepTournamentScore.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/05/2018.
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
public class ResultsScreenStepTournamentScore : ResultsScreenSequenceStep {
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
	[SerializeField] private Image m_scoreIcon = null;
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
		// Only if run was valid
		return HDLiveDataManager.instance.m_tournament.WasLastRunValid();
	}

	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// Make sure the number animator respect the tournament's formatting
		m_scoreText.CustomTextSetter = OnSetScoreText;
	}


	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Aux vars
		HDTournamentManager tournament = HDLiveDataManager.instance.m_tournament;

		// Start score number animation
		m_scoreText.SetValue(0, tournament.GetRunScore());

		// Set high score text
		// Don't show if we have a new high score, the flag animation will cover it! Resolves issue HDK-616.
		if(tournament.IsNewBestScore()) {
			m_highScoreText.gameObject.SetActive(false);
		} else {
			m_highScoreText.gameObject.SetActive(true);
			m_highScoreText.Localize(
				m_highScoreText.tid, 
				tournament.FormatScore(tournament.GetBestScore())
			);
		}

		// Each tournament has a different score item
		HDTournamentDefinition def = tournament.data.definition as HDTournamentDefinition;
		m_scoreIcon.sprite = Resources.Load<Sprite>(UIConstants.LIVE_EVENTS_ICONS_PATH + def.m_goal.m_icon);

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
		if(HDLiveDataManager.instance.m_tournament.IsNewBestScore()) {
			// Show widget and launch animation!
			m_newHighScoreAnim.gameObject.SetActive(true);
			m_newHighScoreAnim.Launch();
		}
	}

	/// <summary>
	/// The score number animator needs to format a new value.
	/// </summary>
	/// <param name="_animator">The number animator requesting the formatting.</param>
	public void OnSetScoreText(NumberTextAnimator _animator) {
		// Depends on tournament type
		_animator.text.text = HDLiveDataManager.instance.m_tournament.FormatScore(_animator.currentValue);
	}
}