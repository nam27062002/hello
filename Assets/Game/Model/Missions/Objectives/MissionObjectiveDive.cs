// MissionObjectiveDive.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Underwater mission objective. Swim X meters.
/// </summary>
[Serializable]
public class MissionObjectiveDive : MissionObjective {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Internal
	private bool m_diving = false;
	private Vector3 m_lastPosition = Vector3.zero;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentMission">The mission this objective belongs to.</param>
	public MissionObjectiveDive(Mission _parentMission) : base(_parentMission) {
		// Subscribe to external events
		Messenger.AddListener<bool>(GameEvents.UNDERWATER_TOGGLED, OnUnderwaterToggled);

		// Register to the Update call from the manager
		MissionManager.OnUpdate += OnUpdate;
	}

	/// <summary>
	/// Leave the objective ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Subscribe to external events
		Messenger.RemoveListener<bool>(GameEvents.UNDERWATER_TOGGLED, OnUnderwaterToggled);

		// Unregister from the Update call from the manager
		MissionManager.OnUpdate -= OnUpdate;

		// Let parent do the rest
		base.Clear();
	}

	//------------------------------------------------------------------//
	// PARENT OVERRIDES													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets the description of this objective properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The description properly formatted.</returns>
	override public string GetDescription() {
		// Replace with the target amount
		return LocalizationManager.SharedInstance.Localize(GetDescriptionTID(), GetTargetValueFormatted());
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The dragon has entered/exit water.
	/// </summary>
	/// <param name="_activated">Whether the dragon has entered or exited the water.</param>
	private void OnUnderwaterToggled(bool _activated) {
		// Update internal flag
		m_diving = _activated;

		// Store initial dive position
		if(_activated) {
			m_lastPosition = InstanceManager.player.transform.position;
		}
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnUpdate() {
		// Only if a game is running
		GameSceneController game = InstanceManager.gameSceneController;
		if(game != null && game.state == GameSceneController.EStates.RUNNING) {
			// Is the dragon underwater?
			if(m_diving) {
				currentValue += (InstanceManager.player.transform.position - m_lastPosition).magnitude;
			}
		}
	}

	/// <summary>
	/// A new game has started.
	/// </summary>
	override public void OnGameStarted() {
		// Call parent
		base.OnGameStarted();

		// Reset flag
		m_diving = false;
	}
}