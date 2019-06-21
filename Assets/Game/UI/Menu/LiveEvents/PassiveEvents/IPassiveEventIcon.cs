// IPassiveEventIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/11/2018.
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
/// Base class used as controller for a passive event icon in the menu HUD.
/// </summary>
public abstract class IPassiveEventIcon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 1f;	// Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_rootAnim = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;

	// Internal
	protected HDPassiveEventManager m_passiveEventManager = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Get tournament manager
		m_passiveEventManager = GetEventManager();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	protected virtual void Start() {
		// Get latest data from the manager
		RefreshData(true);

		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventStateUpdated);
		Messenger.AddListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewEventDefinition);
		Messenger.AddListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, OnEventFinished);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransition);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventStateUpdated);
		Messenger.RemoveListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewEventDefinition);
		Messenger.RemoveListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_FINISHED, OnEventFinished);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenTransition);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);

		// Get latest data from the manager
		RefreshData(true);
	}

	/// <summary>
	/// Called periodically - to avoid doing stuff every frame.
	/// </summary>
	protected virtual void UpdatePeriodic() {
		// Skip if we're not active - probably in a screen we don't belong to
		if(!isActiveAndEnabled) return;

		// Refresh the timer!
		RefreshTimer(true);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {
		// Cancel periodic update
		CancelInvoke();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get latest data from the manager and update visuals.
	/// </summary>
	/// <param name="_checkVisibility">Whether to refresh visibility as well or not.</param>
	public void RefreshData(bool _checkVisibility) {
		// Make sure tournament is at the right state
		m_passiveEventManager.UpdateStateFromTimers();

		// Update visibility
		if(_checkVisibility) RefreshVisibility();

		// Nothing else to do if there is no active event
		if(!m_passiveEventManager.EventExists()) return;

		// Let each specific event type refresh its custom visuals
		RefreshDataInternal();

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
					2
				);
			}
		}

		// Manage timer expiration when the icon is visible
		if(_checkExpiration && remainingSeconds <= 0) {
			RefreshData(true);
		}
	}

	/// <summary>
	/// Check whether the icon can be displayed or not.
	/// </summary>
	public bool RefreshVisibility() {
		// Never during tutorial
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_TOURNAMENTS_AT_RUN) {
			m_rootAnim.ForceHide(false);
			return false;
		}

		// Do we have a valid event?
		bool wasVisible = m_rootAnim != null ? m_rootAnim.visible : true;
		bool show = false;
		if(m_passiveEventManager.EventExists()) {
			// Show only for specific states
			switch(m_passiveEventManager.data.m_state) {
				//case HDLiveEventData.State.TEASING:			// No teasing for passive events
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

			// Perform extra checks based on passive event type
			// Skip if we shouldn't be displayed anyway
			if(show) {
				show &= RefreshVisibilityInternal();
			}
		}
        
		// Just one last check to make sure event is active
        if(show) {
       		show = m_passiveEventManager.isActive;
        }

		// If going visible, refresh data
		if(!wasVisible && show) {
			RefreshData(false);
		}

		// Apply!
		if(m_rootAnim != null) m_rootAnim.Set(show);
		return show;
	}

	//------------------------------------------------------------------------//
	// ABSTRACT AND VIRTUAL METHODS											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the manager for this specific passive event type.
	/// </summary>
	/// <returns>The event manager corresponding to this event type.</returns>
	protected abstract HDPassiveEventManager GetEventManager();

	/// <summary>
	/// Update visuals when new data has been received.
	/// </summary>
	protected virtual void RefreshDataInternal() {
		// To be implemented by heirs if needed
	}

	/// <summary>
	/// Do custom visibility checks based on passive event type.
	/// </summary>
	/// <returns>Whether the icon can be displayed or not.</returns>
	protected virtual bool RefreshVisibilityInternal() {
		// To be implemented by heirs if needed
		return true;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Received a new event definition.
	/// </summary>
    private void OnNewEventDefinition(int _eventId, HDLiveDataManager.ComunicationErrorCodes _error) {
		RefreshData(true);
	}

	/// <summary>
	/// A live event has finished.
	/// </summary>
    private void OnEventFinished(int _eventId, HDLiveDataManager.ComunicationErrorCodes _error) {
        RefreshVisibility();
    }

	/// <summary>
	/// A live event has changed state.
	/// </summary>
    private void OnEventStateUpdated() {
        RefreshData(true);
    }

	/// <summary>
	/// The menu screen change animation has started.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
    private void OnMenuScreenTransition(MenuScreen _from, MenuScreen _to) {
		RefreshVisibility();
	}
}