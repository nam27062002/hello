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

		if ( GlobalEventManager.currentEvent != null ){
			if (GlobalEventManager.currentEvent.isRewardAvailable){
				EventRewardScreen scr = InstanceManager.menuSceneController.GetScreen(MenuScreens.REWARD).GetComponent<EventRewardScreen>();
				scr.StartFlow();
				InstanceManager.menuSceneController.screensController.GoToScreen((int)MenuScreens.REWARD);
				return;
			}
		}

		SelectPanel();

		// Refresh active panel
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
		m_activePanel = Panel.NO_EVENT;
		switch(error) {
			case GlobalEventManager.ErrorCode.NOT_INITIALIZED:
			case GlobalEventManager.ErrorCode.OFFLINE: {
				m_activePanel = Panel.OFFLINE;
			} break;

			case GlobalEventManager.ErrorCode.NOT_LOGGED_IN: {
				m_activePanel = Panel.LOG_IN;
			} break;

			case GlobalEventManager.ErrorCode.NO_VALID_EVENT: {
				m_activePanel = Panel.NO_EVENT;
			} break;

			case GlobalEventManager.ErrorCode.NONE:
			case GlobalEventManager.ErrorCode.EVENT_NOT_ACTIVE: {
				// We have a valid event, select panel based on its state
				switch(GlobalEventManager.currentEvent.state) {
					case GlobalEvent.State.ACTIVE: {
						m_activePanel = Panel.EVENT_ACTIVE;
					} break;

					case GlobalEvent.State.TEASING: {
						m_activePanel = Panel.EVENT_TEASER;
					} break;

					default: {
						m_activePanel = Panel.NO_EVENT;
					} break;
				}
			} break;

			default: {
				m_activePanel = Panel.NO_EVENT;
			} break;
		}

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
		// Get latest event data
		// [AOC] TODO!! Figure out the best place to do so to avoid spamming
		GlobalEventManager.RequestCurrentEventData();
		BusyScreen.Show(this);
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
					BusyScreen.Hide(this);
				} else {
					SelectPanel();
				}
			} break;

			case GlobalEventManager.RequestType.EVENT_STATE: {
				Refresh();
				BusyScreen.Hide(this);
			} break;
		}
	}

	/// <summary>
	/// The Facebook button has been pressed.
	/// </summary>
	public void OnFacebookButton() {
		Application.OpenURL("https://www.facebook.com/HungryDragonGame");
	}

	/// <summary>
	/// The Twitter button has been pressed.
	/// </summary>
	public void OnTwitterButton() {
		Application.OpenURL("https://twitter.com/_HungryDragon");
	}

	/// <summary>
	/// The Instagram button has been pressed.
	/// </summary>
	public void OnInstagramButton() {
		Application.OpenURL("https://www.instagram.com/hungrydragongame");
	}

	/// <summary>
	/// The Web button has been pressed.
	/// </summary>
	public void OnWebButton() {
		Application.OpenURL("http://blog.ubi.com/");
	}
}