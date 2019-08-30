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
		CHESTS,
        LEAGUES
	}

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed
    [SerializeField] private GameObject m_leagueActiveGroup = null;
    [SerializeField] private GameObject m_leagueInactiveGroup = null;
    [SerializeField] private TextMeshProUGUI m_leagueCountdownText = null;
    [SerializeField] private Slider m_leagueCountdownSlider = null;
    [Space]
    [SerializeField] private GameObject m_eventActiveGroup = null;
	[SerializeField] private GameObject m_eventInactiveGroup = null;
	[SerializeField] private TextMeshProUGUI m_eventCountdownText = null;
	[SerializeField] private Slider m_eventCountdownSlider = null;
	[Space]
	[SerializeField] private HorizontalOrVerticalLayoutGroup m_buttonsLayout = null;
	[SerializeField] private SelectableButtonGroup m_buttons = null;

	HDQuestManager m_quest;
    HDLeagueController m_league;
    HDSeasonData m_season;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize references
		m_quest = HDLiveDataManager.quest;
        m_league = HDLiveDataManager.league;
        m_season = HDLiveDataManager.league.season;

        // Subscribe to external events.
        Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStarted);

		// Force a first refresh
		if (InstanceManager.menuSceneController != null && InstanceManager.menuSceneController.transitionManager != null)
		{
			MenuScreen prev = InstanceManager.menuSceneController.transitionManager.prevScreen;
			MenuScreen current = InstanceManager.menuSceneController.transitionManager.currentScreen;
			OnTransitionStarted( prev, current );
		}

		// Remove buttons from the selection group if they can't be selected
		// Chests
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_CHESTS_AT_RUN) {
			ExcludeButton(Buttons.CHESTS);
		}

		// Quests
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_QUESTS_AT_RUN) {
			ExcludeButton(Buttons.GLOBAL_EVENTS);
		}

        // Quests
        if (UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_LEAGUES_AT_RUN)
        {
            ExcludeButton(Buttons.LEAGUES);
        }
    }

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, COUNTDOWN_UPDATE_INTERVAL);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure timers will be updated
		UpdatePeriodic();
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStarted);
	}

	/// <summary>
	/// Called periodically - to avoid doing stuff every frame.
	/// </summary>
	private void UpdatePeriodic() {
		// Skip if we're not active - probably in a screen we don't belong to
		if(!isActiveAndEnabled) return;

		// Refresh visible event button based on event state
		// GlobalEvent evt = GlobalEventManager.currentEvent;
		// bool eventAvailable = evt != null && evt.isActive;
		bool eventAvailable = m_quest.EventExists() && m_quest.IsRunning();
        bool leagueAvailable = m_league.isActive;

		// Consider tutorial as well!
		eventAvailable &= UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_QUESTS_AT_RUN;
        leagueAvailable &= UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_LEAGUES_AT_RUN;

		// Does it actually change?
		bool dirty = false;
		dirty |= m_eventActiveGroup.activeSelf != eventAvailable;
		dirty |= m_eventInactiveGroup.activeSelf != !eventAvailable;
        dirty |= m_leagueActiveGroup.activeSelf != leagueAvailable;
        dirty |= m_leagueInactiveGroup.activeSelf != !leagueAvailable;

        // Apply
        if (dirty) {
			m_eventActiveGroup.SetActive(eventAvailable);
			m_eventInactiveGroup.SetActive(!eventAvailable);

            m_leagueActiveGroup.SetActive(leagueAvailable);
            m_leagueInactiveGroup.SetActive(!leagueAvailable);

            // [AOC] Enabling/disabling objects while the layout is inactive makes the layout to not update properly
            //		 Luckily for us Unity provides us with the right tools to rebuild it
            //		 Fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-690
            if (m_buttonsLayout != null) {
				LayoutRebuilder.ForceRebuildLayoutImmediate(m_buttonsLayout.transform as RectTransform);
			}
		}

		// Event Timer - only if active
		if(m_eventActiveGroup.activeSelf) {

			HDLiveEventData evt = m_quest.data;

			// Timer text
			double remainingSeconds = System.Math.Max(0, evt.remainingTime.TotalSeconds);	// Never go negative!
			m_eventCountdownText.text = TimeUtils.FormatTime(
				remainingSeconds,
				TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
				2
			);

			// Timer bar
			TimeSpan totalSpan = evt.definition.m_endTimestamp - evt.definition.m_startTimestamp;
			m_eventCountdownSlider.value = 1f - (float)(remainingSeconds/totalSpan.TotalSeconds);

			// If time has finished, request new data
			if(remainingSeconds <= 0) {
				// GlobalEventManager.RequestCurrentEventState();
				m_quest.UpdateStateFromTimers();
			}
		}

        // League Timer - only if active
        if (m_leagueActiveGroup.activeSelf)
        {

            double remainingSeconds = 0;
            double durationSeconds = 1; // To avoid division by 0
            if (m_season.state == HDSeasonData.State.TEASING)
            {
                remainingSeconds = m_season.timeToStart.TotalSeconds;
                durationSeconds = m_season.durationTeasing.TotalSeconds;
            }
            else if (m_season.state == HDSeasonData.State.WAITING_RESULTS)
            {
                remainingSeconds = m_season.timeToResuts.TotalSeconds;
                durationSeconds = m_season.durationWaitResults.TotalSeconds;
            }
            else
            {
                remainingSeconds = m_season.timeToEnd.TotalSeconds;
                durationSeconds = m_season.durationWaitNewSeason.TotalSeconds;
            }

            m_eventCountdownText.text = TimeUtils.FormatTime(
                remainingSeconds,
                TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
                2
            );

            // Timer bar
            m_eventCountdownSlider.value = 1f - (float)(remainingSeconds / durationSeconds);

            // If time has finished, request new data
            if (remainingSeconds <= 0)
            {
                m_league = HDLiveDataManager.league;
                m_season = m_league.season;
            }
        }
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Excludes a tab button from the group.
	/// It won't be disabled so it can be tapped, but it will be grayed out and won't be selectable.
	/// To be used when disabling features and wanting to give feedback.
	/// </summary>
	/// <param name="_btn">Button to be excluded.</param>
	private void ExcludeButton(Buttons _btn) {
		// Get target button
		SelectableButton btn = m_buttons.GetButton((int)_btn);
		if(btn == null) return;

		// Mark it as non-selectable
		btn.selectionAllowed = false;

		// Apply visual effect
		// [AOC] Going to hell for this! Simulate game's standard disable FX by tinting the button and the icon.
		//		 We can't use a normal UIColorFX because it would conflict with the button's animator
		Image[] childImages = btn.GetComponentsInChildren<Image>(true);
		for(int i = 0; i < childImages.Length; ++i) {
			// Special color for root image
			if(childImages[i].transform == btn.transform) {
				childImages[i].color = Colors.ParseHexString("#3C3B3B53");	// Semi-transparent gray-ish
			} else {
				childImages[i].name = "RenamedImage";	// Rename it so the animator doesn't override the color!
				childImages[i].color = Colors.ParseHexString("#6F6F6F64");	// Lighter semi-transparent gray-ish
			}
		}
	}

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
	/// Global Events button has been pressed.
	/// </summary>
	public void OnGlobalEventsButton() {
		// Check tutorial!
		int remainingRuns = GameSettings.ENABLE_QUESTS_AT_RUN - UsersManager.currentUser.gamesPlayed;
		if(remainingRuns > 0) {
			// Show error message
			string tid = remainingRuns == 1 ? "TID_MORE_RUNS_REQUIRED" : "TID_MORE_RUNS_REQUIRED_PLURAL";
			UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize(tid, remainingRuns.ToString()),
				new Vector2(0.5f, 0.25f),
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);
		} else {
			// Go to chests screen
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.GLOBAL_EVENTS);
		}
	}

	/// <summary>
	/// Chests button has been pressed.
	/// </summary>
	public void OnChestsButton() {
		// Check tutorial!
		int remainingRuns = GameSettings.ENABLE_QUESTS_AT_RUN - UsersManager.currentUser.gamesPlayed;
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_CHESTS_AT_RUN) {
			// Show error message
			string tid = remainingRuns == 1 ? "TID_MORE_RUNS_REQUIRED" : "TID_MORE_RUNS_REQUIRED_PLURAL";
			UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize(tid, remainingRuns.ToString()),
				new Vector2(0.5f, 0.25f),
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);
		} else {
			// Go to chests screen
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.CHESTS);
		}
	}

    /// <summary>
    /// Leagues button has been pressed.
    /// </summary>
    public void OnLeaguesButton()
    {
        // Check tutorial!
        int remainingRuns = GameSettings.ENABLE_LEAGUES_AT_RUN - UsersManager.currentUser.gamesPlayed;
        if (UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_LEAGUES_AT_RUN)
        {
            // Show error message
            string tid = remainingRuns == 1 ? "TID_MORE_RUNS_REQUIRED" : "TID_MORE_RUNS_REQUIRED_PLURAL";
            UIFeedbackText.CreateAndLaunch(
                LocalizationManager.SharedInstance.Localize(tid, remainingRuns.ToString()),
                new Vector2(0.5f, 0.25f),
                this.GetComponentInParent<Canvas>().transform as RectTransform
            );
        }
        else
        {
            // Go to leagues screen
            InstanceManager.menuSceneController.GoToScreen(MenuScreen.LEAGUES);
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
            case MenuScreen.LEAGUES:        m_buttons.SelectButton((int)Buttons.LEAGUES); break;
        }
	}
}