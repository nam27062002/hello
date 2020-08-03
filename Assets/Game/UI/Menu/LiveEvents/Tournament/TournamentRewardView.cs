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
	private HDLiveData.RankedReward m_rankedReward = null;
	public HDLiveData.RankedReward rankedReward {
		get { return m_rankedReward; }
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
	public void InitFromReward(HDLiveData.RankedReward _rankedReward) {
		// Store new reward
		m_rankedReward = _rankedReward;

		// If given reward is null, disable game object
		this.gameObject.SetActive(m_rankedReward != null);

		// Parent will do the rest
		if(m_rankedReward != null) {
			base.InitFromReward(m_rankedReward.reward);
		}
	}

	/// <summary>
	/// Refresh the visuals using current data.
	/// </summary>
	override public void Refresh() {
		if(m_rankedReward == null) return;
		if(m_reward == null) return;

		// Set target text
		if(m_rankText != null) {
			// [AOC] Mini-hack: use different TID for the first reward
			//		 Use it also when min and max range are the same
			if(m_rankedReward.ranks.min == 0
			|| m_rankedReward.ranks.min == m_rankedReward.ranks.max) {
				m_rankText.Localize(
					"TID_TOURNAMENT_REWARDS_RANK_TOP",
					StringUtils.FormatNumber(m_rankedReward.ranks.max + 1)
				);
			} else {
				m_rankText.Localize(
					"TID_TOURNAMENT_REWARDS_RANK",
					StringUtils.FormatNumber(m_rankedReward.ranks.min + 1),
					StringUtils.FormatNumber(m_rankedReward.ranks.max + 1)
				);
			}
		}

		// Parent will do the rest
		base.Refresh();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}