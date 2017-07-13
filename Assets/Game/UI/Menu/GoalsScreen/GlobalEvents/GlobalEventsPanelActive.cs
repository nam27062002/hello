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
	private const float EVENT_COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private TextMeshProUGUI m_objectiveText = null;
	[SerializeField] private Image m_objectiveIcon = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[SerializeField] private TextMeshProUGUI m_currentValueText_DEBUG = null;
	[Space]
	[SerializeField] private Slider m_progressBar = null;
	[SerializeField] private GlobalEventsRewardInfo[] m_rewardInfos = new GlobalEventsRewardInfo[0];
	[Space]
	[SerializeField] private GlobalEventsLeaderboardView m_leaderboard = null;

	// Internal
	private float m_eventCountdownUpdateTimer = 0f;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure event timer will be updated
		m_eventCountdownUpdateTimer = 0f;

		// Subscribe to external events
		Messenger.AddListener<GlobalEventManager.RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, OnEventDataUpdated);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<GlobalEventManager.RequestType>(GameEvents.GLOBAL_EVENT_UPDATED, OnEventDataUpdated);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Just in case
		if(GlobalEventManager.currentEvent == null) return;

		// Update countdown at intervals
		m_eventCountdownUpdateTimer -= Time.deltaTime;
		if(m_eventCountdownUpdateTimer <= 0f) {
			// Reset timer
			m_eventCountdownUpdateTimer = EVENT_COUNTDOWN_UPDATE_INTERVAL;

			// Parse remaining time
			m_timerText.text = TimeUtils.FormatTime(
				GlobalEventManager.currentEvent.remainingTime.TotalSeconds,
				TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
				4
			);
		}

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
		m_objectiveIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + evt.objective.icon);

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

		// Progress
		m_progressBar.value = evt.progress;
		m_currentValueText_DEBUG.text = StringUtils.FormatBigNumber(evt.currentValue);

		// Leaderboard
		m_leaderboard.Refresh();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// We have new data concerning the event.
	/// </summary>
	/// <param name="_requestType">Request type.</param>
	private void OnEventDataUpdated(GlobalEventManager.RequestType _requestType) {
		// Nothing to do if disabled
		if(!isActiveAndEnabled) return;

		// Different stuff depending on request type
		switch(_requestType) {
			case GlobalEventManager.RequestType.EVENT_STATE: {
				// Chain with state request
				GlobalEventManager.RequestCurrentEventLeaderboard();
				BusyScreen.Show(this);
			} break;

			case GlobalEventManager.RequestType.EVENT_LEADERBOARD: {
				// Toggle busy screen off
				BusyScreen.Hide(this);
			} break;
		}
	}
}