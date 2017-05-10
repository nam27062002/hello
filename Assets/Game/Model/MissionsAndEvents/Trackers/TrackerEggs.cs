// TrackerEggs.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for collected eggs.
/// </summary>
public class TrackerEggs : TrackerBase {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerEggs() {
		// Subscribe to external events
		Messenger.AddListener<CollectibleEgg>(GameEvents.EGG_COLLECTED, OnEggCollected);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerEggs() {
		// Unsubscribe from external events
		Messenger.RemoveListener<CollectibleEgg>(GameEvents.EGG_COLLECTED, OnEggCollected);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// An egg has been collected.
	/// </summary>
	/// <param name="_egg">The collected egg.</param>
	private void OnEggCollected(CollectibleEgg _egg) {
		// Just increase counter
		currentValue++;
	}
}