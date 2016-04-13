// HUDMissionCompleted.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/12/2015.
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
/// Simple controller for a mission completed feedback in the hud.
/// </summary>
[RequireComponent(typeof(Animator))]
public class HUDMissionCompleted : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[SerializeField] Text m_missionObjectiveText = null;
	private Animator m_anim;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_anim = GetComponent<Animator>();
	}

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Mission>(GameEvents.MISSION_COMPLETED, OnMissionCompleted);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Mission>(GameEvents.MISSION_COMPLETED, OnMissionCompleted);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A mission has been completed.
	/// </summary>
	/// <param name="_mission">The mission that has been completed.</param>
	private void OnMissionCompleted(Mission _mission) {
		// Init text!
		m_missionObjectiveText.text = _mission.objective.GetDescription();

		// Play the anim!
		m_anim.SetTrigger("start");
	}
}
