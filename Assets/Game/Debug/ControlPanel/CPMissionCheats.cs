// CPMissionsCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/01/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Allow several operations related to the mission system from the Control Panel.
/// </summary>
public class CPMissionCheats : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum Operation {
		ADD,
		REMOVE,
		SET
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Mission.Difficulty m_difficulty = Mission.Difficulty.EASY;
	[SerializeField] private TMP_InputField m_valueInput = null;
	[SerializeField] private TMP_InputField m_skipInput = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Read the input textfield value and performs the required operation.
	/// </summary>
	/// <param name="_operation">Target operation.</param>
	private void SetValue(Operation _operation) {
		// Error check
		string errorMsg = string.Empty;

		Mission m = MissionManager.GetMission(m_difficulty);
		if(m == null) {
			errorMsg = "NO VALID MISSION!";
		} else if(m.state != Mission.State.ACTIVE) {
			errorMsg = "MISSION MUST BE ACTIVE!";
		}

		if(!string.IsNullOrEmpty(errorMsg)) {
			TextFeedback(errorMsg, Color.red);
			return;
		}

		// Get amount from linked input field
		long amount = long.Parse(m_valueInput.text);

		// Compute amount to add
		switch(_operation) {
			case Operation.ADD: {
				amount = m.objective.currentValue + amount;
			} break;

			case Operation.REMOVE: {
				amount = m.objective.currentValue - amount;
			} break;
		}

		// Apply
		m.objective.currentValue = amount;

		// Process missions in case the mission has been completed
		MissionManager.ProcessMissions();

		// Notify game
		Messenger.Broadcast(MessengerEvents.DEBUG_REFRESH_MISSION_INFO);

		// Done!
		TextFeedback("SUCCESS!", Color.green);
	}

	/// <summary>
	/// Read the input textfield value and performs the required operation.
	/// </summary>
	/// <param name="_operation">Target operation.</param>
	private void SkipCooldownMinutes(Operation _operation) {
		// Error check
		string errorMsg = string.Empty;

		Mission m = MissionManager.GetMission(m_difficulty);
		if(m == null) {
			errorMsg = "NO VALID MISSION!";
		} else if(m.state != Mission.State.COOLDOWN) {
			errorMsg = "MISSION MUST BE IN COOLDOWN!";
		}

		if(!string.IsNullOrEmpty(errorMsg)) {
			TextFeedback(errorMsg, Color.red);
			return;
		}

		// Get amount from linked input field
		float amount = float.Parse(m_skipInput.text);
		amount *= 60f;

		// Compute amount to add
		switch(_operation) {
			case Operation.ADD: {
				amount = -amount;
			} break;

			case Operation.REMOVE: {
				amount = amount;
			} break;

			case Operation.SET: {
				amount = (float)m.cooldownRemaining.TotalSeconds - amount;	// That should give us the amount we need to skip to set the remaining cd to "amount"
			} break;
		}

		// Apply
		m.DEBUG_SkipCooldownTimer(amount);

		// Save persistence
		PersistenceFacade.instance.Save_Request(false);

		// Notify game
		Messenger.Broadcast(MessengerEvents.DEBUG_REFRESH_MISSION_INFO);

		// Done!
		TextFeedback("SUCCESS!", Color.green);
	}

	/// <summary>
	/// Trigger a text feedback.
	/// </summary>
	private void TextFeedback(string _text, Color _color) {
		UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
			_text,
			new Vector2(0.5f, 0.5f),
			ControlPanel.panel.parent as RectTransform
		);
		text.text.color = _color;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Specialized versions to be used as button callbacks.
	/// </summary>
	public void OnAddValue() 	{ SetValue(Operation.ADD); }
	public void OnRemoveValue() { SetValue(Operation.REMOVE); }
	public void OnSetValue() 	{ SetValue(Operation.SET); }

	public void OnAddCooldown() 	{ SkipCooldownMinutes(Operation.ADD); }
	public void OnRemoveCooldown() 	{ SkipCooldownMinutes(Operation.REMOVE); }
	public void OnSetCooldown() 	{ SkipCooldownMinutes(Operation.SET); }

	/// <summary>
	/// Instantly complete the mission.
	/// </summary>
	public void OnCompleteMission() {
		// Error check
		Mission m = MissionManager.GetMission(m_difficulty);
		if(m == null) {
			TextFeedback("NO VALID MISSION!", Color.red);
			return;
		} else if(m.state != Mission.State.ACTIVE) {
			TextFeedback("MISSION MUST BE ACTIVE!", Color.red);
			return;
		}

        // Mark objective as completed
        m.EnableTracker(true);
		m.objective.currentValue = m.objective.targetValue;

		// Process missions so a new mission is generated and rewards are given
		MissionManager.ProcessMissions();

		// Dispatch global event
		Messenger.Broadcast(MessengerEvents.DEBUG_REFRESH_MISSION_INFO);

		// Done!
		TextFeedback("SUCCESS!", Color.green);
	}

	/// <summary>
	/// Instantly complete the mission.
	/// </summary>
	public void OnSkipCooldown() {
		// Error check
		Mission m = MissionManager.GetMission(m_difficulty);
		if(m == null) {
			TextFeedback("NO VALID MISSION!", Color.red);
			return;
		} else if(m.state != Mission.State.COOLDOWN) {
			TextFeedback("MISSION MUST BE IN COOLDOWN!", Color.red);
			return;
		}

		// Skip all the remaining seconds
		m.DEBUG_SkipCooldownTimer((float)m.cooldownRemaining.TotalSeconds);

		// Notify game
		Messenger.Broadcast(MessengerEvents.DEBUG_REFRESH_MISSION_INFO);

		// Done!
		TextFeedback("SUCCESS!", Color.green);
	}
}