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
/// 
/// </summary>
public class TournamentLeaderboardPill : ScrollRectItem<HDTournamentData.LeaderboardLine> {
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


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with the given user data.
	/// </summary>
	/// <param name="_data">The user to be displayed in the pill.</param>
	public override void InitWithData(HDTournamentData.LeaderboardLine _data) {
		// Set position
		// We might not get a valid position if the player hasn't yet participated in the event
		if(_data.m_rank >= 0) {
			m_positionText.text = StringUtils.FormatNumber(_data.m_rank + 1);
		} else {
			m_positionText.text = "?";
		}

		// Apply special colors
		if(_data.m_rank >= 0 && m_positionTextColors.Length > 0) {
			if(_data.m_rank < m_positionTextColors.Length) {
				m_positionText.color = m_positionTextColors[_data.m_rank];
			} else {
				m_positionText.color = m_positionTextColors.Last();
			}
			m_nameText.color = m_positionText.color;
		}

		// Get social info
		// Set name
		if(m_nameText != null) m_nameText.text = _data.m_name;	// [AOC] Name text uses a dynamic font, so any special character should be properly displayed. On the other hand, instantiation time is increased for each pill containing non-cached characters.

		// Set score
		m_scoreText.text = StringUtils.FormatBigNumber(_data.m_score);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}