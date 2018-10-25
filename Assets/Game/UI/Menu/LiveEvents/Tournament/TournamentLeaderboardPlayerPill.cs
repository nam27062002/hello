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

		// Set position
		// We might not get a valid position if the player hasn't yet participated in the event
		if(data.leaderboardLine.m_rank >= 0) {
			m_positionText.text = StringUtils.FormatNumber(data.leaderboardLine.m_rank + 1);
		} else {
			m_positionText.text = "?";
		}

		// Apply special colors
		if(data.leaderboardLine.m_rank >= 0 && m_positionTextColors.Length > 0) {
			if(data.leaderboardLine.m_rank < m_positionTextColors.Length) {
				m_positionText.color = m_positionTextColors[data.leaderboardLine.m_rank];
			} else {
				m_positionText.color = m_positionTextColors.Last();
			}
			m_nameText.color = m_positionText.color;
		}

		// Get social info
		// Set name
		if(m_nameText != null) m_nameText.text = data.leaderboardLine.m_name;   // [AOC] Name text uses a dynamic font, so any special character should be properly displayed. On the other hand, instantiation time is increased for each pill containing non-cached characters.


        // Set score
        HDTournamentManager tournament = HDLiveEventsManager.instance.m_tournament;
        m_scoreText.text = tournament.FormatScore(data.leaderboardLine.m_score);

        if (tournament.IsTimeBasedScore()) {
            m_iconImage.sprite = m_iconClock; 
        } else {
            m_iconImage.sprite = m_iconScore;
        }
	}

	public override void Animate(int _index) {}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}