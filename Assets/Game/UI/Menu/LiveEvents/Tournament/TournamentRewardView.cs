// GlobalEventsScreenRewardInfo.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2017.
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
/// Widget to display the info of a global event reward.
/// </summary>
public class TournamentRewardView : MetagameRewardView {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Space]
	[SerializeField] private Localizer m_rankText = null;

	// Internal
	private HDTournamentDefinition.TournamentReward m_tournamentReward = null;
	public HDTournamentDefinition.TournamentReward tournamentReward {
		get { return m_tournamentReward; }
		set { InitFromReward(value); }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the widget with the data of a specific reward.
	/// </summary>
	public void InitFromReward(HDTournamentDefinition.TournamentReward _tournamentReward) {
		// Store new reward
		m_tournamentReward = _tournamentReward;

		// If given reward is null, disable game object
		this.gameObject.SetActive(m_tournamentReward != null);

		// Parent will do the rest
		if(m_tournamentReward != null) {
			base.InitFromReward(m_tournamentReward.reward);
		}
	}

	/// <summary>
	/// Refresh the visuals using current data.
	/// </summary>
	override public void Refresh() {
		if(m_tournamentReward == null) return;
		if(m_reward == null) return;

		// Set target text
		if(m_rankText != null) {
			m_rankText.Localize(
				"TID_TOURNAMENT_REWARDS_RANK",
				StringUtils.FormatNumber(m_tournamentReward.ranks.min + 1),
				StringUtils.FormatNumber(m_tournamentReward.ranks.max + 1)
			);
		}

		// Parent will do the rest
		base.Refresh();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}