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
public class Chest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum State {
		INIT,		// Before the chest manager has selected a chest
		IDLE,		// Target chest, not yet collected
		COLLECTED	// Target chest, collected
	};

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed to inspector
	[SerializeField] private float m_collisionRadius = 1f;

	// Logic
	private State m_state = State.INIT;
	public State state { get { return m_state; }}
	public bool collected { get { return m_state == State.COLLECTED; }}

	// Internal
	private float m_sqrCollisionRadius = 1f;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Pre-compute squared collision distance to avoid doing it every time
		m_sqrCollisionRadius = m_collisionRadius * m_collisionRadius;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Detect collision with player
		if(InstanceManager.player.IsAlive() && !collected) {
			// Based on MineBehaviour
			Vector2 v = (InstanceManager.player.transform.position - transform.position);
			float distanceSqr = v.sqrMagnitude;
			if(distanceSqr <= m_sqrCollisionRadius) {
				// Collect chest!
				m_state = State.COLLECTED;

				// Launch FX
				// [AOC] TODO!! For now let's disable the object after a short delay
				ParticleManager.Spawn("SmokePuff", transform.position);
				Invoke("OnCollectFXEnded", 0.25f);

				// Dispatch global event
				Messenger.Broadcast<Chest>(GameEvents.CHEST_COLLECTED, this);
			}
		}
	}

	//------------------------------------------------------------------//
	// EDITOR															//
	//------------------------------------------------------------------//
	/// <summary>
	/// A value has changed on the inspector.
	/// </summary>
	private void OnValidate() {
		// Re-compute squared collision distance
		m_sqrCollisionRadius = m_collisionRadius * m_collisionRadius;
	}

	/// <summary>
	/// Draw scene helpers.
	/// </summary>
	private void OnDrawGizmos() {
		Gizmos.color = Colors.WithAlpha(Colors.red, 0.5f);
		Gizmos.DrawWireSphere(transform.position, m_collisionRadius);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The collect FX has ended
	/// </summary>
	private void OnCollectFXEnded() {
		// Disable object
		gameObject.SetActive(false);
	}
}