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

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	public float speedDirectionMultiplier = 2f;
	public Range movementLimitX = new Range(-10000, 50000);

	// Public members
	[HideInInspector] public Rigidbody m_rbody;

	// References to components
	Animator  				m_animator;
	DragonPlayer			m_dragon;
	DragonControl			m_controls;
	DragonOrientation   	m_orientation;

	// Movement control
	private Vector3 m_impulse;
	private Vector3 m_direction;
	private float m_accMultiplier;
	private float m_speedMultiplier;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

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
		m_rbody.angularVelocity = Vector3.zero;


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

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
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
	/// Obtain the current direction of the dragon.
	/// </summary>
	/// <returns>The direction the dragon is currently moving towards.</returns>
	public Vector3 GetDirection(){
		return m_direction;
	}
		
	public Vector3 GetVelocity() {
		return m_rbody.velocity;
	}
	
	// max speed without boost
	public float GetMaxSpeed() {
		return m_dragon.data.speed.value * m_accMultiplier;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	void OnTriggerEnter(Collider other) {}
	void OnTriggerExit(Collider other) {}
	void OnTriggerStay(Collider other) {}
	void OnCollisionEnter(Collision collision) {}
	public void OnImpact(Vector3 _origin, float _damage, float _intensity, DamageDealer _source) {
		
		m_dragon.AddLife(-_damage);
	}	
}

