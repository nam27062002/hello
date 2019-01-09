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
	[Comment(
		"0: Normal Player Pill\n" +
		"1: Current Player Pill\n" +
		"2: Reward Pill"
	)]
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
		m_tournament = HDLiveDataManager.tournament;
		if ( m_tournament.EventExists() )
		{
			m_tournament.RequestLeaderboard();
			m_waitingTournament = true;
		}
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
		// Get current tournament and init some aux vars
		m_tournament = HDLiveDataManager.tournament;
		HDTournamentData tournamentData = (HDTournamentData)m_tournament.data;
		HDTournamentDefinition tournamentDef = tournamentData.definition as HDTournamentDefinition;
		int playerRank = (int)tournamentData.m_rank;

		// Setup player pills
		TournamentLeaderboardPlayerPillData currentPlayerData = null;
		List<HDTournamentData.LeaderboardLine> lbData = tournamentData.m_leaderboard;
		List<ScrollRectItemData<TournamentLeaderboardPillBaseData>> items = new List<ScrollRectItemData<TournamentLeaderboardPillBaseData>>();
		for (int i = 0; i < lbData.Count; ++i) {
			TournamentLeaderboardPlayerPillData playerPillData = new TournamentLeaderboardPlayerPillData();
			playerPillData.leaderboardLine = lbData[i];

			ScrollRectItemData<TournamentLeaderboardPillBaseData> itemData = new ScrollRectItemData<TournamentLeaderboardPillBaseData>();
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

		// Insert reward pills
		// Reverse-iterate since we don't want to change the inserting indexes
		List<HDTournamentDefinition.TournamentReward> rewards = tournamentDef.m_rewards;	// They're already sorted by rank, lower to higher
		for(int i = rewards.Count - 1; i >= 0; --i) {
			// Exclude rewards without anyone yet in the ranks
			if(rewards[i].ranks.min >= lbData.Count) continue;
				
			TournamentLeaderboardRewardPillData rewardPillData = new TournamentLeaderboardRewardPillData();
			rewardPillData.reward = rewards[i];

			ScrollRectItemData<TournamentLeaderboardPillBaseData> itemData = new ScrollRectItemData<TournamentLeaderboardPillBaseData>();
			itemData.data = rewardPillData;
			itemData.pillType = 2;

			items.Insert(rewards[i].ranks.min, itemData);

			// Keep track of player pill index
			if(rewards[i].ranks.min <= playerRank) {	// Reward pill comes before player pill?
				playerPillIdx++;
			}
		}

		// Initialize the scroll list
		m_scrollList.Setup(m_pillPrefabs, items);

		// Initialize current player pill
		if(tournamentData.m_rank < 0) {
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