// ResultsScreenStepLeagueSync.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;


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
	[Space]
	[SerializeField] private Localizer m_titleText = null;
	[SerializeField] private Localizer m_messageText = null;

    private HDSeasonData m_season;
    private bool m_updateEnabled;


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
		//Messenger.RemoveListener<HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LEAGUE_SCORE_SENT, OnLeagueScoreSent);
	}
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this step.
	/// </summary>
	override protected void DoInit() {
        m_season = HDLiveDataManager.league.season;

        // Hide both panels
        m_busyPanel.Hide(false);
		m_errorPanel.Hide(false);

		// Reset flags
		m_hasBeenDismissed = false;

        m_updateEnabled = false;
    }

	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
        // Leagues are active and the player can already participate
		return  HDLiveDataManager.league.season.IsRunning() &&  
                (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_LEAGUES_AT_RUN);
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

        m_season.SetScore(m_controller.score, true);

        m_updateEnabled = true;
    }

    void Update() {
        if (m_updateEnabled) {
            if (m_season.scoreDataState > HDLiveData.State.WAITING_RESPONSE) {
                OnLeagueScoreSent(m_season.scoreDataError);
                m_updateEnabled = false;
            }
        }
    }

	/// <summary>
	/// 
	/// </summary>
	private void InitErrorPanel(HDLiveDataManager.ComunicationErrorCodes _error) {
		// Offline?
		bool offline = Application.internetReachability == NetworkReachability.NotReachable || !GameSessionManager.SharedInstance.IsLogged();
		if(offline || _error == HDLiveDataManager.ComunicationErrorCodes.NET_ERROR) {
			m_titleText.Localize("TID_LEAGUES_OFFLINE_TITLE");  // Sorry! You are offline!
			m_messageText.Localize("TID_LEAGUES_OFFLINE_MESSAGE");  // You must be online to see and participate in the Legendary Leagues!
		}

		// Generic error
		else {
			m_titleText.Localize("TID_EVENT_RESULTS_UNKNOWN_ERROR");    // Something went wrong!
			m_messageText.Localize("TID_REWARD_AMOUNT", HDLiveDataManager.ErrorCodeEnumToInt(_error).ToString(), string.Empty);
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
	private void OnLeagueScoreSent(HDLiveDataManager.ComunicationErrorCodes _errorCode) {
		// Hide busy screen
		m_busyPanel.Hide();

        if (m_season.scoreDataState == HDLiveData.State.VALID) {
            OnFinished.Invoke();
        } else {
            switch (m_season.scoreDataError) {
                case HDLiveDataManager.ComunicationErrorCodes.OTHER_ERROR:
                case HDLiveDataManager.ComunicationErrorCodes.LDATA_NOT_FOUND:
                case HDLiveDataManager.ComunicationErrorCodes.SEASON_NOT_FOUND: {
                    HDLiveDataManager.instance.RequestMyLiveData();
                    OnDismissButton();
                } break;

                case HDLiveDataManager.ComunicationErrorCodes.LEAGUEDEF_NOT_FOUND:
                case HDLiveDataManager.ComunicationErrorCodes.USER_LEAGUE_NOT_FOUND:
                case HDLiveDataManager.ComunicationErrorCodes.SEASON_IS_NOT_ACTIVE: {
                    HDLiveDataManager.instance.ForceRequestLeagues();
                    OnDismissButton();
                } break;

				default: {
					InitErrorPanel(m_season.scoreDataError);
                    m_errorPanel.Show();
				} break;
            }
        }

	}
}