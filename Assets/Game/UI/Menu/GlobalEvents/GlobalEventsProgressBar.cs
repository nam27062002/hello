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
	public Slider progressBar {
		get { return m_progressBar; }
	}
	[SerializeField] [Range(0f, 1f)] private float m_minBarThreshold = 0.05f;
	[Space]
	[SerializeField] private GlobalEventsRewardInfo[] m_rewardInfos = new GlobalEventsRewardInfo[0];

	public void RefreshRewards(HDQuestDefinition _evt, long currentValue) {
		// Initialize bar limits
		if(m_progressBar != null) {
			m_progressBar.minValue = 0f;
			m_progressBar.maxValue = (float)_evt.m_goal.m_amount;
		}

        if (_evt.m_rewards == null)
        {
            throw new System.Exception("HDQuestDefinition name: " + _evt.m_name + " has a null m_rewards array.");
            return;
        }


        // Rewards
        for (int i = 0; i < _evt.m_rewards.Count; ++i) {
			// Break the loop if we don't have more reward info slots
			if(i >= m_rewardInfos.Length) break;

            // Initialize the reward info corresponding to this reward
            if (_evt.m_rewards[i] != null)
            {
                m_rewardInfos[i].InitFromReward(_evt.m_rewards[i]);
            }
            else
            {
                throw new System.Exception("HDQuestDefinition name: " + _evt.m_name + " has a null m_reward at index: " + i);
                continue;
            }
			m_rewardInfos[i].ShowAchieved( currentValue >= _evt.m_rewards[i].target, false );

			// Put into position (except last reward, which has a fixed position)
			//if(i < _evt.m_rewards.Count - 1) {	// [AOC] With new desing, last reward too!
				// Set min and max anchor in X to match the target percentage
				float targetPercentage = (float)_evt.m_rewards[i].target / (float)_evt.m_goal.m_amount;

				Vector2 anchor = m_rewardInfos[i].rectTransform.anchorMin;
				anchor.x = targetPercentage;
				m_rewardInfos[i].rectTransform.anchorMin = anchor;

				anchor = m_rewardInfos[i].rectTransform.anchorMax;
				anchor.x = targetPercentage;
				m_rewardInfos[i].rectTransform.anchorMax = anchor;

				anchor = m_rewardInfos[i].rectTransform.anchoredPosition;
				anchor.x = 0f;
				m_rewardInfos[i].rectTransform.anchoredPosition = anchor;
			//}
		}

		if (m_currentValueText_DEBUG != null) {
			m_currentValueText_DEBUG.text = StringUtils.FormatBigNumber(currentValue);
		}
	}

	/*public void RefreshAchieved(HDQuestDefinition _evt, long currentValue) {
		for(int i = 0; i < _evt.m_rewards.Count; ++i) {
			// Break the loop if we don't have more reward info slots
			if(i >= m_rewardInfos.Length) break;
			m_rewardInfos[i].ShowAchieved( currentValue >= _evt.m_rewards[i].targetAmount, false );
		}
	}*/

	public void RefreshAchieved(bool _animate) {
		// Use current bar value
		for(int i = 0; i < m_rewardInfos.Length; ++i) {
			m_rewardInfos[i].ShowAchieved(m_progressBar.value >= m_rewardInfos[i].questReward.target, _animate);
		}
	}

	public void RefreshProgress(float _value, float _animDuration = -1f, bool _checkAchieved = true) {
		if (m_progressBar != null) {
			// [AOC] For visual purposes, always show a minimum amount of bar
			_value = Mathf.Max(_value, Mathf.Lerp(m_progressBar.minValue, m_progressBar.maxValue, m_minBarThreshold));

			if(_animDuration < 0f) {
				m_progressBar.value = _value;
				if(_checkAchieved) RefreshAchieved(false);
			} else {
				m_progressBar.DOKill();
				Tweener tween = m_progressBar.DOValue(_value, _animDuration).SetEase(Ease.OutQuad);

				if(_checkAchieved) {
					tween.OnUpdate(() => { RefreshAchieved(true); });
				}
			}
		}

	}
}