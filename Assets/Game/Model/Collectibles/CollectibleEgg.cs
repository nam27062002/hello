// CollectibleEgg.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/01/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// In-game collectible egg.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CollectibleEgg : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string TAG = "Egg";

	public enum State {
		INIT,		// Before the CollectibleEgg manager has selected a CollectibleEgg
		IDLE,		// Target CollectibleEgg, not yet collected
		COLLECTED	// Target CollectibleEgg, collected
	};

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed to inspector
	[SerializeField] private DragonTier m_requiredTier = DragonTier.TIER_0;
	public DragonTier requiredTier { get { return m_requiredTier; }}

	[Space]
	[SerializeField] private GameObject m_view = null;
	[SerializeField] private MapMarker m_mapMarker = null;

	[Space]
	[SerializeField] private ParticleSystem m_idleFX = null;
	[SerializeField] private ParticleSystem m_collectFX = null;

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
		m_idleFX.Play();
		m_collectFX.Stop();
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

		if (InstanceManager.player != null && !InstanceManager.player.IsAlive())
			return;

		// If inventory is full, don't collect
		if(EggManager.IsReady() && EggManager.isInventoryFull) {
			// Broadcast message to show some feedback
			Messenger.Broadcast<CollectibleEgg>(GameEvents.EGG_COLLECTED_FAIL, this);
			return;
		}

		// Collect CollectibleEgg!
		m_state = State.COLLECTED;

		// Disable collider
		GetComponent<Collider>().enabled = false;

		// Disable map marker
		if(m_mapMarker != null) m_mapMarker.showMarker = false;

		// Launch FX
		m_idleFX.Stop();
		m_idleFX.gameObject.SetActive(false);	// [AOC] There seems to be some kind of bug where the particles stay on screen. Disable the game object to be 100% sure they are not visible.
		m_collectFX.Play();

		// Disable view after a delay
		DOVirtual.DelayedCall(0.05f, HideAfterDelay, false);

		// Dispatch global event
		Messenger.Broadcast<CollectibleEgg>(GameEvents.EGG_COLLECTED, this);
	}

	/// <summary>
	/// Hide view after some delay.
	/// To be called via Invoke().
	/// </summary>
	private void HideAfterDelay() {
		// Hide the view
		if(m_view != null) m_view.gameObject.SetActive(false);

		// Let's move it down instead so it looks like debris
		/*if(m_view != null) {
			m_view.transform.Translate(0f, -1.15f, 0f, Space.World);	// [AOC] Magic Number >_<
		}*/
	}
}