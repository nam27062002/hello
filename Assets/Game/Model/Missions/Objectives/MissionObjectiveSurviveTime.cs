// MissionObjectiveSurviveTime.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/12/2015.
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
/// Survive mission objective.
/// </summary>
[Serializable]
public class MissionObjectiveSurviveTime : MissionObjective {
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentMission">The mission this objective belongs to.</param>
	public MissionObjectiveSurviveTime(Mission _parentMission) : base(_parentMission) {
		// Register to the Update call from the manager
		MissionManager.OnUpdate += OnUpdate;
	}

	/// <summary>
	/// Leave the objective ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unegister frp, the Update call from the manager
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

	/// <summary>
	/// Gets the current value of this objective properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The current value properly formatted.</returns>
	override public string GetCurrentValueFormatted() {
		// Return as time
		return TimeUtils.FormatTime(currentValue, TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3, TimeUtils.EPrecision.DAYS);
	}
	
	/// <summary>
	/// Gets the target value of this objective properly formatted.
	/// Override to customize text in specific objective types.
	/// </summary>
	/// <returns>The target value properly formatted.</returns>
	override public string GetTargetValueFormatted() {
		return TimeUtils.FormatTime((double)targetValue, TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3, TimeUtils.EPrecision.DAYS);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnUpdate() {
		// Only if a game is running
		GameSceneController game = InstanceManager.gameSceneController;
		if(game != null && game.state == GameSceneController.EStates.RUNNING) {
			currentValue += Time.deltaTime;
		}
	}
}