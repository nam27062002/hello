// HUDMissionCompletedNotification.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Control a UI notification for the missions button in the in-game HUD.
/// </summary>
public class HUDMissionCompletedNotification : MonoBehaviour {
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
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Mission>(GameEvents.MISSION_COMPLETED, OnMissionCompleted);
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Mission>(GameEvents.MISSION_COMPLETED, OnMissionCompleted);
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A mission has been completed!
	/// </summary>
	/// <param name="_mission">The mission that triggered the event</param>
	public void OnMissionCompleted(Mission _mission) {
		// Show notification!
		m_notification.Show();
	}

	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The popup that triggered the event.</param>
	public void OnPopupOpened(PopupController _popup) {
		// If it's the in-game missions popup, hide the notification
		if(_popup.GetComponent<PopupInGameMissions>() != null) {
			m_notification.Hide();
		}
	}
}