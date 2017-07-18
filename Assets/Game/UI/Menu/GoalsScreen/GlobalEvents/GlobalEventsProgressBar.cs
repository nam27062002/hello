// GlobalEventsProgressBar.cs
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
public class GlobalEventsProgressBar : MonoBehaviour {	
	[SerializeField] private TextMeshProUGUI m_currentValueText_DEBUG = null;
	[Space]
	[SerializeField] private Slider m_progressBar = null;
	[SerializeField] private GlobalEventsRewardInfo[] m_rewardInfos = new GlobalEventsRewardInfo[0];

	/// <summary>
	/// Refresh displayed data.
	/// </summary>
	public void RefreshRewards(GlobalEvent _evt) {
		// Initialize visuals
		// Event description

		// Rewards
		for(int i = 0; i < _evt.rewardSlots.Count; ++i) {
			// Break the loop if we don't have more reward info slots
			if(i >= m_rewardInfos.Length) break;

			// Initialize the reward info corresponding to this reward
			m_rewardInfos[i].InitFromReward(_evt.rewardSlots[i]);

			// Put into position (except last reward, which has a fixed position)
			if(i < _evt.rewardSlots.Count - 1) {
				// Set min and max anchor in Y to match the target percentage
				Vector2 anchor = m_rewardInfos[i].rectTransform.anchorMin;
				anchor.y = _evt.rewardSlots[i].targetPercentage;
				m_rewardInfos[i].rectTransform.anchorMin = anchor;

				anchor = m_rewardInfos[i].rectTransform.anchorMax;
				anchor.y = _evt.rewardSlots[i].targetPercentage;
				m_rewardInfos[i].rectTransform.anchorMax = anchor;
			}
		}

		if (m_currentValueText_DEBUG != null) {
			m_currentValueText_DEBUG.text = StringUtils.FormatBigNumber(_evt.currentValue);
		}
	}

	public void RefreshProgress(float _value) {
		if (m_progressBar != null) {
			m_progressBar.value = _value;
		}

	}
}