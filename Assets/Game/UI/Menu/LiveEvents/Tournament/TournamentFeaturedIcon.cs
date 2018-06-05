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
	[SerializeField] private Localizer m_headerText = null;

	[Separator("Shared")]
	[SerializeField] private Localizer m_tournamentTitleText = null;
	[SerializeField] private Localizer m_timerText = null;
	[SerializeField] private Localizer m_goalText = null;
	[SerializeField] private Image m_goalIcon = null;

	[Separator("Active")]
	[SerializeField] private GameObject m_playButton = null;

	[Separator("Rewards")]
	[SerializeField] private GameObject m_collectButton = null;

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
		Messenger.AddListener(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnStateUpdated);
		Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewardsResponse);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy()
	{
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnStateUpdated);
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnStateUpdated);
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
		RefreshTimer();
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
		RefreshVisibility();

		// Nothing else to do if there is no active tournament
		if(!m_tournamentManager.EventExists()) return;

		// Aux vars
		HDTournamentDefinition def = m_tournamentManager.data.definition as HDTournamentDefinition;

		// Fill event data
		m_tournamentTitleText.Localize(def.m_name);
		m_goalText.Localize(def.m_goal.m_desc);
		m_goalIcon.sprite = Resources.Load<Sprite>(UIConstants.LIVE_EVENTS_ICONS_PATH + def.m_goal.m_icon);

		// Toggle visibility of specific items based on tournament state
		HDLiveEventData.State state = m_tournamentManager.data.m_state;
		m_playButton.SetActive(m_tournamentManager.IsRunning());
		m_collectButton.SetActive(state == HDLiveEventData.State.REWARD_AVAILABLE);

		// Adapt title to tournament state
		switch(state) {
			case HDLiveEventData.State.TEASING: {
				m_headerText.Localize("TID_TOURNAMENT_ICON_TITLE_TEASING");
			} break;

			case HDLiveEventData.State.REWARD_AVAILABLE: {
				m_headerText.Localize("TID_TOURNAMENT_ICON_TITLE_PENDING_REWARD");
			} break;

			default: {
				m_headerText.Localize("TID_TOURNAMENT_ICON_TITLE_ACTIVE");
			} break;
		}

		// Update the timer
		//RefreshTimer();
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// </summary>
	private void RefreshTimer() {
		// Is tournament still valid?
		if(!m_tournamentManager.EventExists()) return;
		if(m_tournamentManager.data.m_state == HDLiveEventData.State.FINALIZED) return;

		// Update text
		double remainingSeconds = m_tournamentManager.data.remainingTime.TotalSeconds;
		m_timerText.Localize(m_timerText.tid,
			TimeUtils.FormatTime(
				System.Math.Max(0, remainingSeconds), // Just in case, never go negative
				TimeUtils.EFormat.ABBREVIATIONS,
				4
			)
		);

		// Manage timer expiration when the icon is visible
		if(remainingSeconds <= 0) {
			RefreshData();
		}
	}

	/// <summary>
	/// Check whether the icon can be displayed or not.
	/// </summary>
	private void RefreshVisibility() {
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

		m_root.SetActive(show);
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
	private void OnStateUpdated() {
		RefreshData();
	}
}