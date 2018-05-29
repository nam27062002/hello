// HUDTime.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a time counter in the hud.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class HUDTournamentScore : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private TextMeshProUGUI m_valueTxt;
	private long m_lastScorePrinted;
	private TrackerBase m_tracker;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		if ( SceneController.s_mode == SceneController.Mode.TOURNAMENT )
		{
			HDTournamentData _data = HDLiveEventsManager.instance.m_tournament.GetEventData() as HDTournamentData;
			HDTournamentDefinition _def = _data.definition as HDTournamentDefinition;

			if ( ( _def.m_goal.m_mode == HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL && _def.m_goal.m_type == "survive_time" ) || _def.m_goal.m_type == "score")
			{
				gameObject.SetActive(false);
			}
			else
			{
				// Get external references
				m_valueTxt = GetComponent<TextMeshProUGUI>();
				m_valueTxt.text = "0";
				m_lastScorePrinted = -1;	
				m_tracker = HDLiveEventsManager.instance.m_tournament.m_tracker;
			}
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		UpdateScore();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		UpdateScore();
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the displayed score.
	/// </summary>
	private void UpdateScore() {

		long _score = m_tracker.currentValue;
		if(_score != m_lastScorePrinted) {
			m_valueTxt.text = _score.ToString();
			m_lastScorePrinted = _score;
		}
	}
}
