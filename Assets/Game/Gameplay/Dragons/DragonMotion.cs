// DragonMotion.cs
// Hungry Dragon
// 
// Created by Pere Alsina on 20/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main control of the dragon movement.
/// </summary>
public class DragonMotion : MonoBehaviour, MotionInterface {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	public enum State {
		Idle = 0,
		Fly,
		Fly_Up,
		Fly_Down,
		Stunned,
		InsideWater,
		OutterSpace
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
	DragonHealthBehaviour	m_health;
	DragonControl			m_controls;
	Orientation			   	m_orientation;
	DragonAnimationEvents 	m_animationEventController;


	// Movement control
	private Vector3 m_impulse;
	private Vector3 m_direction;
	private float m_targetSpeedMultiplier;
	public float targetSpeedMultiplier
	{
		get {return m_targetSpeedMultiplier;}
		set { m_targetSpeedMultiplier = value; }
	}

	private float m_currentSpeedMultiplier;
	public float currentSpeedMultiplier
	{
		get
		{
			return m_currentSpeedMultiplier;
		}
	}
	// Speed Value wich results from dragon data + power ups
	private float m_speedValue;
	public float speed{get {return m_speedValue;}}

	private float m_stunnedTimer;

	private int m_groundMask;
	/** Distance from the nearest ground collision below the dragon. The maximum distance checked is 10. */
	private float m_height;

	struct Sensors {
		public Transform top;
		public Transform bottom;
	};
	private Sensors m_sensor;

	private List<Transform> m_hitTargets;

	private State m_state;
	public State state
	{
		get
		{
			return m_state;
		}
	}


	private float m_impulseTransformationSpeed;

	private Transform m_tongue;
	private Transform m_head;
	private Transform m_cameraLookAt;

	// Parabolic movement
	private float m_parabolicMovementValue = 10;

	public ParticleSystem m_bubbles;

	private bool m_canMoveInsideWater = true;
	public bool canDive
	{
		get
		{
			return m_canMoveInsideWater;
		}

		set
		{
			m_canMoveInsideWater = value;
		}
	}

	private float m_waterMovementModifier = 0;

	private Vector3 m_forbiddenDirection;
	private float m_forbiddenValue;

	private List<Vector3> m_normalContacts = new List<Vector3>();

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	public Transform tongue { get { if (m_tongue == null) { m_tongue = transform.FindTransformRecursive("Fire_Dummy"); } return m_tongue; } }
	public Transform head   { get { if (m_head == null)   { m_head = transform.FindTransformRecursive("Dragon_Head");  } return m_head;   } }
	public Transform cameraLookAt   { get { if (m_cameraLookAt == null)   { m_cameraLookAt = transform.FindTransformRecursive("camera");  } return m_cameraLookAt;   } }
	private Vector3 m_lastPosition;
	private Vector3 lastPosition
	{
		get
		{
			return m_lastPosition;
		}
	}
	private float m_lastSpeed;
	public float lastSpeed
	{
		get
		{
			return m_lastSpeed;
		}
	}

		 
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
		m_health			= GetComponent<DragonHealthBehaviour>();
		m_controls 			= GetComponent<DragonControl>();
		m_orientation	 	= GetComponent<Orientation>();
		m_animationEventController = GetComponentInChildren<DragonAnimationEvents>();

		Transform sensors	= transform.FindChild("sensors").transform; 
		m_sensor.top 		= sensors.FindChild("TopSensor").transform;
		m_sensor.bottom		= sensors.FindChild("BottomSensor").transform;

		int n = 0;
		Transform t = null;
		Transform points = transform.FindChild("points");
		m_hitTargets = new List<Transform>();

		while (true) {
			t = points.FindChild("attack_" + n);
			if (t != null) {
				m_hitTargets.Add(t);
				n++;
			} else {
				break;
			}
		}

		m_rbody = GetComponent<Rigidbody>();

		m_height = 10f;

		m_targetSpeedMultiplier = 1;

		// TODO (miguel): This should come from dragon settings
		m_impulseTransformationSpeed = 25.0f;
	}

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Initialize some internal vars
		m_currentSpeedMultiplier = 0.5f;

		m_stunnedTimer = 0;
		canDive = DebugSettings.dive;
		m_impulse = Vector3.zero;
		m_direction = Vector3.right;
		m_lastPosition = transform.position;
		m_lastSpeed = 0;
		ChangeState(State.Idle);

		m_forbiddenDirection = Vector3.zero;
		m_forbiddenValue = 0;

