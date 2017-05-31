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
[RequireComponent(typeof(Collider))]
public class CollectibleChest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string TAG = "Chest";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed to inspector
	[SerializeField] private DragonTier m_requiredTier = DragonTier.TIER_0;
	public DragonTier requiredTier { get { return m_requiredTier; }}

	[Space]
	[SerializeField] private MapMarker m_mapMarker = null;

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
	private void Awake() {
		// Make sure it belongs to the "Collectible" layer
		this.gameObject.layer = LayerMask.NameToLayer("Collectible");

		// Also make sure the object has the right tag
		this.gameObject.tag = TAG;

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
        if (m_chestData.collected) {
			SetCollected();
		}
        else
        {
            m_chestView.ShowGlowFX(true);
        }
	}

	/// <summary>
	/// Update object and visuals to match the "collected" state.
	/// </summary>
	private void SetCollected() {
		// Disable collider
		GetComponent<Collider>().enabled = false;

		// Disable map marker
		if(m_mapMarker != null) m_mapMarker.showMarker = false;

		// Open chest and launch FX
		// Figure out reward type to show the proper FX
		Chest.RewardData rewardData = ChestManager.GetRewardData(ChestManager.collectedAndPendingChests);
//		m_chestView.ShowGlowFX(false);
		m_chestView.Open(rewardData.type, false);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Another collider has collided with this collider.
	/// </summary>
	/// <param name="_other">The other collider.</param>
	private void OnTriggerEnter(Collider _other) {
		// Since it belongs to the "Collectible" layer, only the player will collide with it - no need to check
		// If already collected, skip
		if(m_chestData.collected) return;

		if (InstanceManager.player != null && !InstanceManager.player.IsAlive())
			return;

		// Change chest state
		m_chestData.ChangeState(Chest.State.PENDING_REWARD);

		// Apply collected visuals
		SetCollected();

		// Dispatch global event
		Messenger.Broadcast<CollectibleChest>(GameEvents.CHEST_COLLECTED, this);
	}
}