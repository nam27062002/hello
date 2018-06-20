// ResultsScreenStepTournamentLeaderboard.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepTournamentLeaderboard : ResultsScreenSequenceStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Image m_runScoreIcon = null;
	[SerializeField] private TextMeshProUGUI m_runScoreText = null;

	// Allow other steps to prevent this one from showing (i.e. if score was dismissed).
	private bool m_dontShow = false;
	public bool dontShow {
		get { return m_dontShow; }
		set { m_dontShow = value; }
	}

	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Only if run was valid
		// Not if another step is telling us to not show
		return !m_dontShow && HDLiveEventsManager.instance.m_tournament.WasLastRunValid();
	}

	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// Run score text
		HDTournamentManager tournament = HDLiveEventsManager.instance.m_tournament;
		m_runScoreText.text = tournament.FormatScore(tournament.GetRunScore());

		// Each tournament has a different score item
		HDTournamentDefinition def = tournament.data.definition as HDTournamentDefinition;
		m_runScoreIcon.sprite = Resources.Load<Sprite>(UIConstants.LIVE_EVENTS_ICONS_PATH + def.m_goal.m_icon);
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Nothing to do for now
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

}