// DragonMotion.cs
// Hungry Dragon
// 
// Created by Pere Alsina on 20/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main control of the dragon movement.
/// </summary>
public class DragonMotion : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum EState {
		INIT,
		IDLE,
		BOOST,
		DYING,
		DEAD,
		DEAD_GORUND
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	public float speedDirectionMultiplier = 2f;
	public Range movementLimitX = new Range(-10000, 50000);

	// Public members
	[HideInInspector] public Rigidbody m_rbody;
	[HideInInspector] public bool invulnerable = false;	// Debug purposes
	
	// References to components
	Animator  				m_animator;
	DragonPlayer			m_dragon;
	DragonControl			m_controls;
	DragonOrientation   	m_orientation;
	DragonEatBehaviour		m_eatBehaviour;
	DragonBreathBehaviour 	m_breathBehaviour;
	DragonGrabBehaviour		m_grabBehaviour;

	// Movement control
	private Vector3 m_impulse;
	private Vector3 m_direction;
	private float m_accMultiplier;
	private float m_speedMultiplier;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Control vars
	EState mState = EState.INIT;
	public EState state {
		get { return mState; }
	}
	float mStateTimer = 0f;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Get references
		m_animator			= transform.FindChild("view").GetComponent<Animator>();
		m_dragon			= GetComponent<DragonPlayer>();
		m_controls 			= GetComponent<DragonControl>();
		m_orientation	 	= GetComponent<DragonOrientation>();
		m_eatBehaviour	 	= GetComponent<DragonEatBehaviour>();
		m_breathBehaviour 	= GetComponent<DragonBreathBehaviour>();
		m_grabBehaviour 	= GetComponent<DragonGrabBehaviour>();
		
		m_rbody = GetComponent<Rigidbody>();
	}

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Initialize some internal vars
		m_accMultiplier = 1.25f;
		m_speedMultiplier = 0.5f;

		m_impulse = Vector3.zero;
		m_direction = Vector3.right;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy(){

	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {

	}

	/// <summary>
	/// Called once per frame at regular intervals.
	/// </summary>
	void FixedUpdate(){

		UpdateMovement();

		// limit movement
		Vector3 pos = transform.position;

		if (pos.x < movementLimitX.min) {
			pos.x = movementLimitX.min;
		} else if (pos.x > movementLimitX.max) {
			pos.x = movementLimitX.max;
		} else if (pos.y > 15000f) {
			pos.y = 15000f;
		}
		pos.z = 0;

		transform.position = pos;
	}
	
	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the movement.
	/// </summary>
	void UpdateMovement() {

		Vector3 oldDirection = m_direction;
		Vector3 impulse = m_controls.GetImpulse(m_dragon.data.speed.value * m_speedMultiplier); 

		if (impulse != Vector3.zero) {
			// accelerate the dragon
			m_speedMultiplier = Mathf.Lerp(m_speedMultiplier, Mathf.Max(m_accMultiplier, m_dragon.GetSpeedMultiplier()), 0.25f); //accelerate from stop to normal or boost velocity

			float v = impulse.magnitude;
			m_impulse = Vector3.Lerp(m_impulse, impulse, 0.5f);
			m_impulse.Normalize();

			m_direction = m_impulse;

			m_impulse *= v; 

			m_orientation.SetDirection(m_direction);
		} else {
			// idle
			m_speedMultiplier = Mathf.Lerp(m_speedMultiplier, 0.5f, 0.025f); //don't reduce multipliers too fast 
			m_impulse = Vector3.zero;
			if (oldDirection.x < 0)	m_direction = Vector3.left;
			else 					m_direction = Vector3.right;
			m_orientation.SetDirection(m_direction);
		}

		m_rbody.velocity = m_impulse;

		// Animator state
		if (impulse != Vector3.zero) {	
			if (Vector3.Dot(oldDirection, m_direction) < 0.75f && Mathf.Abs(oldDirection.y) < 0.75f && Mathf.Abs(m_direction.y) < 0.75f) {
				if (m_direction.x < 0f)	m_animator.SetTrigger("turn_left");
				else					m_animator.SetTrigger("turn_right");
			} else {
				m_animator.SetBool("fly", true);
			}
			m_animator.SetBool("plummet", m_speedMultiplier > 2f || m_direction.y < -0.75f);
			m_animator.SetBool("flight_up", m_direction.y > 0.75f);
		} else {
			m_animator.SetBool("fly", false);
			m_animator.SetBool("plummet", false);
			m_animator.SetBool("flight_up", false);
		}
	}
	
	/// <summary>
	/// Enable/Disable the "starving" status.
	/// </summary>
	/// <param name="_bIsStarving">Whether the dragon is starving or not.</param>
	void ToggleStarving(bool _bIsStarving) {

	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply damage to the dragon.
	/// </summary>
	/// <param name="_damage">Amount of damage to be applied.</param>
	/// <param name="_source">The source of the damage.</param>
	public void ApplyDamage(float _damage, DamageDealer _source) {

	}

	/// <summary>
	/// Pretty straightforward.
	/// </summary>
	/// <param name="_force">The force vector to be applied.</param>
	public void ApplyForce(Vector3 _force) {
	
	}

	/// <summary>
	/// Stop dragon's movement
	/// </summary>
	public void Stop() {
		m_rbody.velocity = Vector3.zero;
	}
	
	//------------------------------------------------------------------//
	// GETTERS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Is the dragon alive?
	/// </summary>
	/// <returns><c>true</c> if the dragon is not dead or dying; otherwise, <c>false</c>.</returns>
	public bool IsAlive() {
		return m_dragon.health > 0;
	}

	/// <summary>
	/// Is the dragon starving?
	/// </summary>
	/// <returns><c>true</c> if the dragon is alive and its current life under the specified warning threshold; otherwise, <c>false</c>.</returns>
	public bool IsStarving() {
		return (m_dragon.health > m_dragon.data.health * m_dragon.lifeWarningThreshold);
	}

	/// <summary>
	/// Whether the dragon can take damage or not.
	/// </summary>
	/// <returns><c>true</c> if the dragon currently is invulnerable; otherwise, <c>false</c>.</returns>
	public bool IsInvulnerable() {
		// During fire, we're invulnerable
		if (m_breathBehaviour.IsFuryOn()) return true;

		// If cheat is enable
		if (invulnerable) return true;

		// All checks passed, we're not invulnerable
		return false;
	}

	/// <summary>
	/// Obtain the current direction of the dragon.
	/// </summary>
	/// <returns>The direction the dragon is currently moving towards.</returns>
	public Vector3 GetDirection(){
		return m_direction;
	}
	
	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	void OnTriggerEnter(Collider other) {}
	void OnTriggerExit(Collider other) {}
	void OnTriggerStay(Collider other) {}
	void OnCollisionEnter(Collision collision) {}
	public void OnImpact(Vector3 _origin, float _damage, float _intensity, DamageDealer _source){}
	
}

