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

	[Separator("Shared")]
	[SerializeField] private Localizer m_timerText = null;
	
	[Separator("Teasing")]
	[SerializeField] private GameObject m_teasingGroup = null;

	[Separator("Active")]
	[SerializeField] private GameObject m_activeGroup = null;
	[SerializeField] private GameObject m_newBanner = null;

	[Separator("Rewards")]
	[SerializeField] private GameObject m_rewardsGroup = null;

	[Separator("Others")]
	[SerializeField] private DOTweenAnimation m_playButtonAnimation = null;

	// Internal
	private HDTournamentManager m_tournamentManager;
	private bool m_waitingRewardsData = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get tournament manager
		m_tournamentManager = HDLiveEventsManager.instance.m_tournament;
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Get latest data from the manager
		RefreshData();

		Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnStateUpdated);
		Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnStateUpdatedWithParams);
		Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewardsResponse);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy()
	{
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnStateUpdated);
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnStateUpdatedWithParams);
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewardsResponse);
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
		if(m_activeGroup != null) m_activeGroup.SetActive(m_tournamentManager.IsRunning());
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
		// Is tournament still valid?
		if(!m_tournamentManager.EventExists()) return;
		if(m_tournamentManager.data.m_state == HDLiveEventData.State.FINALIZED) return;

		// Update text
		double remainingSeconds = m_tournamentManager.data.remainingTime.TotalSeconds;
		if(m_timerText != null) {
			m_timerText.Localize(m_timerText.tid,
				TimeUtils.FormatTime(
					System.Math.Max(0, remainingSeconds), // Just in case, never go negative
					TimeUtils.EFormat.ABBREVIATIONS,
					4
				)
			);
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
				case HDLiveEventData.State.REWARD_AVAILABLE: {
					show = true;
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
		// Change game mode
		SceneController.s_mode = SceneController.Mode.TOURNAMENT;
		HDLiveEventsManager.instance.SwitchToTournament();

		// Go to tournament info screen
		InstanceManager.menuSceneController.GoToScreen(MenuScreen.TOURNAMENT_INFO);
	}

	/// <summary>
	/// Collect Rewards button has been pressed.
	/// </summary>
	public void OnCollectButton() {
		// Prevent spamming
		if(m_waitingRewardsData) return;

		// Request rewards data and wait for it to be loaded
		m_tournamentManager.RequestRewards();

		// Show busy screen
		BusyScreen.Setup(true, LocalizationManager.SharedInstance.Localize("TID_TOURNAMENT_REWARDS_LOADING"));
		BusyScreen.Show(this);

		// Toggle flag
		m_waitingRewardsData = true;
	}

	/// <summary>
	/// We got a response on the rewards request.
	/// </summary>
	private void OnRewardsResponse(int _eventId, HDLiveEventsManager.ComunicationErrorCodes _errorCode) {
		// Ignore if we weren't waiting for rewards!
		if(!m_waitingRewardsData) return;
		m_waitingRewardsData = false;

		// Hide busy screen
		BusyScreen.Hide(this);

		// Success?
		if(_errorCode == HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR) {
			// Go to tournament rewards screen!
			TournamentRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.TOURNAMENT_REWARD).ui.GetComponent<TournamentRewardScreen>();
			scr.StartFlow();
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.TOURNAMENT_REWARD);
		} else {
			// Show error message
			UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_TOURNAMENT_REWARDS_ERROR"),
				new Vector2(0.5f, 0.33f),
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);
			text.text.color = UIConstants.ERROR_MESSAGE_COLOR;
		}
	}

	/// <summary>
	/// We got an update on the tournament state.
	/// </summary>
	private void OnStateUpdatedWithParams(int _eventId, HDLiveEventsManager.ComunicationErrorCodes _error) {
		RefreshData();
	}

	private void OnStateUpdated() {
		RefreshData();
	}
}