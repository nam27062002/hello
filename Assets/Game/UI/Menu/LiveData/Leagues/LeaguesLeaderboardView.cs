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
public class LeaguesLeaderboardView : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private LeaguesScrollRect m_scrollList = null;
	public LeaguesScrollRect scrollList {
		get { return m_scrollList; }
	}

	[SerializeField] private GameObject m_loadingIcon = null;
	[SerializeField] private GameObject m_scrollGroup = null;
	[Space]
	[Comment(
		"0: Normal Player Pill\n" +
		"1: Current Player Pill\n"		
	)]
	[SerializeField] private List<GameObject> m_pillPrefabs;


    private HDSeasonData m_season;
    private HDLeagueData m_league;
    private bool m_waitingForLeaderboard;

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

        m_season = HDLiveDataManager.league.season;
        m_league = m_season.currentLeague;

        m_waitingForLeaderboard = true;
    }


	private void Update() {
		if (m_waitingForLeaderboard) {
			if (m_league.leaderboard.liveDataState == HDLiveData.State.VALID) {
				Refresh();
                m_waitingForLeaderboard = false;
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
        // Get current tournament and init some aux vars
        m_season = HDLiveDataManager.league.season;
        m_league = m_season.currentLeague;

		int playerRank = m_season.rank;

		// Setup player pills
		LeaguesLeaderboardPillData currentPlayerData = null;
		List<HDLiveData.Leaderboard.Record> lbData = m_league.leaderboard.records;
		List<ScrollRectItemData<LeaguesLeaderboardPillData>> items = new List<ScrollRectItemData<LeaguesLeaderboardPillData>>();
		for (int i = 0; i < lbData.Count; ++i) {
            LeaguesLeaderboardPillData playerPillData = new LeaguesLeaderboardPillData();
			playerPillData.record = lbData[i];
            playerPillData.reward = m_league.GetReward(0);

			ScrollRectItemData<LeaguesLeaderboardPillData> itemData = new ScrollRectItemData<LeaguesLeaderboardPillData>();
			itemData.data = playerPillData;

			// Is it current player? use different pill type and store data for further use
			if(i == playerRank) {
				itemData.pillType = 1;
				currentPlayerData = playerPillData;
			} else {
				itemData.pillType = 0;
			}

			items.Add(itemData);
		}

		// Keep track of player pill index
		int playerPillIdx = playerRank;


		// Initialize the scroll list
		m_scrollList.Setup(m_pillPrefabs, items);

		// Initialize current player pill
		if(playerRank < 0) {
			m_scrollList.SetupPlayerPill(null, -1, null);
		} else {
			m_scrollList.SetupPlayerPill(m_pillPrefabs[1], playerPillIdx, currentPlayerData);
			m_scrollList.FocusPlayerPill(false);
		}

		// Done!
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