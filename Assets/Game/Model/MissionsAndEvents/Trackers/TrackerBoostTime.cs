// TrackerBoostTime.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 25/10/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for boosting time.
/// </summary>
public class TrackerBoostTime : TrackerBaseTime {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerBoostTime() {
		// Subscribe to external events
		Messenger.AddListener<bool>(MessengerEvents.BOOST_TOGGLED, OnBoostToggled);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBoostTime() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool>(MessengerEvents.BOOST_TOGGLED, OnBoostToggled);

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
	private void OnBoostToggled(bool _activated) {
		// Update internal flag
		m_updateTime = _activated;
	}
}