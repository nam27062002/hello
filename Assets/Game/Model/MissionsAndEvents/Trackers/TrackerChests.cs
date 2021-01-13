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
public class TrackerChests : TrackerBase {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerChests() {
		// Subscribe to external events
		Messenger.AddListener<CollectibleChest>(MessengerEvents.CHEST_COLLECTED, OnChestCollected);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerChests() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<CollectibleChest>(MessengerEvents.CHEST_COLLECTED, OnChestCollected);

		// Call parent
		base.Clear();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A chest has been collected.
	/// </summary>
	/// <param name="_chest">The collected chest.</param>
	private void OnChestCollected(CollectibleChest _chest) {
		// Just increase counter
		currentValue++;
	}
}