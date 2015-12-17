// DebugSimulationSummaryPopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple summary popup to display the results of a simulation.
/// </summary>
public class DebugSimulationSummaryPopup : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Debug/PF_DebugSimulationSummaryPopup";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	public Text m_durationText = null;
	public Text m_xpRewardText = null;
	public Text m_coinsRewardText = null;
	public Text m_pcRewardText = null;
	public Text m_levelUpText = null;
	public Text m_evolutionFactorText = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {

	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the result of the simulation.
	/// </summary>
	public void Init(float _gameDuration, float _xpReward, float _coinsReward, float _pcReward, int _levelUpCount, float _evolutionFactor) {
		// Just put values into textfields
		m_durationText.text = TimeUtils.FormatTime(_gameDuration, TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 2);
		m_xpRewardText.text = "+" + StringUtils.FormatNumber(_xpReward, 0);
		m_coinsRewardText.text = "+" + StringUtils.FormatNumber(_coinsReward, 0);
		m_pcRewardText.text = "+" + StringUtils.FormatNumber(_pcReward, 0);
		m_levelUpText.enabled = _levelUpCount > 0;
		m_levelUpText.text = String.Format("{0} {1} UP!!", StringUtils.FormatNumber(_levelUpCount), _levelUpCount > 1 ? "LEVELS" : "LEVEL");
		m_evolutionFactorText.text = "x" + StringUtils.FormatNumber(_evolutionFactor, 2); 
	}
}
