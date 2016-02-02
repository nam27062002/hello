// HUDDamageMessage.cs
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
/// Simple controller showing a message whenever the player is hurt.
/// </summary>
public class HUDDamageMessage : MonoBehaviour {
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
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The player has received some damage.
	/// </summary>
	/// <param name="_amount">The amount of damage received.</param>
	/// <param name="_source">Optionally, the source of the damage.</param>
	private void OnDamageReceived(float _amount, Transform _source) {
		// Nothing to do if source is not known
		if(_source == null) return;

		// Get the feedback component from the source entity
		PreyStats entity = _source.GetComponent<PreyStats>();
		if(entity != null) {
			// Yes!! Show a message?
			string msg = entity.def.feedbackData.GetFeedback(FeedbackData.Type.DAMAGE);
			if(!String.IsNullOrEmpty(msg)) {
				// Set text and launch anim
				m_text.text = msg;
				m_anim.SetTrigger("start");
			}
		}
	}
}
