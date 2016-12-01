// DebugHUDMultiplierMessage.cs
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
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller showing a message whenever a new score multiplier is triggered.
/// </summary>
public class HUDMultiplierMessage : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private TextMeshProUGUI m_text = null;
	private Animator m_anim = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_text = GetComponent<TextMeshProUGUI>();
		m_text.text = "";

		m_anim = GetComponent<Animator>();
	}

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<ScoreMultiplier, float>(GameEvents.SCORE_MULTIPLIER_CHANGED, OnMultiplierChanged);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<ScoreMultiplier, float>(GameEvents.SCORE_MULTIPLIER_CHANGED, OnMultiplierChanged);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Current score multiplier has changed.
	/// </summary>
	/// <param name="_newMultiplier">The new multiplier.</param>
	private void OnMultiplierChanged(ScoreMultiplier _newMultiplier, float fireRushMultiplier) {
		// Multiplier must have messages!
		if(_newMultiplier.feedbackMessages.Count == 0) return;

		// Select a random message from the multiplier definition
		string message = _newMultiplier.feedbackMessages.GetRandomValue<string>();
		m_text.text = message;
		m_anim.SetTrigger("start");
	}
}
