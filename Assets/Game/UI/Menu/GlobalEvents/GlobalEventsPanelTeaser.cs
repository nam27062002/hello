﻿// GlobalEventsScreenActivePanel.cs
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
	[SerializeField] private Slider m_timerProgressBar = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected override void OnEnable() {
		// Call parent
		base.OnEnable();

		// Initialize progress bar
		if(m_timerProgressBar != null) {
			m_timerProgressBar.minValue = 0;
			m_timerProgressBar.maxValue = 1;
		}

		// Program periodic update call
		InvokeRepeating("UpdatePeriodic", 0f, EVENT_COUNTDOWN_UPDATE_INTERVAL);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected override  void OnDisable() {
		// Call parent
		base.OnDisable();

		// Cancel periodic call
		CancelInvoke();
	}

	/// <summary>
	/// Called periodically.
	/// </summary>
	private void UpdatePeriodic() {
		// Just in case
		if ( !HDLiveDataManager.quest.EventExists() ) return;

		IQuestManager questManager = HDLiveDataManager.quest;
		HDLiveQuestDefinition liveQuestDef = questManager.GetQuestDefinition();

		// Update timer
		double remainingSeconds = questManager.GetRemainingTime();
		m_timerText.text = TimeUtils.FormatTime(
			System.Math.Max(0, remainingSeconds),	// Never show negative time!
			TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
			4
		);

		// Update progress bar
		if(m_timerProgressBar != null) {
			double progress = remainingSeconds / (liveQuestDef.m_startTimestamp - liveQuestDef.m_teasingTimestamp).TotalSeconds;
			m_timerProgressBar.value = 1f - (float)progress;
		}

		// [AOC] Manage timer end when this panel is active
		if(remainingSeconds <= 0) {
			// TODO
			// GlobalEventManager.RequestCurrentEventState();	// This should change the active panel
			questManager.UpdateStateFromTimers();
			// Send Event to update this!
		}

		if ( questManager.GetQuestData().m_state != HDLiveEventData.State.TEASING )
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