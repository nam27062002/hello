// LabGoalsBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

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
/// Controller for the bottom bar of the lab goals screens (missions and leagues).
/// </summary>
public class LabGoalsBar : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds

	public enum Buttons {
		MISSIONS,
		LEAGUES
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_seasonActiveGroup = null;
	[SerializeField] private GameObject m_seasonInactiveGroup = null;
	[SerializeField] private TextMeshProUGUI m_seasonCountdownText = null;
	[SerializeField] private Slider m_seasonCountdownBar = null;
	[Space]
	[SerializeField] private HorizontalOrVerticalLayoutGroup m_buttonsLayout = null;
	[SerializeField] private SelectableButtonGroup m_buttons = null;

	// Internal
	HDSeasonData m_season = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize references
		m_season = HDLiveDataManager.league.season;

		// Subscribe to external events.
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnTransitionStarted);

		// Force a first refresh
		if(InstanceManager.menuSceneController != null && InstanceManager.menuSceneController.transitionManager != null) {
			MenuScreen prev = InstanceManager.menuSceneController.transitionManager.prevScreen;
			MenuScreen current = InstanceManager.menuSceneController.transitionManager.currentScreen;
			OnTransitionStarted(prev, current);
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
		// Make sure timers are updated
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

		// Refresh visible season button based on season state
		bool seasonAvailable = m_season.IsRunning();    // Joined or Not Joined
		double remainingSeconds = 0;
		if(seasonAvailable) {
			// The season might seem available event when the timer has finished because the UpdateState() hasn't been called yet
			// Consider it as not available in that case
			remainingSeconds = m_season.timeToClose.TotalSeconds;
			seasonAvailable &= (remainingSeconds > 0);
		}

		// Does it actually change?
		bool dirty = false;
		dirty |= m_seasonActiveGroup.activeSelf != seasonAvailable;
		dirty |= m_seasonInactiveGroup.activeSelf != !seasonAvailable;

		// Apply
		if(dirty) {
			m_seasonActiveGroup.SetActive(seasonAvailable);
			m_seasonInactiveGroup.SetActive(!seasonAvailable);

			// [AOC] Enabling/disabling objects while the layout is inactive makes the layout to not update properly
			//		 Luckily for us Unity provides us with the right tools to rebuild it
			//		 Fixes issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-690
			if(m_buttonsLayout != null) {
				LayoutRebuilder.ForceRebuildLayoutImmediate(m_buttonsLayout.transform as RectTransform);
			}
		}

		// Event Timer - only if active
		if(m_seasonActiveGroup.activeSelf) {
			// Timer text
			m_seasonCountdownText.text = TimeUtils.FormatTime(
				remainingSeconds,
				TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
				2
			);

			// Timer bar
			TimeSpan totalSpan = m_season.duration;
            m_seasonCountdownBar.value = 1f - (float)(remainingSeconds / totalSpan.TotalSeconds);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// If no special dragon is available, show error message
		if(DragonManager.CurrentDragon == null) {
			UIFeedbackText.CreateAndLaunch(
				"TID_LAB_ERROR_NO_SPECIAL_DRAGON_OWNED",
				GameConstants.Vector2.center,
				GetComponentInParent<Canvas>().transform as RectTransform
			).text.color = UIConstants.ERROR_MESSAGE_COLOR;
			return;
		}

		// Just in case, make sure all assets required for the current dragon are available
		// Usually we shouldn't get access to this screen if not, but in some corner cases it's possible to get to this point (HDK-4661, HDK-4663)
		// Get assets download handle for current dragon
		string currentDragonSku = UsersManager.currentUser.CurrentDragon;
		Downloadables.Handle currentDragonHandle = HDAddressablesManager.Instance.GetHandleForClassicDragon(currentDragonSku);
		if(!currentDragonHandle.IsAvailable()) {
			// If needed, show assets download popup

			PopupAssetsDownloadFlow.OpenPopupByState(currentDragonHandle, PopupAssetsDownloadFlow.PopupType.ANY, AssetsDownloadFlow.Context.NOT_SPECIFIED);

			// Go back to lab dragon selection screen
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.LAB_DRAGON_SELECTION);

			// Don't do anything else!
			return;
		}

		// Tracking
		// [AOC] TODO!! Tournament as reference:
		//HDTrackingManager.Instance.Notify_TournamentClickOnEnter(m_definition.m_name, _flow.currency);

		// Go to play!
		InstanceManager.menuSceneController.GoToGame();
	}

	/// <summary>
	/// The current menu screen has changed (animation starts now).
	/// </summary>
	/// <param name="_from">Source screen.</param>
	/// <param name="_to">Target screen.</param>
	private void OnTransitionStarted(MenuScreen _from, MenuScreen _to) {
		// If the new screen is any of our screens of interest, select the matching button!
		switch(_to) {
			case MenuScreen.LAB_LEAGUES:	m_buttons.SelectButton((int)Buttons.LEAGUES);	break;
			case MenuScreen.LAB_MISSIONS:	m_buttons.SelectButton((int)Buttons.MISSIONS);	break;
		}
	}
}