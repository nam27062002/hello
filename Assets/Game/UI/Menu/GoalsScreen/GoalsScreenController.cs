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

	public enum Tabs {
		MISSIONS,
		CHESTS,
		GLOBAL_EVENTS,
		LEADERBOARDS
	}

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
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Disable some tabs during FTUX
		// Chests
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_CHESTS_AT_RUN) {
			m_tabs.GetTab((int)Tabs.CHESTS).tabEnabled = false;
		}

		// Global Events
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_GLOBAL_EVENTS_AT_RUN) {
			m_tabs.GetTab((int)Tabs.GLOBAL_EVENTS).tabEnabled = false;
		}
	}

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

			// [AOC] Enabling/disabling objects while the layout is inactive makes the layout to not update properly
			//		 Luckily for us Unity provides us with the right tools to rebuild it
			//		 Fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-690
			HorizontalOrVerticalLayoutGroup layout = m_eventActiveGroup.transform.GetComponentInParent<HorizontalOrVerticalLayoutGroup>();
			if(layout != null) LayoutRebuilder.ForceRebuildLayoutImmediate(layout.transform as RectTransform);

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

	/// <summary>
	/// The screen is about to be displayed.
	/// </summary>
	public void OnShowPreAnimation() {
		// If active tab is the Events tab, force a refresh
		if(m_tabs.currentScreen != null) {
			if(m_tabs.currentScreen.screenName == "eventsTab") {
				// [AOC] Hardcore way to do it: simulate an OnShow() event like we just switched to this tab
				m_tabs.currentScreen.OnShow.Invoke();
			}
		}
	}
}