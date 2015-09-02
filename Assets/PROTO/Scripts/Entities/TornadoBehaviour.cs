// TornadoBehaviour.cs
// Furious Dragon
// 
// Created by Alger Ortín Castellví on 29/07/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Experimental tornado code.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TornadoBehaviour : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	[Header("Visuals")]
	public GameObject view = null;
	public float rotationSpeed = 10f;
	public float actionRadius = 200f;
	public float moveSpeed = 10f;

	[Header("Interaction")]
	public float impactDamage = 20f;
	public float impactIntensity = 100000f;
	public Range impactAngle = new Range(-30f, 45f);

	// Properties
	private DragonPlayer player {
		get { return App.Instance.gameLogic.player; }
	}

	// References
	BoxCollider col = null;

	// Internal
	Range moveLimits = new Range(-200f, 200f);
	float velocityX = 10f;
	float angle = 0f;
	float timeSinceLastCollision = float.MaxValue;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Check required members
		DebugUtils.Assert(view != null, "Required member!");

		col = GetComponent<BoxCollider>();
		DebugUtils.Assert(col != null, "Required member!");
	}

	/// <summary>
	/// The object has been enabled.
	/// </summary>
	void OnEnable() {
		// Compute move limits based on spawn position
		moveLimits.min = transform.position.x - actionRadius;
		moveLimits.max = transform.position.x + actionRadius;
		velocityX = (Random.Range(0, 2) * 2 - 1) * moveSpeed;
	}

	/// <summary>
	/// First update.
	/// </summary>
	void Start() {
		// Reset some values
		timeSinceLastCollision = float.MaxValue;
	}

	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		// Keep rollin' rollin' rollin'
		angle += rotationSpeed;
		if(angle > 360f) angle -= 360f;
		view.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.up);

		// Move around
		Vector3 pos = transform.position;
		pos.x += velocityX;

		// Adjust to ground
		// [AOC] A little bit hacky, and I have serious doubts about performance
		RaycastHit ground;
		if(Physics.Linecast(pos + Vector3.up * 10f, pos + Vector3.down * 10000f, out ground, 1 << LayerMask.NameToLayer("Ground"))) {
			pos.y = ground.point.y;
		}

		// Apply new position
		transform.position = pos;

		// Change direction if a limit has been reached
		if(pos.x < moveLimits.min || pos.x > moveLimits.max) velocityX *= -1;

		// Update collision timer
		timeSinceLastCollision += Time.deltaTime;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Someone has collided with us!
	/// </summary>
	/// <param name="_collision">The collision that triggered the event.</param>
	void OnCollisionEnter(Collision _collision) {
		// Is it the player?
		if(_collision != null && _collision.collider.GetComponent<DragonPlayer>() != null) {
			// Find out collision point
			Vector3 p = transform.position;
			if(_collision.contacts.Length > 0) {
				p = _collision.contacts[0].point;
			}

			// Apply impact to dragon
			// [AOC] Proportional to velocity
			player.OnImpact(p, impactDamage, impactIntensity * _collision.relativeVelocity.magnitude, null);
		}
	}

	/// <summary>
	/// Someone has entered our collider!
	/// </summary>
	/// <param name="_target">The object that has invaded our personal space.</param>
	void OnTriggerEnter(Collider _target) {
		// Is it the player?
		if(_target != null && _target.GetComponent<DragonPlayer>() != null) {
			// Dragon has multiple colliders, to avoid triggering more than once, put a small timer before detecting the new collision
			if(timeSinceLastCollision < 0.25f) return;
			timeSinceLastCollision = 0f;	// Reset timer

			// Different approach: random direction and intensity :P
			Vector3 force = Vector3.zero;
			float angle = impactAngle.GetRandom() * Mathf.Deg2Rad;
			float direction = player.m_rbody.velocity.x < 0 ? -1 : 1;	// Keep dragon's current direction
			force.x = Mathf.Cos(angle) * impactIntensity * direction;
			force.y = Mathf.Sin(angle) * impactIntensity;
			player.Stop();
			player.ApplyForce(force);

			// Apply damage, no force applied (we've just done it)
			player.OnImpact(Vector3.zero, impactDamage, 0f, null);
		}
	}
}
