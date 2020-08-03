// GoalsScreenGlobalEventsTab.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// General controller for the Global Events tab in the Goals Screen.
/// </summary>
public class GlobalEventsScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Panel {
		OFFLINE,
		LOG_IN,
		NO_EVENT,
		EVENT_TEASER,
		EVENT_ACTIVE,
		LOADING,
		RETRY_REWARDS,
        REQUIRES_UPDATE,

		COUNT
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private GlobalEventsPanel[] m_panels = new GlobalEventsPanel[(int)Panel.COUNT];

	// Internal
	private Panel m_activePanel = Panel.OFFLINE;
	private HDQuestManager m_questManager;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Make sure we have all the required panels
		// Shouldn't happen, the custom editor makes sure everyhting is ok
		Debug.Assert(m_panels.Length == (int)Panel.COUNT, "Unexpected amount of defined panels");

		m_questManager = HDLiveDataManager.quest;

		// Init panels
		for(int i = 0; i < m_panels.Length; i++) {
			m_panels[i].panelId = (Panel)i;
			m_panels[i].anim = m_panels[i].GetComponent<ShowHideAnimator>();
		}
	}


	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<GlobalEventManager.RequestType>(MessengerEvents.GLOBAL_EVENT_UPDATED, OnEventDataUpdated);
		Messenger.AddListener(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR, OnNoEvent);
		Messenger.AddListener(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_NO_EVENTS, OnNoEvent);

		Messenger.AddListener<int,HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewards);
		Messenger.AddListener<int, HDLiveDataManager.ComunicationErrorCodes> (MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewDefinition);
		Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventsUpdated);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<GlobalEventManager.RequestType>(MessengerEvents.GLOBAL_EVENT_UPDATED, OnEventDataUpdated);
		Messenger.RemoveListener(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR, OnNoEvent);
		Messenger.RemoveListener(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_NO_EVENTS, OnNoEvent);

		Messenger.RemoveListener<int,HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewards);
		Messenger.RemoveListener<int, HDLiveDataManager.ComunicationErrorCodes> (MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewDefinition);
		Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventsUpdated);
	}


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Select active panel based on current global event state.
	/// </summary>
	public void Refresh() {
		// Do we need to go to the rewards screen?
		if ( m_questManager.EventExists() )
		{
			m_questManager.UpdateStateFromTimers();
			// If the current global event has a reward pending, go to the event reward screen
			if(m_questManager.data.m_state == HDLiveEventData.State.REWARD_AVAILABLE ) {
				// Show requesting!
				OnRetryRewardsButton();
				return;
				
			}
		}

		// Select active panel
		SelectPanel();

		// Refresh its content
		m_panels[(int)m_activePanel].Refresh();
	}

	protected void OnRewards(int _eventId ,HDLiveDataManager.ComunicationErrorCodes _err)
	{
		if ( _eventId == m_questManager.data.m_eventId )	
		{
			if ( _err == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR )
			{
				EventRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.EVENT_REWARD).ui.GetComponent<EventRewardScreen>();
				scr.StartFlow();
				InstanceManager.menuSceneController.GoToScreen(MenuScreen.EVENT_REWARD, true);	
			}
			else
			{
				// Show error message and retry button
				(m_panels[(int)Panel.RETRY_REWARDS] as GlobalEventsPanelRetryRewards).SetError(_err);
				SetActivePanel(Panel.RETRY_REWARDS);
			}
		}
	}

	public void OnRetryRewardsButton()
	{
		// Show requesting!
		if (DeviceUtilsManager.SharedInstance.internetReachability == NetworkReachability.NotReachable || !GameSessionManager.SharedInstance.IsLogged ())
		{
			SetActivePanel(Panel.OFFLINE);	
		}
		else
		{
			m_questManager.RequestRewards();
			SetActivePanel(Panel.LOADING);	
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Based on current event state, select which panel should be active.
	/// </summary>
	private void SelectPanel() {
		Panel targetPanel = Panel.NO_EVENT;
		HDQuestManager quest = HDLiveDataManager.quest;
		if ( quest.EventExists() )
		{
			if (DeviceUtilsManager.SharedInstance.internetReachability == NetworkReachability.NotReachable || !GameSessionManager.SharedInstance.IsLogged ())
			{
				targetPanel = Panel.OFFLINE;
			}
            else 
            { 
                if (quest.ShouldRequestDefinition()) {
                    quest.RequestDefinition();
                }

                if (quest.isWaitingForNewDefinition) {
                    targetPanel = Panel.LOADING;
                } else {
                    switch (quest.data.m_state) {
                        case HDLiveEventData.State.TEASING: {
                                targetPanel = Panel.EVENT_TEASER;
                            }
                            break;
                        case HDLiveEventData.State.NOT_JOINED:
                        case HDLiveEventData.State.JOINED: {
                                targetPanel = Panel.EVENT_ACTIVE;
                            }
                            break;
                        case HDLiveEventData.State.REQUIRES_UPDATE: {
                                targetPanel = Panel.REQUIRES_UPDATE;
                            }
                            break;
                        default: {
                                targetPanel = Panel.NO_EVENT;
                            }
                            break;
                    }
                }
			}
		}

		// Toggle active panel
		SetActivePanel(targetPanel);
	}

	/// <summary>
	/// Set the given panel as active one.
	/// </summary>
	/// <param name="_panel">Panel to be set as active.</param>
	/// <param name="_animate">Trigger animations?</param>
	private void SetActivePanel(Panel _panel, bool _animate = true) {
		// Store active panel
		m_activePanel = _panel;

		// Toggle active panel
		for(int i = 0; i < m_panels.Length; ++i) {
			// Use animators if available
			bool show = (i == (int)m_activePanel);
			if(m_panels[i].anim != null) {
				m_panels[i].anim.Set(show, _animate);
			} else {
				m_panels[i].gameObject.SetActive(show);
			}
		}

		// If showing the ACTIVE panel for the first time, trigger the tutorial
		if(m_activePanel == Panel.EVENT_ACTIVE && !UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.QUEST_INFO)) {
			// Open popup!
			string popupName = "PF_PopupInfoGlobalEvents";
			PopupManager.OpenPopupInstant("UI/Popups/Tutorial/" + popupName);

			// Mark tutorial step as completed
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.QUEST_INFO, true);

			// Tracking!
			HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a refresh every time we enter the tab!
	/// </summary>
	public void OnShowPreAnimation() {
		// Show loading panel
		SetActivePanel(Panel.LOADING, false);
		m_questManager.UpdateStateFromTimers();
		OnQuestDataUpdated();
	}

	private void OnQuestDataUpdated()
	{
		Refresh();
	}

	/// <summary>
	/// The global event manager has received new data from the server.
	/// </summary>
	private void OnEventDataUpdated(GlobalEventManager.RequestType _requestType) {
		// Different stuff depending on request type
		switch(_requestType) {
			case GlobalEventManager.RequestType.EVENT_DATA: {
				// If there is no event, instantly refresh the screen. Otherwise wait for the EVENT_STATE response
				if(GlobalEventManager.currentEvent == null) {
					Refresh();
				} else {
					//SelectPanel();
				}
			} break;

			case GlobalEventManager.RequestType.EVENT_REWARDS:
			case GlobalEventManager.RequestType.EVENT_STATE: {
				Refresh();
			} break;
		}
	}

	private void OnNoEvent(){
		Refresh();
	}

	/// <summary>
	/// The retry button on the offline panel has been pressed.
	/// </summary>
	public void OnOfflineRetryButton() {

		if (DeviceUtilsManager.SharedInstance.internetReachability != NetworkReachability.NotReachable && GameSessionManager.SharedInstance.IsLogged ())
		{
			// Show loading and ask for my evetns
			SetActivePanel(Panel.LOADING);
			if (!HDLiveDataManager.instance.RequestMyLiveData())
			{
				StartCoroutine( RemoveLoading());
			}
		}
		else
		{
			// Message no connection
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

    /// <summary>
    /// Send the player to the update button
    /// </summary>
    public void OnUpdateButton()
    {
        ApplicationManager.Apps_OpenAppInStore(ApplicationManager.EApp.HungryDragon);
    }

	void OnNewDefinition(int _eventId, HDLiveDataManager.ComunicationErrorCodes _err)
	{
		if ( _err == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR && _eventId == m_questManager.data.m_eventId)
		{
			Refresh();
		}
	}

	void OnEventsUpdated()
	{
		Refresh();
	}

	IEnumerator RemoveLoading()
	{
		yield return new WaitForSeconds(0.5f);
		Refresh();
	}
}