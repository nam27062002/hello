// CPMissionsCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/01/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Allow several operations related to the mission system from the Control Panel.
/// </summary>
public class CPDailyRewardsCheats : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private CPNumericValueEditor m_rewardIndexSetter = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;

	// Internal
	private bool m_boostedActive = false;
	private DailyRewardsSequence m_dailyRewardSequence;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

		// Reuse this panel to apply cheats to boosted daily rewards feature if welcome back is active.
		m_boostedActive = WelcomeBackManager.instance.IsBoostedDailyRewardActive();

		if (m_boostedActive)
        {
			m_dailyRewardSequence = WelcomeBackManager.instance.boostedDailyRewards;

		} else
        {
			m_dailyRewardSequence = UsersManager.currentUser.dailyRewards;
		}


		// Initialize UI
		m_rewardIndexSetter.SetValue(m_dailyRewardSequence.totalRewardIdx);

		// Subscribe to events
		m_rewardIndexSetter.OnValueChanged.AddListener(OnRewardIndexChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from events
		m_rewardIndexSetter.OnValueChanged.RemoveListener(OnRewardIndexChanged);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update timer text
		System.TimeSpan remainingTime = m_dailyRewardSequence.timeToCollection;
		m_timerText.text = "Skip DR Timer\n" + TimeUtils.FormatTime(remainingTime.TotalSeconds, TimeUtils.EFormat.DIGITS, 3);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The reward index has been changed.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	public void OnRewardIndexChanged(float _newValue) {
		// Tell the rewards sequence
		m_dailyRewardSequence.DEBUG_SetTotalRewardIdx((int)_newValue);

		// Skip timer as well
		m_dailyRewardSequence.DEBUG_SkipCooldownTimer();

		// Save persistence
		PersistenceFacade.instance.Save_Request();

		// Notify game
		Broadcaster.Broadcast(BroadcastEventType.DEBUG_REFRESH_DAILY_REWARDS);
	}

	/// <summary>
	/// Skip cooldown timer.
	/// </summary>
	public void OnSkipCooldownTimer() {
		// Tell the rewards sequence
		m_dailyRewardSequence.DEBUG_SkipCooldownTimer();

		// Save persistence
		PersistenceFacade.instance.Save_Request();

		// Notify game
		Broadcaster.Broadcast(BroadcastEventType.DEBUG_REFRESH_DAILY_REWARDS);
	}

	/// <summary>
	/// Launch the daily rewards flow.
	/// </summary>
	public void OnLaunchFlow() {
		// Close panel
		ControlPanel.instance.Toggle();

        // Shall we show the regular popup or the boosted popup?
        if (m_boostedActive)
        {
			// Show the popup after a short delay
			UbiBCN.CoroutineManager.DelayedCall(() => {
				PopupManager.OpenPopupInstant(PopupBoostedDailyRewards.PATH);
			}, 0.5f);
		}
        else
        {
			// Show the popup after a short delay
			UbiBCN.CoroutineManager.DelayedCall(() => {
				PopupManager.OpenPopupInstant(PopupDailyRewards.PATH);
			}, 0.5f);
		}

	}
}