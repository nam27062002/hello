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
public class TrackerEatWhileActionActive : TrackerBase {

	public enum Actions {
		FreeFall = 0,
		PilotActionA,
		PilotActionB,
		PilotActionC
	}

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	private Actions m_action;
	private List<string> m_targetSkus = null;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	/// <param name="_targetSkus">Skus of the target entities to be considered.</param>
	public TrackerEatWhileActionActive(Actions _action, List<string> _targetSkus) {
		m_action = _action;

		// Store target Skus list
		m_targetSkus = _targetSkus;
		Debug.Assert(m_targetSkus != null);

		// Subscribe to external events
		Messenger.AddListener<IEntity, Reward>(MessengerEvents.ENTITY_EATEN, OnKill);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerEatWhileActionActive() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<IEntity, Reward>(MessengerEvents.ENTITY_EATEN, OnKill);

		// Call parent
		base.Clear();
	}

	/// <summary>
	/// Round a value according to specific rules defined for every tracker type.
	/// Typically used for target values.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="_targetValue">The original value to be rounded.</param>
	override public long RoundTargetValue(long _targetValue) {
		// Round to multiples of 10, except values smaller than 100
		if(_targetValue > 100) {
			_targetValue = MathUtils.Snap(_targetValue, 10);
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
		//
		bool ok = false;
		IEntity prey = _entity.GetComponent<IEntity>();

		switch (m_action) {
			case Actions.FreeFall: 		ok = prey.onDieStatus.isInFreeFall;			break;
			case Actions.PilotActionA:	ok = prey.onDieStatus.isPressed_ActionA;	break;
			case Actions.PilotActionB:	ok = prey.onDieStatus.isPressed_ActionB;	break;
			case Actions.PilotActionC:	ok = prey.onDieStatus.isPressed_ActionC; 	break;
		}

		if (ok) {
			// Count automatically if we don't have any type filter
			if(m_targetSkus.Count == 0) {
				currentValue++;
			} else {
				if(prey != null) {
					if(m_targetSkus.Contains(prey.sku)) {
						// Found!
						currentValue++;
					}
				}
			}
		}
	}
}