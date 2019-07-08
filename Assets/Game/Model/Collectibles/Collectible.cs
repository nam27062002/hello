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

	[SerializeField] protected DragonTier m_maxTier = DragonTier.COUNT;
	public DragonTier maxTier { get { return m_maxTier; }}

	[Space]
	[SerializeField] protected MapMarker m_mapMarker = null;

	[Space]
	[Comment("Optional:")]
	[SerializeField] protected GameObject m_view = null;
	[SerializeField] protected ParticleSystem m_idleFX = null;
	[SerializeField] protected ParticleSystem m_collectFX = null;

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
	}

	/// <summary>
	/// First update call.
	/// </summary>
	protected void Start() {
		// Start in the IDLE state
		// Don't do this on the Awake call, since it would disable the nested MapMarker before it's properly awaken, resulting in issue https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-573
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
		if(m_mapMarker != null) {
			m_mapMarker.showMarker = active;
		}

		// FX
		if(m_idleFX != null) {
			if(active) {
				m_idleFX.Play();
			} else {
				m_idleFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			m_idleFX.gameObject.SetActive(active);	// [AOC] There seems to be some kind of bug where the particles stay on screen. Disable the game object to be 100% sure they are not visible.
		}

		if(m_collectFX != null) {
			m_collectFX.gameObject.SetActive(_state == State.COLLECTED);
			m_collectFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			if(_state == State.COLLECTED) {
				m_collectFX.Play();
			}
		}

		// View (optional)
		if(m_view != null) {
			if(_state == State.COLLECTED) {
				// Hide view after some delay to sync with VFX
				UbiBCN.CoroutineManager.DelayedCall(() => { 
					m_view.SetActive(false);
				}, 0.05f);
			} else {
				m_view.SetActive(true);
			}
		}

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

		// Only if player is alive! and not in intro movement
		if(InstanceManager.player != null && (!InstanceManager.player.IsAlive() || InstanceManager.player.IsIntroMovement())) {
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