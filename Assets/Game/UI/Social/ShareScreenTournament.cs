// ShareScreenTournament.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

#define RARITY_GRADIENT

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Individual layout for a tournament share screen.
/// </summary>
public class ShareScreenTournament : IShareScreen {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[System.Serializable]
	public class RankSetup {
		public int startingRank = 0;
		public float offsetY = 0f;
		public int pillsBefore = 3;
		public int pillsAfter = 3;
	}

	protected delegate void PillInitializerFunction(ref TournamentLeaderboardPlayerPill _pill, TournamentLeaderboardPlayerPillData _data);

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private BaseIcon m_tournamentIcon = null;
	[SerializeField] private TextMeshProUGUI m_tournamentDescriptionText = null;
	[SerializeField] private Localizer m_remainingTimeText = null;
	[Space]
	[SerializeField] private RectTransform m_pillsContainer = null;
	[SerializeField] private GameObject m_pillPrefab = null;
	[SerializeField] private GameObject m_playerPillPrefab = null;
	[Space]
	[SerializeField] private RankSetup[] m_rankSetups = new RankSetup[0];

	// Pills cache
	private List<TournamentLeaderboardPlayerPill> m_normalPills = new List<TournamentLeaderboardPlayerPill>();
	private TournamentLeaderboardPlayerPill m_playerPill = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this screen with current quest data (from HDQuestManager).
	/// </summary>
	/// <param name="_shareLocationSku">Location where this screen is triggered.</param>
	/// <param name="_refCamera">Reference camera. Its properties will be copied to the scene's camera.</param>
	public void Init(string _shareLocationSku, Camera _refCamera) {
		// Set location and camera
		SetLocation(_shareLocationSku);
		SetRefCamera(_refCamera);

		// Aux vars
		HDTournamentManager tournament = HDLiveDataManager.tournament;

        // Initialize UI elements

        // Get the icon definition
        string iconSku = tournament.tournamentData.tournamentDef.m_goal.m_icon;

        // The BaseIcon component will load the proper image or 3d model according to iconDefinition.xml
        m_tournamentIcon.LoadIcon(iconSku);
        m_tournamentIcon.gameObject.SetActive(true);


		// Tournament description
		if(m_tournamentDescriptionText != null) {
			m_tournamentDescriptionText.text = tournament.GetDescription();
		}

		// Remaininig time
		if(m_remainingTimeText != null) {
			double remainingSeconds = tournament.tournamentData.tournamentDef.timeToEnd.TotalSeconds;
			m_remainingTimeText.gameObject.SetActive(remainingSeconds > 0);
			if(remainingSeconds > 0) {
				m_remainingTimeText.Localize(
					m_remainingTimeText.tid,
					TimeUtils.FormatTime(
						System.Math.Max(0, remainingSeconds), // Just in case, never go negative
						TimeUtils.EFormat.WORDS_WITHOUT_0_VALUES, 1
					)
				);
			}
		}

		// Pills
		if(m_pillsContainer != null && m_pillPrefab != null && m_playerPillPrefab != null) {
			// Choose which leaderboard lines to display
			int playerRank = (int)tournament.tournamentData.m_rank;
			RankSetup setup = GetRankSetup(playerRank);
			int startRank = Mathf.Max(playerRank - setup.pillsBefore, 0);    // Don't go negative
			int finalRank = Mathf.Min(playerRank + setup.pillsAfter, 99);    // Max 100, 0-indexed
			List<HDTournamentData.LeaderboardLine> tournamentLeaderboard = tournament.tournamentData.m_leaderboard;
			List<HDTournamentData.LeaderboardLine> lines = new List<HDTournamentData.LeaderboardLine>();
			for(int i = startRank; i < finalRank && i < tournamentLeaderboard.Count; ++i) {
				lines.Add(tournamentLeaderboard[i]);
			}

			// Init pills for the selected set of lines
			InitPills(
				lines,
				playerRank,
				(ref TournamentLeaderboardPlayerPill _pill, TournamentLeaderboardPlayerPillData _data) => {
					_pill.InitWithData(_data);
				}
			);
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize pills with given data.
	/// </summary>
	/// <param name="_lines">Leaderboard entries to be displayed.</param>
	/// <param name="_playerRank">Rank of the player, to know which line corresponds to the player.</param>
	private void InitPills(List<HDTournamentData.LeaderboardLine> _lines, int _playerRank, PillInitializerFunction _pillInitializer) {
		// Hide all pills
		if(m_playerPill != null) m_playerPill.gameObject.SetActive(false);
		for(int i = 0; i < m_normalPills.Count; ++i) {
			m_normalPills[i].gameObject.SetActive(false);
		}

		// Sort list by rank
		_lines.Sort(CompareLines);

		// For each line, instantiate the right pill
		TournamentLeaderboardPlayerPill pill = null;
		TournamentLeaderboardPlayerPillData data = new TournamentLeaderboardPlayerPillData();
		int normalPillIdx = 0;
		for(int i = 0; i < _lines.Count; ++i) {
			// Is it the player's pill?
			if(_lines[i].m_rank == _playerRank) {
				// Yes! Need to instantiate?
				if(m_playerPill == null) {
					m_playerPill = Instantiate<GameObject>(m_playerPillPrefab, m_pillsContainer).GetComponent<TournamentLeaderboardPlayerPill>();
				}
				pill = m_playerPill;
			} else {
				// No! Normal pill: reuse or instantiate?
				if(normalPillIdx >= m_normalPills.Count) {
					m_normalPills.Add(
						Instantiate<GameObject>(m_pillPrefab, m_pillsContainer).GetComponent<TournamentLeaderboardPlayerPill>()
					);
				}
				pill = m_normalPills[normalPillIdx];
				normalPillIdx++;
			}

			// Initialize pill
			if(pill != null) {
				data.leaderboardLine = _lines[i];
				_pillInitializer.Invoke(ref pill, data);
				pill.gameObject.SetActive(true);
				pill.transform.SetAsLastSibling();	// Keep pills sorted
			}
		}

		// Apply offset to the container based on player's rank
		RankSetup setup = GetRankSetup(_playerRank);
		m_pillsContainer.anchoredPosition = new Vector2(
			m_pillsContainer.anchoredPosition.x, 
			setup == null ? 0 : setup.offsetY
		);
	}

	/// <summary>
	/// Get the setup corresponding to a given rank.
	/// </summary>
	/// <returns>The setup corresponding to rank <paramref name="_rank"/>.</returns>
	/// <param name="_rank">Target rank.</param>
	private RankSetup GetRankSetup(int _rank) {
		// Assuming setups are sorted
		RankSetup setup = null;
		for(int i = 0; i < m_rankSetups.Length; ++i) {
			if(m_rankSetups[i].startingRank > _rank) {
				break;
			}
			setup = m_rankSetups[i];
		}
		return setup;
	}

	/// <summary>
	/// Compare two leaderboard lines. Used for sorting.
	/// </summary>
	/// <returns>Comparison result.</returns>
	/// <param name="_l1">First line to compare.</param>
	/// <param name="_l2">Second line to compare.</param>
	private static int CompareLines(HDTournamentData.LeaderboardLine _l1, HDTournamentData.LeaderboardLine _l2) {
		return _l1.m_rank.CompareTo(_l2.m_rank);
	}

	//------------------------------------------------------------------------//
	// TEST METHODS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Clear all testing stuff.
	/// </summary>
	public void TEST_Clear() {
		// Delete all instances from the pills container
		m_pillsContainer.transform.DestroyAllChildren(!Application.isPlaying);

		// Clear lists
		m_normalPills.Clear();
		m_playerPill = null;
	}

	/// <summary>
	/// Initialize the leaderboard at a given position.
	/// </summary>
	/// <param name="_rank">Rank.</param>
	/// <param name="_score">Player score, will be used as reference for other players score range.</param>
	/// <param name="_isTimed">Timed score?</param>
	public void TEST_InitAtRank(int _rank, int _score, bool _isTimed) {
		// Clear previous tests
		TEST_Clear();

		// Figure out how many pills we need to create and their ranks
		RankSetup setup = GetRankSetup(_rank);
		int startRank = Mathf.Max(_rank - setup.pillsBefore, 0);	// Don't go negative
		int finalRank = Mathf.Min(_rank + setup.pillsAfter, 99);	// Max 100, 0-indexed

		// Create fake tournament datas
		Range scoreRange = new Range(_score * 0.9f, _score * 1.1f);
		List<HDTournamentData.LeaderboardLine> entries = new List<HDTournamentData.LeaderboardLine>();
		for(int i = startRank; i <= finalRank; ++i) {
			// Create new entry
			HDTournamentData.LeaderboardLine newEntry = new HDTournamentData.LeaderboardLine();
			newEntry.m_rank = i;
			newEntry.m_name = LeaderboardGenerator.GenerateRandomName();

			// Score
			float delta = Mathf.InverseLerp(startRank, finalRank, i);
			if(!_isTimed) delta = 1 - delta;  // The higher the score, the better!
			newEntry.m_score = (int)scoreRange.Lerp(delta);

			// Add to collection
			entries.Add(newEntry);
		}

		// Initialize pills with the fake data
		InitPills(
			entries, 
			_rank, 
			(ref TournamentLeaderboardPlayerPill _pill, TournamentLeaderboardPlayerPillData _data) => {
				_pill.TEST_InitWithData(_data, _isTimed);
			}
		);
	}
}