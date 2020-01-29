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

	// Public
	private bool m_hasBeenDismissed = false;
	public bool hasBeenDismissed {
		get { return m_hasBeenDismissed; }
	}

	// Internal refs
	private HDLiveEventManager m_event = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		Messenger.RemoveListener<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.TOURNAMENT_SCORE_SENT, OnTournamentScoreSent);
		Messenger.RemoveListener<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.QUEST_SCORE_SENT, OnQuestScoreSent);
	}
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
		// Depends on game mode
		switch(SceneController.mode) {
			case SceneController.Mode.TOURNAMENT: {
				// Store event reference
				m_event = HDLiveDataManager.tournament;

				// Listen to score sent confirmation
				Messenger.AddListener<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.TOURNAMENT_SCORE_SENT, OnTournamentScoreSent);
			} break;

			case SceneController.Mode.DEFAULT: {
				// Store event reference
				m_event = HDLiveDataManager.quest;

				// Listen to score sent confirmation 
				Messenger.AddListener<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.QUEST_SCORE_SENT, OnQuestScoreSent);
			} break;
		}

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
		// Check game mode
		switch(GameSceneController.mode) {
			// Tournament mode: check tournament
			case GameSceneController.Mode.TOURNAMENT: {
				// Never during FTUX
				return UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_TOURNAMENTS_AT_RUN;
			} break;

			// Default mode: check quest
			case GameSceneController.Mode.DEFAULT: {
				// Never during FTUX
				// By this point the gamesPlayed var has already been increased, so we must actually count one less game
				if(UsersManager.currentUser.gamesPlayed - 1 < GameSettings.ENABLE_QUESTS_AT_RUN) {
					return false;
				}

				HDQuestManager questManager = m_event as HDQuestManager;
				if(questManager.EventExists()
					&& questManager.IsRunning()
					&& questManager.isActive
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
        if (SceneController.mode == SceneController.Mode.TOURNAMENT) {
			// If the last run wasnt valid, do not save any score
			if ( !HDLiveDataManager.tournament.WasLastRunValid() )
			{
				// No! :) Go to next step
				OnFinished.Invoke();
				return;
			}
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
		switch (SceneController.mode) {
			// Tournament
			case SceneController.Mode.TOURNAMENT: {
				HDTournamentManager tournament = m_event as HDTournamentManager;
				tournament.SendScore((int)tournament.GetRunScore());
			} break;

			// Quest
			case SceneController.Mode.DEFAULT: {
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
		// Set flag
		m_hasBeenDismissed = true;

		// Skip contribution and move to the next step
		OnFinished.Invoke();
	}

	/// <summary>
	/// The score request has been answered.
	/// </summary>
	/// <param name="_errorCode">Error code.</param>
	private void OnTournamentScoreSent(HDLiveDataManager.ComunicationErrorCodes _errorCode) {
		// Hide busy screen
		m_busyPanel.Hide();

		// Error?
		if(_errorCode == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
			// No! :) Go to next step
			OnFinished.Invoke();
		} else if (_errorCode == HDLiveDataManager.ComunicationErrorCodes.TOURNAMENT_IS_OVER
                || _errorCode == HDLiveDataManager.ComunicationErrorCodes.EVENT_NOT_FOUND
                || _errorCode == HDLiveDataManager.ComunicationErrorCodes.EVENT_IS_NOT_VALID
                || _errorCode == HDLiveDataManager.ComunicationErrorCodes.EVENT_TTL_EXPIRED) {
            m_event.ForceFinishByError();
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
	private void OnQuestScoreSent(HDLiveDataManager.ComunicationErrorCodes _errorCode) {
		// Hide busy screen
		m_busyPanel.Hide();

		// Error?
		if(_errorCode == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
			// No! :) Go to next step
			OnFinished.Invoke();
		} else if (_errorCode == HDLiveDataManager.ComunicationErrorCodes.EVENT_NOT_FOUND 
                || _errorCode == HDLiveDataManager.ComunicationErrorCodes.EVENT_IS_NOT_VALID 
                || _errorCode == HDLiveDataManager.ComunicationErrorCodes.EVENT_TTL_EXPIRED)
		{
            m_event.ForceFinishByError();
            // No! :) Go to next step
            OnFinished.Invoke();
		} else if (_errorCode == HDLiveDataManager.ComunicationErrorCodes.QUEST_IS_OVER) {
			m_event.RequestDefinition(true);
			OnFinished.Invoke();
		} else {
			// Yes :( Show error screen
			m_errorPanel.Show();
		}
	}
}