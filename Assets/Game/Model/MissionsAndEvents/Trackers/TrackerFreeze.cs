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
/// Tracker for entities killed by the ice dragon breath.
/// </summary>
public class TrackerFreeze : TrackerBase {
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
	public TrackerFreeze(List<string> _targetSkus) {
		// Store target Skus list
		m_targetSkus = _targetSkus;
		Debug.Assert(m_targetSkus != null);

		// Subscribe to external events
		Messenger.AddListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnKill);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerFreeze() {

	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnKill);

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
	/// An entity has been burned.
	/// </summary>
	/// <param name="_entity">The source entity, optional.</param>
	/// <param name="_reward">The reward given.</param>
	private void OnKill(Transform _t, IEntity _e, Reward _reward, KillType _type) {

        if (_type == KillType.FROZEN)
        {
            // Count automatically if we don't have any type filter
            if (m_targetSkus.Count == 0)
            {
                currentValue++;
            }
            else
            {
                // Is it one of the target types?			
                if (_e != null)
                {
                    if (m_targetSkus.Contains(_e.sku))
                    {
                        // Found!
                        currentValue++;
                    }
                }
            }
        }
	}
}