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

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
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
		// Go to tournament rewards screen
		InstanceManager.menuSceneController.GoToScreen(MenuScreen.TOURNAMENT_REWARD);
	}
}