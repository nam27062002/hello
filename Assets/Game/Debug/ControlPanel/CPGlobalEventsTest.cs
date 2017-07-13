// CPGlobalEventsTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global class to manage global events testing features.
/// </summary>
public class CPGlobalEventsTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const bool OFFLINE_BY_DEFAULT = true;

	public const string TEST_ENABLED = "EVENTS_TEST_ENABLED";
	public static bool testEnabled {
		get { return Prefs.GetBoolPlayer(TEST_ENABLED, OFFLINE_BY_DEFAULT); }
		set { Prefs.SetBoolPlayer(TEST_ENABLED, value); }
	}

	public const string NETWORK_CHECK = "EVENTS_TEST_NETWORK_CHECK";
	public static bool networkCheck {
		get { 
			if(!testEnabled) return true;
			return Prefs.GetBoolPlayer(NETWORK_CHECK, !OFFLINE_BY_DEFAULT); 
		}
		set { Prefs.SetBoolPlayer(NETWORK_CHECK, value); }
	}

	public const string LOGIN_CHECK = "EVENTS_TEST_LOGIN_CHECK";
	public static bool loginCheck {
		get { 
			if(!testEnabled) return true;
			return Prefs.GetBoolPlayer(LOGIN_CHECK, !OFFLINE_BY_DEFAULT); 
		}
		set { Prefs.SetBoolPlayer(LOGIN_CHECK, value); }
	}

	public enum EventStateTest {
		DEFAULT = 0,
		NO_EVENT,
		TEASING,
		ACTIVE,
		FINISHED
	};
		
	public const string EVENT_STATE = "EVENTS_TEST_STATE";
	public static EventStateTest eventState {
		get {
			if(!testEnabled) return EventStateTest.DEFAULT;
			return (EventStateTest)Prefs.GetIntPlayer(EVENT_STATE, (int)EventStateTest.DEFAULT); 
		}
		set { Prefs.SetIntPlayer(EVENT_STATE, (int)value); }
	}

	private static TMP_Text sm_eventCode = null;
	public static int eventCode {
		get {
			if (sm_eventCode == null) {
				return 0;
			}

			return int.Parse(sm_eventCode.text);
		}
	}
	//------------------------------------------------------------------------//
	// EXPOSED MEMBERS														  //
	//------------------------------------------------------------------------//
	// Exposed stuff
	[SerializeField] private Toggle m_testEnabledToggle = null;
	[SerializeField] private CanvasGroup m_canvasGroup = null;
	[SerializeField] private GameObject m_disabledMessageObj = null;
	[Space]
	[SerializeField] private Toggle m_networkCheckToggle = null;
	[SerializeField] private Toggle m_loginCheckToggle = null;
	[SerializeField] private TMP_Text m_eventCode = null;
	[SerializeField] private CPEnumPref m_eventStateDropdown = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to changed events
		m_testEnabledToggle.onValueChanged.AddListener(
			(bool _toggled) => {
				testEnabled = _toggled;
				m_canvasGroup.interactable = testEnabled;
			}
		);

		m_networkCheckToggle.onValueChanged.AddListener(_toggled => networkCheck = _toggled);
		m_loginCheckToggle.onValueChanged.AddListener(_toggled => loginCheck = _toggled);

		m_eventStateDropdown.InitFromEnum(EVENT_STATE, typeof(EventStateTest), (int)EventStateTest.DEFAULT);
		m_eventStateDropdown.OnPrefChanged.AddListener(
			(int _newValueIdx) => {
				// Clear current event
				GlobalEventManager.ClearCurrentEvent();

				// Request event data
				GlobalEventManager.RequestCurrentEventData();
			}
		);

		sm_eventCode = m_eventCode;
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure all values ar updated
		Refresh();
	}

	/// <summary>
	/// Make sure all fields have the right values.
	/// </summary>
	private void Refresh() {
		// Cover everything with an error message if the debug server is not toggled
		m_disabledMessageObj.SetActive(!DebugSettings.useDebugServer);

		// Set current values
		m_testEnabledToggle.isOn = testEnabled;

		m_networkCheckToggle.isOn = networkCheck;
		m_loginCheckToggle.isOn = loginCheck;
		m_eventStateDropdown.Refresh();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Forces a current event request.
	/// </summary>
	public void OnRequestCurrentEventData() {
		// Manager does it all
		GlobalEventManager.RequestCurrentEventData();
	}

	/// <summary>
	/// Forces a current event state request to the server.
	/// </summary>
	/// <param name="_leaderboard">Whether to include the leaderboard or not.</param>
	public void OnRequestCurrentEventState(bool _leaderboard) {
		// Manager does it all
		GlobalEventManager.RequestCurrentEventState();

		// [AOC] TODO!! Separate into different buttons
		if(_leaderboard) {
			GlobalEventManager.RequestCurrentEventLeaderboard();
		}
	}
}