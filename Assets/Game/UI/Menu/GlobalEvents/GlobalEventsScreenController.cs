// GoalsScreenGlobalEventsTab.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

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

		m_questManager = HDLiveEventsManager.instance.m_quest;

		// Init panels
		for(int i = 0; i < m_panels.Length; i++) {
			m_panels[i].panelId = (Panel)i;
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

		Messenger.AddListener<int,HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewards);

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<GlobalEventManager.RequestType>(MessengerEvents.GLOBAL_EVENT_UPDATED, OnEventDataUpdated);
		Messenger.RemoveListener(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_ERROR, OnNoEvent);
		Messenger.RemoveListener(MessengerEvents.GLOBAL_EVENT_CUSTOMIZER_NO_EVENTS, OnNoEvent);

		Messenger.RemoveListener<int,HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewards);
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
				m_questManager.RequestRewards();
				SetActivePanel(Panel.LOADING);

				/*
				EventRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.EVENT_REWARD).ui.GetComponent<EventRewardScreen>();
				scr.StartFlow();
				InstanceManager.menuSceneController.GoToScreen(MenuScreen.EVENT_REWARD);	
				*/
				return;
				
			}
		}

		// Select active panel
		SelectPanel();

		// Refresh its content
		m_panels[(int)m_activePanel].Refresh();
	}

	protected void OnRewards(int _eventId ,HDLiveEventsManager.ComunicationErrorCodes _err)
	{
		if ( _eventId == m_questManager.data.m_eventId )	
		{
			if ( _err == HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR )
			{
				EventRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.EVENT_REWARD).ui.GetComponent<EventRewardScreen>();
				scr.StartFlow();
				InstanceManager.menuSceneController.GoToScreen(MenuScreen.EVENT_REWARD);	
			}
			else
			{
				// Show error message and retry button

				SetActivePanel(Panel.RETRY_REWARDS);
			}
		}
	}

	public void OnRetryRewardsButton()
	{
		// Show requesting!
		m_questManager.RequestRewards();
		SetActivePanel(Panel.LOADING);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Based on current event state, select which panel should be active.
	/// </summary>
	private void SelectPanel() {

		Panel targetPanel = Panel.NO_EVENT;
		HDQuestManager quest = HDLiveEventsManager.instance.m_quest;
		if ( quest.EventExists() )
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				targetPanel = Panel.OFFLINE;
			}
			else if ( GameSessionManager.SharedInstance.IsLogged () )
			{
				targetPanel = Panel.LOG_IN;
			}
			else
			{
				switch(quest.data.m_state) {
					case HDLiveEventData.State.TEASING: {
						targetPanel = Panel.EVENT_TEASER;
					} break;

					case HDLiveEventData.State.NOT_JOINED:
					case HDLiveEventData.State.JOINED: {
						targetPanel = Panel.EVENT_ACTIVE;
					} break;

					default: {
						targetPanel = Panel.NO_EVENT;
					} break;
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
	private void SetActivePanel(Panel _panel) {
		// Store active panel
		m_activePanel = _panel;

		// Toggle active panel
		// [AOC] Use animators?
		for(int i = 0; i < m_panels.Length; ++i) {
			m_panels[i].gameObject.SetActive(i == (int)m_activePanel);
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
		SetActivePanel(Panel.LOADING);
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

		if (Application.internetReachability != NetworkReachability.NotReachable)
		{
			// TODO: Check if log in!

			// Show loading panel
			SetActivePanel(Panel.LOADING);

			if ( !m_questManager.EventExists() )
			{
				// Request the event
			}
			else
			{
				// Request the current quest state?
			}
			/*
			// Do we have an event?
			if(GlobalEventManager.currentEvent == null && GlobalEventManager.user.globalEvents.Count <= 0) {
				// No! Ask for live events again
				GlobalEventManager.TMP_RequestCustomizer();
				// Wait for events GLOBAL_EVENT_UPDATED GLOBAL_EVENT_CUSTOMIZER_ERROR or GLOBAL_EVENT_CUSTOMIZER_NO_EVENTS
			} else {
				// Yes! Refresh data
				GlobalEventManager.RequestCurrentEventData();
				// Wait for events GLOBAL_EVENT_UPDATED
			}
			*/
		}
		else
		{
			// Message no connection
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	
	}

	/// <summary>
	/// The Facebook button has been pressed.
	/// </summary>
	public void OnFacebookButton() {
		OpenUrlDelayed("https://www.facebook.com/HungryDragonGame");
	}

	/// <summary>
	/// The Twitter button has been pressed.
	/// </summary>
	public void OnTwitterButton() {
		OpenUrlDelayed("https://twitter.com/_HungryDragon");
	}

	/// <summary>
	/// The Instagram button has been pressed.
	/// </summary>
	public void OnInstagramButton() {
		OpenUrlDelayed("https://www.instagram.com/hungrydragongame");
	}

	/// <summary>
	/// The Web button has been pressed.
	/// </summary>
	public void OnWebButton() {
		OpenUrlDelayed("http://blog.ubi.com/");
	}

	/// <summary>
	/// Opens the URL after a short delay.
	/// </summary>
	/// <param name="_url">URL to be opened.</param>
	private void OpenUrlDelayed(string _url) {
		// Add some delay to give enough time for SFX to be played before losing focus
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				Application.OpenURL(_url);
			}, 0.15f
		);
	}
}