// GlobalEventsScreenActivePanel.cs
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
/// Panel corresponding to an active global event.
/// </summary>
public class GlobalEventsPanelActive : GlobalEventsPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private TextMeshProUGUI m_objectiveText = null;
	[SerializeField] private Image m_objectiveIcon = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[Space]
	[SerializeField] private Slider m_progressBar = null;
	[SerializeField] private GlobalEventsRewardInfo[] m_rewardInfos = new GlobalEventsRewardInfo[0];
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Just in case
		if(GlobalEventManager.currentEvent == null) return;

		// Update timer
		// [AOC] Could be done with less frequency
		m_timerText.text = TimeUtils.FormatTime(
			GlobalEventManager.currentEvent.remainingTime.TotalSeconds,
			TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
			4
		);

		// [AOC] TODO!! Manage event end when this panel is active
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh displayed data.
	/// </summary>
	override public void Refresh() {
		// Get current event
		GlobalEvent evt = GlobalEventManager.currentEvent;
		if(evt == null) return;

		// Initialize visuals
		// Event description
		m_objectiveText.text = evt.objective.GetDescription();

		// Target icon
		m_objectiveIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + evt.objective.goalDef.Get("icon"));

		// Rewards
		for(int i = 0; i < evt.rewards.Count; ++i) {
			// Break the loop if we don't have more reward info slots
			if(i >= m_rewardInfos.Length) break;

			// Initialize the reward info corresponding to this reward
			m_rewardInfos[i].InitFromReward(evt.rewards[i]);

			// Put into position (except last reward, which has a fixed position)
			if(i < evt.rewards.Count - 1) {
				// Set min and max anchor in Y to match the target percentage
				Vector2 anchor = m_rewardInfos[i].rectTransform.anchorMin;
				anchor.y = evt.rewards[i].targetPercentage;
				m_rewardInfos[i].rectTransform.anchorMin = anchor;

				anchor = m_rewardInfos[i].rectTransform.anchorMax;
				anchor.y = evt.rewards[i].targetPercentage;
				m_rewardInfos[i].rectTransform.anchorMax = anchor;
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}