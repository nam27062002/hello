// MenuDragonLabUINotification.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Control a UI notification for the dragon selection Lab button.
/// </summary>
public class MenuDragonLabUINotification : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[SerializeField] private UINotification m_notification = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Start with hidden notification
		m_notification.Hide(false);

		// Refresh
		Refresh();
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Refresh each time the component is enabled
		m_notification.Set(false, false);
		Refresh();

		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventsUpdated);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventsUpdated);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether to show the notification or not.
	/// </summary>
	public void Refresh() {
		// Skip if not properly initialized
		if(m_notification == null) return;

		bool show = true;

		// Don't show if the player doesn't own any special dragon
		if(DragonManager.CurrentDragon == null) {
			m_notification.Hide(true, true);
			return;
		}
		
		// Don't show if the player doesn't own any special dragon
		if(show) {
			show &= DragonManager.CurrentDragon != null;
		}

		// Show only if the season is in PENDING_REWARDS state
		if(show) {
			show &= HDLiveDataManager.league.season.state == HDSeasonData.State.PENDING_REWARDS;
		}

		// Set notification's visibility
		m_notification.Set(show);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// New live events data has been received.
	/// </summary>
	void OnEventsUpdated() {
		Refresh();
	}
}