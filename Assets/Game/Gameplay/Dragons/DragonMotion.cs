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

	enum State {
		Idle = 0,
		Fly,
		Fly_Up,
		Fly_Down,
		Stunned
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
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
	private Vector3 m_direction;
	private float m_speedMultiplier;

	private float m_stunnedTimer;

	private int m_groundMask;
	private bool m_nearGround;

	struct Sensors {
		public Transform top;
		public Transform bottom;
	};
	private Sensors m_sensor;

	private State m_state;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	public Transform tongue { get { return transform.FindSubObjectTransform("Dragon_Tongue"); } }
	public Transform jaw { get { return transform.FindSubObjectTransform("Dragon_Jaw"); } }

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		
		m_groundMask = 1 << LayerMask.NameToLayer("Ground");

		// Get references
		m_animator			= transform.FindChild("view").GetComponent<Animator>();
		m_dragon			= GetComponent<DragonPlayer>();
		m_controls 			= GetComponent<DragonControl>();
		m_orientation	 	= GetComponent<DragonOrientation>();

		Transform sensors	= transform.FindChild("sensors").transform; 
		m_sensor.top 		= sensors.FindChild("TopSensor").transform;
		m_sensor.bottom		= sensors.FindChild("BottomSensor").transform;

		m_rbody = GetComponent<Rigidbody>();
	}

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Initialize some internal vars
		m_speedMultiplier = 0.5f;

		m_stunnedTimer = 0;

		m_impulse = Vector3.zero;
		m_direction = Vector3.right;
		
		ChangeState(State.Idle);
	}

	void OnEnable() {
		m_speedMultiplier = 0.5f;
	}
	
	private void ChangeState(State _nextState) {	
		if (m_state != _nextState) {
			// we are leaving old state
			switch (m_state) {
				case State.Fly:
					break;

				case State.Fly_Up:
					m_animator.SetBool("fly up", false);
					break;

				case State.Fly_Down:
					m_animator.SetBool("fly down", false);
					break;

				case State.Stunned:
					m_stunnedTimer = 0;
					break;
			}

			// entering new state
			switch (_nextState) {
				case State.Idle:
					m_animator.SetBool("fly", false);

					m_impulse = Vector3.zero;
					m_rbody.velocity = m_impulse;
					if (m_direction.x < 0)	m_direction = Vector3.left;
					else 					m_direction = Vector3.right;
					m_orientation.SetDirection(m_direction);
					break;

				case State.Fly:
					m_animator.SetBool("fly", true);
					break;

				case State.Fly_Up:
					m_animator.SetBool("fly", true);
					m_animator.SetBool("fly up", true);
					break;

				case State.Fly_Down:
					m_animator.SetBool("fly", true);
					m_animator.SetBool("fly down", true);
					break;

				case State.Stunned:
					m_impulse = Vector3.zero;
					m_rbody.velocity = m_impulse;
					m_stunnedTimer = m_stunnedTime;
					m_speedMultiplier = 0.5f;
					m_animator.SetTrigger("damage");
					break;
			}

			m_state = _nextState;
		}		
	}
			
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		switch (m_state) {
			case State.Idle:
				m_speedMultiplier = Mathf.Lerp(m_speedMultiplier, 0.5f, 0.025f); //don't reduce multipliers too fast 

				if (m_controls.moving) {
					ChangeState(State.Fly);
				}
				break;

			case State.Fly:
				if (m_direction.y < -0.65f) {
					ChangeState(State.Fly_Down);
				} else if (m_direction.y > 0.65f) {
					ChangeState(State.Fly_Up);				
				}
				break;

			case State.Fly_Up:
				if (m_speedMultiplier > 1.5f) {
					ChangeState(State.Fly_Down);
				} else if (m_direction.y < 0.65f) {
					ChangeState(State.Fly);			
				}
				break;

			case State.Fly_Down:
				if (m_speedMultiplier < 1.5f && m_direction.y > -0.65f) {
					ChangeState(State.Fly);
				}
				break;

			case State.Stunned:
				m_stunnedTimer -= Time.deltaTime;
				if (m_stunnedTimer <= 0) {
					ChangeState(State.Idle);
				}
				break;
		}

	}

	/// <summary>
	/// Called once per frame at regular intervals.
	/// </summary>
	void FixedUpdate() {
		switch (m_state) {
			case State.Fly:
			case State.Fly_Up:
			case State.Fly_Down:
				UpdateMovement();
				break;
		}
		
		m_rbody.angularVelocity = Vector3.zero;

		Vector3 position = transform.position;
		position.z = 0f;
		transform.position = position;
	}
	
	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the movement.
	/// </summary>
	void UpdateMovement() {
		Vector3 impulse = m_controls.GetImpulse(m_dragon.data.speed.value * m_speedMultiplier); 

		if (impulse != Vector3.zero) {
			// accelerate the dragon
			m_speedMultiplier = Mathf.Lerp(m_speedMultiplier, m_dragon.GetSpeedMultiplier(), 0.25f); //accelerate from stop to normal or boost velocity

			// we keep the velocity value
			float v = impulse.magnitude;
																		
			// check collision with ground, only down!!
			RaycastHit sensorA;
			RaycastHit sensorB;
			float dot = 0;

			CheckGround((impulse.y < 0)? Vector3.down : Vector3.up, out sensorA, out sensorB);

			if (m_nearGround) {
				// we are moving towards ground or away?
				dot = Vector3.Dot(sensorA.normal, impulse.normalized);
				m_nearGround = dot < 0;
			}

			if (m_nearGround) {
				if (impulse.x < 0) 	m_direction = (sensorA.point - sensorB.point).normalized;
				else 				m_direction = (sensorB.point - sensorA.point).normalized;

				if ((sensorA.distance <= 0.75f || sensorB.distance <= 0.75f)) {
					float f = 1 + ((dot - (-0.5f)) / (-1 - (-0.5f))) * (0 - 1);
					m_impulse = m_direction * Mathf.Min(1f, Mathf.Max(0f, f));
				} else {
					// the direction will be parallel to ground, but we'll still moving down until the dragon is near enough
					m_impulse = impulse.normalized;				
				}			
			} else {
				// on air impulse formula, we don't fully change the velocity vector 
				m_impulse = Vector3.Lerp(m_impulse, impulse, 0.5f);
				m_impulse.Normalize();
				m_direction = m_impulse;
			}			

			m_impulse *= v;

			m_orientation.SetDirection(m_direction);
		} else {
			ChangeState(State.Idle);
		}

		m_rbody.velocity = m_impulse;
	}

	private void CheckGround(Vector3 _direction, out RaycastHit _leftHit, out RaycastHit _rightHit) {
		Vector3 distance = _direction * 2f;
		bool hit_L = false;
		bool hit_R = false;

		Vector3 leftSensor;
		Vector3 rightSensor;

		if (_direction.y < 0) 	leftSensor = m_sensor.bottom.position;
		else 					leftSensor = m_sensor.top.position;

		rightSensor = leftSensor + Vector3.right * 2f;

		hit_L = Physics.Linecast(leftSensor, leftSensor + distance, out _leftHit, m_groundMask);
		hit_R = Physics.Linecast(rightSensor, rightSensor + distance, out _rightHit, m_groundMask);

		m_nearGround = hit_L && hit_R;
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

		ChangeState(State.Stunned);
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


	public void OnImpact(Vector3 _origin, float _damage, float _intensity, DamageDealer_OLD _source) {
		
		m_dragon.AddLife(-_damage);
	}


}

