// GoalsScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

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
/// Main controller for the goals screen.
/// </summary>
public class GoalsScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_eventActiveGroup = null;
	[SerializeField] private GameObject m_eventInactiveGroup = null;
	[SerializeField] private TextMeshProUGUI m_eventCountdownText = null;
	[SerializeField] private Slider m_eventCountdownSlider = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_chestsCountdownText = null;
	[Space]
	[SerializeField] private TabSystem m_tabs = null;
	public TabSystem tabs {
		get { return m_tabs; }
	}

	// Internal
	private float m_countdownUpdateTimer = 0f;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure timers will be updated
		m_countdownUpdateTimer = 0f;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Update countdowns at intervals
		m_countdownUpdateTimer -= Time.deltaTime;
		if(m_countdownUpdateTimer <= 0f) {
			// Reset timer
			m_countdownUpdateTimer = COUNTDOWN_UPDATE_INTERVAL;

			// Chests timer
			m_chestsCountdownText.text = TimeUtils.FormatTime(
				ChestManager.timeToReset.TotalSeconds,
				TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
				2
			);

			// Refresh visible event button based on event state
			GlobalEvent evt = GlobalEventManager.currentEvent;
			bool validEvent = evt != null && evt.isActive;
			m_eventActiveGroup.SetActive(validEvent);
			m_eventInactiveGroup.SetActive(!validEvent);

			// Event Timer - only if active
			if(validEvent) {
				// Timer text
				m_eventCountdownText.text = TimeUtils.FormatTime(
					evt.remainingTime.TotalSeconds,
					TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
					2
				);

				// Timer bar
				TimeSpan totalSpan = evt.endTimestamp - evt.startTimestamp;
				m_eventCountdownSlider.value = 1f - (float)(evt.remainingTime.TotalSeconds/totalSpan.TotalSeconds);

				// If time has finished, request new data
				if (GlobalEventManager.currentEvent.remainingTime.TotalSeconds <= 0 ){
					GlobalEventManager.RequestCurrentEventState();
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	public void OnPlayButton() {
		if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SECOND_RUN)) {
			HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._10_continue_clicked);
		}
	}
}