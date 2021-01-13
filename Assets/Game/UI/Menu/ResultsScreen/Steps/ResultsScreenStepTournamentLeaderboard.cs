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
	[SerializeField] private TextMeshProUGUI m_runScoreText = null;

	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Don't if score has been dismissed
		ResultsScreenStepTournamentSync syncStep = m_controller.GetStep(ResultsScreenController.Step.TOURNAMENT_SYNC) as ResultsScreenStepTournamentSync;
		if(syncStep != null && syncStep.hasBeenDismissed) return false;

		// Only if run was valid
		return HDLiveDataManager.tournament.WasLastRunValid();
	}

	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// Run score text
		HDTournamentManager tournament = HDLiveDataManager.tournament;
		m_runScoreText.text = tournament.FormatScore(tournament.GetRunScore());

		// We used to have different icons for different misssions. 
        // Since OTA, we use the generic score icon.

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