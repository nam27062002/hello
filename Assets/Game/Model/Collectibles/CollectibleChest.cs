// Chest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/01/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Single Chest object.
/// </summary>
public class CollectibleChest : Collectible, IBroadcastListener {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string TAG = "Chest";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Internal
	private ChestViewController m_chestView = null;   

	// Logic
	private Chest m_chestData = null;
	public Chest chestData {
		get { return m_chestData; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

		// Store view
		m_chestView = this.gameObject.GetComponentInChildren<ChestViewController>();

		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_ENDED:
            {
                OnGameEnded();
            }break;
        }
    }

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize with the given chest data.
	/// </summary>
	/// <param name="_chest">The data to be used.</param>
	public void Initialize(Chest _chest) {
		// Store chest data
		m_chestData = _chest;
        
        // If already collected, update visuals
        if(m_chestData.collected) {
			SetState(State.COLLECTED);
			SetCollectedVisuals();
		} else {
            m_chestView.ShowGlowFX(true);
        }
	}

	/// <summary>
	/// Update object and visuals to match the "collected" state.
	/// </summary>
	private void SetCollectedVisuals() {
		// Figure out reward type to show the proper FX
		Chest.RewardData rewardData = ChestManager.GetRewardData(m_chestData.collectionOrder);

		// [AOC] Protection just in case (mainly for the initial integration of the collectionOrder feature)
		// 		 The only consequence is that a chest previously collected that gave gems could be displayed as gold, but just until chests are reset again (24h).
		if(rewardData == null) {
			rewardData = ChestManager.GetRewardData(1);
		}

		// Open chest and launch FX
		m_chestView.Open(rewardData.type, false);
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the unique tag identifying collectible objects of this type.
	/// </summary>
	/// <returns>The tag.</returns>
	override public string GetTag() {
		return TAG;
	}

	//------------------------------------------------------------------------//
	// VIRTUAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Override to check additional conditions when attempting to collect.
	/// </summary>
	/// <returns><c>true</c> if this collectible can be collected, <c>false</c> otherwise.</returns>
	override protected bool CanBeCollected() {
		// Skip if already collected
		if(m_chestData.collected) return false;

		return true;
	}

	/// <summary>
	/// Override to perform additional actions when collected.
	/// </summary>
	override protected void OnCollect() {
		// Change chest state
		m_chestData.ChangeState(Chest.State.PENDING_REWARD);

		// Apply collected visuals
		SetCollectedVisuals();

		// Dispatch global event
		Messenger.Broadcast<CollectibleChest>(MessengerEvents.CHEST_COLLECTED, this);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Game has ended.
	/// </summary>
	private void OnGameEnded() {
		// Close chest (to return particles to their pools)
		m_chestView.Close();
	}
}