// TrackerChests.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/05/2017.
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
/// Tracker for collected chests.
/// </summary>
public class TrackerUnlockDragon : TrackerBase {
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
	public TrackerUnlockDragon(List<string> _targetSkus) {
		m_targetSkus = _targetSkus;
		Debug.Assert(m_targetSkus != null);

		// Subscribe to external events
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquire);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerUnlockDragon() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquire);

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
	private void OnDragonAcquire(IDragonData _dragon) {
		if(m_targetSkus.Contains(_dragon.def.sku)) {
			// Found!
			currentValue++;
		}
	}
    
     /// <summary>
    /// Refreshs the current value. This function will be called on the achievements that need to check a specific value on the profile
    /// Used for example in checking unlocking dragons and number of skins because we cannot unlock a dragon again
    /// </summary>
    public override void RefreshCurrentValue(){
        if ( UsersManager.currentUser != null ) {
            int val = 0;
            int max = m_targetSkus.Count;
            UserProfile profile = UsersManager.currentUser;
            for (int i = 0; i < max; i++)
            {
                if ( profile.dragonsBySku.ContainsKey( m_targetSkus[i] ) && profile.dragonsBySku[ m_targetSkus[i] ].isOwned )
                {
                    val++;
                }
            }
            currentValue = val;
        }
    }
    
}