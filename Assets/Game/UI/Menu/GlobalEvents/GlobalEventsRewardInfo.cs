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
using DG.Tweening;

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
	[SerializeField] protected Image m_tick = null;

	// Internal
	private HDLiveData.Reward m_questReward = null;
	public HDLiveData.Reward questReward {
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
	public void InitFromReward(HDLiveData.Reward _questReward) {
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
			m_targetText.text = StringUtils.FormatBigNumber(m_questReward.target);
		}

		// Parent will do the rest
		base.Refresh();
	}

	public void ShowAchieved(bool _achieved, bool _animate)
	{
		if(m_tick == null) return;

		bool wasAchieved = m_tick.gameObject.activeSelf;
		m_tick.gameObject.SetActive( _achieved );

		// Animate? Only when going from not visible to visible
		if(_animate) {
			if(!wasAchieved && _achieved) {
				m_tick.transform.DOKill();
				m_tick.transform.DOScale(15f, 1f).From().SetEase(Ease.InExpo);
				m_tick.DOFade(0f, 1f).From().SetEase(Ease.InExpo);

				AudioController.Play("hd_pet_add", 1f, 0.25f);	// Delay to sync with anim
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}