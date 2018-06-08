// ResultsScreenStepTournamentSync.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/05/2018.
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
public class ResultsScreenStepTournamentSync : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_busyPanel = null;
	[SerializeField] private ShowHideAnimator m_errorPanel = null;

	// Internal refs
	private HDLiveEventManager m_event = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		Messenger.RemoveListener<HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.TOURNAMENT_SCORE_SENT, OnTournamentScoreSent);
		Messenger.RemoveListener<HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.QUEST_SCORE_SENT, OnQuestScoreSent);
	}
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// Depends on game mode
		switch(GameSceneController.s_mode) {
			case GameSceneController.Mode.TOURNAMENT: {
				// Store event reference
				m_event = HDLiveEventsManager.instance.m_tournament;

				// Listen to score sent confirmation
				Messenger.AddListener<HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.TOURNAMENT_SCORE_SENT, OnTournamentScoreSent);
			} break;

			case GameSceneController.Mode.DEFAULT: {
				// Store event reference
				m_event = HDLiveEventsManager.instance.m_quest;

				// Listen to score sent confirmation 
				Messenger.AddListener<HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.QUEST_SCORE_SENT, OnQuestScoreSent);
			} break;
		}

		// Hide both panels
		m_busyPanel.Hide(false);
		m_errorPanel.Hide(false);
	}

	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Never during FTUX
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_GLOBAL_EVENTS_AT_RUN) return false;

		// Check game mode
		switch(GameSceneController.s_mode) {
			// Tournament mode: check tournament
			case GameSceneController.Mode.TOURNAMENT: {
				return true;	// Always
			} break;

			// Default mode: check quest
			case GameSceneController.Mode.DEFAULT: {
				HDQuestManager questManager = m_event as HDQuestManager;
				if(questManager.EventExists()
					&& questManager.IsRunning()
					&& questManager.m_isActive
					&& questManager.m_questData.remainingTime.TotalSeconds > 0
					&& questManager.GetRunScore() > 0		// Only if we actually got a score!
				)
				{
					return true;
				}
			} break;
		}

		return false;
	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Apply rewards to user profile (only tournament mode)
		if(GameSceneController.s_mode == SceneController.Mode.TOURNAMENT) {
			RewardManager.ApplyEndOfGameRewards();
			PersistenceFacade.instance.Save_Request(true);
		}

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

		// Tell the event to register a score
		switch(GameSceneController.s_mode) {
			// Tournament
			case GameSceneController.Mode.TOURNAMENT: {
				HDTournamentManager tournament = m_event as HDTournamentManager;
				tournament.SendScore((int)tournament.GetRunScore());
			} break;

			// Quest
			case GameSceneController.Mode.DEFAULT: {
				HDQuestManager quest = m_event as HDQuestManager;
				quest.Contribute((int)quest.GetRunScore(), 1f, false, false);	// [AOC] TODO!! Remove deprecated params
			} break;
		}
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
		// Skip contribution and move to the next step
		OnFinished.Invoke();
	}

	/// <summary>
	/// The score request has been answered.
	/// </summary>
	/// <param name="_errorCode">Error code.</param>
	private void OnTournamentScoreSent(HDLiveEventsManager.ComunicationErrorCodes _errorCode) {
		// Hide busy screen
		m_busyPanel.Hide();

		// Error?
		if(_errorCode == HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR) {
			// No! :) Go to next step
			OnFinished.Invoke();
		} else {
			// Yes :( Show error screen
			m_errorPanel.Show();
		}
	}

	/// <summary>
	/// The score request has been answered.
	/// </summary>
	/// <param name="_errorCode">Error code.</param>
	private void OnQuestScoreSent(HDLiveEventsManager.ComunicationErrorCodes _errorCode) {
		// Hide busy screen
		m_busyPanel.Hide();

		// Error?
		if(_errorCode == HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR) {
			// No! :) Go to next step
			OnFinished.Invoke();
		} else {
			// Yes :( Show error screen
			m_errorPanel.Show();
		}
	}
}