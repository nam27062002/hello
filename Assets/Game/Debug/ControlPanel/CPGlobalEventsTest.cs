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

	public enum LeaderboardSize {
		DEFAULT,
		SIZE_0,
		SIZE_5,
		SIZE_10,
		SIZE_25,
		SIZE_50,
		SIZE_75,
		SIZE_100
	};

	public const string LEADERBOARD_SIZE = "EVENTS_TEST_LEADERBOARD_SIZE";
	public static LeaderboardSize leaderboardSize {
		get {
			if(!testEnabled) return LeaderboardSize.DEFAULT;
			return (LeaderboardSize)Prefs.GetIntPlayer(LEADERBOARD_SIZE, (int)LeaderboardSize.DEFAULT); 
		}
		set { Prefs.SetIntPlayer(LEADERBOARD_SIZE, (int)value); }
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
	[Space]
	[SerializeField] private TMP_InputField m_playerScoreText = null;
	[SerializeField] private CPEnumPref m_leaderboardSizeDropdown = null;

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

		m_leaderboardSizeDropdown.InitFromEnum(LEADERBOARD_SIZE, typeof(LeaderboardSize), (int)LeaderboardSize.DEFAULT);
		m_leaderboardSizeDropdown.OnPrefChanged.AddListener(
			(int _newValueIdx) => {
				// Clear current event leaderboard
				if(GlobalEventManager.currentEvent != null) {
					GlobalEventManager.currentEvent.leaderboard.Clear();

					// Clear offline server leaderboard cache
					(GameServerManager.SharedInstance as GameServerManagerOffline).ClearEventLeaderboard(GlobalEventManager.currentEvent.id);
				}

				// Request event data
				GlobalEventManager.RequestCurrentEventLeaderboard();
			}
		);
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

	/// <summary>
	/// Sets the player score to the current event, if any.
	/// </summary>
	/// <param name="_score">Score.</param>
	private void SetPlayerScore(int _score) {
		if(GlobalEventManager.currentEvent == null) return;

		// Clamp value
		if(_score < 0) _score = -1;	// Negative score means the user has never participated to that event

		// Get player contribution to current event
		GlobalEventUserData playerData = UsersManager.currentUser.GetGlobalEventData(GlobalEventManager.currentEvent.id);
		int scoreToAdd = _score;
		if ( playerData.score > 0 )
			scoreToAdd -= playerData.score;
		// int scoreToAdd = _score - Math.Max(playerData.score, 0f);

		// Add to event's total contribution!
		GlobalEventManager.currentEvent.AddContribution(scoreToAdd);

		// Add to current user individual contribution in this event
		playerData.score += scoreToAdd;

		// Check if player can join the leaderboard! (or update its position)
		GlobalEventManager.currentEvent.RefreshLeaderboardPosition(playerData);

		// Notify game
		Messenger.Broadcast<bool>(GameEvents.GLOBAL_EVENT_SCORE_REGISTERED, true);
		Messenger.Broadcast<GlobalEventManager.RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, GlobalEventManager.RequestType.EVENT_LEADERBOARD);
		Messenger.Broadcast(GameEvents.GLOBAL_EVENT_LEADERBOARD_UPDATED);
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

	/// <summary>
	/// Add event score to current player.
	/// </summary>
	public void OnAddScore() {
		int amount = int.Parse(m_playerScoreText.text);
		GlobalEventUserData playerData = UsersManager.currentUser.GetGlobalEventData(GlobalEventManager.currentEvent.id);
		if(playerData.score > 0) {
			amount = playerData.score + amount;
		}
		SetPlayerScore(amount);
	}

	/// <summary>
	/// Remove event score to current player.
	/// </summary>
	public void OnRemoveScore() {
		int amount = int.Parse(m_playerScoreText.text);
		GlobalEventUserData playerData = UsersManager.currentUser.GetGlobalEventData(GlobalEventManager.currentEvent.id);
		amount = playerData.score - amount;
		if(amount < 0) amount = 0;
		SetPlayerScore(amount);
	}

	/// <summary>
	/// Set event score for current player.
	/// </summary>
	public void OnSetScore() {
		int amount = int.Parse(m_playerScoreText.text);
		SetPlayerScore(amount);
	}
}