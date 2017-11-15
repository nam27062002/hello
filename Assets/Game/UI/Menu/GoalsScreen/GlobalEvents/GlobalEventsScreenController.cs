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

		COUNT
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private GlobalEventsPanel[] m_panels = new GlobalEventsPanel[(int)Panel.COUNT];

	// Internal
	private Panel m_activePanel = Panel.OFFLINE;
	
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

		// Init panels
		for(int i = 0; i < m_panels.Length; i++) {
			m_panels[i].panelId = (Panel)i;
		}
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
		// Subscribe to external events
		Messenger.AddListener<GlobalEventManager.RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, OnEventDataUpdated);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<GlobalEventManager.RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, OnEventDataUpdated);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Select active panel based on current global event state.
	/// </summary>
	public void Refresh() {
		// Do we need to go to the rewards screen?
		if ( GlobalEventManager.currentEvent != null ){
			// By checking isRewardAvailable, we make sure the event is finished
			// By checking the reward level, we make sure that the rewards have been received from server!
			if (GlobalEventManager.currentEvent.isRewardAvailable && GlobalEventManager.currentEvent.rewardLevel > -1) {
				EventRewardScreen scr = InstanceManager.menuSceneController.GetScreen(MenuScreens.EVENT_REWARD).GetComponent<EventRewardScreen>();
				scr.StartFlow();
				InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.EVENT_REWARD);
				return;
			}
		}

		// Select active panel
		SelectPanel();

		// Refresh its content
		m_panels[(int)m_activePanel].Refresh();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Based on current event state, select which panel should be active.
	/// </summary>
	private void SelectPanel() {
		// Check events manager to see which panel to show
		GlobalEventManager.ErrorCode error = GlobalEventManager.CanContribute();
		Panel targetPanel = Panel.NO_EVENT;
		switch(error) {
			case GlobalEventManager.ErrorCode.NOT_INITIALIZED:
			case GlobalEventManager.ErrorCode.OFFLINE: {
				targetPanel = Panel.OFFLINE;
			} break;

			case GlobalEventManager.ErrorCode.NOT_LOGGED_IN: {
				targetPanel = Panel.LOG_IN;
			} break;

			case GlobalEventManager.ErrorCode.NO_VALID_EVENT: {
				targetPanel = Panel.NO_EVENT;
			} break;

			case GlobalEventManager.ErrorCode.NONE:
			case GlobalEventManager.ErrorCode.EVENT_NOT_ACTIVE: {
				// We have a valid event, select panel based on its state
				switch(GlobalEventManager.currentEvent.state) {
					case GlobalEvent.State.ACTIVE: {
						targetPanel = Panel.EVENT_ACTIVE;
					} break;

					case GlobalEvent.State.TEASING: {
						targetPanel = Panel.EVENT_TEASER;
					} break;

					default: {
						targetPanel = Panel.NO_EVENT;
					} break;
				}
			} break;

			default: {
				targetPanel = Panel.NO_EVENT;
			} break;
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

		// Get latest event data
		// [AOC] TODO!! Figure out the best place to do so to avoid spamming
		GlobalEventManager.RequestCurrentEventData();
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