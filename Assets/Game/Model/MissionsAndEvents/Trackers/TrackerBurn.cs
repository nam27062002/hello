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
			IEntity prey = _entity.GetComponent<IEntity>();
			if(prey != null) {
				if(m_targetSkus.Contains(prey.sku)) {
					// Found!
					currentValue++;
				}
			}
		}
	}
}