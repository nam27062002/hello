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
		// Initialize UI
		m_rewardIndexSetter.SetValue(UsersManager.currentUser.dailyRewards.totalRewardIdx);

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
		System.TimeSpan remainingTime = UsersManager.currentUser.dailyRewards.timeToCollection;
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
		UsersManager.currentUser.dailyRewards.DEBUG_SetTotalRewardIdx((int)_newValue);

		// Save persistence
		PersistenceFacade.instance.Save_Request();

		// Notify game
		Messenger.Broadcast(MessengerEvents.DEBUG_REFRESH_DAILY_REWARDS);
	}

	/// <summary>
	/// Skip cooldown timer.
	/// </summary>
	public void OnSkipCooldownTimer() {
		// Tell the rewards sequence
		UsersManager.currentUser.dailyRewards.DEBUG_SkipCooldownTimer();

		// Save persistence
		PersistenceFacade.instance.Save_Request();

		// Notify game
		Messenger.Broadcast(MessengerEvents.DEBUG_REFRESH_DAILY_REWARDS);
	}

	/// <summary>
	/// Launch the daily rewards flow.
	/// </summary>
	public void OnLaunchFlow() {
		// Close panel
		ControlPanel.instance.Toggle();

		// Show the popup after a short delay
		UbiBCN.CoroutineManager.DelayedCall(() => {
			PopupManager.OpenPopupInstant(PopupDailyRewards.PATH);
		}, 0.5f);
	}
}