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
using Assets.Code.Game.Currents;

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
		Fly_Down,
		Stunned,
		InsideWater,
		ExitingWater,
		OuterSpace,
		ExitingSpace,
		Intro,
		Latching,
		Dead,
		Reviving,
		None,
	};

	public static float m_waterImpulseMultiplier = 0.75f;
	public static float m_onWaterCollisionMultiplier = 0.5f;

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private float m_stunnedTime;
	[SerializeField] private float m_velocityBlendRate = 256.0f;
	[SerializeField] private float m_rotBlendRate = 350.0f;

	[SerializeField] private bool m_capVerticalRotation = true;
	[SerializeField] private float m_capUpRotationAngle = 40.0f;
	[SerializeField] private float m_capDownRotationAngle = 60.0f;
	[SerializeField] private float m_noGlideAngle = 50.0f;

	protected Rigidbody m_rbody;
	public Rigidbody rbody
	{
		get{ return m_rbody; }
	}

	// References to components
	Animator  				m_animator;
	FlyLoopBehaviour		m_flyLoopBehaviour;
	DragonPlayer			m_dragon;
	// DragonHealthBehaviour	m_health;
	DragonControl			m_controls;
	DragonAnimationEvents 	m_animationEventController;
	DragonParticleController m_particleController;
	SphereCollider 			m_groundCollider;
	DragonEatBehaviour		m_eatBehaviour;

	public SphereCollider groundCollider { get { return m_groundCollider; } } 


	// Movement control
	private Vector3 m_impulse;
	private Vector3 m_direction;
	private Vector3 m_externalForce;	// Used for wind flows, to be set every frame
	private Quaternion m_desiredRotation;
	private Vector3 m_angularVelocity = Vector3.zero;
	private float m_boostSpeedMultiplier;
	public float boostSpeedMultiplier
	{
		get {return m_boostSpeedMultiplier;}
		set { m_boostSpeedMultiplier = value; }
	}

	private float m_holdSpeedMultiplier;
	public float holdSpeedMultiplier
	{
		get {return m_holdSpeedMultiplier;}
		set { m_holdSpeedMultiplier = value; }
	}

	private float m_latchedOnSpeedMultiplier = 0;
	private bool m_latchedOn = false;
	public bool isLatchedMovement 
	{ 
		get{return m_latchedOn;} 
	} 

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

	private State m_state = State.None;
	public State state
	{
		get
		{
			return m_state;
		}
	}

	private State m_previousState = State.None;

	private Transform m_tongue;
	private Transform m_head;
	private Transform m_cameraLookAt;
	private Transform m_transform;

	[CommentAttribute("Back navigation bend multiplier when boost or attack target")]
	[Range(0, 1f)]
	public float m_backBlendMultiplier = 0.35f;
	private Vector2 m_currentFrontBend;
	private Vector2 m_currentBackBend;

	// Parabolic movement
	[Header("Parabolic Movement")]
	[SerializeField] private float m_parabolicMovementConstant = 10;
	public float parabolicMovementConstant
	{
		get { return m_parabolicMovementConstant; }
		set { m_parabolicMovementConstant = value; }
	}
	[SerializeField] private float m_parabolicMovementAdd = 10;
	public float parabolicMovementAdd
	{
		get { return m_parabolicMovementAdd; }
		set { m_parabolicMovementAdd = value; }
	}
	private Vector3 m_startParabolicPosition;
	public float m_parabolicXControl = 10;

	[Space]
	[SerializeField] private float m_cloudTrailMinSpeed = 7.5f;
	[SerializeField] private float m_outerSpaceRecoveryTime = 0.5f;
	[SerializeField] private float m_insideWaterRecoveryTime = 0.1f;
	private float m_recoverTimer;

	private bool m_canMoveInsideWater = true;
	public bool canDive{
		get{
			return m_canMoveInsideWater;
		}

		set{
			m_canMoveInsideWater = value;
		}
	}

	// private float m_waterMovementModifier = 0;

	public float m_dragonForce = 20;
	public float m_dragonMass = 10;
	public float m_dragonFricction = 15.0f;
	public float m_dragonGravityModifier = 0.3f;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	public Transform tongue { get { if (m_tongue == null) { m_tongue = transform.FindTransformRecursive("Fire_Dummy"); } return m_tongue; } }
	public Transform head   { get { if (m_head == null)   { m_head = transform.FindTransformRecursive("Dragon_Head");  } return m_head;   } }
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

	RaycastHit m_raycastHit = new RaycastHit();

	[Space]
	private float m_introTimer;
	private const float m_introDuration = 2.5f;
	private Vector3 m_introTarget;
	private Vector3 m_destination;
	private const float m_introDisplacement = 75;
	public float introDisplacement{ get{return m_introDisplacement * transform.localScale.x;} }
	public AnimationCurve m_introDisplacementCurve;
	public float m_introStopAnimationDelta = 0.1f;

	private Transform m_preyPreviousTransformParent;
	private AI.Machine m_holdPrey = null;
	private Transform m_holdPreyTransform = null;

	private float m_boostMultiplier;

	private bool m_grab = false;

	private float m_inverseGravityWater = -0.5f;
	private float m_accWaterFactor = 0.72f;

    private RegionManager m_regionManager;
	public Current  	current { get; set; }

	private Vector3 m_diePosition;
	private Vector3 m_revivePosition;
	private float m_reviveTimer;
	private const float m_reviveDuration = 1;
	private float m_deadTimer = 0;


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		m_groundMask = LayerMask.GetMask("Ground", "GroundVisible");

		// Get references
		m_animator			= transform.FindChild("view").GetComponent<Animator>();
		m_flyLoopBehaviour	= m_animator.GetBehaviour<FlyLoopBehaviour>();
		m_dragon			= GetComponent<DragonPlayer>();
		// m_health			= GetComponent<DragonHealthBehaviour>();
		m_controls 			= GetComponent<DragonControl>();
		m_animationEventController = GetComponentInChildren<DragonAnimationEvents>();
		m_particleController = GetComponentInChildren<DragonParticleController>();
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
		m_groundCollider = GetComponentInChildren<SphereCollider>();
		m_eatBehaviour = GetComponent<DragonEatBehaviour>();
		m_height = 10f;

		m_boostSpeedMultiplier = 1;
		m_holdSpeedMultiplier = 1;
		m_latchedOnSpeedMultiplier = 1;

		m_transform = transform;
		m_currentFrontBend = Vector2.zero;
		m_currentBackBend = Vector2.zero;

		m_boostMultiplier = m_dragon.data.def.GetAsFloat("boostMultiplier");

		// Movement Setup
		m_dragonForce = m_dragon.data.def.GetAsFloat("force");
		// m_dargonAcceleration = m_dragon.data.def.GetAsFloat("speedBase");
		m_dragonMass = m_dragon.data.def.GetAsFloat("mass");
		m_dragonFricction = m_dragon.data.def.GetAsFloat("friction");
		m_dragonGravityModifier = m_dragon.data.def.GetAsFloat("gravityModifier");

		m_tongue = transform.FindTransformRecursive("Fire_Dummy");
	}

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Initialize some internal vars
		m_stunnedTimer = 0;
		canDive = DebugSettings.dive;
		m_impulse = Vector3.zero;
		m_direction = Vector3.right;
		m_angularVelocity = Vector3.zero;
		m_lastPosition = transform.position;
		m_lastSpeed = 0;

		if (m_state == State.None)
			ChangeState(State.Fly);

		// Add modifiers

	}

	void OnEnable() {
	}
	
	private void ChangeState(State _nextState) {
		if (m_state != _nextState) {
			// we are leaving old state
			switch (m_state) {
				case State.Fly:
					break;

				case State.Fly_Down:
					m_animator.SetBool("fly down", false);
					break;

				case State.Stunned:
					m_impulse = Vector3.zero;
					m_stunnedTimer = 0;
					m_rbody.freezeRotation = false;
					break;
				case State.InsideWater:
				{
					//m_inverseGravityWater = 1.5f;
					m_animator.SetBool("swim", false);
					m_animator.SetBool("fly down", false);
				}break;
				case State.OuterSpace:
				{
					m_animator.SetBool("fly down", false);
				}break;
				case State.Intro:
				{
					m_rbody.isKinematic = false;
					m_animator.SetBool("boost", false);
					m_animator.SetBool("move", false);
 				}break;
				case State.Latching:
				{
					m_groundCollider.enabled = true;
				}break;
				case State.Reviving:
				{
					m_rbody.detectCollisions = true;
				}break;
			}

			// entering new state
			switch (_nextState) {
				case State.Idle:
					m_animator.SetBool("move", false);

					// m_impulse = Vector3.zero;
					// m_rbody.velocity = m_impulse;
					if (m_direction.x < 0)	m_direction = Vector3.left;
					else 					m_direction = Vector3.right;
					RotateToDirection( m_direction );
					break;

				case State.Fly:
					m_animator.SetBool("move", true);
					break;

				case State.Fly_Down:
					m_animator.SetBool("move", true);
					m_animator.SetBool("fly down", true);
					break;

				case State.Stunned:
					m_rbody.velocity = m_impulse;
					m_rbody.angularVelocity = Vector3.zero;
					m_direction = m_impulse.normalized;
					m_stunnedTimer = m_stunnedTime;
					m_rbody.freezeRotation = true;
					m_animator.SetTrigger("damage");
					break;
				case State.InsideWater:
				{
					//if ( m_canMoveInsideWater )
					{
						//m_animator.SetBool("move", false);
						//m_animator.SetBool("swim", true);
					}
					/*else*/
					{
						m_animator.SetBool("fly down", true);
					}
					if ( m_state != State.Stunned ){
	                    m_accWaterFactor = 0.80f;
	                    m_inverseGravityWater = 1.5f;
						m_startParabolicPosition = transform.position;
					}
				}break;
				case State.ExitingWater:
				{
					m_recoverTimer = m_insideWaterRecoveryTime;
                    m_accWaterFactor = 2.0f;

                }
                    break;
				case State.OuterSpace:
				{
					m_animator.SetBool("fly down", true);
					if ( m_state != State.Stunned ){
						m_startParabolicPosition = transform.position;
					}
				}break;
				case State.ExitingSpace:
				{
					m_recoverTimer = m_outerSpaceRecoveryTime;
				}break;
				case State.Intro:
				{
					m_rbody.isKinematic = true;
					m_animator.SetBool("boost", true);
					m_animator.SetBool("move", true);
					m_introTimer = m_introDuration;
					m_impulse = Vector3.zero;
					m_direction = Vector3.right;
					m_desiredRotation = Quaternion.Euler(0,90.0f,0);
					transform.rotation = m_desiredRotation;
				}break;
				case State.Latching:
				{
					m_groundCollider.enabled = false;
				}break;
				case State.Dead:
				{
					m_animator.SetTrigger("dead");
					// Save Position!
					m_diePosition = transform.position;
					m_deadTimer = 0;
				}break;
				case State.Reviving:
				{
					m_rbody.detectCollisions = false;
					m_reviveTimer = m_reviveDuration;
					m_impulse = Vector3.zero;
					m_rbody.velocity = Vector3.zero;
					m_revivePosition = transform.position;
					m_animator.Play("BaseLayer.Idle");

					if ( m_direction.x > 0 ){
						m_direction = Vector3.right;	
					}else{
						m_direction = Vector3.left;
					}

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
				if (m_controls.moving) {
					ChangeState(State.Fly);
				}
				break;

			case State.Fly:
				if (m_direction.y < -0.65f) {
					ChangeState(State.Fly_Down);
				}
				break;

			case State.Fly_Down:
				if (m_direction.y > -0.65f) {
					ChangeState(State.Fly);
				}
				break;

			case State.Stunned:
				m_stunnedTimer -= Time.deltaTime;
				if (m_stunnedTimer <= 0) {
					switch( m_previousState )
					{
						case State.InsideWater:{
							ChangeState( State.InsideWater );
						}break;
						case State.OuterSpace:{
							ChangeState( State.OuterSpace );
						}break;
						default:{
							ChangeState(State.Idle);
						}break;
					}
				}
				break;
			case State.Intro:
			{
			/*
				m_introTimer -= Time.deltaTime;
				if ( m_introTimer <= 0 )
				{
					ChangeState( State.Idle );
				}else{	
					float delta = m_introTimer / m_introDuration;
					m_destination = Vector3.left * m_introDisplacement * delta;//Mathf.Sin( delta * Mathf.PI * 0.5f);
					m_destination += m_introTarget;

					m_impulse = Vector3.zero;
					m_direction = Vector3.right;
				}
				*/
			}break;
			case State.Latching:
			{
				RotateToDirection( m_holdPreyTransform.forward );
				Vector3 deltaPosition = Vector3.Lerp( m_tongue.position, m_holdPreyTransform.position, Time.deltaTime * 8);	// Mouth should be moving and orienting
				transform.position += deltaPosition - m_tongue.position;
			}break;
			case State.ExitingWater:
			{
				m_recoverTimer -= Time.deltaTime;
				if ( m_recoverTimer <= 0 )
					ChangeState( State.Fly );
			}break;
			case State.ExitingSpace:
			{
				m_recoverTimer -= Time.deltaTime;
				if ( m_recoverTimer <= 0 )
					ChangeState( State.Fly_Down );
			}break;
		}


				
		m_animator.SetFloat("height", m_height);

		UpdateBodyBending();

		if(m_regionManager == null)
		{
			RegionManager.Init();
			m_regionManager = RegionManager.Instance;
		}
		CheckForCurrents();
		CheckAllowGlide();
	}


 	private void CheckForCurrents()
    {
		// if the region manager is in place...
        if(m_regionManager != null)
        {
			// if it's not in a current...
			if(current == null)
            {
				// ... and it's visible...
				// if(m_isVisible)
				{
					// we're not inside a current, check for entry
					current = m_regionManager.CheckIfObjIsInCurrent(gameObject);
					if(current != null)
					{
						// notify the machine that it's now in a current.
						// m_machine.EnteredInCurrent(current);


					}
				}
            }
            else
            {
                Vector3 pos = m_transform.position;
				if(!current.Contains(pos.x, pos.y))
                {
					if(current.splineForce != null)
					{
						// gently apply an exit force before leaving the current
						current.splineForce.RemoveObject(gameObject, false);
					}
					current = null;
                }
			}
        }	
	}	

	void CheckAllowGlide()
	{
		// if the region manager is in place...
        if(m_regionManager != null)
        {
			// if it's not in a current...
			if(current == null)
            {
				float angle = Util.ToAngleDegrees( m_direction );
				if ( angle > m_noGlideAngle && angle < 180-m_noGlideAngle ){
					m_flyLoopBehaviour.allowGlide = false;	
					m_animator.SetBool("glide", false);
				}
				else
				{
					m_flyLoopBehaviour.allowGlide = true;	
				}
            }
            else
            {
                Vector3 pos = m_transform.position;
				if(current != null)
				{
					if ( !current.IsInCurrentDirection( gameObject ) )	// if agains current we dont allow to glide
					{
						m_flyLoopBehaviour.allowGlide = true;
					}
					else
					{
						m_animator.SetBool("glide", false);
						m_flyLoopBehaviour.allowGlide = false;
					}
				}
			}
        }	

	}


	void LateUpdate()
	{
		if ( m_holdPrey != null )
		{
			if (m_grab)
			{
				// Rotation
				Quaternion rot = m_holdPrey.transform.localRotation;
				m_holdPrey.transform.localRotation = Quaternion.identity;
				Vector3 holdDirection = m_tongue.InverseTransformDirection(m_holdPreyTransform.forward);
				Vector3 holdUpDirection = m_tongue.InverseTransformDirection(m_holdPreyTransform.up);
				// m_holdPrey.transform.localRotation = Quaternion.LookRotation( -holdDirection, holdUpDirection );
				m_holdPrey.transform.localRotation = Quaternion.Lerp( rot, Quaternion.LookRotation( -holdDirection, holdUpDirection ), Time.deltaTime * 20);

				// Position
				Vector3 pos = m_holdPrey.transform.localPosition;
				m_holdPrey.transform.localPosition = Vector3.zero;
				Vector3 holdPoint = m_tongue.InverseTransformPoint( m_holdPreyTransform.position );
				// m_holdPrey.transform.localPosition = -holdPoint;
				m_holdPrey.transform.localPosition = Vector3.Lerp( pos, -holdPoint, Time.deltaTime * 20);
			}
		}
	}

	void UpdateBodyBending()
	{		
		float dt = Time.deltaTime;
		Vector3 dir = m_desiredRotation * Vector3.forward;
		float backMultiplier = 1;

		if (GetTargetForceMultiplier() > 1)// if boost active
		{
			backMultiplier = m_backBlendMultiplier;
		}

		if (m_eatBehaviour.GetAttackTarget() != null)
		{
			dir = m_eatBehaviour.GetAttackTarget().position - m_eatBehaviour.mouth.position;
			backMultiplier = m_backBlendMultiplier;
		}

		Vector3 localDir = m_transform.InverseTransformDirection(dir.normalized);	// todo: replace with direction to target if trying to bite, or during bite?

		float blendRate = 3.0f;	// todo: blend @ slower rate when stopped?
		float blendDampingRange = 0.2f;


		float desiredBendX = Mathf.Clamp(localDir.x*3.0f, -1.0f, 1.0f);	// max X bend is about 30 degrees, so *3
		m_currentFrontBend.x = Util.MoveTowardsWithDamping(m_currentFrontBend.x, desiredBendX, blendRate*dt, blendDampingRange);
		m_animator.SetFloat("direction X", m_currentFrontBend.x);
		m_currentBackBend.x = Util.MoveTowardsWithDamping(m_currentBackBend.x, desiredBendX * backMultiplier, blendRate*dt, blendDampingRange);
		m_animator.SetFloat("back direction X", m_currentBackBend.x);


		float desiredBendY = Mathf.Clamp(localDir.y*2.0f, -1.0f, 1.0f);		// max Y bend is about 45 degrees, so *2.
		m_currentFrontBend.y = Util.MoveTowardsWithDamping(m_currentFrontBend.y, desiredBendY, blendRate*dt, blendDampingRange);
		m_animator.SetFloat("direction Y", m_currentFrontBend.y);
		m_currentBackBend.y = Util.MoveTowardsWithDamping(m_currentBackBend.y, desiredBendY * backMultiplier, blendRate*dt, blendDampingRange);
		m_animator.SetFloat("back direction Y", m_currentBackBend.y);

		// update 'body bending' boolean parameter, we use this in the anim state machine
		// to notify things like straight swim variations that they should break out and return
		// to normal directional swim
		float m_isBendingThreshold = 0.1f;
		float maxBend = Mathf.Max(Mathf.Abs(m_currentFrontBend.x), Mathf.Abs(m_currentFrontBend.y));
		bool isBending = (maxBend > m_isBendingThreshold);
		m_animator.SetBool("Bend", isBending);
		
	}


	/// <summary>
	/// Called once per frame at regular intervals.
	/// </summary>
	void FixedUpdate() {
		switch (m_state) {
			case State.Idle:
				UpdateIdleMovement();
				// UpdateMovement();
				break;

			case State.Fly:
			case State.Fly_Down:
				UpdateMovement();
				break;
			case State.InsideWater:
			case State.ExitingWater:
			{
				//if (m_canMoveInsideWater)
				{
					UpdateWaterMovement();
				}
				/*else
				{
					float distance = m_startParabolicPosition.y - transform.position.y;
					UpdateParabolicMovement( 1, distance);
				}*/
			}break;
			case State.OuterSpace:
			case State.ExitingSpace:
			{
				float distance = transform.position.y - m_startParabolicPosition.y;
				UpdateParabolicMovement( -1, distance);
			}break;
			case State.Intro:
			{
				m_introTimer -= Time.deltaTime;
				if ( m_introTimer <= 0 )
				{
					ChangeState( State.Idle );
				}else{	
					float delta = m_introTimer / m_introDuration;
					m_destination = Vector3.left * introDisplacement * m_introDisplacementCurve.Evaluate(1.0f - delta);
					m_destination += m_introTarget;
					m_rbody.MovePosition( m_destination );
					if ( delta < m_introStopAnimationDelta )
					{
						m_animator.SetBool("boost", false);
						m_animator.SetBool("move", false);
					}
				}

			}break;
			case State.Latching:
			{
				m_impulse = Vector3.zero;
				m_rbody.velocity = Vector3.zero;
			}break;
			case State.Dead:
			{
				DeadFall();
			}break;
			case State.Reviving:
			{
				m_reviveTimer -= Time.deltaTime;
				transform.position = Vector3.Lerp(m_diePosition, m_revivePosition, m_reviveTimer/ m_reviveDuration);

				RotateToDirection(m_direction, false);
				m_desiredRotation = m_transform.rotation;

				if ( m_reviveTimer <= 0 )
				{
					transform.position = m_diePosition;
					switch( m_previousState )
					{
						case State.InsideWater:
						{
							ChangeState( State.InsideWater );
						}break;
						case State.OuterSpace:
						{
							ChangeState( State.OuterSpace );
						}break;
						default:
						{
							ChangeState( State.Idle);
						}break;
					}
				}
			}break;
		}
		
		m_rbody.angularVelocity = m_angularVelocity;

		m_lastSpeed = (transform.position - m_lastPosition).magnitude / Time.fixedDeltaTime;

		if ( m_state != State.Intro)
		{
			Vector3 position = transform.position;
			position.z = 0f;
			transform.position = position;
		}

		m_lastPosition = transform.position;
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the movement.
	/// </summary>
	private void UpdateMovement() 
	{
		Vector3 impulse = m_controls.GetImpulse(1); 
		if ( impulse != Vector3.zero )
		{
			// http://stackoverflow.com/questions/667034/simple-physics-based-movement

			// bool ignoreGravity = OverWaterMovement( ref impulse );
			bool ignoreGravity = false;

			// v_max = a/f
			// t_max = 5/f

			Vector3 gravityAcceleration = Vector3.zero;
            if (!ignoreGravity)
            {
                //if (impulse.y < 0) impulse.y *= m_dragonGravityModifier;
                gravityAcceleration = Vector3.down * 9.81f * m_dragonGravityModifier;// * m_dragonMass;
            }
            Vector3 dragonAcceleration = (impulse * m_dragonForce * GetTargetForceMultiplier()) / m_dragonMass;
            Vector3 acceleration = gravityAcceleration + dragonAcceleration;

			// stroke's Drag
			m_impulse = m_rbody.velocity;
			float impulseMag = m_impulse.magnitude;

			m_impulse += (acceleration * Time.deltaTime) - ( m_impulse.normalized * m_dragonFricction * impulseMag * Time.deltaTime); // velocity = acceleration - friction * velocity

			m_direction = m_impulse.normalized;
			RotateToDirection( impulse );


		}
		else
		{
			ComputeImpulseToZero();
			ChangeState( State.Idle );
		}

		ApplyExternalForce();

		m_rbody.velocity = m_impulse;
	}

	private bool OverWaterMovement( ref Vector3 impulse )
	{
		float degrees = impulse.ToAngleDegrees();
		// Debug.Log(degrees);
		float angle = 30.0f;
		if ( (degrees < angle && degrees > -angle) || (degrees > 180-angle || degrees < -180.0f+angle) )
		{
			// if hitting water check impulse angle 
			bool hittingWater = Physics.Raycast( m_transform.position, Vector3.down, out m_raycastHit, 100, 1<<LayerMask.NameToLayer("Water"));
			if ( hittingWater )
			{
				
				// Debug.Log("Correct!");
				// Correct impulse
				if (degrees < angle && degrees > -angle)
				{
					impulse = Vector3.right;
					return true;
				}else if(degrees > 180-angle || degrees < -180.0f+angle)
				{	
					impulse = Vector3.left;
					return true;
				}
			}
		}
		return false;
	}

	private void ApplyExternalForce()
	{
		m_impulse += m_externalForce;
		m_externalForce = Vector3.zero;
	}

	float GetTargetForceMultiplier()
	{
		return m_boostSpeedMultiplier * m_holdSpeedMultiplier * m_latchedOnSpeedMultiplier;
	}

	Vector3 Damping( Vector3 src, Vector3 dst, float dt, float factor)
	{
		return ((src * factor) + (dst * dt)) / (factor + dt);
	}

    /*
    private void UpdateMovement() 
	{
		Vector3 impulse = m_controls.GetImpulse(1); 
		if ( impulse != Vector3.zero )
		{
			// http://stackoverflow.com/questions/667034/simple-physics-based-movement

			// bool ignoreGravity = OverWaterMovement( ref impulse );
			bool ignoreGravity = false;

			// v_max = a/f
			// t_max = 5/f

			float gravity = 0;
			if (!ignoreGravity)
				gravity = 9.81f * m_dragonGravityModifier;
			Vector3 acceleration = Vector3.down * gravity * m_dragonMass;	// Gravity
			acceleration += impulse * m_dargonAcceleration * GetTargetSpeedMultiplier() * m_dragonMass;	// User Force

			// stroke's Drag
			m_impulse = m_rbody.velocity;
			float impulseMag = m_impulse.magnitude;

			m_impulse += (acceleration * Time.deltaTime) - ( m_impulse.normalized * m_dragonFricction * impulseMag * Time.deltaTime); // velocity = acceleration - friction * velocity

			m_direction = m_impulse.normalized;
			RotateToDirection( impulse );


		}
		else
		{
			ComputeImpulseToZero();
			ChangeState( State.Idle );
		}

		ApplyExternalForce();

		m_rbody.velocity = m_impulse;
	}
*/
	private void UpdateWaterMovement()
	{
		Vector3 impulse = m_controls.GetImpulse(1);
        if (impulse.y < 0) impulse.y *= m_inverseGravityWater;

		Vector3 gravityAcceleration = Vector3.up * 9.81f * m_dragonGravityModifier * 3.5f;   // Gravity
        Vector3 dragonAcceleration = (impulse * m_dragonForce * GetTargetForceMultiplier()) / m_dragonMass * m_accWaterFactor;
        Vector3 acceleration = gravityAcceleration + dragonAcceleration;

		// stroke's Drag
		m_impulse = m_rbody.velocity;

		float impulseMag = m_impulse.magnitude;
		m_impulse += (acceleration * Time.deltaTime) - ( m_impulse.normalized * m_dragonFricction * impulseMag * Time.deltaTime); // velocity = acceleration - friction * velocity
		m_direction = m_impulse.normalized;
		RotateToDirection(m_direction);


        m_rbody.velocity = m_impulse;

        m_inverseGravityWater -= Time.deltaTime * 0.28f;
        if (m_inverseGravityWater < 0.05f) m_inverseGravityWater = 0.05f;




        /*

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
			m_waterMovementModifier = Mathf.Lerp( m_waterMovementModifier, 0.0f, Time.deltaTime);
		}

		m_impulse = m_rbody.velocity;
		m_impulse.y += m_parabolicMovementValue * Time.deltaTime;
		m_impulse.x += m_dargonAcceleration * 0.75f * impulse.x * Time.deltaTime;

		m_direction = m_impulse.normalized;
		RotateToDirection( m_direction );
		m_rbody.velocity = m_impulse;
		*/
}
	
	private void UpdateParabolicMovement( float sign, float distance )
	{
		// Vector3 impulse = m_controls.GetImpulse(m_speedValue * m_currentSpeedMultiplier * Time.deltaTime * 0.1f);
		Vector3 impulse = m_controls.GetImpulse(Time.deltaTime * GetTargetForceMultiplier());

		// check collision with ground, only down?
		float moveValue = sign * (m_parabolicMovementConstant + ( m_parabolicMovementAdd * distance ));
		m_impulse.y += Time.deltaTime * moveValue;
		/*
		float abs = Mathf.Abs( moveValue ) * 10;
#if DEBUG
		if ( m_impulse.y < -abs || m_impulse.y > abs )
			Debug.LogWarning("Possible Movement error!");
#endif
		m_impulse.y = Mathf.Clamp( m_impulse.y, -abs, abs);
		*/

		m_impulse.x += impulse.x * m_parabolicXControl;

		m_direction = m_impulse.normalized;
		RotateToDirection( m_impulse );
		m_rbody.velocity = m_impulse;
	}

	private void UpdateIdleMovement() {

		Vector3 oldDirection = m_direction;
		CheckGround( out m_raycastHit);
		if (m_height < 2f * transform.localScale.y) { // dragon will fly up to avoid mesh intersection
			
			// Vector3 impulse = Vector3.up * m_speedValue * 0.1f;			
			Vector3 impulse = Vector3.up * 1 * 0.1f;			
			ComputeFinalImpulse(impulse);	
		}
		else 
		{
			ComputeImpulseToZero();
		}
		bool slowly = true;
		if ( current == null){
			if ( oldDirection.x > 0 ){
				m_direction = Vector3.right;	
			}else{
				m_direction = Vector3.left;
			}
		}else{
			m_direction = (m_impulse + m_externalForce).normalized;
			slowly = false;
		}

		RotateToDirection(m_direction, slowly);
		m_desiredRotation = m_transform.rotation;

		ApplyExternalForce();



		m_rbody.velocity = m_impulse;
	}

	private void IdleRotation( Vector3 oldRotation )
	{
		
	}

	private void DeadFall(){

		Vector3 oldDirection = m_direction;
		CheckGround( out m_raycastHit);
		if (m_height >= 2f * transform.localScale.y) { // dragon will fly up to avoid mesh intersection

			m_deadTimer += Time.deltaTime;

			m_impulse = m_rbody.velocity;
			if ( m_deadTimer < 1.5f * Time.timeScale )
			{
				float gravity = 9.81f * m_dragonGravityModifier * 35;
				Vector3 acceleration = Vector3.down * gravity * m_dragonMass;	// Gravity

				// stroke's Drag
				float impulseMag = m_impulse.magnitude;
				m_impulse += (acceleration * Time.deltaTime) - ( m_impulse.normalized * m_dragonFricction * impulseMag * Time.deltaTime); // velocity = acceleration - friction * velocity
			}
			else
			{
				ComputeImpulseToZero();
			}
		}

		if ( oldDirection.x > 0 )
		{
			m_direction = Vector3.right;	
		}
		else
		{
			m_direction = Vector3.left;
		}


		RotateToDirection(m_direction, false);
		m_desiredRotation = m_transform.rotation;

		ApplyExternalForce();
		m_rbody.velocity = m_impulse;
	}


	private void ComputeFinalImpulse(Vector3 _impulse) {
		// we keep the velocity value
		{
			// on air impulse formula, we don't fully change the velocity vector 
			// m_impulse = Vector3.Lerp(m_impulse, _impulse, m_impulseTransformationSpeed * Time.deltaTime);
			// m_impulse.Normalize();

			Util.MoveTowardsVector3XYWithDamping(ref m_impulse, ref _impulse, m_velocityBlendRate * Time.deltaTime, 8.0f);
			m_direction = m_impulse.normalized;
		}
	}

	private void ComputeImpulseToZero()
	{
		float impulseMag = m_impulse.magnitude;
		m_impulse += -(m_impulse.normalized * m_dragonFricction * impulseMag * Time.deltaTime);
		m_direction = m_impulse.normalized;
	}

	protected virtual void RotateToDirection(Vector3 dir, bool slowly = false)
	{
		float len = dir.magnitude;
		// m_rotBlendRate is param
		float blendRate = m_rotBlendRate;
		if ( GetTargetForceMultiplier() > 1 )
			blendRate *= 2;

		if ( slowly )
			blendRate = m_rotBlendRate * 0.2f;
		float slowRange = 0.05f;
		if(len < slowRange)
			blendRate *= (len/slowRange);

		
		if(blendRate > Mathf.Epsilon)
		{
			float angle = dir.ToAngleDegrees();
			float roll = angle;
			float pitch = angle;
			float yaw = 0;


			Quaternion qRoll = Quaternion.Euler(0.0f, 0.0f, roll);
			Quaternion qYaw = Quaternion.Euler(0.0f, yaw, 0.0f);
			Quaternion qPitch = Quaternion.Euler(pitch, 0.0f, 0.0f);
			m_desiredRotation = qYaw * qRoll * qPitch;
			Vector3 eulerRot = m_desiredRotation.eulerAngles;
			if (m_capVerticalRotation)
			{
				// top cap
				if (eulerRot.z > m_capUpRotationAngle && eulerRot.z < 180 - m_capUpRotationAngle) 
				{
					eulerRot.z = m_capUpRotationAngle;
				}
				// bottom cap
				else if ( eulerRot.z > 180 + m_capDownRotationAngle && eulerRot.z < 360-m_capDownRotationAngle )
				{
					eulerRot.z = -m_capDownRotationAngle;
				}
			}

			m_desiredRotation = Quaternion.Euler(eulerRot) * Quaternion.Euler(0,90.0f,0);
			m_angularVelocity = Util.GetAngularVelocityForRotationBlend(transform.rotation, m_desiredRotation, blendRate);
		}
		else
		{
			m_angularVelocity = Vector3.zero;
		}
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
		m_impulse = _force;
		ChangeState(State.Stunned);
	}

	public void AddExternalForce( Vector3 _force )
	{
		m_externalForce += _force;
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
	public Vector3 position {
		get { return transform.position; }
		set { transform.position = value; }
	}
	/// <summary>
	/// Obtain the current direction of the dragon.
	/// </summary>
	/// <returns>The direction the dragon is currently moving towards.</returns>
	public Vector3 direction {
		get { return m_direction; }
	}

	public Vector3 groundDirection {
		get { return Vector3.zero; }
	}
		
	public Vector3 velocity {
		get { return m_impulse; }
	}

	public Vector3 angularVelocity{
		get  { return m_rbody.angularVelocity; }
	}

	public float howFast
	{
		get{ 
			float f =m_impulse.magnitude / absoluteMaxSpeed;
			return Mathf.Clamp01(f);
		}
	}

	public float absoluteMaxSpeed
	{
		get 
		{
			return (m_dragonForce * m_boostMultiplier / m_dragonFricction) / m_dragonMass;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//

	public void NoDamageImpact()
	{
		m_animator.SetTrigger("impact");
	}

	public bool IsInsideWater() {
		return m_state == State.InsideWater;
	}

	public void StartWaterMovement( Collider _other )
	{
		// m_waterMovementModifier = 0;

		// Trigger animation
		m_animationEventController.OnInsideWater();

		// Trigger particles
		if ( m_particleController != null )
			m_particleController.OnEnterWater( _other );

        rbody.velocity = rbody.velocity * 2.0f;// m_waterImpulseMultiplier;
		m_impulse = rbody.velocity;


		// Change state
		ChangeState(State.InsideWater);

		// Notify game
		Messenger.Broadcast<bool>(GameEvents.UNDERWATER_TOGGLED, true);
	}

	public void EndWaterMovement( Collider _other )
	{
		if (m_animator )
			m_animator.SetBool("boost", false);

		// Trigger animation
		m_animationEventController.OnExitWater();

		// Trigger particles
		if (m_particleController != null)
			m_particleController.OnExitWater( _other );

		// Wait a second
		ChangeState( State.ExitingWater );

		// Notify game
		Messenger.Broadcast<bool>(GameEvents.UNDERWATER_TOGGLED, false);
	}

	public void StartSpaceMovement()
	{
		// Trigger animation
		m_animationEventController.OnEnterOuterSpace();

		// Trigger particles (min. speed required)
		if(m_particleController != null && Mathf.Abs(m_impulse.y) >= m_cloudTrailMinSpeed) {
			m_particleController.OnEnterOuterSpace();
		}

		// Change state
		ChangeState(State.OuterSpace);
	}

	public void EndSpaceMovement()
	{
		// Trigger animation
		m_animationEventController.OnExitOuterSpace();

		// Trigger particles (min. speed required)
		if(m_particleController != null && Mathf.Abs(m_impulse.y) >= m_cloudTrailMinSpeed) {
			m_particleController.OnExitOuterSpace();
		}

		ChangeState( State.ExitingSpace );
	}

	public void StartGrabPreyMovement(AI.Machine prey, Transform _holdPreyTransform)
	{
		// TODO: Calculate hold speed multiplier
		m_holdSpeedMultiplier = 0.8f;

		m_grab = true;
		m_holdPrey = prey;
		m_holdPreyTransform = _holdPreyTransform;
	
		m_preyPreviousTransformParent = prey.transform.parent;
		prey.transform.parent = m_tongue;
		
	}

	public void EndGrabMovement()
	{
		m_holdSpeedMultiplier = 1;
		m_holdPrey.transform.parent = m_preyPreviousTransformParent;
		m_holdPrey = null;
		m_holdPreyTransform = null;
		m_grab = false;
	}

	public void StartLatchMovement( AI.Machine prey, Transform _holdPreyTransform )
	{
		m_grab = false;
		m_holdPrey = prey;
		m_holdPreyTransform = _holdPreyTransform;
		ChangeState(State.Latching);
	}

	public void EndLatchMovement()
	{
		ChangeState(State.Idle);
		m_holdPrey = null;
		m_holdPreyTransform = null;
		m_grab = false;
	}

	/// <summary>
	/// Starts the latched on movement. Called When a prey starts latching on us
	/// </summary>
	public void StartLatchedOnMovement()
	{
		m_latchedOnSpeedMultiplier = 0.4f;
		m_latchedOn = true;
		m_animator.SetBool("eaten", true);
	}

	/// <summary>
	/// Ends the latched on movement. Called when a prey stops laching on us
	/// </summary>
	public void EndLatchedOnMovement()
	{
		m_latchedOnSpeedMultiplier = 1f;
		m_latchedOn = false;
		m_animator.SetBool("eaten", false);
	}

	public void StartIntroMovement(Vector3 introTarget)
	{
		m_introTarget = introTarget;
		m_transform.position = introTarget + Vector3.left * introDisplacement;
		m_introTimer = m_introDuration;
		ChangeState(State.Intro);
	}

	public void EndIntroMovement()
	{
		
	}

	public void Die(){
		ChangeState(State.Dead);
	}

	public void Revive(){
		ChangeState(State.Reviving);
	}

	/// <summary>
	/// Raises the trigger enter event.
	/// </summary>
	/// <param name="_other">Other.</param>
	void OnTriggerEnter(Collider _other)
	{
		if ( _other.tag == "Water" )
		{
			// Check direction?

			// Enable Bubbles
			if (IsAliveState())
				StartWaterMovement( _other );
			m_previousState = State.InsideWater;
		}
		else if ( _other.tag == "Space" )
		{
			if (IsAliveState())
				StartSpaceMovement();
			m_previousState = State.OuterSpace;
		}
		
	}

	void OnTriggerExit( Collider _other )
	{
		if ( _other.tag == "Water" )
		{
			// Disable Bubbles
			if (IsAliveState() )
				EndWaterMovement( _other );
			m_previousState = State.Idle;
		}
		else if ( _other.tag == "Space" )
		{
			if (IsAliveState())
				EndSpaceMovement();
			m_previousState = State.Idle;
		}
		
	}

	void OnCollisionEnter(Collision collision) 
	{
		switch( m_state )
		{
			case State.InsideWater:
			{
				if ( m_impulse.y < 0 )	// if going deep
				{
					//m_impulse = m_impulse * m_onWaterCollisionMultiplier;	
                    //m_impulse = m_impulse * 8;	
				}
			}break;

			case State.OuterSpace: {
				// Move down
				if(m_impulse.y > 0) {
					//m_impulse.y = 0;
					m_impulse.y = -1f;
				}

				// Smooth bounce effect on X
				m_impulse.x = -m_impulse.x * 0.05f;
			} break;

			default:
			{
			}break;
		}

	}

	private bool IsAliveState()
	{
		if (m_state == State.Dead || m_state == State.Reviving )
			return false;
		return true;
	}

}

