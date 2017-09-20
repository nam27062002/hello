// TrackerChests.cs
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
/// Tracker for collected chests.
/// </summary>
public class TrackerBuySkins : TrackerBase {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerBuySkins() {
		// Subscribe to external events
		//Messenger.AddListener<Chest>(GameEvents.CHEST_COLLECTED, OnChestCollected);
		Messenger.AddListener<string>(GameEvents.SKIN_ACQUIRED, OnSkinAcquired);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBuySkins() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.SKIN_ACQUIRED, OnSkinAcquired);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A chest has been collected.
	/// </summary>
	/// <param name="_chest">The collected chest.</param>
	private void OnSkinAcquired(string _skinSku) {
		// Just increase counter
		currentValue++;
	}
}