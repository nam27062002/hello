﻿// DebugHUDMultiplierController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/10/2015.
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
/// Simple controller for a score counter in the debug hud.
/// </summary>
public class DebugHUDMultiplierController : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private Text m_text = null;
	private Animator m_anim = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_text = GetComponent<Text>();
		m_text.text = "";

		m_anim = GetComponent<Animator>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		UpdateText(RewardManager.currentScoreMultiplier);
	}

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, OnMultiplierChanged);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, OnMultiplierChanged);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		if(m_anim != null) {
			m_anim.SetFloat("timer", RewardManager.scoreMultiplierTimer);
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the displayed multiplier.
	/// </summary>
	/// <param name="_mult">The multiplier we want to display.</param>
	private void UpdateText(ScoreMultiplier _mult) {
		// Do it! Except if going back to "no multiplier"
		if(_mult != RewardManager.defaultScoreMultiplier) {
			m_text.text = "x" + StringUtils.FormatNumber(_mult.multiplier, 0);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Current score multiplier has changed.
	/// </summary>
	/// <param name="_oldMultiplier">The previous multiplier.</param>
	/// <param name="_newMultiplier">The new multiplier.</param>
	private void OnMultiplierChanged(ScoreMultiplier _oldMultiplier, ScoreMultiplier _newMultiplier) {
		// Update text
		UpdateText(_newMultiplier);

		// Launch anim
		if(m_anim != null) {
			// If it's the default multiplier, fade out
			if(_newMultiplier == RewardManager.defaultScoreMultiplier) {
				m_anim.SetTrigger("out");
			} else {
				m_anim.SetTrigger("start");
			}
		}
	}
}
