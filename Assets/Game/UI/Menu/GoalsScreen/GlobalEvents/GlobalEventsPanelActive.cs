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

		// Progress
		if (m_progressBar != null) {
			m_progressBar.RefreshRewards(evt);
			m_progressBar.RefreshProgress(evt.progress);
		}

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