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
public class HUDTime : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const float COUNTDOWN_WARNING_1 = 60f;
	private const float COUNTDOWN_WARNING_2 = 30f;
	private const float COUNTDOWN_FINAL_WARNING = 10f;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[SerializeField] private TextMeshProUGUI m_valueTxt;
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
		// Init text
		if(m_valueTxt == null) {
			m_valueTxt = GetComponentInChildren<TextMeshProUGUI>();
		}
		m_valueTxt.text = TimeUtils.FormatTime(0, TimeUtils.EFormat.DIGITS, 2);

		// Get external references
		m_lastSecondsPrinted = -1;
		m_anim = GetComponent<Animator>();

		if ( SceneController.mode == SceneController.Mode.TOURNAMENT )
		{
			HDTournamentData data = HDLiveEventsManager.instance.m_tournament.data as HDTournamentData;
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

		long seconds = (long)InstanceManager.gameSceneControllerBase.elapsedSeconds;
		if ( m_countdown )
		{
			seconds = m_timelimit - seconds;
		}

		if(seconds != m_lastSecondsPrinted) {		
			if ( m_countdown )
			{
				if ( seconds == COUNTDOWN_WARNING_1 || seconds == COUNTDOWN_WARNING_2 )
				{
					m_anim.SetTrigger(GameConstants.Animator.BEEP);

					// Play beep sound!!
					AudioController.Play("Countdown");
				}
				else if ( seconds < COUNTDOWN_FINAL_WARNING )
				{
					if(seconds <= 0) {
						m_anim.SetBool(GameConstants.Animator.COUNTDOWN, false);
						AudioController.Play("hd_dragon_revive");
					} else {
						m_anim.SetBool(GameConstants.Animator.COUNTDOWN, true);
						AudioController.Play("Countdown");
					}
				}
			}

			// Set text
			m_valueTxt.text = TimeUtils.FormatTime(seconds, TimeUtils.EFormat.DIGITS, 2, TimeUtils.EPrecision.MINUTES);
			m_lastSecondsPrinted = seconds;
		}
	}
}
