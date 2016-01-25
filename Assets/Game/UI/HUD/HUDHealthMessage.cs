// HUDHealthMessage.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller showing a message when the player's health is critical.
/// </summary>
public class HUDHealthMessage : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private Text m_text = null;

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
	}

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnStarvingToggled);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnStarvingToggled);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The player is starving (or not anymore).
	/// </summary>
	/// <param name="_isStarving">Whether the player is starving or not.</param>
	private void OnStarvingToggled(bool _isStarving) {
		// Toggle tweens
		if(_isStarving) {
			m_text.text = "STARVING!";
			GetComponent<DOTweenAnimation>().DORewind();
			DOTween.Play(gameObject, "in");
		} else {
			DOTween.Pause(gameObject);
			DOTween.Restart(gameObject, "out");
		}
	}
}
