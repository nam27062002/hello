// TournamentFeaturedIcon.cs
// Hungry Dragon
//
// Created by Alger Ortín Castellví on 31/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the featured tournament icon in the play screen.
/// </summary>
public class TournamentFeaturedIcon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 1f;	// Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_root = null;
    	

	[Separator("Teasing")]
	[SerializeField] private GameObject m_teasingGroup = null;
    [SerializeField] private Localizer m_teasingTimerText = null;

	[Separator("Active")]
	[SerializeField] private GameObject m_activeGroup = null;
	[SerializeField] private GameObject m_newBanner = null;
	[SerializeField] private TextMeshProUGUI m_activeTimerText = null;

	[Separator("Rewards")]
	[SerializeField] private GameObject m_rewardsGroup = null;

	[Separator("Others")]
	[SerializeField] private DOTweenAnimation m_playButtonAnimation = null;

	// Internal
	private HDTournamentManager m_tournamentManager;
	private bool m_waitingRewardsData = false;
    private bool m_waitingDefinition = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get tournament manager
		m_tournamentManager = HDLiveDataManager.tournament;
        Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnStateUpdated);
        Messenger.AddListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnStateUpdatedWithParams);
        Messenger.AddListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewardsResponse);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Get latest data from the manager
		RefreshData();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy()
	{
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnStateUpdated);
		Messenger.RemoveListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnStateUpdatedWithParams);
		Messenger.RemoveListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewardsResponse);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);

		// Get latest data from the manager
		RefreshData();
	}

	/// <summary>
	/// Called periodically - to avoid doing stuff every frame.
	/// </summary>
	private void UpdatePeriodic() {
		// Skip if we're not active - probably in a screen we don't belong to
		if(!isActiveAndEnabled) return;

		// Refresh the timer!
		RefreshTimer(true);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Cancel periodic update
		CancelInvoke();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a refresh of the offers manager asking for new featured offers to be displayed
	/// </summary>
	public void RefreshData() {
		// Make sure tournament is at the right state
		m_tournamentManager.UpdateStateFromTimers();

		// Update visibility
		bool show = RefreshVisibility();

		// Don't play play button animation if tournaments button is visible
		if(m_playButtonAnimation != null) {
			if(show) {
				m_playButtonAnimation.DOPause();
				m_playButtonAnimation.DORewind();
			} else {
				m_playButtonAnimation.DOPlay();
			}
		}

		// Nothing else to do if there is no active tournament
		if(!show) return;

		// Toggle visibility of target object based on tournament state
		HDLiveEventData.State state = m_tournamentManager.data.m_state;

		// Teasing
		if(m_teasingGroup != null) m_teasingGroup.SetActive(state == HDLiveEventData.State.TEASING);

		// Active
		if(m_activeGroup != null) m_activeGroup.SetActive(m_tournamentManager.IsRunning() || m_tournamentManager.RequiresUpdate());
		if(m_newBanner != null) m_newBanner.gameObject.SetActive(state == HDLiveEventData.State.NOT_JOINED);

		// Rewards
		if(m_rewardsGroup != null) m_rewardsGroup.SetActive(state == HDLiveEventData.State.REWARD_AVAILABLE);

		// Update the timer for the first time
		RefreshTimer(false);
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// </summary>
	/// <param name="_checkExpiration">Refresh data if timer reaches 0?</param>
	private void RefreshTimer(bool _checkExpiration) {
		// Aux vars
		HDLiveEventData.State state = m_tournamentManager.data.m_state;

		// Is tournament still valid?
		if(!m_tournamentManager.EventExists()) return;
		if(state == HDLiveEventData.State.FINALIZED) return;

		// Update text
		double remainingSeconds = m_tournamentManager.data.remainingTime.TotalSeconds;
		string timeString = TimeUtils.FormatTime(
						System.Math.Max(0, remainingSeconds), // Just in case, never go negative
						TimeUtils.EFormat.ABBREVIATIONS,
						3
					);

        if (state == HDLiveEventData.State.TEASING) {
			if(m_teasingTimerText != null) {
				if(m_teasingTimerText.gameObject.activeSelf) {
					m_teasingTimerText.Localize(
						"TID_TOURNAMENT_ICON_STARTS_IN",
						timeString
					);
				}
			}
        } else {
			if(m_activeTimerText != null) {
				if(m_activeTimerText.gameObject.activeSelf) {
					m_activeTimerText.text = timeString;	// [AOC] Not enough space for "Ends In" with the new layout
				}
			}
        }

		// Manage timer expiration when the icon is visible
		if(_checkExpiration && remainingSeconds <= 0) {
			RefreshData();
		}
	}

	/// <summary>
	/// Check whether the icon can be displayed or not.
	/// </summary>
	private bool RefreshVisibility() {
		// Never during tutorial
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_TOURNAMENTS_AT_RUN) {
			m_root.SetActive(false);
			return false;
		}

		// Do we have a valid tournament?
		bool show = false;
		if(m_tournamentManager.EventExists()) {
			// Show only for specific states
			switch(m_tournamentManager.data.m_state) {
				case HDLiveEventData.State.TEASING:
				case HDLiveEventData.State.NOT_JOINED:
				case HDLiveEventData.State.JOINED:
				case HDLiveEventData.State.REWARD_AVAILABLE:
                case HDLiveEventData.State.REQUIRES_UPDATE:
                {
					// Must have a valid definition first
					if(m_tournamentManager.data.definition != null && m_tournamentManager.data.definition.initialized) {
						show = true;
					}
				} break;
			}
		}

		if(m_root != null) m_root.SetActive(show);
		return show;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Tournament Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {

        // OTA: Check for assets for all the downloadable content
        // If we dont have downloaded the content yet, dont go to the tournament screen
        Downloadables.Handle tournamentHandle = HDAddressablesManager.Instance.GetHandleForAllDownloadables();
        if (!tournamentHandle.IsAvailable())
        {
            // Get the download handle from the parent scene controller
            MenuDragonScreenController parent = transform.parent.GetComponent<MenuDragonScreenController>();
            AssetsDownloadFlow downloadHandle = parent.assetsDownloadFlow;

            // Initialize download flow with handle for ALL assets
            downloadHandle.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());
            downloadHandle.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY, AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_TOURNAMENT);

            return;
        }


        if ( m_tournamentManager.RequiresUpdate() )
			{
					// Show update popup!
					PopupManager.OpenPopupInstant( PopupUpdateEvents.PATH );

					// Go to tournament info screen
					HDTrackingManager.Instance.Notify_TournamentClickOnMainScreen("no_tournament");
		}
		else
		{
            if ( InstanceManager.menuSceneController.transitionManager.transitionAllowed )
            {
            	// Change game mode
    	        SceneController.SetMode(SceneController.Mode.TOURNAMENT);
        	    HDLiveDataManager.instance.SwitchToTournament();
    	
        	    // Send Tracking event
            	HDTrackingManager.Instance.Notify_TournamentClickOnMainScreen(m_tournamentManager.data.definition.m_name);
    
            	// Go to tournament info screen
            	InstanceManager.menuSceneController.GoToScreen(MenuScreen.TOURNAMENT_INFO);
    		}
        }
	}

	/// <summary>
	/// Collect Rewards button has been pressed.
	/// </summary>
	public void OnCollectButton() {
        // Prevent spamming
        if (m_waitingRewardsData) return;

        // if info is from cache wait to recieve definitions!
        if ( m_tournamentManager.isDataFromCache )
        {
            m_waitingDefinition = true;
            m_tournamentManager.RequestDefinition(true);
        }
        else
        {
            // Request rewards data and wait for it to be loaded
            m_tournamentManager.RequestRewards();
        }

        // Show busy screen
        BusyScreen.Setup(true, LocalizationManager.SharedInstance.Localize("TID_TOURNAMENT_REWARDS_LOADING"));
        BusyScreen.Show(this);

        // Toggle flag
        m_waitingRewardsData = true;
	}

	/// <summary>
	/// We got a response on the rewards request.
	/// </summary>
	private void OnRewardsResponse(int _eventId, HDLiveDataManager.ComunicationErrorCodes _errorCode) {
		// Ignore if we weren't waiting for rewards!
		if(!m_waitingRewardsData) return;
		m_waitingRewardsData = false;

		// Hide busy screen
		BusyScreen.Hide(this);

		// Success?
		if(_errorCode == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
			// Go to tournament rewards screen!
			TournamentRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.TOURNAMENT_REWARD).ui.GetComponent<TournamentRewardScreen>();
			scr.StartFlow();
            InstanceManager.menuSceneController.GoToScreen(MenuScreen.TOURNAMENT_REWARD, true);
		} else {
			// Show error message
			UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_TOURNAMENT_REWARDS_ERROR"),
				new Vector2(0.5f, 0.33f),
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);
			text.text.color = UIConstants.ERROR_MESSAGE_COLOR;

             // Finish tournament if 607 / 608 / 622
            if ( (_errorCode == HDLiveDataManager.ComunicationErrorCodes.EVENT_NOT_FOUND ||
                _errorCode == HDLiveDataManager.ComunicationErrorCodes.EVENT_IS_NOT_VALID ||
                _errorCode == HDLiveDataManager.ComunicationErrorCodes.EVENT_TTL_EXPIRED ) &&
                m_tournamentManager.data.m_eventId == _eventId
                )
                {
                    m_tournamentManager.ForceFinishByError();
                }

		}
	}

	/// <summary>
	/// We got an update on the tournament state.
	/// </summary>
	private void OnStateUpdatedWithParams(int _eventId, HDLiveDataManager.ComunicationErrorCodes _error) {
		RefreshData();
        if ( m_waitingDefinition && m_waitingRewardsData )
        {
            m_waitingDefinition = false;
            if ( _error == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
            {
                // Request rewards data and wait for it to be loaded
                m_tournamentManager.RequestRewards();
            }
            else
            {
                m_waitingRewardsData = false;

                // Hide busy screen
                BusyScreen.Hide(this);

                // Show error message
                UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
                    LocalizationManager.SharedInstance.Localize("TID_TOURNAMENT_REWARDS_ERROR"),
                    new Vector2(0.5f, 0.33f),
                    this.GetComponentInParent<Canvas>().transform as RectTransform
                );
                text.text.color = UIConstants.ERROR_MESSAGE_COLOR;
            }
        }
	}

	private void OnStateUpdated() {
		RefreshData();
	}
}