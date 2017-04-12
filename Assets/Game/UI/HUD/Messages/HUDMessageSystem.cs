// HUDMessageSystem.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Manager for HUD messages, avoiding overlapping between them.
/// If a message with higher priority is triggered, previous message will be closed.
/// Works similar to Unity's ToggleGroup.
/// </summary>
public class HUDMessageSystem : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	private List<HUDMessage> m_messages = new List<HUDMessage>();

	// Internal logic
	private HUDMessage m_currentMessage = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Register a message to work with this system.
	/// </summary>
	/// <param name="_msg">The message to be registered.</param>
	public void Add(HUDMessage _msg) {
		// Skip invalid params
		if(_msg == null) return;

		// Skip if message is already registered
		if(m_messages.Contains(_msg)) return;

		// Register and subscribe to events
		m_messages.Add(_msg);
		_msg.OnShow.AddListener(OnMessageShow);
		_msg.OnHide.AddListener(OnMessageHide);
	}

	/// <summary>
	/// Check whether a given message can be displayed or not within the current system.
	/// </summary>
	/// <returns><c>true</c>, if show was requested, <c>false</c> otherwise.</returns>
	/// <param name="_msg">Message.</param>
	public bool RequestShow(HUDMessage _msg) {
		// Don't allow if there is an open message of higher priority
		if(m_currentMessage != null) {
			if(m_currentMessage.priority > _msg.priority) return false;
		}

		// All checks passed!
		return true;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A HUD message has been triggered.
	/// </summary>
	/// <param name="_msg">The message that triggered the event.</param>
	private void OnMessageShow(HUDMessage _msg) {
		// Hide current message (if any)
		if(m_currentMessage != null && m_currentMessage != _msg) m_currentMessage.Hide();

		// Store as the current message
		m_currentMessage = _msg;
	}

	/// <summary>
	/// A HUD message has been hidden.
	/// </summary>
	/// <param name="_msg">The message that triggered the event.</param>
	private void OnMessageHide(HUDMessage _msg) {
		// Clear current message ref
		m_currentMessage = null;
	}

	/// <summary>
	/// The player has been knocked out.
	/// </summary>
	/// <param name="_type">Type of damage.</param>
	/// <param name="_source">Source that killed the player.</param>
	private void OnPlayerKo(DamageType _type, Transform _source) {
		// Hide current message (if any)
		if(m_currentMessage != null) m_currentMessage.Hide();
	}
}