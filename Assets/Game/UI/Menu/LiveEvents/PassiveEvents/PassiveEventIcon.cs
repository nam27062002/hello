// PassiveEventIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2018.
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
/// Controller for the passive event icon in the menu HUD.
/// </summary>
public class PassiveEventIcon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 1f;	// Seconds

	//		__/\\\\\\\\\\\\\\\________/\\\\\________/\\\\\\\\\\\\___________/\\\\\___________/\\\_________/\\\____
	//		 _\///////\\\/////_______/\\\///\\\_____\/\\\////////\\\_______/\\\///\\\_______/\\\\\\\_____/\\\\\\\__
	//		  _______\/\\\__________/\\\/__\///\\\___\/\\\______\//\\\____/\\\/__\///\\\____/\\\\\\\\\___/\\\\\\\\\_
	//		   _______\/\\\_________/\\\______\//\\\__\/\\\_______\/\\\___/\\\______\//\\\__\//\\\\\\\___\//\\\\\\\__
	//		    _______\/\\\________\/\\\_______\/\\\__\/\\\_______\/\\\__\/\\\_______\/\\\___\//\\\\\_____\//\\\\\___
	//		     _______\/\\\________\//\\\______/\\\___\/\\\_______\/\\\__\//\\\______/\\\_____\//\\\_______\//\\\____
	//		      _______\/\\\_________\///\\\__/\\\_____\/\\\_______/\\\____\///\\\__/\\\________\///_________\///_____
	//		       _______\/\\\___________\///\\\\\/______\/\\\\\\\\\\\\/_______\///\\\\\/__________/\\\_________/\\\____
	//		        _______\///______________\/////________\////////////___________\/////___________\///_________\///_____
	/*private enum DisplayLocation {
		PLAY_SCREEN,
		NORMAL_MODE_SCREEN,
		TOURNAMENT_MODE_SCREEN,
		INGAME
	}*/

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_root = null;
	[SerializeField] private ModifierIcon m_modifierIcon = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;

	// Internal
	private HDPassiveEventManager m_passiveEventManager;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get tournament manager
		m_passiveEventManager = HDLiveEventsManager.instance.m_passive;
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Get latest data from the manager
		RefreshData();

		Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnStateUpdated);
		Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnStateUpdatedWithParams);
		Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, OnStateUpdatedWithParams);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnStateUpdated);
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnStateUpdatedWithParams);
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, OnStateUpdatedWithParams);
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
		m_passiveEventManager.UpdateStateFromTimers();

		// Update visibility
		bool show = RefreshVisibility();

		// Nothing else to do if there is no active event
		if(!show) return;

		// Make sure icon is showing the right info
		if(m_modifierIcon != null) {
			// Init icon
			HDPassiveEventDefinition def = m_passiveEventManager.data.definition as HDPassiveEventDefinition;
			if(def.mainMod != null) {
				m_modifierIcon.gameObject.SetActive(true);
				m_modifierIcon.InitFromDefinition(def.mainMod);
			} else {
				m_modifierIcon.gameObject.SetActive(false);
			}
		}

		// Update the timer for the first time
		RefreshTimer(false);
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// </summary>
	/// <param name="_checkExpiration">Refresh data if timer reaches 0?</param>
	private void RefreshTimer(bool _checkExpiration) {
		// Aux vars
		HDLiveEventData.State state = m_passiveEventManager.data.m_state;

		// Is tournament still valid?
		if(!m_passiveEventManager.EventExists()) return;
		if(state == HDLiveEventData.State.FINALIZED) return;

		// Update text
		double remainingSeconds = m_passiveEventManager.data.remainingTime.TotalSeconds;
		if(m_timerText != null) {
			// Set text
			if(m_timerText.gameObject.activeSelf) {
				m_timerText.text = TimeUtils.FormatTime(
					System.Math.Max(0, remainingSeconds), // Just in case, never go negative
					TimeUtils.EFormat.ABBREVIATIONS,
					4
				);
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

		// Do we have a valid event?
		bool show = false;
		if(m_passiveEventManager.EventExists()) {
			// Show only for specific states
			switch(m_passiveEventManager.data.m_state) {
				//case HDLiveEventData.State.TEASING:		// No teasing for passive events
				case HDLiveEventData.State.NOT_JOINED:
				case HDLiveEventData.State.JOINED:
				//case HDLiveEventData.State.REWARD_AVAILABLE:	// They don't have rewards either
				{
					// Must have a valid definition first
					if(m_passiveEventManager.data.definition != null 
					&& m_passiveEventManager.data.definition.initialized) {
						show = true;
					}
				} break;
			}

			// Check current screen and event's UI settings
			Modifier mod = m_passiveEventManager.m_passiveEventDefinition.mainMod;
			if(mod != null) {
				/*// Are we in the menu?
				if(InstanceManager.menuSceneController != null) {
					MenuScreen currentScreen = InstanceManager.menuSceneController.currentScreen;
					switch(mod.def.Get("uiCategory")) {
						// case "stats":
						// case "metagame":
						// case "levelUp":
						case "stats": {
							switch(currentScreen) {
								case MenuScreen.PLAY:
								case MenuScreen.DRAGON_SELECTION:
								case MenuScreen.TOURNAMENT_INFO: {
									show = true;
								} break;
							}
						} break;

						case "metagame": {
							switch(currentScreen) {
								case MenuScreen.PLAY:
								case MenuScreen.DRAGON_SELECTION:
								case MenuScreen.TOURNAMENT_INFO: {
									show = true;
								} break;
							}
						} break;
					}
				} else {
					// No! Loading / Ingame / Results
					// Tournament mode?
					if(GameSceneController.s_mode == SceneController.Mode.TOURNAMENT) {

					}
				}*/


				//		__/\\\\\\\\\\\\\\\________/\\\\\________/\\\\\\\\\\\\___________/\\\\\___________/\\\_________/\\\____
				//		 _\///////\\\/////_______/\\\///\\\_____\/\\\////////\\\_______/\\\///\\\_______/\\\\\\\_____/\\\\\\\__
				//		  _______\/\\\__________/\\\/__\///\\\___\/\\\______\//\\\____/\\\/__\///\\\____/\\\\\\\\\___/\\\\\\\\\_
				//		   _______\/\\\_________/\\\______\//\\\__\/\\\_______\/\\\___/\\\______\//\\\__\//\\\\\\\___\//\\\\\\\__
				//		    _______\/\\\________\/\\\_______\/\\\__\/\\\_______\/\\\__\/\\\_______\/\\\___\//\\\\\_____\//\\\\\___
				//		     _______\/\\\________\//\\\______/\\\___\/\\\_______\/\\\__\//\\\______/\\\_____\//\\\_______\//\\\____
				//		      _______\/\\\_________\///\\\__/\\\_____\/\\\_______/\\\____\///\\\__/\\\________\///_________\///_____
				//		       _______\/\\\___________\///\\\\\/______\/\\\\\\\\\\\\/_______\///\\\\\/__________/\\\_________/\\\____
				//		        _______\///______________\/////________\////////////___________\/////___________\///_________\///_____
			}
		}

		if(m_root != null) m_root.SetActive(show);
		return show;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
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