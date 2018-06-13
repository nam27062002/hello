// GlobalEventsScreenActivePanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Panel corresponding to an active global event.
/// </summary>
public class GlobalEventsPanelTeaser : GlobalEventsPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float EVENT_COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private TextMeshProUGUI m_timerText = null;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	public void OnEnable() {
		// Program periodic update call
		InvokeRepeating("UpdatePeriodic", 0f, EVENT_COUNTDOWN_UPDATE_INTERVAL);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	public void OnDisable() {
		// Cancel periodic call
		CancelInvoke();
	}

	/// <summary>
	/// Called periodically.
	/// </summary>
	private void UpdatePeriodic() {
		// Just in case
		if ( !HDLiveEventsManager.instance.m_quest.EventExists() ) return;

		HDQuestManager questManager = HDLiveEventsManager.instance.m_quest;

		// Update timer
		double remainingSeconds = questManager.data.remainingTime.TotalSeconds;
		m_timerText.text = TimeUtils.FormatTime(
			System.Math.Max(0, remainingSeconds),	// Never show negative time!
			TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
			4
		);

		// [AOC] Manage timer end when this panel is active
		if(remainingSeconds <= 0) {
			// TODO
			// GlobalEventManager.RequestCurrentEventState();	// This should change the active panel
			questManager.UpdateStateFromTimers();
			// Send Event to update this!
		}

		if ( questManager.data.m_state != HDLiveEventData.State.TEASING )
		{
			// Exit from here!!
			Messenger.Broadcast(MessengerEvents.LIVE_EVENT_STATES_UPDATED);
		}
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	override public void Refresh() {
		// Force a first update on the timer
		UpdatePeriodic();
	}

}