// PopupSummaryMissionPill.cs
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
/// Pill representing a single mission in the summary popup.
/// </summary>
public class PopupSummaryMissionPill : MonoBehaviour {
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
	
	// References
	[SerializeField] private Text m_descText = null;
	[SerializeField] private Text m_rewardText = null;

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
		DebugUtils.Assert(m_descText != null, "Missing field!");
		DebugUtils.Assert(m_rewardText != null, "Missing field!");
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Get mission
		m_mission = MissionManager.GetMission(m_missionDifficulty);
		if(m_mission == null) return;

		// If mission is not completed, hide pill and return
		if(!m_mission.objective.isCompleted) {
			gameObject.SetActive(false);
			return;
		}
		
		// Update visuals
		m_descText.text = m_mission.objective.GetDescription();
		m_rewardText.text = StringUtils.FormatNumber(m_mission.rewardCoins);
	}
}
