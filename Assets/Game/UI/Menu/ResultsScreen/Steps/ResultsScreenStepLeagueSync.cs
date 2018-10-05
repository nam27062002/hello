// ResultsScreenStepLeagueSync.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepLeagueSync : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_busyPanel = null;
	[SerializeField] private ShowHideAnimator m_errorPanel = null;

	// Public
	private bool m_hasBeenDismissed = false;
	public bool hasBeenDismissed {
		get { return m_hasBeenDismissed; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// [AOC] TODO!!
		//Messenger.RemoveListener<HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LEAGUE_SCORE_SENT, OnLeagueScoreSent);
	}
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// Listen to score sent confirmation
		// [AOC] TODO!!
		//Messenger.AddListener<HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LEAGUE_SCORE_SENT, OnLeagueScoreSent);

		// Hide both panels
		m_busyPanel.Hide(false);
		m_errorPanel.Hide(false);

		// Reset flags
		m_hasBeenDismissed = false;
	}

	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Always show for now
		return true;
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Send score!
		SendScore();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Send the score to the server.
	/// </summary>
	private void SendScore() {
		// Hide error screen
		m_errorPanel.Hide();

		// Show busy screen
		m_busyPanel.Show();

		// Tell the league to register a score
		// [AOC] TODO!!
		//HDTournamentManager tournament = m_event as HDTournamentManager;
		//tournament.SendScore((int)tournament.GetRunScore());
		UbiBCN.CoroutineManager.DelayedCall(() => { OnLeagueScoreSent(HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR); }, 1f);	// [AOC] TODO!! Simulate server response for now
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Retry button has been pressed.
	/// </summary>
	public void OnRetryButton() {
		// Just do it!
		SendScore();
	}

	/// <summary>
	/// The dismiss score button has been pressed.
	/// </summary>
	public void OnDismissButton() {
		// Set flag
		m_hasBeenDismissed = true;

		// Skip contribution and move to the next step
		OnFinished.Invoke();
	}

	/// <summary>
	/// The score request has been answered.
	/// </summary>
	/// <param name="_errorCode">Error code.</param>
	private void OnLeagueScoreSent(HDLiveEventsManager.ComunicationErrorCodes _errorCode) {
		// Hide busy screen
		m_busyPanel.Hide();

		// Error?
		// [AOC] TODO!!
		/*
		if(_errorCode == HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR) {
			// No! :) Go to next step
			OnFinished.Invoke();
		} else if(_errorCode == HDLiveEventsManager.ComunicationErrorCodes.TOURNAMENT_IS_OVER) {
			// No! :) Go to next step
			OnFinished.Invoke();
		} else {
			// Yes :( Show error screen
			m_errorPanel.Show();
		}
		*/
		OnFinished.Invoke();	// Just skip to next step for now
	}
}