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
	private void SetValue(CPOperation _operation) {
		// Error check
		string errorMsg = string.Empty;

		Mission m = MissionManager.GetMission(m_difficulty);
		if(m == null) {
			errorMsg = "NO VALID MISSION!";
		} else if(m.state != Mission.State.ACTIVE) {
			errorMsg = "MISSION MUST BE ACTIVE!";
		}

		if(!string.IsNullOrEmpty(errorMsg)) {
			ControlPanel.LaunchTextFeedback(errorMsg, Color.red);
			return;
		}

		// Get amount from linked input field
		long amount = long.Parse(m_valueInput.text);

		// Compute amount to add
		switch(_operation) {
			case CPOperation.ADD: {
				amount = m.objective.currentValue + amount;
			} break;

			case CPOperation.REMOVE: {
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
		ControlPanel.LaunchTextFeedback("SUCCESS!", Color.green);
	}

	/// <summary>
	/// Read the input textfield value and performs the required operation.
	/// </summary>
	/// <param name="_operation">Target operation.</param>
	private void SkipCooldownMinutes(CPOperation _operation) {
		// Error check
		string errorMsg = string.Empty;

		Mission m = MissionManager.GetMission(m_difficulty);
		if(m == null) {
			errorMsg = "NO VALID MISSION!";
		} else if(m.state != Mission.State.COOLDOWN) {
			errorMsg = "MISSION MUST BE IN COOLDOWN!";
		}

		if(!string.IsNullOrEmpty(errorMsg)) {
			ControlPanel.LaunchTextFeedback(errorMsg, Color.red);
			return;
		}

		// Get amount from linked input field
		float amount = float.Parse(m_skipInput.text);
		amount *= 60f;

		// Compute amount to add
		switch(_operation) {
			case CPOperation.ADD: {
				amount = -amount;
			} break;

			case CPOperation.REMOVE: {
				amount = amount;
			} break;

			case CPOperation.SET: {
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
		ControlPanel.LaunchTextFeedback("SUCCESS!", Color.green);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Specialized versions to be used as button callbacks.
	/// </summary>
	public void OnAddValue() 	{ SetValue(CPOperation.ADD); }
	public void OnRemoveValue() { SetValue(CPOperation.REMOVE); }
	public void OnSetValue() 	{ SetValue(CPOperation.SET); }

	public void OnAddCooldown() 	{ SkipCooldownMinutes(CPOperation.ADD); }
	public void OnRemoveCooldown() 	{ SkipCooldownMinutes(CPOperation.REMOVE); }
	public void OnSetCooldown() 	{ SkipCooldownMinutes(CPOperation.SET); }

	/// <summary>
	/// Instantly complete the mission.
	/// </summary>
	public void OnCompleteMission() {
		// Error check
		Mission m = MissionManager.GetMission(m_difficulty);
		if(m == null) {
			ControlPanel.LaunchTextFeedback("NO VALID MISSION!", Color.red);
			return;
		} else if(m.state != Mission.State.ACTIVE) {
			ControlPanel.LaunchTextFeedback("MISSION MUST BE ACTIVE!", Color.red);
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
		ControlPanel.LaunchTextFeedback("SUCCESS!", Color.green);
	}

	/// <summary>
	/// Instantly complete the mission.
	/// </summary>
	public void OnSkipCooldown() {
		// Error check
		Mission m = MissionManager.GetMission(m_difficulty);
		if(m == null) {
			ControlPanel.LaunchTextFeedback("NO VALID MISSION!", Color.red);
			return;
		} else if(m.state != Mission.State.COOLDOWN) {
			ControlPanel.LaunchTextFeedback("MISSION MUST BE IN COOLDOWN!", Color.red);
			return;
		}

		// Skip all the remaining seconds
		m.DEBUG_SkipCooldownTimer((float)m.cooldownRemaining.TotalSeconds);

		// Notify game
		Messenger.Broadcast(MessengerEvents.DEBUG_REFRESH_MISSION_INFO);

		// Done!
		ControlPanel.LaunchTextFeedback("SUCCESS!", Color.green);
	}

    public void OnCompleteAndSkip() {
        OnCompleteMission();
        OnSkipCooldown();
    }
}