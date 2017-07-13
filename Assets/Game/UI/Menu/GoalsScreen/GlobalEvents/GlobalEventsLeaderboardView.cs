// GlobalEventsLeaderboardView.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class GlobalEventsLeaderboardView : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private ScrollRect m_scrollList = null;
	[Space]
	[SerializeField] private GameObject m_pillPrefab = null;
	[SerializeField] private GameObject m_playerPillPrefab = null;
	[Space]
	[SerializeField] private int m_maxPills = 100;

	// Internal
	private List<GlobalEventsLeaderboardPill> m_pills = null;
	private GlobalEventsLeaderboardPill m_playerPill = null;
	
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
		Messenger.AddListener(GameEvents.GLOBAL_EVENT_LEADERBOARD_UPDATED, OnLeaderboardUpdated);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GLOBAL_EVENT_LEADERBOARD_UPDATED, OnLeaderboardUpdated);
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
	/// Refresh leaderboard with current data.
	/// </summary>
	public void Refresh() {
		// Get current event
		GlobalEvent evt = GlobalEventManager.currentEvent;
		if(evt == null) return;

		// If pills list not yet initialized, do it now!
		if(m_pills == null) {
			m_pills = new List<GlobalEventsLeaderboardPill>(m_maxPills);
		}

		// Aux vars
		GlobalEventsLeaderboardPill pill;
		GameObject pillInstance;
		RectTransform pillTransform;
		Vector2 pillSize = (m_pillPrefab.transform as RectTransform).sizeDelta;	// Assuming normal and player pills have the same size!
		bool playerFound = false;
		GlobalEventUserData playerData = UsersManager.currentUser.GetGlobalEventData(evt.id);

		// Same with the player pill, which we'll have for sure
		if(m_playerPill == null) {
			pillInstance = GameObject.Instantiate<GameObject>(m_playerPillPrefab, m_scrollList.content, false);
			m_playerPill = pillInstance.GetComponent<GlobalEventsLeaderboardPill>();
			m_pills.Add(m_playerPill);
		}

		// Remove player pill from the list, we will insert it at the proper position when it matters
		m_pills.Remove(m_playerPill);

		// Iterate event leaderboard
		int numPills = evt.leaderboard.Count;
		for(int i = 0; i < evt.leaderboard.Count && i < m_maxPills; ++i) {
			// Super-special case: Is it the current player?
			if(!playerFound && evt.leaderboard[i].userID == playerData.userID) {
				// Use special pill
				pill = m_playerPill;

				// Insert the pill to the list
				// This shouldn't affect the loop since we are only pushing back the pills that are not yet processed
				m_pills.Insert(i, m_playerPill);

				// Do flag
				playerFound = true;
			} else {
				// Normal player
				if(i < m_pills.Count) {
					// Reuse existing pills
					pill = m_pills[i];
				} else {
					// Create a new pill!
					pillInstance = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
					pill = pillInstance.GetComponent<GlobalEventsLeaderboardPill>();
					m_pills.Add(pill);
				}
			}

			// We got a pill! Initialize it
			pill.InitWithData(evt.leaderboard[i]);
		}

		// If player pill wasn't added, do it now!
		if(!playerFound) {
			// Insert at the right position
			m_pills.Insert(evt.leaderboard.Count, m_playerPill);
			numPills++;

			// Intiialize!
			m_playerPill.InitWithData(playerData);
		}

		// Loop all the pills to put them into position and hide those not used
		float deltaY = pillSize.y/2f;	// Pill's anchor is at the middle!
		for(int i = 0; i < m_pills.Count; ++i) {
			// Active pill?
			pill = m_pills[i];
			if(i < numPills) {
				// Show pill
				pill.gameObject.SetActive(true);
				pill.name = "Pill_" + i;	// Debug purposes

				// Make sure anchor is properly set! (we want the pills to fill the content from the top)
				pillTransform = pill.transform as RectTransform;
				pillTransform.anchorMin = new Vector2(0.5f, 1f);
				pillTransform.anchorMax = new Vector2(0.5f, 1f);

				// Put into position
				pillTransform.SetLocalPosY(-deltaY);	// Going down!
				deltaY += pillTransform.sizeDelta.y;

				// [AOC] TODO!! Show intro anim?
			} else {
				// Hide pill
				pill.gameObject.SetActive(false);
				pill.name = "Pill_" + i + "_OFF";	// Debug purposes
			}
		}

		// Set content size
		m_scrollList.content.sizeDelta = new Vector2(m_scrollList.content.sizeDelta.x, deltaY - pillSize.y/2f);	// delta is pointing to where the next pill should be placed

		// Launch animation?
		m_scrollList.ScrollToPositionDelayedFrames(Vector2.zero, 1);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// We have new data for the current event!
	/// </summary>
	public void OnLeaderboardUpdated() {
		Refresh();
	}
}