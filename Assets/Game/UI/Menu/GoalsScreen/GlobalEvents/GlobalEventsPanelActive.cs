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
	[SerializeField] private Image m_bonusDragonIcon = null;
	[SerializeField] private Localizer m_topRewardPercentileText = null;
	[SerializeField] private Localizer m_playerPercentileText = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[Space]
	[SerializeField] private GlobalEventsProgressBar m_progressBar = null;
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

		// Bonus dragon icon
		m_bonusDragonIcon.sprite = Resources.Load<Sprite>(UIConstants.DISGUISE_ICONS_PATH + evt.bonusDragonSku + "/icon_disguise_0");	// Default skin

		// Top reward info text
		float topPercentile = evt.topContributorsRewardSlot.targetPercentage * 100f;
		m_topRewardPercentileText.Localize(
			m_topRewardPercentileText.tid, 
			StringUtils.FormatNumber(topPercentile, 2)
		);

		// Player percentile text
		if(m_playerPercentileText != null) {
			// Don't show if player is not yet classified
			GlobalEventUserData playerData = UsersManager.currentUser.GetGlobalEventData(evt.id);
			if(playerData.position < 0) {
				m_playerPercentileText.gameObject.SetActive(false);
			} else {
				m_playerPercentileText.gameObject.SetActive(false);

				// Compute using player current position and total amount of players
				float playerPercentile = (float)playerData.position / (float)evt.totalPlayers * 100f;
				
				// Snap to a nice value
				float snapValue = 100f;
				float snappedValue = snapValue;
				while(playerPercentile < snapValue && snapValue > topPercentile) {	// At least the top percentile!
					// Store last valid value
					snappedValue = snapValue;
					
					// More precision for the smallest percentiles
					if(snapValue <= 5f) {
						snapValue -= 1f;
					} else if(snapValue <= 20f) {
						snapValue -= 5f;
					} else {
						snapValue -= 10f;
					}
				}
				
				// Localize
				m_playerPercentileText.Localize(
					m_playerPercentileText.tid, 
					StringUtils.FormatNumber(snapValue, 2)
				);
			}
		}

		// Progress
		if (m_progressBar != null) {
			m_progressBar.RefreshRewards(evt);
			m_progressBar.RefreshProgress(evt.progress);
		}

		// Leaderboard
		// m_leaderboard.Refresh();
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
				// Chain with leaderboard request
				GlobalEventManager.RequestCurrentEventLeaderboard(false);
				BusyScreen.Show(this);
			} break;

			case GlobalEventManager.RequestType.EVENT_LEADERBOARD: {
				// Toggle busy screen off
				BusyScreen.Hide(this);
			} break;
		}
	}

	/// <summary>
	/// The bonus dragon info button has been pressed.
	/// </summary>
	public void OnBonusDragonInfoButton() {
		// Get bonus dragon definition
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, GlobalEventManager.currentEvent.bonusDragonSku);
		if(def == null) return;	// Shouldn't happen

		// Show feedback
		UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize("TID_EVENT_BONUS_DRAGON_INFO_MESSAGE", def.GetLocalized("tidName")),
			new Vector2(0.5f, 0.5f),
			(RectTransform)this.GetComponentInParent<Canvas>().transform
		);
	}
}