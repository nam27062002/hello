// PopupMissionsPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Pill representing a single mission for the temp popup.
/// </summary>
public class PopupMissionsPill : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[Separator]
	[HideEnumValues(false, true)]
	[SerializeField] private Mission.Difficulty m_missionDifficulty = Mission.Difficulty.EASY;
	[SerializeField] private bool m_showProgressForSingleRunMissions = false;	// It doesn't make sense to show progress for single-run missions in the menu
	
	// References
	[Separator]
	[SerializeField] private GameObject m_lockedObj = null;
	[SerializeField] private GameObject m_cooldownObj = null;
	[SerializeField] private GameObject m_activeObj = null;

	// Cooldown group
	[Separator]
	[SerializeField] private Text m_cooldownText = null;
	[SerializeField] private Slider m_cooldownBar = null;

	[Space(5)]
	[SerializeField] private Text m_skipCostText = null;	// Optional

	// Active group
	[Separator]
	[SerializeField] private Text m_descText = null;

	[Space(5)]
	[SerializeField] private GameObject m_progressGroup = null;
	[SerializeField] private Text m_progressText = null;
	[SerializeField] private Slider m_progressBar = null;

	[Space(5)]
	[SerializeField] private Text m_rewardText = null;
	[SerializeField] private Text m_removeCostText = null;	// Optional

	// Data
	private Mission m_mission = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Check required references
		Debug.Assert(m_lockedObj != null, "Required reference!");
		Debug.Assert(m_cooldownObj != null, "Required reference!");
		Debug.Assert(m_activeObj != null, "Required reference!");

		// Subscribe to external events
		Messenger.AddListener<Mission>(GameEvents.MISSION_REMOVED, OnMissionRemoved);
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Mission>(GameEvents.MISSION_REMOVED, OnMissionRemoved);
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure we're up to date
		Refresh();
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update time-dependant fields
		RefreshCooldown();
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update the pill with the data from the target mission.
	/// </summary>
	public void Refresh() {
		// Get mission
		m_mission = MissionManager.GetMission(m_missionDifficulty);
		if(m_mission == null) return;

		// Select which object should be visible
		// [AOC] TODO!! We still don't have the locked and cooldown implemented
		m_lockedObj.SetActive(false);
		m_cooldownObj.SetActive(false);
		m_activeObj.SetActive(true);

		// Update visuals
		RefreshActive();
		RefreshCooldown();
	}

	/// <summary>
	/// Refresh active state object.
	/// </summary>
	private void RefreshActive() {
		if(m_activeObj.activeSelf) {
			m_descText.text = m_mission.objective.GetDescription();

			// Optionally hide progress for singlerun missions
			bool show = !m_mission.def.singleRun || m_showProgressForSingleRunMissions;
			m_progressGroup.SetActive(show);
			if(show) {
				m_progressText.text = System.String.Format("{0}/{1}", m_mission.objective.GetCurrentValueFormatted(), m_mission.objective.GetTargetValueFormatted());
				m_progressBar.value = m_mission.objective.progress;
			}

			m_rewardText.text = StringUtils.FormatNumber(m_mission.rewardCoins);

			if(m_removeCostText != null) m_removeCostText.text = StringUtils.FormatNumber(m_mission.removeCostPC);
		}
	}

	/// <summary>
	/// Refresh the cooldown state object.
	/// </summary>
	private void RefreshCooldown() {
		if(m_cooldownObj.activeSelf) {
			// [AOC] TODO!! Update cooldown time text, bar and cost to skip
			if(m_skipCostText != null) m_skipCostText.text = StringUtils.FormatNumber(m_mission.skipCostPC);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Callback for the remove mission button.
	/// </summary>
	public void OnRemoveMission() {
		// Ignore if mission not initialized
		if(m_mission == null) return;

		// Make sure we have enough PC to remove the mission
		int costPC = m_mission.removeCostPC;
		if(UserProfile.pc >= costPC) {
			// Do it!
			UserProfile.AddPC(-costPC);
			MissionManager.RemoveMission(m_missionDifficulty);
			PersistenceManager.Save();
		} else {
			// Open shop popup
			PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);
		}
	}

	/// <summary>
	/// Callback for the skip mission button.
	/// </summary>
	public void OnSkipMission() {
		// Ignore if mission not initialized
		if(m_mission == null) return;

		// [AOC] TODO!!
	}

	/// <summary>
	/// A mission has been removed. If it matches the difficulty of this pill, refresh 
	/// with the new mission data.
	/// </summary>
	/// <param name="_newMission">The new mission replacing the one removed.</param>
	private void OnMissionRemoved(Mission _newMission) {
		if(_newMission.def.difficulty == m_missionDifficulty) {
			m_mission = _newMission;
			Refresh();
		}
	}
}
