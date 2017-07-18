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
using DG.Tweening;
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
	[Space]
	[SerializeField] private float m_listPadding = 0f;
	[SerializeField] private float m_pillSpacing = 5f;
	[SerializeField] private float m_playerPillMarginOffset = -15f;	// [AOC] Some extra margin to make up for the pill's transparency

	// Internal
	private List<GlobalEventsLeaderboardPill> m_pills = null;
	private GlobalEventsLeaderboardPill m_playerPill = null;

	// Snap player pill to scrollList viewport
	private RectTransform m_playerPillSlot = null;
	private Bounds m_playerPillDesiredBounds;	// Original rect where the player pill should be (scrollList's content local coords)
	
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
	private void LateUpdate() {
		// Keep player pill within the viewport
		RefreshPlayerPillPosition();
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
			// Player pill will be placed in the viewport, so it renders in top of everything else
			pillInstance = GameObject.Instantiate<GameObject>(m_playerPillPrefab, m_scrollList.viewport, false);
			m_playerPill = pillInstance.GetComponent<GlobalEventsLeaderboardPill>();
			pillTransform = m_playerPill.transform as RectTransform;
			InitPillAnchors(ref pillTransform);
			m_pills.Add(m_playerPill);

			// Create empty slot rectangle as well, which will be used for the snapping logic
			GameObject slotGo = new GameObject("PlayerPillSlot");
			m_playerPillSlot = slotGo.AddComponent<RectTransform>();
			m_playerPillSlot.SetParent(m_scrollList.content, false);

			// Set size and anchoring properties
			m_playerPillSlot.sizeDelta = pillTransform.sizeDelta;
			InitPillAnchors(ref m_playerPillSlot);
		}

		// Remove player pill from the list, we will insert it at the proper position when it matters
		m_pills.Remove(m_playerPill);

		// Iterate event leaderboard
		int numPills = Mathf.Min(evt.leaderboard.Count, m_maxPills);
		Debug.Log("<color=orange>We have " + evt.leaderboard.Count + " entries on the leaderboard, creating " + numPills + " pills!</color>");
		for(int i = 0; i < numPills; ++i) {
			// Super-special case: Is it the current player?
			if(!playerFound && evt.leaderboard[i].userID == playerData.userID) {
				//Debug.Log("<color=orange>Inserting player pill at " + i + "!</color>");
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

					// Make sure anchor is properly set!
					pillTransform = pill.transform as RectTransform;
					InitPillAnchors(ref pillTransform);
				}

				//Debug.Log("<color=red>Pill created at " + i + "!</color>");
			}

			// We got a pill! Initialize it
			pill.InitWithData(evt.leaderboard[i]);
		}

		// If player pill wasn't added, do it now!
		if(!playerFound) {
			//Debug.Log("<color=orange>Player wasnt found! Inserting player pill at " + numPills + "!</color>");

			// Insert at the right position
			m_pills.Insert(numPills, m_playerPill);
			numPills++;

			// Intiialize!
			m_playerPill.InitWithData(playerData);
		}

		// Loop all the pills to put them into position and hide those not used
		float deltaY = m_listPadding + pillSize.y/2f;	// Pill's anchor is at the middle!
		for(int i = 0; i < m_pills.Count; ++i) {
			// Active pill?
			pill = m_pills[i];
			if(i < numPills) {
				//Debug.Log("<color=blue>Pill " + i + "</color> <color=green>ON</color>");
				// Show pill
				pill.gameObject.SetActive(true);

				// If target pill is the player pill, use its placeholder slot instead
				if(pill == m_playerPill) {
					pill.name = "Pill_" + i + "_PLAYER";
					pillTransform = m_playerPillSlot;
				} else {
					pill.name = "Pill_" + i;	// Debug purposes
					pillTransform = pill.transform as RectTransform;
				}

				// Put into position
				pillTransform.SetLocalPosY(-deltaY);	// Going down!
				if(i < numPills - 1) {
					deltaY += pillTransform.sizeDelta.y + m_pillSpacing;
				}

				// Show intro anim
				Transform animAnchor = pillTransform.FindChild("Margins");	// [AOC] TODO!! Do this better :P
				if(animAnchor != null) {
					// Stop any previous animation
					animAnchor.DOKill(true);

					// Only first X pills
					if(i < 10) {
						animAnchor.DOScale(0f, 0.25f)
							.From()
							.SetDelay(i * 0.1f)
							.SetEase(Ease.OutBack);
					}
				}
			} else {
				// Hide pill
				//Debug.Log("<color=blue>Pill " + i + "</color> <color=red>OFF</color>");
				pill.gameObject.SetActive(false);
				pill.name = "Pill_" + i + "_OFF";	// Debug purposes
			}
		}

		// Make sure player's pill is always rendered on top
		m_playerPill.transform.SetAsLastSibling();

		// Set content size
		m_scrollList.content.sizeDelta = new Vector2(m_scrollList.content.sizeDelta.x, deltaY + pillSize.y/2f + m_listPadding);	// delta is pointing to where the next pill should be placed

		// Launch animation?
		m_scrollList.verticalNormalizedPosition = 1f;
		m_scrollList.ScrollToPositionDelayedFrames(m_scrollList.normalizedPosition, 1);
	}

	/// <summary>
	/// Keeps player pill within scroll list viewport, snapping to the nearest edge 
	/// to its actual position.
	/// </summary>
	private void RefreshPlayerPillPosition() {
		// Must be initialized!
		if(m_playerPill == null || m_playerPillSlot == null) return;

		// Black magic math to snap the pill in the viewport
		Rect viewportRect = m_scrollList.viewport.rect;
		Bounds slotBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_scrollList.viewport, m_playerPillSlot);
		Range yRange = new Range(
			viewportRect.y + m_listPadding + slotBounds.extents.y + m_playerPillMarginOffset,
			viewportRect.y + viewportRect.height - (slotBounds.extents.y + m_listPadding) - m_playerPillMarginOffset
		);
		float newY = yRange.Clamp(slotBounds.center.y);
		m_playerPill.transform.SetLocalPosY(newY);
	}

	/// <summary>
	/// Initializes a rect transform to fit the scrolllist content.
	/// </summary>
	/// <param name="_rt">Rect transform to be initialized.</param>
	private void InitPillAnchors(ref RectTransform _rt) {
		_rt.anchorMin = new Vector2(0f, 1f);
		_rt.anchorMax = new Vector2(1f, 1f);
		_rt.offsetMin = new Vector2(0f, _rt.offsetMin.y);
		_rt.offsetMax = new Vector2(0f, _rt.offsetMax.y);
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