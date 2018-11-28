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

	[Flags]
	private enum DisplayLocation {
		// [AOC] Max 32 values (try inheriting from long if more are needed)
		NONE					= 1 << 0,
		PLAY_SCREEN				= 1 << 1,
		NORMAL_MODE				= 1 << 2,
		TOURNAMENT_MODE			= 1 << 3,
		INGAME					= 1 << 4,
		OPEN_EGG_SCREEN			= 1 << 5
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_root = null;
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

		Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventStateUpdated);
		Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewEventDefinition);
		Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, OnEventFinished);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransition);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventStateUpdated);
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewEventDefinition);
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, OnEventFinished);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransition);
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
		if(!m_passiveEventManager.EventExists()) return;

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
			m_root.ForceHide(false);
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
			// Skip if we shouldn't be displayed anyway
			if(show) {
				Modifier mod = m_passiveEventManager.m_passiveEventDefinition.mainMod;
				if(mod != null) {
					// Where can this mod be displayed?
					DisplayLocation location = DisplayLocation.NONE;
                    switch(mod.GetUICategory()) {
						case "stats": {
							location = DisplayLocation.PLAY_SCREEN 
								| DisplayLocation.NORMAL_MODE
								| DisplayLocation.TOURNAMENT_MODE
								| DisplayLocation.INGAME;
						} break;

						case "metagame": {
							location = DisplayLocation.PLAY_SCREEN 
								| DisplayLocation.NORMAL_MODE
								| DisplayLocation.OPEN_EGG_SCREEN;
						} break;

						case "levelUp": {
							location = DisplayLocation.PLAY_SCREEN 
								| DisplayLocation.NORMAL_MODE
								| DisplayLocation.INGAME;
						} break;
					}

					// Are we in the right place?
					// Loading Screen
					if(LoadingScreen.isVisible) {
						// Check tournament mode
						switch(GameSceneController.mode) {
							case GameSceneController.Mode.DEFAULT: {
								show &= CheckLocation(location, DisplayLocation.INGAME | DisplayLocation.NORMAL_MODE);
							} break;

							case GameSceneController.Mode.TOURNAMENT: {
								show &= CheckLocation(location, DisplayLocation.INGAME | DisplayLocation.TOURNAMENT_MODE);
							} break;
						}
					}

					// Menu
					else if(InstanceManager.menuSceneController != null) {
						// Which screen?
						MenuScreen currentScreen = InstanceManager.menuSceneController.currentScreen;
						switch(currentScreen) {
							case MenuScreen.PLAY: {
								show &= CheckLocation(location, DisplayLocation.PLAY_SCREEN);
							} break;

							case MenuScreen.DRAGON_SELECTION: {
								show &= CheckLocation(location, DisplayLocation.NORMAL_MODE);
							} break;

							case MenuScreen.TOURNAMENT_INFO: {
								show &= CheckLocation(location, DisplayLocation.TOURNAMENT_MODE);
							} break;

							case MenuScreen.OPEN_EGG: {
								show &= CheckLocation(location, DisplayLocation.OPEN_EGG_SCREEN);
							} break;
						}
					}

					// Ingame / Results
					else {
						// Check tournament mode
						switch(GameSceneController.mode) {
							case GameSceneController.Mode.DEFAULT: {
								show &= CheckLocation(location, DisplayLocation.INGAME | DisplayLocation.NORMAL_MODE);
							} break;

							case GameSceneController.Mode.TOURNAMENT: {
								show &= CheckLocation(location, DisplayLocation.INGAME | DisplayLocation.TOURNAMENT_MODE);
							} break;
						}
					}
				}
			}
		}
        
        if ( show ) {
            show = m_passiveEventManager.m_isActive;
        }

		if(m_root != null) m_root.Set(show);
		return show;
	}

	//------------------------------------------------------------------------//
	// INTERNAL	UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Compare two location flags.
	/// </summary>
	/// <returns><c>true</c>, if the location to check is included in the reference location.</returns>
	/// <param name="_ref">Reference location.</param>
	/// <param name="_toCheck">Location to check.</param>
	private bool CheckLocation(DisplayLocation _ref, DisplayLocation _toCheck) {
		return (_ref & _toCheck) == _toCheck;	// This tells us if all the flags in _toCheck are included in _ref
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// We got an update on the tournament state.
	/// </summary>
    private void OnNewEventDefinition(int _eventId, HDLiveEventsManager.ComunicationErrorCodes _error) {
		RefreshData();
	}

    private void OnEventFinished(int _eventId, HDLiveEventsManager.ComunicationErrorCodes _error) {
        RefreshVisibility();
    }

    private void OnEventStateUpdated() {
        RefreshData();
    }

    private void OnMenuScreenTransition(MenuScreen _from, MenuScreen _to) {
		RefreshVisibility();
	}
}