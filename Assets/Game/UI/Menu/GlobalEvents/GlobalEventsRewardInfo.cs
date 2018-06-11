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
public class GlobalEventsRewardInfo : MetagameRewardView {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Space]
	[SerializeField] private TextMeshProUGUI m_targetText = null;

	// Internal
	private HDQuestDefinition.QuestReward m_questReward = null;
	public HDQuestDefinition.QuestReward questReward {
		get { return m_questReward; }
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
	public void InitFromReward(HDQuestDefinition.QuestReward _questReward) {
		// Store new reward
		m_questReward = _questReward;

		// If given reward is null, disable game object
		this.gameObject.SetActive(m_questReward != null);

		// Parent will do the rest
		if(m_questReward != null) {
			base.InitFromReward(m_questReward.reward);
		}
	}

	/// <summary>
	/// Refresh the visuals using current data.
	/// </summary>
	override public void Refresh() {
		if(m_questReward == null) return;
		if(m_reward == null) return;

		// Set target text
		if(m_targetText != null) {
			// Abbreviated for big amounts
			m_targetText.text = StringUtils.FormatBigNumber(m_questReward.targetAmount);
		}

		// Parent will do the rest
		base.Refresh();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}