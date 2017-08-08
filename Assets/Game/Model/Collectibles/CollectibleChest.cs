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
public class CollectibleChest : Collectible {
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
		// Open chest and launch FX
		// Figure out reward type to show the proper FX
		Chest.RewardData rewardData = ChestManager.GetRewardData(ChestManager.collectedAndPendingChests);
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
		Messenger.Broadcast<CollectibleChest>(GameEvents.CHEST_COLLECTED, this);
	}
}