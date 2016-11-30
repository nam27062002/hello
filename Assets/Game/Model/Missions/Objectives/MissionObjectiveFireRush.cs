// MissionObjectiveFireRush.cs
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
/// Fire rush mission objective. Trigger fire rush X times.
/// </summary>
[Serializable]
public class MissionObjectiveFireRush : MissionObjective {
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentMission">The mission this objective belongs to.</param>
	public MissionObjectiveFireRush(Mission _parentMission) : base(_parentMission) {
		// Subscribe to external events
		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFireRushToggled);
	}

	/// <summary>
	/// Leave the objective ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFireRushToggled);

		// Let parent do the rest
		base.Clear();
	}

	//------------------------------------------------------------------//
	// PARENT OVERRIDES													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets the description of this objective.
	/// Override to customize text in specific objectives.
	/// </summary>
	override public string GetDescription() {
		// Replace with the target amount
        return LocalizationManager.SharedInstance.Localize(GetDescriptionTID(), GetTargetValueFormatted());
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The fire rush has been toggled.
	/// </summary>
	/// <param name="_toggled">Whether it has been activated or deactivated.</param>
	/// <param name="_type">The type of fire rush (mega?).</param>
	private void OnFireRushToggled(bool _toggled, DragonBreathBehaviour.Type _type) {
		// If activated, increase current value
		if(_toggled) currentValue++;
	}
}