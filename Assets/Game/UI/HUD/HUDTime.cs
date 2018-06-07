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
public class HUDTime : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private TextMeshProUGUI m_valueTxt;
	private long m_lastSecondsPrinted;

	private bool m_countdown = false;
	private long m_timelimit = 0;

	protected Animator m_anim;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_valueTxt = GetComponent<TextMeshProUGUI>();
		m_valueTxt.text = "00:00";
		m_lastSecondsPrinted = -1;
		m_anim = GetComponent<Animator>();

		if ( SceneController.s_mode == SceneController.Mode.TOURNAMENT )
		{
			HDTournamentData data = HDLiveEventsManager.instance.m_tournament.GetEventData() as HDTournamentData;
			HDTournamentDefinition def = data.definition as HDTournamentDefinition;
			if ( def.m_goal.m_mode == HDTournamentDefinition.TournamentGoal.TournamentMode.TIME_LIMIT )
			{
				m_countdown = true;
				m_timelimit = def.m_goal.m_seconds;
			}
		}

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		UpdateTime();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		UpdateTime();
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the displayed score.
	/// </summary>
	private void UpdateTime() {

		long elapsedSeconds = (long)InstanceManager.gameSceneControllerBase.elapsedSeconds;
		if ( m_countdown )
		{
			elapsedSeconds = m_timelimit - elapsedSeconds;
		}

		if(elapsedSeconds != m_lastSecondsPrinted) {		
			if ( m_countdown )
			{
				if ( elapsedSeconds == 60 || elapsedSeconds == 30 )
				{
					m_anim.SetTrigger(GameConstants.Animator.BEEP);
					// Play beep sound!!
				}
				else if ( elapsedSeconds < 10 )
				{
					m_anim.SetBool(GameConstants.Animator.COUNTDOWN, true);
					// Play beep sound!!
				}
			}
			// Do it!
			// Both for game and level editor
			m_valueTxt.text = TimeUtils.FormatTime(elapsedSeconds, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES);
			m_lastSecondsPrinted = elapsedSeconds;
		}
	}
}
