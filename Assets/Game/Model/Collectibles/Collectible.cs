// Collectible.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class for an in-game collectible item.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class Collectible : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum State {
		INIT,		// Before the CollectiblesManager has selected a Collectible of this type
		IDLE,		// Target Collectible, not yet collected
		COLLECTED	// Target Collectible, collected
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed to inspector
	[SerializeField] protected DragonTier m_requiredTier = DragonTier.TIER_0;
	public DragonTier requiredTier { get { return m_requiredTier; }}

	[Space]
	[SerializeField] protected MapMarker m_mapMarker = null;

	// Internal logic
	protected State m_state = State.INIT;
	public State state { get { return m_state; }}
	public bool collected { get { return m_state == State.COLLECTED; }}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	virtual protected void Awake() {
		// Make sure it belongs to the "Collectible" layer
		this.gameObject.layer = LayerMask.NameToLayer("Collectible");

		// Also make sure the object has the right tag
		this.gameObject.tag = GetTag();

		// Start in the IDLE state
		SetState(State.IDLE);
	}

	/// <summary>
	/// Change the state to a target one.
	/// Will only update visuals and internal vars, won't perform any check nor dispatch any event.
	/// </summary>
	/// <param name="_state">New state of the collectible.</param>
	public void SetState(State _state) {
		// Update visuals based on new state
		bool active = _state == State.IDLE;

		// Collider
		GetComponent<Collider>().enabled = active;

		// Map marker
		if(m_mapMarker != null) m_mapMarker.showMarker = active;

		// Save new state
		m_state = _state;
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the unique tag identifying collectible objects of this type.
	/// </summary>
	/// <returns>The tag.</returns>
	public abstract string GetTag();

	//------------------------------------------------------------------------//
	// VIRTUAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Override to check additional conditions when attempting to collect.
	/// </summary>
	/// <returns><c>true</c> if this collectible can be collected, <c>false</c> otherwise.</returns>
	protected virtual bool CanBeCollected() {
		return true;
	}

	/// <summary>
	/// Override to perform additional actions when collected.
	/// </summary>
	protected virtual void OnCollect() {
		// To be implemented by heirs if needed
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Another collider has collided with this collider.
	/// </summary>
	/// <param name="_other">The other collider.</param>
	private void OnTriggerEnter(Collider _other) {
		// Since it belongs to the "Collectible" layer, only the player will collide with it - no need to check
		// If already collected, skip
		if(m_state == State.COLLECTED) return;

		// Only if player is alive!
		if(InstanceManager.player != null && !InstanceManager.player.IsAlive()) {
			return;
		}

		// Check specific conditions for each type
		if(!CanBeCollected()) return;

		// Update visuals
		SetState(State.COLLECTED);

		// Notify heirs
		OnCollect();
	}
}