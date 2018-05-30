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
public class TournamentLeaderboardView : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Localizer m_titleText = null;
	[SerializeField] private ScrollRect m_scrollList = null;
	[SerializeField] private GameObject m_loadingIcon = null;
	[SerializeField] private GameObject m_scrollGroup = null;
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
	private HDTournamentManager m_tournament;
	private bool m_waitingTournament;

	private List<TournamentLeaderboardPill> m_pills = null;
	private TournamentLeaderboardPill m_playerPill = null;

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
		// Init title
		if(m_titleText != null) m_titleText.Localize(m_titleText.tid, StringUtils.FormatNumber(m_maxPills));
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Show loading widget
		ToggleLoading(true);

		// Request leaderboard!
		m_tournament = HDLiveEventsManager.instance.m_tournament;
		m_tournament.RequestLeaderboard();
		m_waitingTournament = true;
	}


	private void Update() {
		if (m_waitingTournament) {
			if (m_tournament.IsLeaderboardReady()) {
				Refresh();
				m_waitingTournament = false;
			}
		}
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
		HDTournamentData data = (HDTournamentData)m_tournament.data;
		List<HDTournamentData.LeaderboardLine> leaderboard = data.m_leaderboard;

		// If pills list not yet initialized, do it now!
		if (m_pills == null) {
			m_pills = new List<TournamentLeaderboardPill>(m_maxPills);
		}

		// Aux vars
		TournamentLeaderboardPill pill;
		GameObject pillInstance;
		RectTransform pillTransform;
		Vector2 pillSize = (m_pillPrefab.transform as RectTransform).sizeDelta;	// Assuming normal and player pills have the same size!
		bool playerFound = false;


		// Same with the player pill, which we'll have for sure
		if (m_playerPill == null) {
			// Player pill will be placed in the viewport, so it renders in top of everything else
			pillInstance = GameObject.Instantiate<GameObject>(m_playerPillPrefab, m_scrollList.viewport, false);
			m_playerPill = pillInstance.GetComponent<TournamentLeaderboardPill>();
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

			// Scroll to player's position on click
			m_playerPill.GetComponent<Button>().onClick.AddListener(OnPlayerPillClick);
		}

		// Remove player pill from the list, we will insert it at the proper position when it matters
		m_pills.Remove(m_playerPill);

		// Iterate event leaderboard
		int numPills = Mathf.Min(leaderboard.Count, m_maxPills);
		Debug.Log("<color=orange>We have " + leaderboard.Count + " entries on the leaderboard, creating " + numPills + " pills!</color>");

		for (int i = 0; i < numPills; ++i) {
			// Super-special case: Is it the current player?
			if (i == data.m_rank) {//data has the rank of the player
				// Initialize player pill data before the rest
				m_playerPill.InitWithData(leaderboard[i]);

				Debug.Log("<color=orange>Inserting player pill at " + i + "!</color>");
				// Use special pill
				pill = m_playerPill;

				// Insert the pill to the list
				// This shouldn't affect the loop since we are only pushing back the pills that are not yet processed
				m_pills.Insert(i, m_playerPill);

				// Do flag
				playerFound = true;
			} else {
				// Normal player
				if (i < m_pills.Count) {
					// Reuse existing pills
					pill = m_pills[i];
				} else {
					// Create a new pill!
					pillInstance = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
					pill = pillInstance.GetComponent<TournamentLeaderboardPill>();
					m_pills.Add(pill);

					// Make sure anchor is properly set!
					pillTransform = pill.transform as RectTransform;
					InitPillAnchors(ref pillTransform);
				}

				//Debug.Log("<color=red>Pill created at " + i + "!</color>");
				// We got a pill! Initialize it
				pill.InitWithData(leaderboard[i]);
			}
		}

		// If player pill wasn't added, do it now!
		if (!playerFound) {
			// Insert at the right position
			//Debug.Log("<color=orange>Player wasnt found! Inserting player pill at " + numPills + "!</color>");
			m_pills.Insert(numPills, m_playerPill);	
			numPills++;
		}

		// Loop all the pills to put them into position and hide those not used
		float deltaY = m_listPadding + pillSize.y/2f;	// Pill's anchor is at the middle!
		for (int i = 0; i < m_pills.Count; ++i) {
			// Active pill?
			pill = m_pills[i];
			if (i < numPills) {
				//Debug.Log("<color=blue>Pill " + i + "</color> <color=green>ON</color>");
				// Show pill
				pill.gameObject.SetActive(true);

				// If target pill is the player pill, use its placeholder slot instead
				if (pill == m_playerPill) {
					pill.name = "Pill_" + i + "_PLAYER";
					pillTransform = m_playerPillSlot;
				} else {
					pill.name = "Pill_" + i;	// Debug purposes
					pillTransform = pill.transform as RectTransform;
				}

				// Put into position
				pillTransform.SetLocalPosY(-deltaY);	// Going down!
				if (i < numPills - 1) {
					deltaY += pillTransform.sizeDelta.y + m_pillSpacing;
				}

				// Show intro anim
				Transform animAnchor = pillTransform.Find("Margins");	// [AOC] TODO!! Do this better :P
				if (animAnchor != null) {
					// Stop any previous animation
					animAnchor.DOKill(true);

					// Only first X pills
					if (i < 10) {
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

		// Launch animation
		m_scrollList.verticalNormalizedPosition = 1f;
		m_scrollList.GetComponent<ShowHideAnimator>().RestartShow();
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

	/// <summary>
	/// Toggle loading icon on/off.
	/// </summary>
	/// <param name="_toggle">Whether to toggle loading icon on or off.</param>
	private void ToggleLoading(bool _toggle) {
		m_loadingIcon.SetActive(_toggle);
		m_scrollGroup.SetActive(!_toggle);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// We have new data for the current event!
	/// </summary>
	public void OnLeaderboardUpdated() {
		// Hide loading widget
		ToggleLoading(false);

		// Refresh list
		Refresh();
	}

	/// <summary>
	/// The player's pill has been clicked.
	/// </summary>
	public void OnPlayerPillClick() {
		// Scroll to player's position
		// Compensate content size with viewport to properly compute delta corresponding to the target pill
		float viewportHeight = m_scrollList.viewport.rect.height;
		float contentHeight = m_scrollList.content.sizeDelta.y - viewportHeight;

		// Compute pill's delta
		float targetDeltaY = Mathf.Abs(m_playerPillSlot.anchoredPosition.y / contentHeight);

		// Center in viewport
		float viewportRelativeSize = viewportHeight / contentHeight;
		targetDeltaY -= viewportRelativeSize/2f;

		// Reverse delta (scroll list goes 1 to 0)
		targetDeltaY = 1f - targetDeltaY;

		// Stop inertia
		m_scrollList.velocity = Vector2.zero;

		// Launch anim!
		m_scrollList.DOVerticalNormalizedPos(targetDeltaY, 0.5f).SetEase(Ease.OutExpo);
	}
}