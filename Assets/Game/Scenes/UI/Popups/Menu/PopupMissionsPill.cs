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
	
	// References
	[Separator]
	[SerializeField] private Text m_nameText = null;
	[SerializeField] private Text m_descText = null;

	[Space(5)]
	[SerializeField] private Text m_progressText = null;
	[SerializeField] private Slider m_progressBar = null;

	[Space(5)]
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
		DebugUtils.Assert(m_progressText != null, "Missing field!");
		DebugUtils.Assert(m_progressBar != null, "Missing field!");

		DebugUtils.Assert(m_rewardText != null, "Missing field!");
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure we're up to date
		Refresh();
	}

	/// <summary>
	/// Update the pill with the data from the target mission.
	/// </summary>
	public void Refresh() {
		// Get mission
		m_mission = MissionManager.GetMission(m_missionDifficulty);
		if(m_mission == null) return;

		// Update visuals
		if(m_nameText != null) m_nameText.text = Localization.Localize(m_mission.def.tidName);
		if(m_descText != null) m_descText.text = m_mission.objective.GetDescription();

		m_progressText.text = System.String.Format("{0}/{1}", m_mission.objective.GetCurrentValueFormatted(), m_mission.objective.GetTargetValueFormatted());
		m_progressBar.value = m_mission.objective.progress;

		m_rewardText.text = StringUtils.FormatNumber(m_mission.rewardCoins);;
	}
}
