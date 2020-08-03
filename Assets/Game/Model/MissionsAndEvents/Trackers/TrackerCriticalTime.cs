// TrackerDiveTime.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for diving time.
/// </summary>
public class TrackerCriticalTime : TrackerBaseTime {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerCriticalTime() {
		// Subscribe to external events
		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerCriticalTime() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);

		// Call parent
		base.Clear();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Raises the health modifier changed event.
	/// </summary>
	/// <param name="_oldModifier">Old modifier.</param>
	/// <param name="_newModifier">New modifier.</param>
	private void OnHealthModifierChanged(DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier) {
		m_updateTime = (_newModifier != null && _newModifier.IsCritical());
		if (!m_updateTime)
			currentValue = 0;
	}
}