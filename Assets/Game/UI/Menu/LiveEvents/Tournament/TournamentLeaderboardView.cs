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
	[SerializeField] private TournamentScrollRect m_scrollList = null;
	[SerializeField] private GameObject m_loadingIcon = null;
	[SerializeField] private GameObject m_scrollGroup = null;
	[Space]
	[SerializeField] private List<GameObject> m_pillPrefabs;

	// Internal
	private HDTournamentManager m_tournament;
	private bool m_waitingTournament;

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
		if(m_titleText != null) m_titleText.Localize(m_titleText.tid);
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
		m_tournament = HDLiveEventsManager.instance.m_tournament;
		HDTournamentData data = (HDTournamentData)m_tournament.data;

		List<HDTournamentData.LeaderboardLine> lbData = data.m_leaderboard;


		List<ScrollRectItemData<HDTournamentData.LeaderboardLine>> leaderboard = new List<ScrollRectItemData<HDTournamentData.LeaderboardLine>>();
		for (int i = 0; i < lbData.Count; ++i) {
			ScrollRectItemData<HDTournamentData.LeaderboardLine> itemData = new ScrollRectItemData<HDTournamentData.LeaderboardLine>();
			itemData.data = lbData[i];
			itemData.pillType = (i == data.m_rank)? 1 : 0;
			leaderboard.Add(itemData);
		}

		m_scrollList.SetupPlayerPill(m_pillPrefabs[1], (int)data.m_rank, leaderboard[(int)data.m_rank].data);
		m_scrollList.Setup(m_pillPrefabs, leaderboard);

		ToggleLoading(false);
	}

	/// <summary>
	/// Keeps player pill within scroll list viewport, snapping to the nearest edge 
	/// to its actual position.
	/// </summary>
	private void RefreshPlayerPillPosition() {
	/*	// Must be initialized!
		if(m_playerPill == null || m_playerPillSlot == null) return;

		// Black magic math to snap the pill in the viewport
		Rect viewportRect = m_scrollList.viewport.rect;
		Bounds slotBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_scrollList.viewport, m_playerPillSlot);
		Range yRange = new Range(
			viewportRect.y + m_listPadding + slotBounds.extents.y + m_playerPillMarginOffset,
			viewportRect.y + viewportRect.height - (slotBounds.extents.y + m_listPadding) - m_playerPillMarginOffset
		);
		float newY = yRange.Clamp(slotBounds.center.y);
		m_playerPill.transform.SetLocalPosY(newY);*/
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