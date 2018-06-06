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
public class TournamentLeaderboardView : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private TournamentScrollRect m_scrollList = null;
	public TournamentScrollRect scrollList {
		get { return m_scrollList; }
	}

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

		m_scrollList.FocusPlayerPill();

		ToggleLoading(false);
	}


	/// <summary>
	/// Toggle loading icon on/off.
	/// </summary>
	/// <param name="_toggle">Whether to toggle loading icon on or off.</param>
	private void ToggleLoading(bool _toggle) {
		m_loadingIcon.SetActive(_toggle);
		m_scrollGroup.SetActive(!_toggle);
	}
}