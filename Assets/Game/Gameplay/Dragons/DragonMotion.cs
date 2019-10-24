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
public class DragonMotion : MonoBehaviour, IMotion, IBroadcastListener {
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
		ChangingArea,
		Extra_1,
		Extra_2,
		Extra_3,
		None,
	};

	public enum ChangeAreaState
	{
		Enter,
		Wait_End_Eating,
		Loading_Next_Area,
		Exit
	};
	ChangeAreaState m_changeAreaState;

    //------------------------------------------------------------------//
    // MEMBERS															//
    //------------------------------------------------------------------//
    // Exposed members
    [SerializeField] private float m_stunnedTime;
	[SerializeField] private float m_velocityBlendRate = 256.0f;
	[SerializeField] protected float m_rotBlendRate = 350.0f;

	[SerializeField] protected bool m_capVerticalRotation = true;
	[SerializeField] private float m_capUpRotationAngle = 40.0f;
	[SerializeField] private float m_capDownRotationAngle = 60.0f;
	[SerializeField] private float m_noGlideAngle = 50.0f;

	public class PetsEatingTest
	{
		public bool m_eating = false;
	}

	protected Rigidbody m_rbody;
	public Rigidbody rbody
	{
		get{ return m_rbody; }
	}

	// References to components
	protected Animator  				m_animator;
	FlyLoopBehaviour		m_flyLoopBehaviour;
	protected DragonPlayer			m_dragon;
	// DragonHealthBehaviour	m_health;
	protected DragonControlPlayer m_controls;
	public DragonControlPlayer control
	{
		get{ return m_controls; }
	}
	protected DragonAnimationEvents 	m_animationEventController;
	DragonParticleController m_particleController;
    protected SphereCollider 	m_mainGroundCollider;
	Collider[] 				m_groundColliders;
	Collider[]				m_hitColliders;
	int m_hitCollidersSize = 0;
	public Collider[] hitColliders
	{
		get{ return m_hitColliders; }
	}
	public int hitCollidersSize
	{
		get{ return m_hitCollidersSize; }
	}
	DragonEatBehaviour		m_eatBehaviour;


	// Movement control
	protected Vector3 m_impulse;
	private float m_impulseMagnitude = 0;
	protected Vector3 m_direction;
    private Vector3 m_directionWhenBoostPressed;
    protected Vector3 m_externalForce;	// Used for wind flows, to be set every frame
	protected Quaternion m_desiredRotation;
	protected Vector3 m_angularVelocity = Vector3.zero;
	private float m_boostSpeedMultiplier;
	public float boostSpeedMultiplier
	{
		get {return m_boostSpeedMultiplier;}
		set { m_boostSpeedMultiplier = value; }
	}
    protected DragonBoostBehaviour m_boost;

	private float m_holdSpeedMultiplier;
	public float holdSpeedMultiplier
	{
		get {return m_holdSpeedMultiplier;}
		set { m_holdSpeedMultiplier = value; }
	}

	private float m_superSizeSpeedMultiplier = 1;
	public float superSizeSpeedMultiplier
	{
		get {return m_superSizeSpeedMultiplier;}
		set { m_superSizeSpeedMultiplier = value; }
	}


	private float m_latchedOnSpeedMultiplier = 0;
	private bool m_latchedOn = false;
	public bool isLatchedMovement
	{
		get{return m_latchedOn;}
	}

	private float m_stunnedTimer;

	
	/** Distance from the nearest ground collision below the dragon. The maximum distance checked is 10. */
	protected float m_height;
	public float height
	{
		get { return m_height; }
	}
	protected bool m_closeToGround = false;
	public bool closeToGround
	{
		get{ return m_closeToGround; }
	}
	protected Vector3 m_lastGroundHit = Vector3.zero;
	public Vector3 lastGroundHit
	{
		get{ return m_lastGroundHit; }
	}
	protected Vector3 m_lastGroundHitNormal = Vector3.zero;
	public Vector3 lastGroundHitNormal
	{
		get{ return m_lastGroundHitNormal; }
	}

	protected struct Sensors {
		public Transform top;
		public Transform bottom;
	};
	protected Sensors m_sensor;

	private Transform[] m_hitTargets;

	protected State m_state = State.None;
	public State state
	{
		get
		{
			return m_state;
		}
	}

	protected State m_previousState = State.Idle;

	// private Transform m_tongue;
	private Transform m_head;
	private Transform m_suction;
	private Transform m_cameraLookAt;
	protected Transform m_transform;

	[CommentAttribute("Back navigation bend multiplier when boost or attack target")]
	[Range(0, 1f)]
	public float m_backBlendMultiplier = 0.35f;
	private Vector2 m_currentFrontBend;
	private Vector2 m_currentBackBend;

	// Parabolic movement
	protected Vector3 m_startParabolicPosition;

	[Space]
	[SerializeField] private float m_cloudTrailMinSpeed = 7.5f;
	[SerializeField] private float m_outerSpaceRecoveryTime = 0.5f;
	[SerializeField] private float m_insideWaterRecoveryTime = 0.1f;
	private const float m_waterGravityMultiplier = 3.5f;
	private Vector3 m_waterEnterPosition;
	protected bool m_insideWater = false;
	public bool insideWater
	{
		get{ return m_insideWater; }
	}
	protected bool m_outterSpace = false;
	private string m_destinationArea = "";
	private Assets.Code.Game.Spline.BezierSpline m_followingSpline;
	private float m_followingClosestT;
	private int m_followingClosestStep;
	private float m_recoverTimer;

	private bool m_canMoveInsideWater = false;
	public bool canDive{
		get{
			return m_canMoveInsideWater;
		}

		set{
			m_canMoveInsideWater = value;
		}
	}

	// private float m_waterMovementModifier = 0;

	[SerializeField] public float m_dragonForce = 20;
	private float m_dragonForcePowerupMultiplier = 0;
	private float m_airCurrentModifier = 0f;
	public float m_dragonMass = 10;
	public float m_dragonFricction = 15.0f;
	public float m_dragonGravityModifier = 0.3f;
	[SerializeField] public float m_dragonAirGravityModifier = 0.3f;
	public float m_dragonAirExpMultiplier = 0.1f;
	public float m_dragonAirBoostForce = 4;
	public float m_dragonAirFreeFallMultiplier = 1;
	public float m_dragonAirBoostFallMultiplier = 1;
	public float m_dragonAirEnterSpeedMultiplier = 1;
	//TONI
	public bool m_startingParabolic = false;
    public float m_dragonWaterGravityModifier = 0.3f;
    private bool m_waterDeepLimit = false;
    protected bool m_spinning = true;
    protected bool m_canSpin = true;
    private bool m_rotateOnIdle = false;

    private bool m_waterMovement = false;
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	public Transform head   { get { if (m_head == null)   { m_head = m_transform.FindTransformRecursive("Dragon_Head");  } return m_head;   } }
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

	protected RaycastHit m_raycastHit = new RaycastHit();

	[Space]
	private float m_introTimer;
	private const float m_introDuration = 2.5f;
	private Vector3 m_introTarget;
	private Vector3 m_destination;
	private const float m_introDisplacement = 75;
	public float introDisplacement{ get{return m_introDisplacement * m_transform.localScale.x;} }
	public AnimationCurve m_introDisplacementCurve;
    public bool m_useBoostOnIntro = true;

	private AI.IMachine m_holdPrey = null;
	private Transform m_holdPreyTransform = null;

	private float m_boostMultiplier;

	private bool m_grab = false;

	private float m_inverseGravityWater = -0.5f;
	private float m_accWaterFactor = 0.72f;

    private RegionManager m_regionManager;
	public Current  	current { get; set; }

	private Vector3 m_diePosition;
	public Vector3 diePosition{ get{return m_diePosition;} }
	private Vector3 m_revivePosition;
	private float m_reviveTimer;
	private const float m_reviveDuration = 1.3f;
	protected float m_deadTimer = 0;
	private const float m_deadGravityMultiplier = 5;

	private float m_latchingTimer;

	private RectAreaBounds m_hitBounds = new RectAreaBounds(Vector3.zero, Vector3.one);
	public RectAreaBounds hitBounds
	{
		get{ return m_hitBounds; }
	}

	string m_previousArea = "";
	float m_switchAreaStart;


	public const float FlightCeiling = 370f;
	public const float SpaceStart = 171f;
    public int m_limitsCheck = 0;
    public Vector3 m_lastPhysicsValidPos = Vector3.zero;
    public Vector3 m_lastValidPos = Vector3.zero;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		m_transform = transform;

		// Get references
		m_animator			= m_transform.Find("view").GetComponent<Animator>();
		m_flyLoopBehaviour	= m_animator.GetBehaviour<FlyLoopBehaviour>();
		m_dragon			= GetComponent<DragonPlayer>();
        // m_health			= GetComponent<DragonHealthBehaviour>();
        m_boost             = GetComponent<DragonBoostBehaviour>();
		m_controls 			= GetComponent<DragonControlPlayer>();
		m_animationEventController = GetComponentInChildren<DragonAnimationEvents>();
		m_particleController = GetComponentInChildren<DragonParticleController>();
		Transform sensors	= m_transform.Find("sensors").transform;
		m_sensor.top 		= sensors.Find("TopSensor").transform;
		m_sensor.bottom		= sensors.Find("BottomSensor").transform;

		int n = 0;
		Transform t = null;
		Transform points = m_transform.Find("points");
		List<Transform> hitTargets = new List<Transform>();

		while (true) {
			t = points.Find("attack_" + n);
			if (t != null) {
				hitTargets.Add(t);
				n++;
			} else {
				break;
			}
		}
		m_hitTargets = hitTargets.ToArray();
		m_rbody = GetComponent<Rigidbody>();

		int playerLayer = LayerMask.NameToLayer("Player");
		int groundLayer = LayerMask.NameToLayer("PlayerGround");
		Collider[] colliders = GetComponentsInChildren<Collider>();
		List<Collider> hitCollidersArray = new List<Collider>();
		List<Collider> groundColliders = new List<Collider>();
		for( int i = 0; i<colliders.Length; i++ )
		{
			int colliderLayer = colliders[i].gameObject.layer;
			if ( colliderLayer == playerLayer)
			{
				// hitting collider
				hitCollidersArray.Add( colliders[i] );
			}
			else if ( colliderLayer == groundLayer )
			{
				// groundLayer
				groundColliders.Add( colliders[i]);
			}
		}
		m_groundColliders = groundColliders.ToArray();
		m_hitCollidersSize = hitCollidersArray.Count;
		m_hitColliders = hitCollidersArray.ToArray();


		// Find ground collider
		Transform ground = m_transform.FindTransformRecursive("ground");
		if ( ground != null )
		{
			m_mainGroundCollider = ground.GetComponent<SphereCollider>();
		}
		if ( m_mainGroundCollider == null )
		{
			m_mainGroundCollider = GetComponentInChildren<SphereCollider>();
		}

		m_eatBehaviour = GetComponent<DragonEatBehaviour>();
		m_height = 10f;

		m_boostSpeedMultiplier = 1;
		m_holdSpeedMultiplier = 1;
		m_latchedOnSpeedMultiplier = 1;


		m_currentFrontBend = Vector2.zero;
		m_currentBackBend = Vector2.zero;

        m_boostMultiplier = m_dragon.data.boostMultiplier;

		// Movement Setup
		RecalculateDragonForce();
        // m_dargonAcceleration = m_dragon.data.def.GetAsFloat("speedBase");
        m_dragonMass = m_dragon.data.mass;
        m_dragonFricction = m_dragon.data.friction;
        m_dragonGravityModifier = m_dragon.data.gravityModifier;
        m_dragonAirGravityModifier = m_dragon.data.airGravityModifier;
        m_dragonWaterGravityModifier = m_dragon.data.waterGravityModifier;
	}

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	protected virtual void Start() {
		// Initialize some internal vars
		m_stunnedTimer = 0;
		m_impulse = Vector3.zero;
		m_direction = Vector3.right;
		m_angularVelocity = Vector3.zero;
		m_lastPosition = m_transform.position;
		m_lastSpeed = 0;
		m_suction = m_eatBehaviour.suction;

        RegionManager.Init();
        m_regionManager = RegionManager.Instance;

        m_lastPhysicsValidPos = m_mainGroundCollider.transform.position;
        m_lastValidPos = m_transform.position;

		if (m_state == State.None)
			ChangeState(State.Fly);

	}

	public void RecalculateDragonForce()
	{
		//m_dragonForce = m_dragon.data.def.GetAsFloat("force");
		//TONI
		m_dragonForce = m_dragon.data.maxForce;
		m_dragonForce = m_dragonForce + m_dragonForce * m_dragonForcePowerupMultiplier / 100.0f;
	}

	public void AddSpeedModifier( float value )
	{
		m_dragonForcePowerupMultiplier += value;
		RecalculateDragonForce();
	}

	public void AddAirCurrentModifier(float _percentage) {
		m_airCurrentModifier += _percentage;
	}

	void OnEnable() {
		Messenger.AddListener(MessengerEvents.PLAYER_DIED, PnPDied);
		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}

	void OnDisable()
	{
		Messenger.RemoveListener(MessengerEvents.PLAYER_DIED, PnPDied);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}



    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                OnGameAreaEnter();
            }break;
        }
    }


	private void PnPDied()
	{
		m_impulse = Vector3.zero;
		m_rbody.velocity = m_impulse;
		m_deadTimer = 1000;
	}
	

	public void OnPetPreFreeRevive()
	{
		m_impulse = Vector3.zero;
		m_rbody.velocity = m_impulse;
		m_deadTimer = 1000;
	}

	private void OnGameAreaEnter()
	{
		if ( m_dragon.changingArea && m_changeAreaState == ChangeAreaState.Loading_Next_Area )
		{
			m_changeAreaState = ChangeAreaState.Exit;
		}
	}

	protected virtual void ChangeState(State _nextState) {
		if (m_state != _nextState) {
			// we are leaving old state
			switch (m_state) {
				case State.Fly:
					break;

				case State.Fly_Down:
					m_animator.SetBool( GameConstants.Animator.FLY_DOWN , false);
					break;

				case State.Stunned:
					m_impulse = Vector3.zero;
					m_stunnedTimer = 0;
					m_rbody.freezeRotation = false;
					break;
				case State.InsideWater:
				{
					m_animator.SetBool( GameConstants.Animator.SWIM , false);
					m_animator.SetBool( GameConstants.Animator.FLY_DOWN, false);
				}break;
				case State.OuterSpace:
				{
					m_animator.SetBool(GameConstants.Animator.FLY_DOWN, false);
				}break;
				case State.Intro:
				{
					// Enable boost!!
					InstanceManager.player.dragonBoostBehaviour.enabled = true;
					m_rbody.isKinematic = false;
					m_animator.SetBool(GameConstants.Animator.BOOST, false);
					m_animator.SetBool(GameConstants.Animator.MOVE, false);
 				}break;
				case State.Dead:
				{
					m_animator.ResetTrigger(GameConstants.Animator.DEAD);
				}break;
				case State.Reviving:
				{
					m_rbody.detectCollisions = true;
				}break;
				case State.ChangingArea:
				{
					HDTrackingManager.Instance.Notify_LoadingAreaEnd(m_previousArea,LevelManager.currentArea, Time.time - m_switchAreaStart);
					m_dragon.changingArea = false;
					Messenger.Broadcast(MessengerEvents.PLAYER_ENTERING_AREA);
				}break;
			}

			// entering new state
			switch (_nextState) {
				case State.Idle:
					m_animator.SetBool(GameConstants.Animator.MOVE, false);

					if (m_rotateOnIdle){
						if (m_direction.x < 0){
							m_direction = Vector3.left;
						}else {
							m_direction = Vector3.right;
						}
					}
					RotateToDirection( m_direction );
					break;

				case State.Fly:
					m_animator.SetBool(GameConstants.Animator.MOVE, true);
					break;

				case State.Fly_Down:
					m_animator.SetBool(GameConstants.Animator.MOVE, true);
					m_animator.SetBool(GameConstants.Animator.FLY_DOWN, true);
					break;

				case State.Stunned:
					m_rbody.velocity = m_impulse;
					m_rbody.angularVelocity = Vector3.zero;
					m_direction = m_impulse.normalized;
					m_stunnedTimer = m_stunnedTime;
					m_rbody.freezeRotation = true;
					break;
				case State.InsideWater:
				{
					// if ( m_canMoveInsideWater )

					m_animator.SetBool(GameConstants.Animator.SWIM, true);
					m_animator.SetBool(GameConstants.Animator.FLY_DOWN, true);
					if ( m_state != State.Stunned && m_state != State.Reviving && m_state != State.Latching){
	                    m_accWaterFactor = 0.80f;
	                    m_inverseGravityWater = 1.5f;
						m_startParabolicPosition = m_transform.position;
					}
				}break;
				case State.ExitingWater:
				{
					m_recoverTimer = m_insideWaterRecoveryTime;
                    m_accWaterFactor = 1.0f;
                }
                    break;
				case State.OuterSpace:
				{
					m_animator.SetBool(GameConstants.Animator.FLY_DOWN, true);

                    if (m_state != State.Stunned && m_state != State.Reviving && m_state != State.Latching)
                    {
						m_startParabolicPosition = m_transform.position;
                    }

				}break;
				case State.ExitingSpace:
				{
					m_recoverTimer = m_outerSpaceRecoveryTime;
				}break;
				case State.Intro:
				{
					m_rbody.isKinematic = true;
					m_animator.SetBool(GameConstants.Animator.BOOST, m_useBoostOnIntro);
					m_animator.SetBool(GameConstants.Animator.MOVE, true);
					m_introTimer = m_introDuration;
					m_impulse = Vector3.zero;
					m_direction = Vector3.right;
					m_desiredRotation = Quaternion.Euler(0,90.0f,0);
					m_transform.rotation = m_desiredRotation;


                    }break;
				case State.Latching:
				{
					m_latchingTimer = 0;
				}break;
				case State.Dead:
				{
					m_controls.enabled = false;
					m_animator.SetTrigger(GameConstants.Animator.DEAD);
					if ( m_previousState == State.InsideWater )
						m_animator.SetBool(GameConstants.Animator.SWIM, true);
					// Save Position!
					m_diePosition = m_transform.position;
					m_deadTimer = 0;
				}break;
				case State.Reviving:
				{
					m_controls.enabled = true;
					m_rbody.detectCollisions = false;
					m_reviveTimer = m_reviveDuration;
					m_impulse = Vector3.zero;
					m_rbody.velocity = Vector3.zero;
					m_revivePosition = m_transform.position;
					m_animator.Play(GameConstants.Animator.BASE_IDLE);
					m_transform.position = m_diePosition;
					if ( m_direction.x > 0 ){
						m_direction = Vector3.right;
					}else{
						m_direction = Vector3.left;
					}

				}break;
				case State.ChangingArea:
				{
					m_previousArea = LevelManager.currentArea;
					m_switchAreaStart = Time.time;
					HDTrackingManager.Instance.Notify_LoadingAreaStart( m_previousArea, m_destinationArea);
					m_changeAreaState = ChangeAreaState.Enter;
					// m_eatBehaviour.PauseEating();
					// Send event to tell pets we are leaging the area
					Vector3 pos = m_followingSpline.GetPoint( 0.5f );
					float distance = (pos - m_transform.position).magnitude;
					float estimatedTime = distance / absoluteMaxSpeed;

					Messenger.Broadcast<float>(MessengerEvents.PLAYER_LEAVING_AREA, estimatedTime);
				}break;
			}

			m_state = _nextState;
		}
	}
	/// <summary>
	/// Called once per frame.
	/// </summary>
	protected virtual void Update() {

#if UNITY_EDITOR
	if ( Input.GetKeyDown(KeyCode.B) )
		Bounce( GameConstants.Vector3.up );
#endif
		switch (m_state) {
			case State.Idle:
				if (m_controls.moving || m_boost.IsBoostActive()) {
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
			}break;
			case State.Latching:
			{
			}break;
			case State.InsideWater:
			{
				if (m_direction.y > -0.65f) {
					m_animator.SetBool(GameConstants.Animator.FLY_DOWN, false);
				}
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

		UpdateBodyBending();

		CheckForCurrents();
		CheckAllowGlide();

		// Update hitColliders Bounding box
		UpdateHitCollidersBoundingBox();

		CheckOutterSpace();

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
					current = m_regionManager.CheckIfObjIsInCurrent(gameObject, 1f + (m_airCurrentModifier / 100f));
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
            	// Do not tremble
				m_animator.SetBool(GameConstants.Animator.AGAINST_CURRENT, false);

				float angle = Util.ToAngleDegrees( m_direction );
				if ( angle > m_noGlideAngle && angle < 180-m_noGlideAngle ){
					m_flyLoopBehaviour.allowGlide = false;
					m_animator.SetBool(GameConstants.Animator.GLIDE, false);
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
					if ( current.IsInCurrentDirection( gameObject ) )	// if goes in the same direction as the current
					{
						m_flyLoopBehaviour.allowGlide = true;
						// Do not tremble
						m_animator.SetBool(GameConstants.Animator.AGAINST_CURRENT, false);
					}
					else
					{
						m_animator.SetBool(GameConstants.Animator.GLIDE, false);
						m_flyLoopBehaviour.allowGlide = false;
						// Allow tremble
						m_animator.SetBool(GameConstants.Animator.AGAINST_CURRENT, true);
					}
				}
			}
        }

	}

	protected virtual void CheckOutterSpace()
	{
		if (!m_outterSpace && m_transform.position.y > SpaceStart){
			OnEnterSpaceEvent();
		}else if ( m_outterSpace && m_transform.position.y < SpaceStart ){
			OnExitSpaceEvent();
		}
	}


	protected virtual void LateUpdate()
	{
		if ( m_holdPrey != null )
		{
			if (!m_grab)	// if latching
			{
				m_latchingTimer += Time.deltaTime;
				RotateToDirection( m_holdPreyTransform.forward, true );
				Vector3 deltaPosition = Vector3.Lerp( m_suction.position, m_holdPreyTransform.position, m_latchingTimer * 8);	// Mouth should be moving and orienting
				// Vector3 deltaPosition = m_holdPreyTransform.position;
				m_transform.position += deltaPosition - m_suction.position;
				m_impulse = Vector3.zero;
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
		m_animator.SetFloat(GameConstants.Animator.DIR_X, m_currentFrontBend.x);
		m_currentBackBend.x = Util.MoveTowardsWithDamping(m_currentBackBend.x, desiredBendX * backMultiplier, blendRate*dt, blendDampingRange);
		m_animator.SetFloat(GameConstants.Animator.BACK_DIR_X, m_currentBackBend.x);


		float desiredBendY = Mathf.Clamp(localDir.y*2.0f, -1.0f, 1.0f);		// max Y bend is about 45 degrees, so *2.
		m_currentFrontBend.y = Util.MoveTowardsWithDamping(m_currentFrontBend.y, desiredBendY, blendRate*dt, blendDampingRange);
		m_animator.SetFloat(GameConstants.Animator.DIR_Y, m_currentFrontBend.y);
		m_currentBackBend.y = Util.MoveTowardsWithDamping(m_currentBackBend.y, desiredBendY * backMultiplier, blendRate*dt, blendDampingRange);
		m_animator.SetFloat(GameConstants.Animator.BACK_DIR_Y, m_currentBackBend.y);

		// update 'body bending' boolean parameter, we use this in the anim state machine
		// to notify things like straight swim variations that they should break out and return
		// to normal directional swim
		float m_isBendingThreshold = 0.1f;
		float maxBend = Mathf.Max(Mathf.Abs(m_currentFrontBend.x), Mathf.Abs(m_currentFrontBend.y));
		bool isBending = (maxBend > m_isBendingThreshold);
		m_animator.SetBool(GameConstants.Animator.BEND, isBending);

	}

	void UpdateHitCollidersBoundingBox()
	{
		m_hitBounds.UpdateBounds( m_transform.position, Vector3.zero);
		m_hitBounds.Encapsulate( m_hitColliders );
	}


	/// <summary>
	/// Called once per frame at regular intervals.
	/// </summary>
	protected virtual void FixedUpdate() {

		m_closeToGround = false;
		switch (m_state) {
			case State.Idle:
				UpdateIdleMovement(Time.fixedDeltaTime);
				break;

			case State.Fly:
			case State.Fly_Down:
				UpdateMovement(Time.fixedDeltaTime);
				break;
			case State.InsideWater:
			case State.ExitingWater:
			{
				UpdateWaterMovement(Time.fixedDeltaTime);
			}break;
			case State.OuterSpace:
			case State.ExitingSpace:
		    {
                UpdateSpaceMovement(Time.fixedDeltaTime);
			}break;
			case State.Intro:
			{
				m_introTimer -= Time.deltaTime;
				if ( m_introTimer < 0.5f && !m_dragon.playable )
					m_dragon.playable = true;

				if ( m_introTimer <= 0 )
				{
					ChangeState( State.Idle );
				}else{
					float delta = m_introTimer / m_introDuration;
					m_destination = GameConstants.Vector3.left * introDisplacement * m_introDisplacementCurve.Evaluate(1.0f - delta);
					m_destination += m_introTarget;
					m_rbody.MovePosition( m_destination );
					/*
					if ( delta < m_introStopAnimationDelta )
					{
						m_animator.SetBool(GameConstants.Animator.BOOST, false);
						m_animator.SetBool(GameConstants.Animator.MOVE, false);
					}
					*/
				}

			}break;
			case State.Latching:
			{
				m_impulse = GameConstants.Vector3.zero;
				m_rbody.velocity = GameConstants.Vector3.zero;
			}break;
			case State.Dead:
			{
				if ( m_previousState == State.InsideWater || m_insideWater)
				{
					DeadDrowning( Time.fixedDeltaTime );
				}
				else
				{
					DeadFall( Time.fixedDeltaTime );
				}
			}break;
			case State.Reviving:
			{
				m_reviveTimer -= Time.deltaTime;
				// m_transform.position = Vector3.Lerp(m_diePosition, m_revivePosition, m_reviveTimer/ m_reviveDuration);
				m_transform.position = m_diePosition;

				RotateToDirection(m_direction, false);
				m_desiredRotation = m_transform.rotation;

				if ( m_reviveTimer <= 0 )
				{
					m_transform.position = m_diePosition;
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

				m_externalForce = GameConstants.Vector3.zero;	// Avoid building up external force!
			}break;
			case State.ChangingArea:
			{
				// TODO (miguel): Improve this part of code, specially questioning pets if they are eating
				switch( m_changeAreaState )
				{
					case ChangeAreaState.Enter:
					{
						m_followingSpline.GetClosestPointToPoint( m_transform.position, 100, out m_followingClosestT, out m_followingClosestStep);
						m_followingClosestT += 0.1f;	// Add dragon speed?
						if ( m_followingClosestT >= 0.5f )
						{
							m_followingClosestT = 0.5f;
							m_changeAreaState = ChangeAreaState.Wait_End_Eating;
							Stop();
						}
						Vector3 target = m_followingSpline.GetPoint( m_followingClosestT );
						UpdateMovementToPoint( Time.fixedDeltaTime, target );
					}break;
					case ChangeAreaState.Wait_End_Eating:
					{
						Stop();
						// Wait pets and dragon to stop eating
						bool noeating = true;
						// no eating and not beign latched on!
						if ( m_eatBehaviour.IsEating() || m_eatBehaviour.IsLatching() || m_eatBehaviour.IsGrabbing() || m_latchedOn)
						{
							noeating = false;
						}
						if ( noeating )
						{
							// Check pets
							PetsEatingTest test = new PetsEatingTest();
							Messenger.Broadcast<DragonMotion.PetsEatingTest>(MessengerEvents.PLAYER_ASK_PETS_EATING, test);
							noeating = !test.m_eating;
						}


						if ( noeating )
						{
							if ( !string.IsNullOrEmpty( m_destinationArea) )
							{
								InstanceManager.gameSceneController.SwitchArea(m_destinationArea);
								m_changeAreaState = ChangeAreaState.Loading_Next_Area;
							}
							else
							{
								m_changeAreaState = ChangeAreaState.Exit;
							}
						}
					}break;
					case ChangeAreaState.Loading_Next_Area:
					{
						Stop();
						// Waiting for Game Area Enter event
					}break;
					case ChangeAreaState.Exit:
					{
						m_followingSpline.GetClosestPointToPoint( m_transform.position, 100, out m_followingClosestT, out m_followingClosestStep);
						if ( m_followingClosestT >= 1.0f )
						{
							// Exit eating
							ChangeState( State.Fly );
						}
						m_followingClosestT += 0.1f;	// Add dragon speed?
						Vector3 target = m_followingSpline.GetPoint( m_followingClosestT );
						UpdateMovementToPoint( Time.fixedDeltaTime, target );
					}break;
				}

			}break;
		}
        AfterFixedUpdate();
	}
    
    protected void AfterFixedUpdate()
    {
        m_rbody.angularVelocity = m_angularVelocity;
        if ( m_spinning )
        {
            float d = Vector3.Dot(m_direction, m_transform.forward);
            if (d > 0)
            {
                m_rbody.AddRelativeTorque( Vector3.forward * 20 * d, ForceMode.VelocityChange);
            }
        }
        // if ( FeatureSettingsManager.IsDebugEnabled )
        {
            m_lastSpeed = (m_transform.position - m_lastPosition).magnitude / Time.fixedDeltaTime;
        }

        if ( m_state != State.Intro)
        {
            Vector3 newPhysicsPos = m_mainGroundCollider.transform.position;
            newPhysicsPos.z = 0;
            
            Vector3 pos = m_transform.position;
            pos.z = 0;
                
            // check pos
            m_limitsCheck++;
            if ( m_limitsCheck > 2 )
            {                
                if (DebugSettings.ingameDragonMotionSafe && Physics.Linecast( m_lastPhysicsValidPos, newPhysicsPos, out m_raycastHit, GameConstants.Layers.GROUND_PLAYER_COLL, QueryTriggerInteraction.Ignore ))
                {
                    // Return to previous position
                    pos = m_lastValidPos;
                    CustomOnCollisionEnter( m_raycastHit.collider, m_raycastHit.normal, m_raycastHit.point );
                }
                else
                {
                    m_lastPhysicsValidPos = newPhysicsPos;
                    m_lastValidPos = pos;
                }
            }
            m_transform.position = pos;
            
            
        }
        else
        {
            m_lastPhysicsValidPos = m_mainGroundCollider.transform.position;
            m_lastValidPos = m_transform.position;
        }

        

        /*
        Vector3 rewardDistance = RewardManager.distance;
        Vector3 diff = transform.position-m_lastPosition;
        rewardDistance.x += Mathf.Abs( diff.x );
        rewardDistance.y += Mathf.Abs( diff.y );
        rewardDistance.z += Mathf.Abs( diff.z );
        RewardManager.distance = rewardDistance;
        */

        m_impulseMagnitude = m_impulse.magnitude;
        m_lastPosition = m_transform.position;
    }

    private void UpdateMovementToPoint( float _deltaTime, Vector3 targetPoint )
	{
		Vector3 impulse = (targetPoint - m_transform.position).normalized;
		UpdateMovementImpulse( _deltaTime, impulse, true);
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the movement.
	/// </summary>
	protected void UpdateMovement( float _deltaTime)
	{
		Vector3 impulse = Vector3.zero;
		m_controls.GetImpulse(1, ref impulse);
		UpdateMovementImpulse( _deltaTime, impulse);
	}

	private void UpdateMovementImpulse( float _deltaTime, Vector3 impulse, bool rotateSlowly = false)
	{
		CheckGround( out m_raycastHit);
		if (m_boost.IsBoostActive())
        {
			if (impulse == GameConstants.Vector3.zero)
            {
                impulse = m_directionWhenBoostPressed;
            }
        }

		if (m_controls.moving )
			m_directionWhenBoostPressed = impulse;

		if ( impulse != GameConstants.Vector3.zero )
		{
			// http://stackoverflow.com/questions/667034/simple-physics-based-movement

			// v_max = a/f
			// t_max = 5/f

			Vector3 gravityAcceleration;
			gravityAcceleration.x = 0;
            gravityAcceleration.y = -9.81f * m_dragonGravityModifier;// * m_dragonMass;
			gravityAcceleration.z = 0;// * m_dragonMass;

            Vector3 dragonAcceleration = (impulse * m_dragonForce * GetTargetForceMultiplier()) / m_dragonMass;
            Vector3 acceleration = gravityAcceleration + dragonAcceleration;

			// stroke's Drag
			m_impulse = m_rbody.velocity;
			float impulseMag = m_impulse.magnitude;

			m_impulse += (acceleration * _deltaTime) - ( m_impulse.normalized * m_dragonFricction * impulseMag * _deltaTime); // velocity = acceleration - friction * velocity

			m_direction = m_impulse.normalized;
			RotateToDirection( impulse, rotateSlowly );
		}
		else
		{
			ComputeImpulseToZero(_deltaTime);
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
			bool hittingWater = Physics.Raycast( m_transform.position, GameConstants.Vector3.down, out m_raycastHit, 100, 1<<LayerMask.NameToLayer("Water"));
			if ( hittingWater )
			{

				// Debug.Log("Correct!");
				// Correct impulse
				if (degrees < angle && degrees > -angle)
				{
					impulse = GameConstants.Vector3.right;
					return true;
				}else if(degrees > 180-angle || degrees < -180.0f+angle)
				{
					impulse = GameConstants.Vector3.left;
					return true;
				}
			}
		}
		return false;
	}

	protected void ApplyExternalForce()
	{
		m_impulse += m_externalForce;
		m_externalForce = GameConstants.Vector3.zero;
	}

	protected float GetTargetForceMultiplier( bool includeBoost = true )
	{
		if ( includeBoost )
			return m_boostSpeedMultiplier * m_holdSpeedMultiplier * m_latchedOnSpeedMultiplier * m_superSizeSpeedMultiplier;
		return m_holdSpeedMultiplier * m_latchedOnSpeedMultiplier * m_superSizeSpeedMultiplier;
	}

	Vector3 Damping( Vector3 src, Vector3 dst, float dt, float factor)
	{
		return ((src * factor) + (dst * dt)) / (factor + dt);
	}

	private void UpdateWaterMovement( float _deltaTime )
	{
		Vector3 impulse = GameConstants.Vector3.zero;
		m_controls.GetImpulse(1, ref impulse);
		UpdateWaterMovementImpulse(_deltaTime, impulse);
    }

	private void UpdateWaterMovementImpulse( float _deltaTime, Vector3 impulse )
	{
		if (impulse.y < 0) impulse.y *= m_inverseGravityWater;

		Vector3 gravityAcceleration = GameConstants.Vector3.up * 9.81f * m_dragonWaterGravityModifier * m_waterGravityMultiplier;   // Gravity
        Vector3 dragonAcceleration = (impulse * m_dragonForce * GetTargetForceMultiplier()) / m_dragonMass * m_accWaterFactor;
        Vector3 acceleration = gravityAcceleration + dragonAcceleration;

		// stroke's Drag
		m_impulse = m_rbody.velocity;

		float impulseMag = m_impulse.magnitude;
		m_impulse += (acceleration * _deltaTime) - ( m_impulse.normalized * m_dragonFricction * impulseMag * _deltaTime); // velocity = acceleration - friction * velocity
		m_direction = Vector3.Lerp(m_direction, m_impulse.normalized, Time.deltaTime * 10 );
		RotateToDirection(m_direction);

        if ( !m_canMoveInsideWater )
        {
	        m_inverseGravityWater -= _deltaTime * 0.28f;
	        if (m_inverseGravityWater < 0.05f)
	        {
	        	m_inverseGravityWater = 0.05f;
	        }

			if (!m_waterDeepLimit)
			{
				float maxPushDown = ((m_inverseGravityWater * m_dragonForce * GetTargetForceMultiplier()) / m_dragonMass * m_accWaterFactor);
				if (maxPushDown < (gravityAcceleration.y + m_impulse.y))
				{
					m_waterDeepLimit = true;
					if (m_particleController.ShouldShowDeepLimitParticles())
						m_animator.SetTrigger(GameConstants.Animator.NO_AIR);
				}
			}
        }

		ApplyExternalForce();

		m_rbody.velocity = m_impulse;
	}

	protected void CheckStartParabolicHeight()
	{
		if ( m_boost.IsBoostActive() )
		{
			if (!m_startingParabolic) {
				m_startingParabolic = true;
				m_startParabolicPosition.y = m_transform.position.y;
			}
		}
		else
		{
			m_startingParabolic = false;
		}
	}

	protected void UpdateSpaceMovement(float _deltaTime)
	{
		// impulse direction
		Vector3 impulse = GameConstants.Vector3.zero;
		m_controls.GetImpulse(1, ref impulse);
		CheckStartParabolicHeight();

		if ( m_controls.moving )
			m_directionWhenBoostPressed = impulse;
		else
			impulse = m_directionWhenBoostPressed;

		// Calculate gravity acceleration
		Vector3 gravityAcceleration = GameConstants.Vector3.zero;
		gravityAcceleration = GameConstants.Vector3.down * 9.81f * m_dragonAirGravityModifier;
		if ( !m_boost.IsBoostActive() )
		{
			float distance = (m_transform.position.y - SpaceStart);
			if (distance > 0) {
				gravityAcceleration = gravityAcceleration + (GameConstants.Vector3.down * 9.81f * distance * m_dragonAirExpMultiplier);
			}
			if (m_lastSpeed > (absoluteMaxSpeed * m_dragonAirFreeFallMultiplier) && m_direction.y < 0f) gravityAcceleration = GameConstants.Vector3.zero;
		}
		impulse.y = 0;

		Vector3 dragonAcceleration = (impulse * m_dragonForce * GetTargetForceMultiplier()) / m_dragonMass;
		Vector3 acceleration = gravityAcceleration + dragonAcceleration;
		Vector3 impulseCapped = m_impulse;
		impulseCapped.y = 0;
		float impulseMag = impulseCapped.magnitude;
		m_impulse += (acceleration * _deltaTime) - (impulseCapped.normalized * m_dragonFricction * impulseMag * _deltaTime);	// drag only on x coordinate

		if ( m_boost.IsBoostActive() )	// if boosting push up
		{
			float distance = (m_transform.position.y - m_startParabolicPosition.y);
			if (distance >= 1){
				m_impulse += m_directionWhenBoostPressed * (m_dragonAirBoostForce / distance); //BOOST to player direction
			}else{
				if (m_directionWhenBoostPressed.y > 0) {
					m_impulse += m_directionWhenBoostPressed * m_dragonAirBoostForce;
				}else{
					m_impulse += m_directionWhenBoostPressed * m_dragonAirBoostForce * m_dragonAirBoostFallMultiplier * _deltaTime;
				}
			}
		}

		m_direction = m_impulse.normalized;

		//if ((boostSpeedMultiplier > 1) && (m_transform.position.y - SpaceStart) > 0 && (m_transform.position.y - SpaceStart) < 25 && (m_impulse.y > 0)) {
		//if ((m_transform.position.y - SpaceStart) > 0 && (m_transform.position.y - SpaceStart) < 425 && (m_impulse.y < -10)) {
		if (m_lastSpeed > (absoluteMaxSpeed * m_dragonAirFreeFallMultiplier) && m_direction.y < 0f) {
			RotateToDirection (m_direction, false, m_canSpin);
		} else
		{
			RotateToDirection (m_direction);
		}

		ApplyExternalForce();

		float topMargin = 10.0f;
		if(m_transform.position.y > (FlightCeiling-topMargin))
		{
			float t = 1.0f - Mathf.Clamp01((FlightCeiling - m_transform.position.y) / topMargin);
			float clamp = Mathf.Lerp(60, 0.0f, t);
			m_impulse.y = Mathf.Min(m_impulse.y, clamp);
		}

		m_rbody.velocity = m_impulse;
	}

	private void UpdateIdleMovement(float _deltaTime) {

		Vector3 oldDirection = m_direction;
		CheckGround( out m_raycastHit );
		if ( m_closeToGround ) { // dragon will fly up to avoid mesh intersection

			Vector3 impulse = GameConstants.Vector3.up * 0.1f;
			Util.MoveTowardsVector3XYWithDamping(ref m_impulse, ref impulse, m_velocityBlendRate * _deltaTime, 8.0f);
		}
		else
		{
			ComputeImpulseToZero(_deltaTime);
		}
		bool slowly = true;
		if ( current == null ){
			if (m_rotateOnIdle || m_closeToGround){
				if ( oldDirection.x > 0 ){
					m_direction = GameConstants.Vector3.right;
				}else{
					m_direction = GameConstants.Vector3.left;
				}
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

	protected void DeadFall(float _deltaTime){

		Vector3 oldDirection = m_direction;
		CheckGround( out m_raycastHit);
		if (!m_closeToGround) { // dragon will fly up to avoid mesh intersection

			m_deadTimer += _deltaTime;
			m_impulse = m_rbody.velocity;
			if ( m_deadTimer < 1.5f * Time.timeScale )
			{
				float gravity = 9.81f * m_dragonGravityModifier * m_deadGravityMultiplier;
				Vector3 acceleration = GameConstants.Vector3.down * gravity * m_dragonMass;	// Gravity

				// stroke's Drag
				float impulseMag = m_impulse.magnitude;
				m_impulse += (acceleration * _deltaTime) - ( m_impulse.normalized * m_dragonFricction * impulseMag * _deltaTime); // velocity = acceleration - friction * velocity
			}
			else
			{
				ComputeImpulseToZero(_deltaTime);
			}
		}

		if ( oldDirection.x > 0 )
		{
			m_direction = GameConstants.Vector3.right;
		}
		else
		{
			m_direction = GameConstants.Vector3.left;
		}


		RotateToDirection(m_direction, false);
		m_desiredRotation = m_transform.rotation;

		ApplyExternalForce();
		m_rbody.velocity = m_impulse;
	}


	protected void DeadDrowning(float _deltaTime){

		Vector3 oldDirection = m_direction;
		m_deadTimer += Time.deltaTime;
		m_impulse = m_rbody.velocity;
		if ( m_deadTimer < 1.5f * Time.timeScale )
		{
			Vector3 gravityAcceleration = GameConstants.Vector3.up * 9.81f * m_dragonGravityModifier * m_waterGravityMultiplier * m_deadGravityMultiplier;   // Gravity
			if ( m_transform.position.y > (m_waterEnterPosition.y - m_mainGroundCollider.radius))
				gravityAcceleration -= gravityAcceleration;

			Vector3 acceleration = gravityAcceleration;	// Gravity

			// stroke's Drag
			float impulseMag = m_impulse.magnitude;
			m_impulse += (acceleration * _deltaTime) - ( m_impulse.normalized * m_dragonFricction * impulseMag * _deltaTime); // velocity = acceleration - friction * velocity
		}
		else
		{
			ComputeImpulseToZero(_deltaTime);
		}

		if ( oldDirection.x > 0 )
		{
			m_direction = GameConstants.Vector3.right;
		}
		else
		{
			m_direction = GameConstants.Vector3.left;
		}

		RotateToDirection(m_direction, false);
		m_desiredRotation = m_transform.rotation;

		ApplyExternalForce();
		m_rbody.velocity = m_impulse;
	}

	protected void ComputeImpulseToZero(float _deltaTime)
	{
		float impulseMag = m_impulse.magnitude;
		m_impulse += -(m_impulse.normalized * m_dragonFricction * impulseMag * _deltaTime * 0.37f);
	}

	protected virtual void RotateToDirection(Vector3 dir, bool slowly = false, bool spin = false)
	{
		float blendRate = m_rotBlendRate;
		if ( GetTargetForceMultiplier() > 1 )
			blendRate *= 2;

		if ( slowly )
			blendRate = m_rotBlendRate * 0.2f;

		if(blendRate > Mathf.Epsilon)
		{

			/*
			float angle = dir.ToAngleDegrees();
			float roll = angle;
			float pitch = angle;
			Quaternion qRoll = Quaternion.Euler(0.0f, 0.0f, roll);
			Quaternion qPitch = Quaternion.Euler(pitch, 0.0f, 0.0f);
			m_desiredRotation = qRoll * qPitch;
			*/
			float rads = dir.ToAngleRadiansXY();
			m_desiredRotation = MathUtils.DragonRotation( rads );

			Vector3 eulerRot 	= m_desiredRotation.eulerAngles;
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
			m_angularVelocity = Util.GetAngularVelocityForRotationBlend(m_transform.rotation, m_desiredRotation, blendRate);


		}
		else
		{
			m_angularVelocity = GameConstants.Vector3.zero;
		}

		if ( m_spinning != spin )
			m_animator.SetBool(GameConstants.Animator.SPIN, spin);
		m_spinning = spin;

	}


	protected virtual bool CheckGround(out RaycastHit _bottomHit) {
		Vector3 distance = GameConstants.Vector3.down * 10f;
		bool hit_Bottom = false;

		Vector3 bottomSensor  = m_sensor.bottom.position;
        hit_Bottom = Physics.Linecast(bottomSensor, bottomSensor + distance, out _bottomHit, GameConstants.Layers.GROUND_PLAYER_COLL, QueryTriggerInteraction.Ignore );

		if (hit_Bottom) {
			m_height = _bottomHit.distance * m_transform.localScale.y;
			m_closeToGround = m_height < 1f;
			m_lastGroundHit = _bottomHit.point;
			m_lastGroundHitNormal = _bottomHit.normal;
		} else {
			m_height = 100f;
			m_closeToGround = false;
		}
		return m_closeToGround;
	}

	private bool CheckCeiling(out RaycastHit _leftHit) {
		Vector3 distance = GameConstants.Vector3.up * 10f;
		bool hit_L = false;

		Vector3 leftSensor 	= m_sensor.top.position;
        hit_L = Physics.Linecast(leftSensor, leftSensor + distance, out _leftHit, GameConstants.Layers.GROUND);

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
		m_impulse = GameConstants.Vector3.zero;
		m_rbody.velocity = m_impulse;
		m_direction = m_impulse.normalized;
		m_desiredRotation = m_transform.rotation;
		m_angularVelocity = GameConstants.Vector3.zero;
	}

	public virtual void AddForce(Vector3 _force, bool isDamage = true) {
		if ( m_dragon.IsInvulnerable() )
			return;
		if ( isDamage )
		{
			m_animator.SetTrigger(GameConstants.Animator.DAMAGE);
		}
		m_impulse = _force;
		if ( IsAliveState() )
			ChangeState(State.Stunned);
	}

	public void AddExternalForce( Vector3 _force )
	{
		m_externalForce += _force;
	}

	public Transform GetAttackPointNear(Vector3 _point) {
		Transform target = transform;
		float minDistSqr = 999999f;

		int size = m_hitTargets.Length;
		for (int i = 0; i < size; i++) {
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
	public Quaternion orientation {
		get { return m_transform.rotation; }
		set { m_transform.rotation = value; }
	}

	public Vector3 position {
		get { return m_transform.position; }
		set { m_transform.position = value; }
	}

    public Vector3 forward {
        get { return m_transform.forward; }
    }

	/// <summary>
	/// Obtain the current direction of the dragon.
	/// </summary>
	/// <returns>The direction the dragon is currently moving towards.</returns>
	public Vector3 direction {
		get { return m_direction; }
	}

	public Vector3 groundDirection {
		get { return GameConstants.Vector3.zero; }
	}

	public Vector3 velocity {
		get { return m_impulse; }
	}

	public float speed{
		get { return m_impulseMagnitude; }
	}

	public Vector3 angularVelocity{
		get  { return m_rbody.angularVelocity; }
	}

	public float howFast
	{
		get{
			float f = m_impulseMagnitude / absoluteMaxSpeed;
			return Mathf.Clamp01(f);
		}
	}

	public virtual float absoluteMaxSpeed
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
		m_animator.SetTrigger(GameConstants.Animator.IMPACT);
	}

	public bool IsInsideWater() {
		return m_state == State.InsideWater;
	}

    public bool IsInSpace()
    {
        return m_state == State.OuterSpace;
    }

    public void StartWaterMovement( Collider _other )
	{
        if (!m_waterMovement)
        {
            m_waterMovement = true;

    		m_waterDeepLimit = false;

    		bool createsSplash = false;
    		// Trigger particles
    		if ( m_particleController != null )
    			createsSplash = m_particleController.OnEnterWater( _other );

    		// Trigger animation
    		m_animationEventController.OnInsideWater(createsSplash);

    		if ( CanChangeStateToInsideWater() )
    		{
    			if ( m_impulse.y < 0 )
    			{
    				m_impulse = m_impulse * 2.0f;
					m_rbody.velocity = m_impulse;
    			}

    			// Change state
    			ChangeState(State.InsideWater);
    		}

    		// Notify game
    		Messenger.Broadcast<bool>(MessengerEvents.UNDERWATER_TOGGLED, true);
        }
	}
    
    protected virtual bool CanChangeStateToInsideWater()
    {
        return m_state != State.Latching;
    }

	public void EndWaterMovement( Collider _other )
	{
        if (m_waterMovement)
        {
            m_waterMovement = false;

    		if (m_animator )
    			m_animator.SetBool(GameConstants.Animator.BOOST, m_boost.IsBoostActive());

    		bool createsSplash = false;
    		// Trigger particles
    		if (m_particleController != null)
    			createsSplash = m_particleController.OnExitWater( _other );

    		// Trigger animation
    		m_animationEventController.OnExitWater(createsSplash);

    		if (CanChangeStateToExitWater())
    		{
    			// Wait a second
    			ChangeState( State.ExitingWater );
    		}

    		// Notify game
    		Messenger.Broadcast<bool>(MessengerEvents.UNDERWATER_TOGGLED, false);
        }
	}
    
    protected virtual bool CanChangeStateToExitWater()
    {
        return m_state != State.Latching;
    }
    

	public void StartSpaceMovement()
	{
		// Trigger animation
		m_animationEventController.OnEnterOuterSpace();

		if ( m_state != State.Latching )
		{
			// Change state

			// Check min speed!
			if (m_impulse.magnitude < absoluteMaxSpeed * 0.75f)
			{
				m_impulse = m_impulse.normalized * absoluteMaxSpeed * m_dragonAirEnterSpeedMultiplier;
			}

			ChangeState(State.OuterSpace);
		}

		// If we didn't show the boost on space message, do it here
		if (UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.BOOST)) {
			if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.BOOST_SPACE)) {
				Messenger.Broadcast(MessengerEvents.BOOST_SPACE);
				UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.BOOST_SPACE, true);
			}
		}

        // Notify game
        Messenger.Broadcast<bool>(MessengerEvents.INTOSPACE_TOGGLED, true);
    }

	public void EndSpaceMovement()
	{
		// Trigger animation
		m_animationEventController.OnExitOuterSpace();

		if ( m_state != State.Latching )
		{
			ChangeState( State.ExitingSpace );
		}
    }

	public void StartGrabPreyMovement(AI.IMachine prey, Transform _holdPreyTransform)
	{
		// TODO: Calculate hold speed multiplier
		m_holdSpeedMultiplier = 0.6f;
		m_grab = true;
		m_holdPrey = prey;
		m_holdPreyTransform = _holdPreyTransform;
	}

	public void EndGrabMovement()
	{
		m_holdSpeedMultiplier = 1;
		m_holdPrey = null;
		m_holdPreyTransform = null;
		m_grab = false;
	}

	public void StartLatchMovement( AI.IMachine prey, Transform _holdPreyTransform )
	{
		m_grab = false;
		m_holdPrey = prey;
		m_holdPreyTransform = _holdPreyTransform;
		ChangeState(State.Latching);
	}

	public void EndLatchMovement()
	{
		if ( m_state != State.Dead )
			ChangeState(m_previousState);
		m_holdPrey = null;
		m_holdPreyTransform = null;
		m_grab = false;
	}

	public void StartedSkimming()
	{
		m_animationEventController.StartedSkimming();
	}

	public void EndedSkimming()
	{
		m_animationEventController.EndedSkimming();
	}

	/// <summary>
	/// Starts the latched on movement. Called When a prey starts latching on us
	/// </summary>
	public void StartLatchedOnMovement()
	{
		m_latchedOnSpeedMultiplier = 0.7f;
		m_latchedOn = true;
		m_animator.SetBool(GameConstants.Animator.HOLDED, true);
	}

	/// <summary>
	/// Ends the latched on movement. Called when a prey stops laching on us
	/// </summary>
	public void EndLatchedOnMovement()
	{
		m_latchedOnSpeedMultiplier = 1f;
		m_latchedOn = false;
		m_animator.SetBool(GameConstants.Animator.HOLDED, false);
	}

	public void StartIntroMovement(Vector3 introTarget)
	{
		m_introTarget = introTarget;
		m_transform.position = introTarget + GameConstants.Vector3.left * introDisplacement;
		m_introTimer = m_introDuration;
		ChangeState(State.Intro);
	}

	public void EndIntroMovement()
	{

	}

    public void MoveToSpawnPosition(Vector3 _pos) {
        m_lastPosition = _pos;
        m_lastPhysicsValidPos = _pos;
        m_lastValidPos = _pos;
        m_transform.position = _pos;
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
	// This is done on Dragon Head Trigger now
	protected virtual void OnTriggerEnter(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			OnEnterWaterEvent( _other );
		}
		/*
		else if ( _other.CompareTag("Space"))
		{
			OnEnterSpaceEvent();
		}
		*/
		else if ( _other.CompareTag("AreaChange")  )
		{
			OnAreaChangeEvent( _other );
		}
		else if ( _other.CompareTag("Bounce") )
		{
			Bounce(GameConstants.Vector3.up);
		}
	}

	private void Bounce( Vector3 inNormal )
	{
		m_impulse = Vector3.Reflect( m_impulse, inNormal);
		if ( m_impulse.magnitude < absoluteMaxSpeed * 3.5f )
		{
			m_impulse = m_impulse.normalized * absoluteMaxSpeed * 4f;
		}
	}

	public void OnEnterWaterEvent( Collider _other )
	{
		if (!m_insideWater)
		{
			// if we are exiting water and moving up, what touched the water was because of the animation
			if ( m_state != State.ExitingWater || m_rbody.velocity.y <= 0 )
			{
				// Check direction?
				m_waterEnterPosition = m_transform.position;
				m_insideWater = true;
				// Modify Y to match real pos?

				// Enable Bubbles
				if (IsAliveState())
				{
					StartWaterMovement( _other );
					m_previousState = State.InsideWater;
				}
				else
				{
					m_animator.SetBool( GameConstants.Animator.SWIM , true);
				}
			}
			
		}
	}

	public void OnEnterSpaceEvent()
	{
		if (!m_outterSpace)
		{
			m_outterSpace = true;
			if (IsAliveState())
			{
				StartSpaceMovement();
				m_previousState = State.OuterSpace;
				// Trigger particles (min. speed required)
		        if (m_particleController != null) {
					m_particleController.OnEnterOuterSpace( Mathf.Abs(m_impulse.y) >= m_cloudTrailMinSpeed );
				}
				// Notify game
        		Messenger.Broadcast<bool>(MessengerEvents.INTOSPACE_TOGGLED, true);
			}
		}
	}

	public void OnAreaChangeEvent(Collider _other)
	{
        //if ( !m_dragon.changingArea && InstanceManager.gameSceneController != null && _other.bounds.Intersects(m_mainGroundCollider.bounds))
		if ( IsAliveState() && !m_dragon.changingArea && InstanceManager.gameSceneController != null && _other.bounds.Intersects(m_mainGroundCollider.bounds))
		{
			string destinationArea = _other.GetComponent<AreaPortal>().m_areaPortal;
			if ( LevelManager.currentArea != destinationArea )
			{
				m_dragon.changingArea = true;
				// Start moving through Spline
				m_followingSpline = _other.GetComponent<Assets.Code.Game.Spline.BezierSpline>();
				m_destinationArea = destinationArea;
				ChangeState(State.ChangingArea);
			}
		}
	}

	// This is done on Dragon Head Trigger now
	void OnTriggerExit( Collider _other )
	{
		if ( _other.CompareTag("Water") )
		{
			OnExitWaterEvent(_other);
		}
		/*
		else if ( _other.CompareTag("Space") )
		{
			OnExitSpaceEvent( );
		}
		*/
	}


	public void OnExitWaterEvent(Collider _other)
	{
		if (m_insideWater)
		{
			m_insideWater = false;
			// Disable Bubbles
			if (IsAliveState())
			{
				EndWaterMovement( _other );
				m_previousState = State.Idle;
			}
			else
			{
				m_animator.SetBool( GameConstants.Animator.SWIM, false);
			}
		}
	}

	public void OnExitSpaceEvent()
	{
		if (m_outterSpace )
		{
			m_outterSpace = false;
			if (IsAliveState())
			{
				EndSpaceMovement();
				m_previousState = State.Idle;
				// Trigger particles (min. speed required)
				if(m_particleController != null ) {
					m_particleController.OnExitOuterSpace(  Mathf.Abs(m_impulse.y) >= m_cloudTrailMinSpeed );
				}
				// Notify game
        		Messenger.Broadcast<bool>(MessengerEvents.INTOSPACE_TOGGLED, false);
			}
		}
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
        CustomOnCollisionEnter(collision.collider, collision.contacts[0].normal, collision.contacts[0].point);
	}
    
    protected virtual void CustomOnCollisionEnter( Collider _collider, Vector3 _normal, Vector3 _point )
    {
        if ( _collider.CompareTag("Bounce") )
        {
            if (Vector3.Dot( _normal, m_impulse) < 0)
                Bounce( _normal );
        }

        switch( m_state )
        {
            case State.InsideWater:
            {
            }break;

            case State.OuterSpace: {
                OutterSpaceCollision( _normal );
            } break;

            default:
            {
            }break;
        }
    }
    

    public virtual void OnCollisionStay(Collision collision)
    {
        switch (m_state)
        {
            case State.OuterSpace:
            {
				OutterSpaceCollision( collision.contacts[0].normal );
            }
            break;
        }
    }

    void OutterSpaceCollision( Vector3 normal)
    {
		if(m_impulse.y <= 0)
        {
        	if ( normal.y >= 0.15f )
        	{
	        	float magnitude = m_impulse.magnitude;
				m_impulse = m_impulse - Vector3.Dot( m_impulse, normal) * normal;
			    m_impulse.z = 0;
			    m_impulse = m_impulse.normalized * magnitude;
			}
        }
        else{
        	if ( normal.y <= -0.85f )
        		m_impulse.y = 0;
        }
		m_impulse -= m_impulse * 0.05f;
		m_rbody.velocity = m_impulse;
    }

    protected bool IsAliveState()
	{
		if (m_state == State.Dead || m_state == State.Reviving )
			return false;
		return true;
	}

	public virtual bool CanIResumeEating()
	{
		bool ret = true;
		return ret;
	}

	/// <summary>
	/// Determines whether this instance is breaking movement. To be overrided by special dragons movement, ex: Sonic
	/// </summary>
	/// <returns><c>true</c> if this instance is breaking movement; otherwise, <c>false</c>.</returns>
	public virtual bool IsBreakingMovement()
	{
		return false;
	}

	protected virtual void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube( m_hitBounds.center, m_hitBounds.bounds.size);
	}
}
