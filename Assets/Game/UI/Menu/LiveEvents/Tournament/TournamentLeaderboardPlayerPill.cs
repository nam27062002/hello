// GlobalEventsLeaderboardPill.cs
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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Data class.
/// </summary>
public class TournamentLeaderboardPlayerPillData : TournamentLeaderboardPillBaseData {
	public HDTournamentData.LeaderboardLine leaderboardLine = null;
}

/// <summary>
/// Item class.
/// </summary>
public class TournamentLeaderboardPlayerPill : TournamentLeaderboardPillBase {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private TextMeshProUGUI m_positionText = null;
	[Tooltip("Special colors for top positions!")]
	[SerializeField] private Color[] m_positionTextColors = new Color[4];
	[SerializeField] private Text m_nameText = null;
	[SerializeField] private TextMeshProUGUI m_scoreText = null;
    [Space]
    [SerializeField] private Image m_iconImage = null;
    [SerializeField] private Sprite m_iconScore = null;
    [SerializeField] private Sprite m_iconClock = null;


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with the given user data.
	/// </summary>
	/// <param name="_data">The user to be displayed in the pill.</param>
	public override void InitWithData(TournamentLeaderboardPillBaseData _data) {
		// Cast data
		TournamentLeaderboardPlayerPillData data = _data as TournamentLeaderboardPlayerPillData;
		Debug.Assert(data != null, Color.red.Tag("UNKNOWN PILL DATA FORMAT!"));

		// Use internal initializer
		InitWithDataInternal(
			data,
			HDLiveDataManager.tournament.FormatScore(data.leaderboardLine.m_score),
			HDLiveDataManager.tournament.IsTimeBasedScore()
		);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_index"></param>
	public override void Animate(int _index) {}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with the given data, without any external dependency.
	/// </summary>
	/// <param name="_data">Leaderboard entry data.</param>
	/// <param name="_formattedScore">Formatted score.</param>
	/// <param name="_isTimedTournament">Is it a timed tournament?</param>
	protected virtual void InitWithDataInternal(TournamentLeaderboardPlayerPillData _data, string _formattedScore, bool _isTimedTournament) {
		// Set position
		// We might not get a valid position if the player hasn't yet participated in the event
		if(_data.leaderboardLine.m_rank >= 0) {
			m_positionText.text = StringUtils.FormatNumber(_data.leaderboardLine.m_rank + 1);
		} else {
			m_positionText.text = "?";
		}

		// Apply special colors
		if(_data.leaderboardLine.m_rank >= 0 && m_positionTextColors.Length > 0) {
			if(_data.leaderboardLine.m_rank < m_positionTextColors.Length) {
				m_positionText.color = m_positionTextColors[_data.leaderboardLine.m_rank];
			} else {
				m_positionText.color = m_positionTextColors.Last();
			}
			if(m_nameText != null) m_nameText.color = m_positionText.color;
		}

		// Get social info
		// Set name
		if(m_nameText != null) m_nameText.text = _data.leaderboardLine.m_name;   // [AOC] Name text uses a dynamic font, so any special character should be properly displayed. On the other hand, instantiation time is increased for each pill containing non-cached characters.

		// Set score
		if(m_scoreText != null) m_scoreText.text = _formattedScore;

		if(_isTimedTournament) {
			m_iconImage.sprite = m_iconClock;
		} else {
			m_iconImage.sprite = m_iconScore;
		}
	}

	//------------------------------------------------------------------------//
	// TEST METHODS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with given test data.
	/// </summary>
	/// <param name="_data">Data to be used.</param>
	/// <param name="_isTimedTournament">Simulate timed tournament?</param>
	public void TEST_InitWithData(TournamentLeaderboardPlayerPillData _data, bool _isTimedTournament) {
		// Different score format
		string formattedScore = "";
		if(_isTimedTournament) {
			formattedScore = TimeUtils.FormatTime((double)_data.leaderboardLine.m_score, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES, true);
		} else {
			formattedScore = StringUtils.FormatNumber(_data.leaderboardLine.m_score);
		}

		// Use internal initializer
		InitWithDataInternal(_data, formattedScore, _isTimedTournament);
	}
}