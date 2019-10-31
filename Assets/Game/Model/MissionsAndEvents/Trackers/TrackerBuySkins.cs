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
		Messenger.AddListener<string>(MessengerEvents.SKIN_ACQUIRED, OnSkinAcquired);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBuySkins() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.SKIN_ACQUIRED, OnSkinAcquired);

		// Call parent
		base.Clear();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
    /// On skin acquired.
    /// </summary>
    /// <param name="_skinSku">Skin sku.</param>
	private void OnSkinAcquired(string _skinSku) {
		// Just increase counter
		currentValue++;
	}
    
    
    public override void RefreshCurrentValue(){
        if ( UsersManager.currentUser != null ) {
            currentValue = UsersManager.currentUser.wardrobe.GetNumAcquiredSkins();
        }
    }
    
}