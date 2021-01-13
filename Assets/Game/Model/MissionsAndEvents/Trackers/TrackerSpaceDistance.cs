// TrackerSpaceDistance.cs
// Hungry Dragon
// 
// Created by Jose M. Olea
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
/// Tracker for diving distance.
/// </summary>
public class TrackerSpaceDistance : TrackerBaseDistance {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerSpaceDistance() {
		// Subscribe to external events
		Messenger.AddListener<bool>(MessengerEvents.INTOSPACE_TOGGLED, OnSpaceToggled);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerSpaceDistance() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool>(MessengerEvents.INTOSPACE_TOGGLED, OnSpaceToggled);

		// Call parent
		base.Clear();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The dragon has entered/exit water.
	/// </summary>
	/// <param name="_activated">Whether the dragon has entered or exited the water.</param>
	private void OnSpaceToggled(bool _activated) {
		// Update internal flag
		m_updateDistance = _activated;
		
		// Store initial dive position
		if (_activated) {
			UpdateLastPosition();
		}
	}
}