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
            typeStr = LocalizationManager.SharedInstance.Localize("TID_MISSION_OBJECTIVE_GENERIC_ENEMIES");
		}

		// Next types
		for(int i = 0; i < m_targets.Length; i++) {
			// Comma separated list except the first and last one in the list
			if(i > 0) {
				if(i == m_targets.Length - 1) {
                    typeStr += LocalizationManager.SharedInstance.Localize("TID_GEN_LIST_SEPARATOR_OR");
				} else {
                    typeStr += LocalizationManager.SharedInstance.Localize("TID_GEN_LIST_SEPARATOR");
				}
			}

			// Add type name
			DefinitionNode targetDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.ENTITIES, m_targets[0]);
			if(targetDef != null) {
				typeStr = targetDef.GetLocalized("tidName");
			} else {
				typeStr = m_targets[i];
			}
		}

		// Compose full text and return
        return LocalizationManager.SharedInstance.Localize(GetDescriptionTID(), GetTargetValueFormatted(), typeStr);
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
			Entity_Old prey = _entity.GetComponent<Entity_Old>();
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