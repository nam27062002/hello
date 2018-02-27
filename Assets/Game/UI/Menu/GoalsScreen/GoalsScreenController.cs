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

	public enum Buttons {
		MISSIONS,
		GLOBAL_EVENTS,
		CHESTS
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
	[SerializeField] private SelectableButtonGroup m_buttons = null;

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
			m_buttons.GetButton((int)Buttons.CHESTS).button.interactable = false;
		}

		// Global Events
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_GLOBAL_EVENTS_AT_RUN) {
			m_buttons.GetButton((int)Buttons.GLOBAL_EVENTS).button.interactable = false;
		}

		// Subscribe to external events.
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStarted);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure timers will be updated
		m_countdownUpdateTimer = 0f;
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStarted);
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
				double remainingSeconds = System.Math.Max(0, evt.remainingTime.TotalSeconds);	// Never go negative!
				m_eventCountdownText.text = TimeUtils.FormatTime(
					remainingSeconds,
					TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
					2
				);

				// Timer bar
				TimeSpan totalSpan = evt.endTimestamp - evt.startTimestamp;
				m_eventCountdownSlider.value = 1f - (float)(remainingSeconds/totalSpan.TotalSeconds);

				// If time has finished, request new data
				if(remainingSeconds <= 0) {
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
	/// <summary>
	/// Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SECOND_RUN)) {
			HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._10_continue_clicked);
		}
	}

	/// <summary>
	/// The current menu screen has changed (animation starts now).
	/// </summary>
	/// <param name="_from">Source screen.</param>
	/// <param name="_to">Target screen.</param>
	private void OnTransitionStarted(MenuScreen _from, MenuScreen _to) {
		// If the new screen is any of our screens of interest, select the matching button!
		switch(_to) {
			case MenuScreen.MISSIONS:		m_buttons.SelectButton((int)Buttons.MISSIONS);	break;
			case MenuScreen.CHESTS:			m_buttons.SelectButton((int)Buttons.CHESTS);	break;
			case MenuScreen.GLOBAL_EVENTS:	m_buttons.SelectButton((int)Buttons.GLOBAL_EVENTS);	break;
		}
	}
}