		// Set to base
		m_speedValue = m_dragon.data.speedSkill.value;

		// Add modifiers

	}

	void OnEnable() {
		m_currentSpeedMultiplier = 0.5f;
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
				case State.InsideWater:
				{
					m_animator.SetBool("fly down", false);
				}break;
				case State.OutterSpace:
				{
					m_animator.SetBool("fly down", false);
				}break;
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
					m_currentSpeedMultiplier = 0.5f;
					m_animator.SetTrigger("damage");
					break;
				case State.InsideWater:
				{
					m_animator.SetBool("fly down", true);
				}break;
				case State.OutterSpace:
				{
					m_animator.SetBool("fly down", true);
				}break;
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
				m_currentSpeedMultiplier = Mathf.Lerp(m_currentSpeedMultiplier, 0.5f, 0.025f); //don't reduce multipliers too fast 

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
				if (m_currentSpeedMultiplier > 1.5f) {
					ChangeState(State.Fly_Down);
				} else if (m_direction.y < 0.65f) {
					ChangeState(State.Fly);			
				}
				break;

			case State.Fly_Down:
				if (m_currentSpeedMultiplier < 1.5f && m_direction.y > -0.65f) {
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
				
		m_animator.SetFloat("height", m_height);
	}

	/// <summary>
	/// Called once per frame at regular intervals.
	/// </summary>
	void FixedUpdate() {
		switch (m_state) {
			case State.Idle:
				FlyAwayFromGround();
				break;

			case State.Fly:
			case State.Fly_Up:
			case State.Fly_Down:
				UpdateMovement();
				break;
			case State.InsideWater:
			{
				if (m_canMoveInsideWater)
				{
					UpdateWaterMovement();
				}
				else
				{
					UpdateParabolicMovement( m_parabolicMovementValue);
				}
			}break;
			case State.OutterSpace:
				UpdateParabolicMovement( -m_parabolicMovementValue);
				break;
		}
		
		m_rbody.angularVelocity = Vector3.zero;

		m_lastSpeed = (transform.position - m_lastPosition).magnitude / Time.fixedDeltaTime;

		Vector3 position = transform.position;
		position.z = 0f;
		transform.position = position;

		m_lastPosition = transform.position;
	}
	
	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the movement.
	/// </summary>
	private void UpdateMovement() {
		Vector3 impulse = m_controls.GetImpulse(m_speedValue * m_currentSpeedMultiplier); 

		if (impulse != Vector3.zero) {
			// accelerate the dragon
			float speedUp = (m_state == State.Fly_Down)? 1.2f : 1f;
			m_currentSpeedMultiplier = Mathf.Lerp(m_currentSpeedMultiplier, m_targetSpeedMultiplier * speedUp, Time.deltaTime * 20.0f); //accelerate from stop to normal or boost velocity

			ComputeFinalImpulse(impulse);

			m_orientation.SetDirection(m_direction);
		} else {
			ChangeState(State.Idle);
		}

		m_rbody.velocity = m_impulse;
	}

	private void UpdateWaterMovement()
	{
		Vector3 impulse = m_controls.GetImpulse(m_speedValue * 0.5f * m_currentSpeedMultiplier);

		if ( impulse.y > 0 )
		{
			m_waterMovementModifier = Mathf.Lerp( m_waterMovementModifier, 1, Time.deltaTime);
		}
		else if (impulse.y == 0)
		{
			m_waterMovementModifier = Mathf.Lerp( m_waterMovementModifier, 0.25f, Time.deltaTime);
		}
		else
		{
			m_waterMovementModifier = Mathf.Lerp( m_waterMovementModifier, 0.25f, Time.deltaTime);
		}

		impulse.y += m_speedValue * m_waterMovementModifier;
	


		if (impulse != Vector3.zero) 
		{
			// accelerate the dragon
			m_currentSpeedMultiplier = Mathf.Lerp(m_currentSpeedMultiplier, m_targetSpeedMultiplier, Time.deltaTime * 20.0f); //accelerate from stop to normal or boost velocity

			ComputeFinalImpulse(impulse);

			m_orientation.SetDirection(m_direction);
		} else {
			ChangeState(State.Idle);
		}

		m_rbody.velocity = m_impulse;
	}

	private void UpdateParabolicMovement( float moveValue )
	{
		Vector3 impulse = m_controls.GetImpulse(m_speedValue * m_currentSpeedMultiplier * Time.deltaTime * 0.1f); 

		// check collision with ground, only down?
		m_impulse.y += moveValue * Time.deltaTime;

		m_impulse.x += impulse.x;

		m_direction = m_impulse.normalized;
		m_orientation.SetDirection(m_direction);
		m_rbody.velocity = m_impulse;
	}

	private void FlyAwayFromGround() {
		if (m_height < 2f * transform.localScale.y) { // dragon will fly up to avoid mesh intersection
			Vector3 oldDirection = m_direction;
			Vector3 impulse = Vector3.up * m_speedValue * 0.1f;			

			ComputeFinalImpulse(impulse);	
			m_direction = oldDirection;

			m_orientation.SetDirection(m_direction);
			
			m_rbody.velocity = m_impulse;
		} else {
			m_rbody.velocity = Vector3.zero;
		}
	}

	private void ComputeFinalImpulse(Vector3 _impulse) {
		// we keep the velocity value
		float v = _impulse.magnitude;
		/*
		// check collision with ground, only down!!
		RaycastHit sensorA = new RaycastHit();
		float dot = 0;

		float minDis = 2f;
		bool nearGround = CheckGround(out sensorA);
		if (_impulse.y > 0) {
			minDis = 2f;
			nearGround = CheckCeiling(out sensorA);
		} 

		if (nearGround) {
			// we are moving towards ground or away?
			dot = Vector3.Dot(sensorA.normal, _impulse.normalized);
			nearGround = dot < 0;
		}

		if (nearGround && false) {
			//m_direction = Vector3.right;
			if ((sensorA.normal.y < 0 && _impulse.x < 0) || (sensorA.normal.y > 0 && _impulse.x > 0)) {
				m_direction = new Vector3(sensorA.normal.y, -sensorA.normal.x, sensorA.normal.z);
			} else {
				m_direction = new Vector3(-sensorA.normal.y, sensorA.normal.x, sensorA.normal.z);
			}
			m_direction.Normalize();

			if ((sensorA.distance <= minDis)) {
				float f = 1 + ((dot - (-0.5f)) / (-1 - (-0.5f))) * (0 - 1);
				m_impulse = m_direction * Mathf.Min(1f, Mathf.Max(0f, f));
			} else {
				// the direction will be parallel to ground, but we'll still moving down until the dragon is near enough
				m_impulse = _impulse.normalized;				
			}	
		} else 
		*/

		{
			// on air impulse formula, we don't fully change the velocity vector 
			m_impulse = Vector3.Lerp(m_impulse, _impulse, m_impulseTransformationSpeed * Time.deltaTime);
			m_impulse.Normalize();


			if (m_normalContacts.Count > 0)
			{
				m_forbiddenDirection = Vector3.zero;
				for( int i = 0; i < m_normalContacts.Count; i++ )
				{
					m_forbiddenDirection += m_normalContacts[i];
				}
				m_forbiddenDirection.Normalize();
				m_forbiddenValue = 1;
				m_normalContacts.Clear();
			}


			if ( m_forbiddenValue > 0)
			{
				float forbiddenDot = Vector3.Dot( m_impulse, m_forbiddenDirection);	
				if ( forbiddenDot < 0 )
				{
					m_forbiddenValue -= Time.deltaTime * 2;
					float outPush = Mathf.Sin( m_forbiddenValue * Mathf.PI * 0.5f);	
					m_impulse = m_impulse + (m_forbiddenDirection * -forbiddenDot * outPush);
					m_impulse.Normalize();
				}
				else
				{
					m_forbiddenValue = 0;
					m_forbiddenDirection = Vector3.zero;
				}
			}

			m_direction = m_impulse;
		}			
		
		m_impulse *= v;
	}

	private bool CheckGround(out RaycastHit _leftHit) {
		Vector3 distance = Vector3.down * 10f;
		bool hit_L = false;

		Vector3 leftSensor  = m_sensor.bottom.position;
		hit_L = Physics.Linecast(leftSensor, leftSensor + distance, out _leftHit, m_groundMask);

		if (hit_L) {
			float d = _leftHit.distance;
			m_height = d;
			return (d <= 1f);
		} else {
			m_height = 10f;
			return false;
		}
	}

	private bool CheckCeiling(out RaycastHit _leftHit) {
		Vector3 distance = Vector3.up * 10f;
		bool hit_L = false;
		
		Vector3 leftSensor 	= m_sensor.top.position;						
		hit_L = Physics.Linecast(leftSensor, leftSensor + distance, out _leftHit, m_groundMask);

		if (hit_L) {
			return (_leftHit.distance <= 1f);
		}
		return false;
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

	public Transform GetAttackPointNear(Vector3 _point) {
		Transform target = transform;
		float minDistSqr = 999999f;

		for (int i = 0; i < m_hitTargets.Count; i++) {
			Vector2 v = (_point - m_hitTargets[i].position);
			float distSqr = v.sqrMagnitude;
			if (distSqr <= minDistSqr) {
				target = m_hitTargets[i];
				minDistSqr = distSqr;
			}
		}

		return target;
	}
	
	//------------------------------------------------------------------//
	// GETTERS															//
	//------------------------------------------------------------------//
	public Vector2 position {
		get { return transform.position; }
	}
	/// <summary>
	/// Obtain the current direction of the dragon.
	/// </summary>
	/// <returns>The direction the dragon is currently moving towards.</returns>
	public Vector2 direction {
		get { return m_direction; }
	}
		
	public Vector2 velocity {
		get { return m_rbody.velocity; }
	}
	
	// current speed
	public float maxSpeed {
		get { return m_speedValue * m_currentSpeedMultiplier; }
	}

	// max speed with boost but withoung going down
	public float absoluteMaxSpeed
	{
		get { return m_speedValue * m_dragon.data.def.GetAsFloat("boostMultiplier"); }
	}

	public void SetSpeedMultiplier(float _value) {
		m_targetSpeedMultiplier = _value;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//


	public void OnImpact(Vector3 _origin, float _damage, float _intensity, DamageDealer_OLD _source) {
		
		// m_dragon.AddLife(-_damage);
		m_health.ReceiveDamage( _damage , null, false);
	}

	public void StartWaterMovement()
	{
		m_waterMovementModifier = 0;
		if ( m_bubbles != null )
			m_bubbles.Play();
		m_animationEventController.OnInsideWater();
		ChangeState(State.InsideWater);
	}

	public void EndWaterMovement()
	{
		// Wait a second 
		// Disable Bubbles
		if ( m_bubbles != null )
			m_bubbles.Stop();
		if (m_animator )
			m_animator.SetBool("boost", false);
		m_animationEventController.OnExitWater();
		StartCoroutine( EndWaterCoroutine() );
	}

	IEnumerator EndWaterCoroutine()
	{
		yield return new WaitForSeconds(0.1f);
		ChangeState( State.Fly_Up);
	} 

	public void StartSpaceMovement()
	{
		m_animationEventController.OnOutterSpace();
		ChangeState(State.OutterSpace);
	}

	public void EndSpaceMovement()
	{
		// Wait a second 
		m_animationEventController.OnReturnFromOutterSpace();
		StartCoroutine( EndSpaceCoroutine() );
	}

	IEnumerator EndSpaceCoroutine()
	{
		yield return new WaitForSeconds(0.1f);
		ChangeState( State.Fly_Down);
	} 


	/// <summary>
	/// Raises the trigger enter event.
	/// </summary>
	/// <param name="_other">Other.</param>
	void OnTriggerEnter(Collider _other)
	{
		if ( _other.tag == "Water" )
		{
			// Enable Bubbles
			StartWaterMovement();
		}
		else if ( _other.tag == "Space" )
		{
			StartSpaceMovement();
		}
	}

	void OnTriggerExit( Collider _other )
	{
		if ( _other.tag == "Water" )
		{
			// Disable Bubbles
			EndWaterMovement();
		}
		else if ( _other.tag == "Space" )
		{
			EndSpaceMovement();
		}
	}

	void OnCollisionEnter(Collision collision) 
	{
		switch( m_state )
		{
			case State.InsideWater:
			{
				if ( m_impulse.y < 0 )
				{
					m_impulse.y = 0;		
				}
				m_impulse.x = -m_impulse.x;
			}break;
			case State.OutterSpace:
			{
				if ( m_impulse.y > 0 )
				{
					m_impulse.y = 0;		
				}
				m_impulse.x = -m_impulse.x;
			}break;
			default:
			{
				for( int i = 0; i<collision.contacts.Length; i++ )
				{
					m_normalContacts.Add( collision.contacts[i].normal);
				}
			}break;
		}

	}

	void OnCollisionStay(Collision collision)
	{
		for( int i = 0; i<collision.contacts.Length; i++ )
		{
			m_normalContacts.Add( collision.contacts[i].normal);
		}
	}

}

