// TrackerBase.cs
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
/// Tracker for score.
/// </summary>
public class TrackerFireRush : TrackerBase {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerFireRush() {
		// Subscribe to external events
		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(MessengerEvents.FURY_RUSH_TOGGLED, OnFireRushToggled);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerFireRush() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(MessengerEvents.FURY_RUSH_TOGGLED, OnFireRushToggled);

		// Call parent
		base.Clear();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The fire rush has been toggled.
	/// </summary>
	/// <param name="_toggled">Whether it has been activated or deactivated.</param>
	/// <param name="_type">The type of fire rush (mega?).</param>
	private void OnFireRushToggled(bool _toggled, DragonBreathBehaviour.Type _type) {
		// If activated, increase current value
		if(_toggled) currentValue++;
	}
}