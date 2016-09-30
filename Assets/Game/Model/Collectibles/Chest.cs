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
public class Chest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string TAG = "Chest";

	public enum State {
		INIT,		// Before the chest manager has selected a chest
		IDLE,		// Target chest, not yet collected
		COLLECTED	// Target chest, collected
	};

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed to inspector
	[SerializeField] private DragonTier m_requiredTier = DragonTier.TIER_0;
	public DragonTier requiredTier { get { return m_requiredTier; }}

	[Space]
	[SerializeField] private ChestViewController m_chestView = null;
	[SerializeField] private GameObject m_mapMarker = null;

	// Logic
	private State m_state = State.INIT;
	public State state { get { return m_state; }}
	public bool collected { get { return m_state == State.COLLECTED; }}

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

		// Start in the IDLE state
		m_state = State.IDLE;
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Setup view
		m_chestView.ShowGlowFX(true);
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
		if(m_state == State.COLLECTED) return;

		// Collect chest!
		m_state = State.COLLECTED;

		// Disable collider
		GetComponent<Collider>().enabled = false;

		// Disable map marker
		if(m_mapMarker != null) m_mapMarker.SetActive(false);

		// Open chest and launch FX
		m_chestView.ShowGlowFX(false);
		m_chestView.Open();

		// Dispatch global event
		Messenger.Broadcast<Chest>(GameEvents.CHEST_COLLECTED, this);
	}
}