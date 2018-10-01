﻿// HUDTime.cs
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
public class HUDTournamentScore : HudWidget {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private long m_lastScorePrinted;
	private TrackerBase m_tracker;
	private long m_targetScore = 0;
	private HDTournamentDefinition.TournamentGoal.TournamentMode m_mode = HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL;

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
			HDTournamentData _data = HDLiveEventsManager.instance.m_tournament.data as HDTournamentData;
			HDTournamentDefinition _def = _data.definition as HDTournamentDefinition;

			m_mode = _def.m_goal.m_mode;
			if ( ( m_mode == HDTournamentDefinition.TournamentGoal.TournamentMode.NORMAL && _def.m_goal.m_type == "survive_time" ) || _def.m_goal.m_type == "score")
			{
				gameObject.SetActive(false);
			}
			else
			{
				m_lastScorePrinted = 0;	
				m_targetScore = _def.m_goal.m_targetAmount;
				m_tracker = HDLiveEventsManager.instance.m_tournament.m_tracker;
				UpdateIcon();
			}
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private void UpdateIcon()
	{
		Transform tr = transform.Find("Icon_score");
		if ( tr != null )
		{
			Image img = tr.GetComponent<Image>();
			if ( img != null )
			{
				HDTournamentManager tournament = HDLiveEventsManager.instance.m_tournament;
				HDTournamentDefinition def = tournament.data.definition as HDTournamentDefinition;
				img.sprite = Resources.Load<Sprite>(UIConstants.LIVE_EVENTS_ICONS_PATH + def.m_goal.m_icon);
			}
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		base.Start();
		UpdateScore();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	override protected void Update() {
		base.Update();
		UpdateScore();
	}

	protected override string GetValueAsString() {
		string ret = "";
		if ( m_mode == HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_ATTACK ){
			ret = m_lastScorePrinted + "/" + m_targetScore;
		}else{
			ret = m_lastScorePrinted + "";
		}
		return ret;
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
			UpdateValue(_score, true); 
			m_lastScorePrinted = _score;
		}
	}
}
