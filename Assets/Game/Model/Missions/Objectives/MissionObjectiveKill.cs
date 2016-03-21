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
	private string[] m_targets;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentMission">The mission this objective belongs to.</param>
	public MissionObjectiveKill(Mission _parentMission) : base(_parentMission) {
		// Store targets
		m_targets = parentMission.def.GetAsArray<string>("parameters");

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
		string typeStr = "";

		// Any type
		if(m_targets.Length == 0) {
			typeStr = Localization.Localize("enemies");	// [AOC] HARDCODED!!
		}

		// First type
		if(m_targets.Length > 0) {
			typeStr = m_targets[0];

		}

		// Next types
		for(int i = 1; i < m_targets.Length; i++) {
			// Comma separated list except the last one in the list
			if(i == m_targets.Length - 1) {
				typeStr += Localization.Localize(" or ");	// [AOC] HARDCODED!!
			} else {
				typeStr += Localization.Localize(", ");	// [AOC] HARDCODED!!
			}

			// Attach name
			// [AOC] TODO!! Use TIDs!!
			typeStr += m_targets[i];
		}

		// Compose full text and return
		return Localization.Localize(GetDescriptionTID(), GetTargetValueFormatted(), typeStr);
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
		if(m_targets.Length == 0) {
			currentValue++;
		} else {
			// Is it one of the target types?
			Entity prey = _entity.GetComponent<Entity>();
			if(prey != null) {
				for(int i = 0; i < m_targets.Length; i++) {
					if(m_targets[i] == prey.sku) {
						// Found!
						currentValue++;
						break;
					}
				}
			}
		}
	}
}