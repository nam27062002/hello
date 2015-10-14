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

	[SerializeField] private float m_stunnedTime;

	// Public members
	[HideInInspector] public Rigidbody m_rbody;

	// References to components
	Animator  				m_animator;
	DragonPlayer			m_dragon;
	DragonControl			m_controls;
	DragonOrientation   	m_orientation;

	// Movement control
	private Vector3 m_impulse;
	private Vector3 m_externalImpulse;
	private Vector3 m_direction;
	private float m_speedMultiplier;
	private float m_stunnedTimer;
	private float m_glideTimer;

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
		m_speedMultiplier = 0.5f;

		m_stunnedTimer = 0;
		m_glideTimer = 6f;

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

		if (m_stunnedTimer > 0) {
			m_stunnedTimer -= Time.deltaTime;
			if (m_stunnedTimer <= 0) {
				m_stunnedTimer = 0;
			}
		}

		if (m_glideTimer > 0) {
			m_glideTimer -= Time.deltaTime;
			if (m_glideTimer <= 0) {
				m_glideTimer = 0;
			}
		}
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
			m_speedMultiplier = Mathf.Lerp(m_speedMultiplier, m_dragon.GetSpeedMultiplier(), 0.25f); //accelerate from stop to normal or boost velocity

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

			m_glideTimer = Random.Range(3f, 5f);
		}

		if (m_stunnedTimer <= 0) {
			m_rbody.velocity = m_impulse + (m_externalImpulse * m_speedMultiplier);
		} else {
			m_rbody.velocity = (m_externalImpulse * m_speedMultiplier);
		}
				
		m_rbody.angularVelocity = Vector3.zero;
		
		m_externalImpulse = Vector3.Lerp(m_externalImpulse, Vector3.zero, 0.05f * m_speedMultiplier);

		// Animator state
		if (impulse != Vector3.zero) {	
			bool plummet = m_speedMultiplier > 1.5f || m_direction.y < -0.65f;
			bool flyUp = m_direction.y > 0.65f;

			m_animator.SetBool("fly", true);
			m_animator.SetBool("plummet", plummet);
			m_animator.SetBool("flight_up", flyUp);

			if (!plummet && !flyUp && !m_animator.GetBool("bite") && !m_animator.GetBool("fire")) {
				if (m_glideTimer <= 0) {
					m_animator.SetTrigger("glide");
					m_glideTimer = Random.Range(3f, 5f);
				}
			} else {
				m_glideTimer = Random.Range(3f, 5f);
			}
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

	public void AddForce(Vector3 _force) {

		m_externalImpulse += _force;
		m_stunnedTimer = m_stunnedTimer;
		m_animator.SetTrigger("damage");
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
		return m_dragon.data.speed.value * m_speedMultiplier;
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

