// MissionObjectiveScore.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/12/2015.
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
/// Score mission objective.
/// </summary>
[Serializable]
public class MissionObjectiveScore : MissionObjective {
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentMission">The mission this objective belongs to.</param>
	public MissionObjectiveScore(Mission _parentMission) : base(_parentMission) {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}

	/// <summary>
	/// Leave the objective ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);

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
		return Localization.Localize(parentMission.def.tidDesc, GetTargetValueFormatted());
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A reward has been applied.
	/// </summary>
	/// <param name="_reward">The reward.</param>
	/// <param name="_entity">The source entity, optional.</param>
	private void OnRewardApplied(Reward _reward, Transform _entity) {
		// We only care about score rewards
		if(_reward.score > 0) {
			currentValue += _reward.score;
		}
	}
}