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
	public enum Panels {
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
	[SerializeField] private GameObject[] m_panels = new GameObject[(int)Panels.COUNT];
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

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
		Messenger.AddListener(GameEvents.GLOBAL_EVENT_DATA_UPDATED, OnEventDataUpdated);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GLOBAL_EVENT_DATA_UPDATED, OnEventDataUpdated);
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
	/// Request current event data to the server.
	/// </summary>
	public void RequestEventData() {
		GlobalEventManager.RequestCurrentEventData();
	}

	/// <summary>
	/// Request current event's state to the server.
	/// Leaderboard wont be updated to save data bandwith.
	/// </summary>
	public void RequestEventState() {
		GlobalEventManager.RequestCurrentEventState(false);
	}

	/// <summary>
	/// Select active panel based on current global event state.
	/// </summary>
	public void Refresh() {
		// Check events manager to see which panel to show
		GlobalEventManager.ErrorCode error = GlobalEventManager.CanContribute();
		Panels targetPanel = Panels.NO_EVENT;
		switch(error) {
			case GlobalEventManager.ErrorCode.NOT_INITIALIZED:
			case GlobalEventManager.ErrorCode.OFFLINE: {
				targetPanel = Panels.OFFLINE;
			} break;

			case GlobalEventManager.ErrorCode.NOT_LOGGED_IN: {
				targetPanel = Panels.LOG_IN;
			} break;

			case GlobalEventManager.ErrorCode.NO_VALID_EVENT: {
				targetPanel = Panels.NO_EVENT;
			} break;

			case GlobalEventManager.ErrorCode.NONE:
			case GlobalEventManager.ErrorCode.EVENT_NOT_ACTIVE: {
				// We have a valid event, select panel based on its state
				switch(GlobalEventManager.currentEvent.state) {
					case GlobalEvent.State.ACTIVE: {
						targetPanel = Panels.EVENT_ACTIVE;
					} break;

					case GlobalEvent.State.TEASING: {
						targetPanel = Panels.EVENT_TEASER;
					} break;

					default: {
						targetPanel = Panels.NO_EVENT;
					} break;
				}
			} break;

			default: {
				targetPanel = Panels.NO_EVENT;
			} break;
		}

		// Toggle active panel
		// [AOC] Use animators?
		for(int i = 0; i < m_panels.Length; ++i) {
			m_panels[i].SetActive(i == (int)targetPanel);
		}

		// Refresh target panel data (if required)
		switch(targetPanel) {
			case Panels.EVENT_ACTIVE: {
				//m_panels[(int)targetPanel].GetComponent<GlobalEventsActivePanel>().Refresh();
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a refresh every time we enter the tab!
	/// </summary>
	public void OnShowPreAnimation() {
		Refresh();
	}

	/// <summary>
	/// The global event manager has received new data from the server.
	/// </summary>
	private void OnEventDataUpdated() {
		Refresh();
	}
}