// TrackerBurn.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for burned entities.
/// </summary>
public class TrackerBurn : TrackerBase {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	private List<string> m_targetSkus = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	/// <param name="_targetSkus">Skus of the target entities to be considered.</param>
	public TrackerBurn(List<string> _targetSkus) {
		// Store target Skus list
		m_targetSkus = _targetSkus;
		Debug.Assert(m_targetSkus != null);

		// Subscribe to external events
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnBurn);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBurn() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnBurn);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Localizes and formats the description according to this tracker's type
	/// (i.e. "Eat 52 birds", "Dive 500m", "Survive 10 minutes").
	/// </summary>
	/// <returns>The localized and formatted description for this tracker's type.</returns>
	/// <param name="_tid">Description TID to be formatted.</param>
	/// <param name="_targetValue">Target value.</param>
	override public string FormatDescription(string _tid, float _targetValue) {
		// Replace with the target amount and type
		string typeStr = "";

		// Any type
		if(m_targetSkus.Count == 0) {
			typeStr = LocalizationManager.SharedInstance.Localize("TID_MISSION_OBJECTIVE_ENEMIES");
		}

		// Next types
		for(int i = 0; i < m_targetSkus.Count; i++) {
			// Comma separated list except the first and last one in the list
			if(i > 0) {
				if(i == m_targetSkus.Count - 1) {
					typeStr += LocalizationManager.SharedInstance.Localize("TID_GEN_LIST_SEPARATOR_OR");
				} else {
					typeStr += LocalizationManager.SharedInstance.Localize("TID_GEN_LIST_SEPARATOR");
				}
			}

			// Add type name
			DefinitionNode targetDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.ENTITIES, m_targetSkus[0]);
			if(targetDef != null) {
				typeStr = targetDef.GetLocalized("tidName");
			} else {
				typeStr = m_targetSkus[i];
			}
		}

		// Compose full text and return
		return LocalizationManager.SharedInstance.Localize(_tid, FormatValue(_targetValue), typeStr);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// An entity has been burned.
	/// </summary>
	/// <param name="_entity">The source entity, optional.</param>
	/// <param name="_reward">The reward given.</param>
	private void OnBurn(Transform _entity, Reward _reward) {
		// Count automatically if we don't have any type filter
		if(m_targetSkus.Count == 0) {
			currentValue++;
		} else {
			// Is it one of the target types?
			Entity prey = _entity.GetComponent<Entity>();
			if(prey != null) {
				if(m_targetSkus.Contains(prey.sku)) {
					// Found!
					currentValue++;
				}
			}
		}
	}
}