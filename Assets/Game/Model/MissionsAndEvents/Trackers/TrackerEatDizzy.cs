// TrackerKill.cs
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
/// Tracker for killed entities.
/// </summary>
public class TrackerEatDizzy : TrackerBase {
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
	public TrackerEatDizzy(List<string> _targetSkus) {
		// Store target Skus list
		m_targetSkus = _targetSkus;
		Debug.Assert(m_targetSkus != null);

		// Subscribe to external events
		Messenger.AddListener<Transform, Reward>(MessengerEvents.ENTITY_EATEN, OnKill);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerEatDizzy() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Transform, Reward>(MessengerEvents.ENTITY_EATEN, OnKill);

		// Call parent
		base.Clear();
	}

	/// <summary>
	/// Round a value according to specific rules defined for every tracker type.
	/// Typically used for target values.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="_targetValue">The original value to be rounded.</param>
	override public float RoundTargetValue(float _targetValue) {
		// Round to multiples of 10, except values smaller than 100
		if(_targetValue > 100f) {
			_targetValue = MathUtils.Snap(_targetValue, 10f);
		}

		// Apply default filter
		return base.RoundTargetValue(_targetValue);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// An entity has been killed.
	/// </summary>
	/// <param name="_entity">The source entity, optional.</param>
	/// <param name="_reward">The reward given.</param>
	private void OnKill(Transform _entity, Reward _reward) {
		if ( InstanceManager.player.IsDrunk() ){
			IEntity prey = _entity.GetComponent<IEntity>();
			if (prey != null && prey.onDieStatus.source == IEntity.Type.PLAYER){
				// Count automatically if we don't have any type filter
				if(m_targetSkus.Count == 0) {
					currentValue++;
				} else {
					// Is it one of the target types?
					if(m_targetSkus.Contains(prey.sku)) {
						// Found!
						currentValue++;
					}
				}
			}
		}
	}


}