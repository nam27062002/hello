// MissionObjectiveKill.cs
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
/// Kill mission objective.
/// </summary>
[Serializable]
public class MissionObjectiveKill : MissionObjective {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentMission">The mission this objective belongs to.</param>
	public MissionObjectiveKill(Mission _parentMission) : base(_parentMission) {
		// Subscribe to external events
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnKill);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnKill);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnKill);
	}

	/// <summary>
	/// Leave the objective ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnKill);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnKill);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnKill);

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
		// Replace with the target amount and type
		string[] parameters = parentMission.def.parameters;
		string typeStr = "";

		// Any type
		if(parameters.Length == 0) {
			typeStr = Localization.Localize("enemies");
		}

		// First type
		if(parameters.Length > 0) {
			typeStr = parameters[0];

		}

		// Next types
		for(int i = 1; i < parameters.Length; i++) {
			// Comma separated list except the last one in the list
			if(i == parameters.Length - 1) {
				typeStr += Localization.Localize(" or ");
			} else {
				typeStr += Localization.Localize(", ");
			}

			// Attach name
			// [AOC] TODO!! Use TIDs!!
			typeStr += parameters[i];
		}

		// Compose full text and return
		return Localization.Localize(parentMission.def.tidDesc, GetTargetValueFormatted(), typeStr);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A reward has been applied.
	/// </summary>
	/// <param name="_entity">The source entity, optional.</param>
	/// <param name="_reward">The reward.</param>
	private void OnKill(Transform _entity, Reward _reward) {
		// Count automatically if we don't have any type filter
		if(parentMission.def.parameters.Length == 0) {
			currentValue++;
		} else {
			// Is it one of the target types?
			PreyStats prey = _entity.GetComponent<PreyStats>();
			if(prey != null) {
				for(int i = 0; i < parentMission.def.parameters.Length; i++) {
					if(parentMission.def.parameters[i] == prey.typeID) {
						// Found!
						currentValue++;
						break;
					}
				}
			}
		}
	}
}