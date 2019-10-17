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
public class TrackerKill : TrackerBase {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	private List<string> m_targetSkus = null;
    private List<string> m_zoneTriggers = null;
    private bool m_enteredTargetZone = false;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="_targetSkus">Skus of the target entities to be considered.</param>
    /// <param name="_zones">Ids of the targeted zones. If empty, there is no zone restriction.</param>
    public TrackerKill(List<string> _targetSkus, string _zoneSku = null) {
		// Store target Skus list
		m_targetSkus = _targetSkus;
		Debug.Assert(m_targetSkus != null);

        // Zones parameter is optional
        if (!string.IsNullOrEmpty(_zoneSku)) 
        {
            // Get all the triggers of this zone
            m_zoneTriggers = GetZoneTriggers(_zoneSku);

            if (m_zoneTriggers != null && m_zoneTriggers.Count > 0)
            {
                Messenger.AddListener<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, OnZone);
            }
            
        }
        

		// Subscribe to external events
		Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_EATEN, OnKill);
		Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, OnKill);
		Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_DESTROYED, OnKill);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerKill() {
		
	}



    //------------------------------------------------------------------------//
    // PARENT OVERRIDES														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Finalizer method. Leave the tracker ready for garbage collection.
    /// </summary>
    override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_EATEN, OnKill);
		Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, OnKill);
		Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_DESTROYED, OnKill);

        if (m_zoneTriggers != null && m_zoneTriggers.Count > 0)
        {
            Messenger.AddListener<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, OnZone);
        }

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
	/// <param name="_e">The source entity, optional.</param>
	/// <param name="_reward">The reward given.</param>
	private void OnKill(Transform _t, IEntity _e, Reward _reward) {
		// Count automatically if we don't have any type filter
		if(m_targetSkus.Count == 0) {
			currentValue++;
		} else {
			// Is it one of the target types?
			if(_e != null) {
				if(m_targetSkus.Contains(_e.sku)) {
                    // Do we need to check the zone?
                    if (m_zoneTriggers != null && m_zoneTriggers.Count>0)
                    {
                        // Restricted to a target zone
                        if (m_enteredTargetZone)
                        {
                            currentValue++;
                        }

                    }else
                    {
                        // Not restricted to zones
                        currentValue++;
                    }
					
				}
			}
		}
	}

    /// <summary>
    /// Call this event when the player is moving from/to another zone
    /// </summary>
    private void OnZone(bool isEntering, ZoneTrigger zone)
    {
        if (m_zoneTriggers.Contains(zone.m_zoneId))
        {
            m_enteredTargetZone = isEntering;
        }
    }
}