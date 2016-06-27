// MissionButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Button for the mission selector in the level selection menu.
/// </summary>
public class MissionButton : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[Separator]
	[HideEnumValues(false, true)]
	[SerializeField] private Mission.Difficulty m_missionDifficulty = Mission.Difficulty.EASY;

	// References - keep references to objects that are often accessed
	[Separator]
	[SerializeField] private Image m_missionTypeIcon = null;
	[SerializeField] private Image m_lockIcon = null;
	[SerializeField] private Text m_cooldownText = null;
	[SerializeField] private Slider m_cooldownBar = null;

	// Data
	private Mission m_mission = null;
	public Mission mission {
		get {
			if(m_mission == null) {
				m_mission = MissionManager.GetMission(m_missionDifficulty);
			}
			return m_mission;
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Check required references
		Debug.Assert(m_lockIcon != null, "Required reference!");
		Debug.Assert(m_cooldownText != null, "Required reference!");
		Debug.Assert(m_cooldownBar != null, "Required reference!");

		// Subscribe to external events
		Messenger.AddListener<Mission>(GameEvents.MISSION_REMOVED, OnMissionRemoved);
		Messenger.AddListener<Mission, Mission.State, Mission.State>(GameEvents.MISSION_STATE_CHANGED, OnMissionStateChanged);
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Mission>(GameEvents.MISSION_REMOVED, OnMissionRemoved);
		Messenger.RemoveListener<Mission, Mission.State, Mission.State>(GameEvents.MISSION_STATE_CHANGED, OnMissionStateChanged);
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure we're up to date
		Refresh();
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update time-dependant fields
		if(mission != null && mission.state == Mission.State.COOLDOWN) {
			RefreshCooldownTimers();
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Update the pill with the data from the target mission.
	/// </summary>
	public void Refresh() {
		// Make sure mission is valid
		if(mission == null) return;

		// Mission type icon
		if(m_missionTypeIcon != null) {
			// Special icon when mission is not active
			string iconPath = "";
			if(m_mission.state != Mission.State.ACTIVE) {
				DefinitionNode unknownTypeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, "unknown");
				iconPath = unknownTypeDef.GetAsString("icon");
			} else {
				iconPath = m_mission.typeDef.GetAsString("icon");
			}
			m_missionTypeIcon.sprite = Resources.Load<Sprite>(iconPath);
		}

		// Show lock icon?
		if(m_lockIcon != null) m_lockIcon.gameObject.SetActive(m_mission.state == Mission.State.LOCKED);

		// Show cooldown timer and bar
		// [AOC] Text is child of the bar, so we only need to set the active state of the bar
		if(m_cooldownBar != null) m_cooldownBar.gameObject.SetActive(m_mission.state == Mission.State.COOLDOWN || m_mission.state == Mission.State.ACTIVATION_PENDING);
		RefreshCooldownTimers();
	}

	/// <summary>
	/// Refresh the timers part of the cooldown. Optimized to be called every frame.
	/// </summary>
	private void RefreshCooldownTimers() {
		// Since cooldown must be refreshed every frame, keep the reference to the objects rather than finding them every time
		// Cooldown remaining time
		if(m_cooldownText != null) m_cooldownText.text = TimeUtils.FormatTime(mission.cooldownRemaining.TotalSeconds, TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3);

		// Cooldown bar
		if(m_cooldownBar != null) m_cooldownBar.normalizedValue = mission.cooldownProgress;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A mission has been removed. If it matches the difficulty of this pill, refresh 
	/// with the new mission data.
	/// </summary>
	/// <param name="_newMission">The new mission replacing the one removed.</param>
	private void OnMissionRemoved(Mission _newMission) {
		// Is it this mission?
		if(_newMission.difficulty == m_missionDifficulty) {
			m_mission = _newMission;
			Refresh();
		}
	}

	/// <summary>
	/// A mission state has changed. If it matches the difficulty of this pill,
	/// refresh visuals.
	/// </summary>
	/// <param name="_mission">The mission that has finished its cooldown.</param>
	/// <param name="_oldState">The previous state of the mission.</param>
	/// <param name="_newState">The new state of the mission.</param>
	private void OnMissionStateChanged(Mission _mission, Mission.State _oldState, Mission.State _newState) {
		// Is it this mission?
		if(m_mission == _mission) {
			Refresh();
		}
	}
}
