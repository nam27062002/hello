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
public class HUDTournamentScore : IHUDCounter {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private long m_lastScorePrinted;
	private TrackerBase m_tracker;
	private long m_targetScore = 0;
	private HDTournamentDefinition.TournamentGoal.TournamentMode m_mode = HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL;
	private HDTournamentDefinition m_tournamentDef = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		base.Awake();
		if ( SceneController.mode == SceneController.Mode.TOURNAMENT )
		{
			HDTournamentData _data = HDLiveDataManager.tournament.data as HDTournamentData;
			m_tournamentDef = _data.definition as HDTournamentDefinition;

			m_mode = m_tournamentDef.m_goal.m_mode;
			if ( ( m_mode == HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL && m_tournamentDef.m_goal.m_type == "survive_time" ) || m_tournamentDef.m_goal.m_type == "score")
			{
				gameObject.SetActive(false);
			}
			else
			{
				m_lastScorePrinted = 0;	
				m_targetScore = m_tournamentDef.m_goal.m_targetAmount;
				m_tracker = HDLiveDataManager.tournament.m_tracker;
				Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
                // We used to update the icon here depending on the quest objective
                // but since OTA the icon will be always the gold badge plain 2d icon. 

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
	override protected void Start() {
		base.Start();
		UpdateScore();
	}

	void OnDestroy()
	{
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
	}

	public void OnGameStarted(){
		m_lastScorePrinted = m_tracker.currentValue;
		PrintValue();

	}
	/// <summary>
	/// Called every frame.
	/// </summary>
	public override void PeriodicUpdate() {
		base.PeriodicUpdate();
		UpdateScore();
	}

	protected override string GetValueAsString() {
		string ret = "";
		if(m_mode == HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK) {
			// Show the target score for time attack tournaments
			ret = m_lastScorePrinted + "/" + m_targetScore;
		} else if(m_tournamentDef.m_goal.m_type == "birthday_stay_mode_time") {
			// Special format for "stay mode" trackers
			ret = m_tracker.FormatValue(m_lastScorePrinted);
		} else {
			// Default behaviour
			ret = m_lastScorePrinted + "";
		}
		return ret;
    }

	protected override float GetMinimumAnimInterval() {
		if(m_tournamentDef.m_goal.m_type == "birthday_stay_mode_time") {
			return 1.1f;
		}
		return 0.25f;
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
			m_lastScorePrinted = _score;
			UpdateValue(_score, true); 
		}
	}
}
