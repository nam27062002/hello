// ResultsScreenStepLeagueLeaderboard.cs
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
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepLeagueLeaderboard : ResultsScreenSequenceStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private LeagueIcon m_leagueIcon = null;

	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {		
		// Don't if score has been dismissed
		ResultsScreenStepLeagueSync syncStep = m_controller.GetStep(ResultsScreenController.Step.LEAGUE_SYNC) as ResultsScreenStepLeagueSync;
		if(syncStep != null && syncStep.hasBeenDismissed) return false;


        // Show only if the leagues are available and we have the minimum tier
        return HDLiveDataManager.league.season.IsRunning() && 
               UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_LEAGUES_AT_RUN &&
               DragonManager.CurrentDragon.tier >= HDLiveDataManager.league.GetMinimumTierToShowLeagues();

	}

	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// Initialize league icon with current player's league info
		HDLeagueData leagueData = HDLiveDataManager.league.season.currentLeague;
        if (leagueData != null) {
            m_leagueIcon.Init(leagueData.tidName, leagueData.icon);
        }
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