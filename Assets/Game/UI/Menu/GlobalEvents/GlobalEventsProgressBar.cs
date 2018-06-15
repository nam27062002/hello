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
using DG.Tweening;

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
	[SerializeField] [Range(0f, 1f)] private float m_minBarThreshold = 0.05f;
	[Space]
	[SerializeField] private bool m_repositionRewards = true;
	[SerializeField] private GlobalEventsRewardInfo[] m_rewardInfos = new GlobalEventsRewardInfo[0];

	public void RefreshRewards(HDQuestDefinition _evt, long currentValue) {
		// Initialize visuals
		// Event description

		// Rewards
		for(int i = 0; i < _evt.m_rewards.Count; ++i) {
			// Break the loop if we don't have more reward info slots
			if(i >= m_rewardInfos.Length) break;

			// Initialize the reward info corresponding to this reward
			m_rewardInfos[i].InitFromReward(_evt.m_rewards[i]);

			m_rewardInfos[i].ShowAchieved( currentValue >= _evt.m_rewards[i].targetAmount );

			// Put into position (except last reward, which has a fixed position)
			if(m_repositionRewards && i < _evt.m_rewards.Count - 1) {
				// Set min and max anchor in Y to match the target percentage
				Vector2 anchor = m_rewardInfos[i].rectTransform.anchorMin;
				anchor.y = _evt.m_rewards[i].targetPercentage;
				m_rewardInfos[i].rectTransform.anchorMin = anchor;

				anchor = m_rewardInfos[i].rectTransform.anchorMax;
				anchor.y = _evt.m_rewards[i].targetPercentage;
				m_rewardInfos[i].rectTransform.anchorMax = anchor;
			}
		}

		if (m_currentValueText_DEBUG != null) {
			m_currentValueText_DEBUG.text = StringUtils.FormatBigNumber(currentValue);
		}
	}

	public void RefreshAchieved(HDQuestDefinition _evt, long currentValue) {
		for(int i = 0; i < _evt.m_rewards.Count; ++i) {
			// Break the loop if we don't have more reward info slots
			if(i >= m_rewardInfos.Length) break;
			m_rewardInfos[i].ShowAchieved( currentValue >= _evt.m_rewards[i].targetAmount );
		}
	}

	public void RefreshProgress(float _value, float _animDuration = -1f) {
		// [AOC] For visual purposes, always show a minimum amount of bar
		_value = Mathf.Max(_value, m_minBarThreshold);

		if (m_progressBar != null) {
			if(_animDuration < 0f) {
				m_progressBar.value = _value;
			} else {
				m_progressBar.DOValue(_value, _animDuration).SetEase(Ease.OutQuad);
			}
		}

	}
}