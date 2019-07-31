using UnityEngine;
using System.Collections.Generic;

public abstract class EatBehaviour : MonoBehaviour, ISpawnable {
	protected class PreyData {		
		public float absorbTimer;
		public float eatingAnimationTimer;
		public float eatingAnimationDuration;
		public Transform startParent;
		public Vector3 startScale;
		public AI.IMachine prey;
		public Quaternion dyingRotation;
	};

	private const float m_rotateToMouthSpeed = 800;
	private const float m_rotateToMouthThreshold = 5;

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------	

	// [SerializeField]private float m_absorbTime = 1;
	[SerializeField]private float m_minEatAnimTime = 1;
	[SerializeField]protected float m_eatDistance = 1;
	// Multiplies eating distance to detect targets
	[SerializeField]protected float m_eatDetectionRadiusMultiplier = 2.75f;
	// Inceases bite distance based on speed
	[SerializeField]protected float m_eatDetectionSpeedRadiusMultiplier = 0.15f;

	public float eatDistanceSqr { get { return (m_eatDistance * transform.localScale.x) * (m_eatDistance * transform.localScale.x); } }
	[SerializeField] protected ParticleData m_holdingBloodParticle = new ParticleData( "PS_Blood_Explosion_Medium", "Blood", Vector3.zero);
	[SerializeField] protected ParticleData m_holdingFreezeParticle = new ParticleData( "PS_IceExplosionSmall", "", Vector3.zero);

    private static int maxPreysSoFar = 0;
    private const int MAX_PREYS = 40; // Max amount of preys allowed to be eaten simultaneously
    protected PreyData[] m_prey;// each prey that falls near the mouth while running the eat animation, will be swallowed at the same time
    private AI.IMachine[] m_preysToEat; // Temporary array needed when eating. It's defined here to prevent memory from being generated when eating

    protected int PreyCount { get; set; }
    
	protected DragonTier m_tier;
	protected float m_eatSpeedFactor = 1f;	// eatTime (s) = eatSpeedFactor * preyResistance

	// Hold stuff
	private float m_holdPreyTimer = 0;
	protected AI.IMachine m_holdingPrey = null;
	protected DragonPlayer m_holdingPlayer = null;
	protected DragonHealthBehaviour m_holdingPlayerHealth = null;
	protected Transform m_holdTransform = null;
	public Transform holdTransform{ get{ return m_holdTransform; } }
	protected HoldPreyPoint m_holdPoint;
	protected bool m_grabbingPrey = false;
	protected Transform m_grabbingPreyPreviousTransformParent = null;
	protected bool m_advanceHold = true;

	// Attacking/Targeting
	protected Transform m_attackTarget = null;
	protected float m_attackTimer = 0;

	// First position when swallowing. Rotation has to end here
	protected Transform m_suction;
	public Transform suction
	{
		get{ return m_suction; }
	}
	// Second position when swallowing. Prey moved directly to this point while applying swallow shader
	protected Transform m_swallow;
	// Only used to update swallow shader
	protected Transform m_bite;
	// Used to check near target and check distance for killing/eating. Also to reparent eating preys
	protected Transform m_mouth;
	public Transform mouth
	{
		get{ return m_mouth; }
	}

	protected IMotion m_motion;

	private bool m_useBlood = true;
	private List<GameObject> m_bloodEmitter;
	private List<GameObject> m_freezeEmitter;

	public string m_burpSound = "";
	// public AudioSource m_burpAudio;

	private float m_noAttackTime = 0;
	private float m_holdingBlood = 0;

	// config
	protected IEntity.Type m_type = IEntity.Type.OTHER;		// If eating entity is the player, a pet or other
	protected bool m_rewardsPlayer = false;	
	protected bool m_canLatchOnPlayer = false;
	public bool canLatchOnPlayer
	{
		get{ return m_canLatchOnPlayer; }
		set{ m_canLatchOnPlayer = value; }
	}
	protected bool m_canBitePlayer = false;
	public bool canBitePlayer
	{
		get{ return m_canBitePlayer; }
		set{ m_canBitePlayer = value; }
	}

	public virtual bool canMultipleLatchOnPlayer { get { return false; } }

	[SerializeField] private bool m_canEatEntities = false;
	private bool m_eatingEntitiesEnabled = false;
	public bool eatingEntitiesEnabled { set { m_eatingEntitiesEnabled = value && m_canEatEntities; } get { return m_eatingEntitiesEnabled; } }

	protected bool m_waitJawsEvent = false;	// if wait for jaws closing event or just wait time
	protected bool m_limitEating = false;	// If there is a limit on eating preys at a time
	protected int m_limitEatingValue = 1;	// limit value
	protected bool m_canHold = true;		// if this eater can hold a prey

	protected virtual bool isAquatic { get { return false; } }

		// Hold config
	protected float m_holdStunTime;
	protected float m_holdDamage;
	public float holdDamage{ get{ return m_holdDamage; } set{ m_holdDamage = value; } }
	protected float m_holdBoostDamageMultiplier;
	protected float m_holdHealthGainRate;
	protected float m_holdDuration;
	public float holdDuration{ get{ return m_holdDuration; } set{ m_holdDuration = value; } }

	// Arc detection values
	private const float m_minAngularSpeed = 0;
	private const float m_maxAngularSpeed = 12;
	private const float m_minArcAngle = 75;
	private const float m_maxArcAngle = 90;
		
		// Increases bite distance based on angular speed
	private const float m_angleSpeedMultiplier = 1.2f;
		

	// This are tmp variables we reuse every time we need to find targets
	private Entity[] m_checkEntities = new Entity[30];
	private Collider[] m_checkPlayer = new Collider[2];
	private int m_numCheckEntities = 0;
	private int m_playerColliderMask = -1;

	protected bool m_pauseEating = false;

	public delegate void OnEvent();
	public OnEvent onJawsClosed;
	public OnEvent onBitePlayer;
	public OnEvent onEndEating;
	public OnEvent onEndLatching;

	protected string m_origin = "";
	protected IEntity m_entity = null;


	public enum SpecialEatAction
	{
		Eat,	// Eat always. It ignores eating tier
		CannotEat,	// Cannot eat no matter what.
		None	// No special case
	};
	private Dictionary<string, SpecialEatAction> m_specialEatActions = new Dictionary<string, SpecialEatAction>();

    private const float m_absorbDuration = 0.2f;

    private bool m_eatEverything = false;
    public bool eatEverything
    {
		get { return m_eatEverything; }
		set { m_eatEverything = value; }
    }

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	protected virtual void Awake () {		
        m_bloodEmitter = new List<GameObject>();
        m_freezeEmitter = new List<GameObject>();

		m_holdingBloodParticle.changeStartColor = true;
		m_holdingBloodParticle.startColor = Color.red;

		MouthCache();
		m_holdStunTime = 0.5f;
		m_holdDamage = 10;
		m_holdBoostDamageMultiplier = 3;
		m_holdHealthGainRate = 10;
		m_holdDuration = 1;

		m_eatingEntitiesEnabled = m_canEatEntities;
		m_playerColliderMask = 1 << LayerMask.NameToLayer("Player");
	}

    protected virtual void Start () {
        m_useBlood = FeatureSettingsManager.instance.IsBloodEnabled();
		m_eatingEntitiesEnabled = m_canEatEntities;

		m_holdingBloodParticle.CreatePool();
		m_holdingFreezeParticle.CreatePool();

        if (m_canEatEntities) {
            int amount = (m_limitEating) ? m_limitEatingValue : MAX_PREYS;
            m_preysToEat = new AI.IMachine[amount];
            m_prey = new PreyData[amount];
			for (int i = 0; i < amount; i++) {
                m_prey[i] = new PreyData();
            }
        }
    }

	public void Spawn(ISpawner _spawner) {
		if (m_mouth == null) {
			MouthCache();
		}
	}

	// find mouth transform 
	protected virtual void MouthCache() 
	{
        Transform cacheTransform = transform;
		m_mouth = cacheTransform.FindTransformRecursive("SuctionPoint");	// Check to eat
		m_bite = cacheTransform.FindTransformRecursive("BitePoint");	// only to shader HSW
		m_swallow = cacheTransform.FindTransformRecursive("SwallowPoint"); // second and last eating pre position
		m_suction = cacheTransform.FindTransformRecursive("SuctionPoint");	// first eating prey position

		if (m_mouth == null)
			m_mouth = cacheTransform.FindTransformRecursive("Fire_Dummy");
		if (m_swallow == null)
			m_swallow = cacheTransform.FindTransformRecursive("Dragon_Head");
		if (m_bite == null)
			m_bite = m_mouth;	
		if (m_suction == null)
			m_suction = m_mouth;
	}

	/// <summary>
	/// Adds to ignore tier list. Adds and sku to an ignore tier list. This eating behaviour will be able to eat this entities event if it doesn't meet the tier requierement
	/// </summary>
	/// <param name="entitySku">Entity sku.</param>
	public void AddToEatExceptionList( string entitySku )
	{
		// m_ignoreTierList.Add( entitySku );
		if ( !m_specialEatActions.ContainsKey(entitySku) )
			m_specialEatActions.Add( entitySku, SpecialEatAction.Eat);
		else
			m_specialEatActions[ entitySku ] = SpecialEatAction.Eat;
	}

	public void AddToIgnoreList(string entitySku)
	{
		if ( !m_specialEatActions.ContainsKey(entitySku) )
			m_specialEatActions.Add( entitySku, SpecialEatAction.CannotEat);
		else
			m_specialEatActions[ entitySku ] = SpecialEatAction.CannotEat;
	}

	public SpecialEatAction GetSpecialEatAction( string entitySku )
	{
		if ( m_specialEatActions.ContainsKey(entitySku) )
			return m_specialEatActions[ entitySku ];
		return SpecialEatAction.None;
	}

	protected void SetupHoldParametersForTier( string tierSku )
	{
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.HOLD_PREY_TIER, "tier", tierSku);
		if (def != null)
		{
			m_holdStunTime = def.GetAsFloat("stunTime");
			m_holdDamage = def.GetAsFloat("damage");
			m_holdBoostDamageMultiplier = def.GetAsFloat("boostMultiplier");
			m_holdHealthGainRate = def.GetAsFloat("healthDrain");
			m_holdDuration = def.GetAsFloat("duration");
		}
	}

	protected virtual void OnDisable()
    {
		if (ApplicationManager.IsAlive)
		{
	        if (m_prey != null)
	        {
	            int count = m_prey.Length;
	            for (int i = 0; i < count; i++)
	            {
	                if (m_prey[i].prey != null)
	                {
	                    PreyData prey = m_prey[i];
	                    prey.prey.transform.parent = prey.startParent;
	                    if ( prey.absorbTimer > 0 )
							StartSwallow(m_prey[i].prey);
	                    EndSwallow(m_prey[i].prey);
	                    prey.prey = null;
	                    prey.startParent = null;
	                }
	            }
	            PreyCount = 0;
	        }	

			if ( IsLatching() || IsGrabbing() )
				EndHold();

			if ( m_attackTarget != null )
				StopAttackTarget();	
        }
	}

	public bool IsEating() {
		return enabled && PreyCount > 0;
	}

	public bool IsLatching()
	{
		return m_holdingPlayer != null || (!m_grabbingPrey && m_holdingPrey != null);
	}

	public bool IsGrabbing()
	{
		return m_grabbingPrey && m_holdingPrey != null;
	}


	public void CustomUpdate() {}

	// Update is called once per frame
	protected virtual void Update() 
	{
		if (!m_pauseEating && PreyCount <= 0 && m_attackTarget != null && m_type == IEntity.Type.PLAYER && m_holdingPrey == null)
		{
			BiteKill( true );
			if ( PreyCount > 0 || m_holdingPrey != null )
				StopAttackTarget();
		}

		if ( m_attackTarget != null )
		{
			m_attackTimer += Time.deltaTime;
			if ( m_attackTimer > 0.2f )
			{
				OnJawsClose();
			}
		}
		else
		{            
			if ( m_noAttackTime > 0 )
			{
				m_noAttackTime -= Time.deltaTime;
			}

			if ( m_holdingPrey != null )
			{
				UpdateHoldingPrey();
			}
			else if ( m_holdingPlayer != null )
			{
				UpdateLatchOnPlayer();	// A Type of holding
			}
			else 
			{
				if (m_noAttackTime <= 0 && !m_pauseEating)	// no attack time y no estoy intentando comerme algo? y si ya estoy comiendo? empiezo un nuevo bite?
					TargetSomethingToEat();	// Buscar target
				
			}            
		}

		UpdateBlood();        
		UpdateFreezing();
	}

	void LateUpdate()
	{
		if (PreyCount > 0)
		{
			UpdateEating();
		}
		else if ( m_grabbingPrey && m_holdingPrey != null )
		{
			// Update hold movement
			UpdateGrabMotion();
		}

	}

	void UpdateGrabMotion()
	{
		// Rotation
		Quaternion rot = m_holdingPrey.transform.localRotation;
        m_holdingPrey.transform.localRotation = GameConstants.Quaternion.identity;
		Vector3 holdDirection = m_mouth.InverseTransformDirection(m_holdTransform.forward);
		Vector3 holdUpDirection = m_mouth.InverseTransformDirection(m_holdTransform.up);
		m_holdingPrey.transform.localRotation = Quaternion.Lerp( rot, Quaternion.LookRotation( -holdDirection, holdUpDirection ), Time.deltaTime * 20);
		/*
		Quaternion localRot = m_holdingPrey.transform.localRotation;
		float fixRotStep = m_rotateToMouthSpeed * Time.deltaTime;
		m_holdingPrey.transform.localRotation = Quaternion.RotateTowards( localRot, m_holdingPrey.GetDyingFixRot(), fixRotStep);
*/

		// Position
		Vector3 pos = m_holdingPrey.transform.localPosition;
        m_holdingPrey.transform.localPosition = GameConstants.Vector3.zero;
		Vector3 holdPoint = m_mouth.InverseTransformPoint( m_holdTransform.position );
		// m_holdPrey.transform.localPosition = -holdPoint;
		m_holdingPrey.transform.localPosition = Vector3.Lerp( pos, -holdPoint, Time.deltaTime * 20);
	}

	public void InstantGrabMotion()
	{
		// Rotation
		if ( m_holdingPrey != null )
		{
			Quaternion rot = m_holdingPrey.transform.localRotation;
			m_holdingPrey.transform.localRotation = GameConstants.Quaternion.identity;
			Vector3 holdDirection = m_mouth.InverseTransformDirection(m_holdTransform.forward);
			Vector3 holdUpDirection = m_mouth.InverseTransformDirection(m_holdTransform.up);
			m_holdingPrey.transform.localRotation = Quaternion.LookRotation( -holdDirection, holdUpDirection );

			// Position
			Vector3 pos = m_holdingPrey.transform.localPosition;
			m_holdingPrey.transform.localPosition = GameConstants.Vector3.zero;
			Vector3 holdPoint = m_mouth.InverseTransformPoint( m_holdTransform.position );
			m_holdingPrey.transform.localPosition = -holdPoint;
		}
	}

	/// <summary>
	/// Pauses the eating. End eating what it has in the mouth but does not let anything else in
	/// </summary>
	public virtual void PauseEating()
	{
		m_pauseEating = true;
		StopAttackTarget();
		if ( m_holdingPrey != null || m_holdingPlayer != null)
			EndHold();
	}

	/// <summary>
	/// Resumes the eating.
	/// </summary>
	public void ResumeEating( float timeNoAttack = 0 )
	{
		m_pauseEating = false;	
		m_noAttackTime = timeNoAttack;
	}

	/// <summary>
	/// Function called when the eater closes the jaw. It can come from an animation event or timed event
	/// </summary>
	public void OnJawsClose()
	{
		if ( !enabled ) return;
		// Bite kill!
		if ( m_attackTarget != null )
			StopAttackTarget();

		if ( m_holdingPrey == null && !m_pauseEating && m_holdingPlayer == null)
		{
			BiteKill(PreyCount <= 0 && m_canHold);
			 //if ( m_holdingPrey == null )
			 //	TargetSomethingToEat();	// Buscar target -> al hacer el bite mirar si entran presas
		}
		if (onJawsClosed != null)
			onJawsClosed();
	}

	public Transform GetAttackTarget()
	{
		return m_attackTarget;
	}

	protected void Burp()
	{
		if ( !string.IsNullOrEmpty(m_burpSound) )
		{
			AudioController.Play( m_burpSound );
		}
	}

	/// <summary>
	/// EATING FUNCTIONS
	/// </summary>

	/// Start Eating _prey
	protected void Eat(AI.IMachine prey, bool overrideEatTime = false, float time = 0.1f)
    {
        PreyData preyData = null;
        if (m_prey != null)
        {
            // Searches for an empty PreyData
            int i;
            int count = m_prey.Length;
            for (i = 0; i < count; i++)
            {
                if (m_prey[i].prey == null)
                {
                    break;
                }
            }

            // prey is bitten If an available PreyData was found the
            if (i < count)
            {
                prey.Bite();

                preyData = m_prey[i];
				float eatTime = Mathf.Max(m_minEatAnimTime, prey.biteResistance * GetEatSpeedFactor());
                if ( overrideEatTime )
                	eatTime = time;
                preyData.startParent = prey.transform.parent;
                prey.transform.parent = m_mouth;
                preyData.startScale = prey.transform.localScale;
                preyData.absorbTimer = m_absorbDuration;// eatTime * 0.5f;
                preyData.eatingAnimationTimer = eatTime;
                preyData.eatingAnimationDuration = preyData.eatingAnimationTimer;
                preyData.prey = prey;
                preyData.dyingRotation = prey.GetDyingFixRot();
                PreyCount++;

                if (PreyCount > maxPreysSoFar)
                {
                    maxPreysSoFar = PreyCount;
                    //Debug.LogWarning("MAX = " + maxPreysSoFar + " i = " + i + " count = " + count);
                }

				
            }
            else
            {
                Debug.Log("Eat is not allowed: Not available PreyData found");
            }
        }

        if (preyData != null)
			EatExtended(preyData); 
               
	}

	protected virtual float GetEatSpeedFactor()
	{
		return m_eatSpeedFactor;
	}

    protected virtual void EatExtended(PreyData preyData) {}

	/// <summary>
	/// Eating Update
	/// </summary>
	protected virtual void UpdateEating() {		
        int count = (m_prey == null) ? 0 : m_prey.Length;
        PreyCount = 0;
		for (int i = 0; i < count; i++) {
			if (m_prey[i].prey != null) {
				PreyData prey = m_prey[i];

				Quaternion localRot = prey.prey.transform.localRotation;
				float fixRotStep = m_rotateToMouthSpeed * Time.deltaTime;
				prey.prey.transform.localRotation = Quaternion.RotateTowards( localRot, prey.dyingRotation, fixRotStep);
				// prey.prey.transform.rotation = Quaternion.Lerp(prey.prey.transform.rotation, Quaternion.AngleAxis(-90f, tongueDir), 0.25f);

				if ( prey.absorbTimer > 0 )
				{
					prey.absorbTimer -= Time.deltaTime;
					float t = 1.0f - Mathf.Max(0, prey.absorbTimer / m_absorbDuration);
					// swallow entity
					prey.prey.transform.position = Vector3.Lerp(prey.prey.transform.position, m_suction.position, t);

					if (!prey.prey.HasCorpse()) {
						prey.prey.transform.localScale = Vector3.Lerp(prey.startScale, prey.startScale * 0.5f, t);
					}
                    
					PreyCount++;
					if (prey.absorbTimer <= 0) {					
                    	StartSwallow(prey.prey);
					}
                }
				else
				{
					prey.eatingAnimationTimer -= Time.deltaTime;
					float t = 1.0f - Mathf.Max(0, prey.eatingAnimationTimer / prey.eatingAnimationDuration);

					prey.prey.transform.position = Vector3.Lerp(m_suction.position, m_swallow.position, 0);

					// remaining time eating
					if (prey.eatingAnimationTimer <= 0) 
					{
						prey.prey.transform.parent = prey.startParent;
						EndSwallow(prey.prey);
						prey.prey = null;
						prey.startParent = null;
					}
                    else
                    {
                        PreyCount++;
                    }
				}                
            }
		}

		if (PreyCount == 0) {			
			if ( onEndEating != null)
				onEndEating();
		}
		else
		{
			BiteKill(false);
		}
	}

	///
	/// END EATING
	///

	/// <summary>
	/// HOLDING FUNCTIONS
	/// </summary>

	/// Start holding a prey machine
	virtual public void StartHold( AI.IMachine _prey, bool grab = false) 
	{
		m_grabbingPrey = grab;
		// look for closer hold point
		SerchClosestTransform( _prey.holdPreyPoints );

		if ( m_holdTransform == null )
			m_holdTransform = _prey.transform;
		else
			m_holdPoint.holded = true;

		// TODO (MALH): Check if bite and grab or bite and hold
		_prey.BiteAndHold();

		m_holdingPrey = _prey;
		m_holdingPlayer = null;
		m_holdPreyTimer = m_holdDuration;

		if ( grab )
		{
			m_grabbingPreyPreviousTransformParent = _prey.transform.parent;
			_prey.transform.parent = m_mouth;
		}
	}

	virtual public void AdvanceHold( bool _advance )
	{
		m_advanceHold = _advance;
	}

	virtual protected void StartLatchOnPlayer( DragonPlayer player )
	{
		m_grabbingPrey = false;

		SerchClosestTransform( player.holdPreyPoints );

		if ( m_holdTransform == null )
			m_holdTransform = player.transform;
		else
			m_holdPoint.holded = true;

		// TODO (MALH): Check if bite and grab or bite and hold

		m_holdingPlayer = player;
		m_holdingPlayer.StartLatchedOn();

		m_holdingPrey = null;
		m_holdPreyTimer = m_holdDuration;

	}

	virtual protected void SerchClosestTransform( HoldPreyPoint[] holdPreyPoints )
	{
		float distance = float.MaxValue;

		m_holdTransform = null;
		if ( holdPreyPoints != null )
		for( int i = 0; i<holdPreyPoints.Length; i++ )
		{
			HoldPreyPoint point = holdPreyPoints[i];
			if ( !point.holded && Vector3.SqrMagnitude( m_mouth.position - point.transform.position) < distance )
			{
				distance = Vector3.SqrMagnitude( m_mouth.position - point.transform.position);
				m_holdTransform = point.transform;
				m_holdPoint = point;
			}
		}
	}

	/// <summary>
	/// Update holding a prey
	/// </summary>
	protected virtual void UpdateHoldingPrey()
	{
		if (!m_advanceHold)
			return;

		if (m_holdingBlood <= 0)
		{
			StartBlood();
			if ( FreezingObjectsRegistry.instance.IsFreezing( m_holdingPrey ) ){
				StartFreezing();
			}
		}
		else
		{
			m_holdingBlood -= Time.deltaTime;
		}

		// damage prey
		float damage = m_holdDamage;
		if ( IsBoosting() )
		{
			damage *= m_holdBoostDamageMultiplier;
		}

		m_holdingPrey.ReceiveDamage(damage * Time.deltaTime);
		if (m_holdingPrey.IsDead())
		{
			StartBlood();
			AI.IMachine toEat = m_holdingPrey;
			EndHold();
			Eat( toEat, true, 0.1f);
			// StartSwallow(m_holdingPrey);
			// EndSwallow(m_holdingPrey);

		}
		else
		{
			// Swallow
			m_holdPreyTimer -= Time.deltaTime;
			if ( m_holdPreyTimer <= 0 ) // or prey is death
			{
				// release prey
				// Escaped Event
				if ( m_type == IEntity.Type.PLAYER )
					Messenger.Broadcast<Transform>(MessengerEvents.ENTITY_ESCAPED, m_holdingPrey.transform);
				EndHold();
			}	
		}
	}

	protected virtual void UpdateLatchOnPlayer()
	{
		if (m_holdingBlood <= 0)
		{
			StartBlood();
		}
		else
		{
			m_holdingBlood -= Time.deltaTime;
		}

		// damage prey
		float damage = m_holdDamage;
		if ( IsBoosting() )
		{
			damage *= m_holdBoostDamageMultiplier;
		}

		m_holdingPlayer.dragonHealthBehaviour.ReceiveDamage( damage * Time.deltaTime, DamageType.LATCH, transform, false, m_origin, m_entity as Entity);
		if (!m_holdingPlayer.IsAlive())
		{
			StartBlood();
			EndHold();
		}
		else
		{
			// Swallow
			m_holdPreyTimer -= Time.deltaTime;

			if ( m_holdingPlayer.dragonBoostBehaviour.IsBoostActive() )
				m_holdPreyTimer -= Time.deltaTime;

			if (isAquatic) {
				if (!m_holdingPlayer.dragonMotion.IsInsideWater())
					m_holdPreyTimer -= Time.deltaTime;
			}

			if ( m_holdPreyTimer <= 0 ) // or prey is death
			{
				EndHold();
			}	
		}
	}


	/// Check if boosting while eating. To be implemented by derived classes
	public virtual bool IsBoosting()
	{ 
		return false; 
	}

	/// <summary>
	/// Ends the hold.
	/// </summary>
	virtual public void EndHold()
	{		
        if ( m_holdingPrey != null)
		{
			if ( !m_grabbingPrey && onEndLatching != null)
			{
				onEndLatching();
			}
			else
			{
				m_holdingPrey.transform.parent = m_grabbingPreyPreviousTransformParent;
			}
			m_holdingPrey.ReleaseHold();
			m_holdingPrey.SetVelocity(m_motion.velocity * 2f);
			m_holdingPrey = null;
		}
		else if ( m_holdingPlayer != null)
		{
			m_holdingPlayer.EndLatchedOn();
			m_holdingPlayer = null;
			if (onEndLatching != null)
				onEndLatching();
		}
		m_grabbingPrey = false;
		m_holdTransform = null;
		if ( m_holdPoint != null )
			m_holdPoint.holded = false;

		m_noAttackTime = m_holdStunTime;        
	}

	/// <summary>
	/// END HOLD
	/// </summary>


	/// <summary>
	/// Targets something to eat. Searches fot a victim and starts the jaw movement
	/// </summary>
	private void TargetSomethingToEat()
	{
		float angularSpeed = m_motion.angularVelocity.magnitude;

		float eatDistance = GetEatDistance();
		eatDistance = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, eatDistance, eatDistance * m_angleSpeedMultiplier);

		float speed = m_motion.velocity.magnitude;
		float arcRadius = eatDistance * m_eatDetectionRadiusMultiplier;
		arcRadius = arcRadius + speed * m_eatDetectionSpeedRadiusMultiplier;

		Vector3 dir = m_motion.direction;
		dir.z = 0;
		dir.Normalize();
		float arcAngle = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, m_minArcAngle, m_maxArcAngle);
		Vector3 mouthPos = m_mouth.position;
		mouthPos.z = 0;
		Vector3 arcOrigin = mouthPos - (dir * eatDistance);

		if (m_canLatchOnPlayer || m_canBitePlayer) {
			Vector3 heading = (InstanceManager.player.transform.position - arcOrigin);
			float dot = Vector3.Dot(heading, dir);
			if ( dot > 0)
			{
				bool found = false;
				Collider[] hitColliders = InstanceManager.player.dragonMotion.hitColliders;
				int size = InstanceManager.player.dragonMotion.hitCollidersSize;
				for( int i = 0; i<size && !found; i++ )
				{
					if (MathUtils.TestArcVsBounds( arcOrigin, arcAngle, arcRadius, dir, hitColliders[i].bounds))
					{
						StartAttackTarget(InstanceManager.player.transform);
						found = true;
					}
				}
			}
		}

		if (m_eatingEntitiesEnabled && m_attackTarget == null) {
			m_numCheckEntities = EntityManager.instance.GetOverlapingEntities( arcOrigin, arcRadius, m_checkEntities);
			for (int e = 0; e < m_numCheckEntities; e++) 
			{
				Entity entity = m_checkEntities[e];
				if (entity.transform != transform && entity.IsEdible())
				{
					// if not player check that it can be eaten
					if ( m_type != IEntity.Type.PLAYER )
					{
						SpecialEatAction specialAction = GetSpecialEatAction( entity.sku );
						if ( specialAction != SpecialEatAction.Eat ){
							if ( specialAction == SpecialEatAction.CannotEat || (entity.hideNeedTierMessage && !entity.IsEdible( m_tier ))){
								continue;
							}
						}
					}
					else
					{
						if ( (entity.hideNeedTierMessage /*|| (Time.time - entity.lastTargetedTime <= 1f)*/) && !entity.IsEdible( m_tier ) && !eatEverything)
							continue;		
					}

					// Start bite attempt
					Vector3 heading = (entity.transform.position - arcOrigin);
					float dot = Vector3.Dot(heading, dir);
					if ( dot > 0)
					{
						// Check arc
						Vector3 circleCenter = entity.circleArea.center;
						circleCenter.z = 0;
						if (MathUtils.TestArcVsCircle( arcOrigin, arcAngle, arcRadius, dir, circleCenter, entity.circleArea.radius))
						{
							entity.lastTargetedTime = Time.time;
							StartAttackTarget( entity.transform );
							break;
						}
					}
				}
			}
		}
	}

	public virtual void StartAttackTarget( Transform _transform )
	{        
		m_attackTarget = _transform;
		m_attackTimer = 0;
	}

	public virtual void StopAttackTarget()
	{
		m_attackTarget = null;
	}

	/// <summary>
	/// On jaws closed we check what can we eat
	/// </summary>
	/// <param name="_canHold">If set to <c>true</c> can hold.</param>
	protected virtual void BiteKill( bool _canHold = true ) 
	{
		float angularSpeed = m_motion.angularVelocity.magnitude;

		float eatDistance = GetEatDistance();
		eatDistance = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, eatDistance, eatDistance * m_angleSpeedMultiplier);

		if ((m_canLatchOnPlayer || m_canBitePlayer) && InstanceManager.player != null)
		{
			bool canLatch = false;
			if (m_canLatchOnPlayer)
			{
				if ( canMultipleLatchOnPlayer && InstanceManager.player.HasFreeHoldPoint())
					canLatch = true;
				else
					canLatch = !InstanceManager.player.BeingLatchedOn();
			}

			if ( canLatch || m_canBitePlayer)
			{
				m_numCheckEntities = Physics.OverlapSphereNonAlloc( m_mouth.position, eatDistance, m_checkPlayer, m_playerColliderMask);
				if ( m_numCheckEntities > 0 )
				{
					if ( canLatch )
					{
						// Sart latching on player
						StartLatchOnPlayer( InstanceManager.player );
					}
					else
					{
						// Bite player!!!
						InstanceManager.player.GetComponent<DragonHealthBehaviour>().ReceiveDamage( GetBitePlayerDamage(), DamageType.NORMAL, transform, true, m_origin, m_entity as Entity);
						if (onBitePlayer != null) {
							onBitePlayer();
						}
					}
				}
			}
		}

		if (m_eatingEntitiesEnabled && m_holdingPlayer == null )
		{
			AI.IMachine preyToHold = null;
			Entity entityToHold = null;
            
			m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities(m_mouth.position, eatDistance, m_checkEntities);

            int numPreysToEat = 0;
            int maxPreysToEat = m_preysToEat.Length;            
            for (int e = 0; e < m_numCheckEntities && preyToHold == null; e++)
            {
				Entity entity = m_checkEntities[e];
				SpecialEatAction specialAction = SpecialEatAction.None;
            	if ( m_specialEatActions.ContainsKey( entity.sku ) )
					specialAction = m_specialEatActions[ entity.sku ];
				if ( (entity.IsEdible() && specialAction != SpecialEatAction.CannotEat) || eatEverything)
                {
					if (entity.IsEdible(m_tier) || specialAction == SpecialEatAction.Eat || eatEverything)
                    {
						if (m_limitEating && (numPreysToEat + PreyCount) < m_limitEatingValue || !m_limitEating)
                        {
                            // Makes sure that it won't exceed the max limit
                            if (numPreysToEat < maxPreysToEat)
                            {
								AI.IMachine machine = entity.machine;
                                if ( machine.CanBeBitten() )
                                {
                                    m_preysToEat[numPreysToEat] = machine;
                                    numPreysToEat++;
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Current amount of entities being eaten exceeds the max limit " + maxPreysToEat);
                            }
                        }
					}
					else if (entity.CanBeHolded(m_tier))
					{
						if (_canHold)
						{
							AI.IMachine machine = entity.machine;
							if ( machine.CanBeBitten() )
							{
                                Vector3 target = GameConstants.Vector3.zero;
                                if ( entity.circleArea != null )
                                {
                                    target = entity.circleArea.center;
                                }
                                else
                                {
                                    target = machine.position;
                                }
                                // Check if collision between us!
                                if (!Physics.Linecast(m_swallow.position, target, GameConstants.Layers.GROUND))
                                {
    								preyToHold = machine;
    								entityToHold = entity;
                                }
							}
						}
					}
					else 
					{
						if (m_type == IEntity.Type.PLAYER && !entity.hideNeedTierMessage)
						{
							DragonTier tier = entity.edibleFromTier;
							if ( entity.canBeGrabbed && entity.grabFromTier < tier )
								tier = entity.grabFromTier;
							if ( entity.canBeLatchedOn && entity.latchFromTier < tier )
								tier = entity.latchFromTier;
							Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, tier, entity.sku);
						}
					}
				}
			}                       

			if ( preyToHold != null )
			{
				StartHold(preyToHold, entityToHold.CanBeGrabbed( m_tier));
			}
			else if (numPreysToEat > 0 )
			{
				for( int i = 0; i< numPreysToEat; i++ )
					Eat(m_preysToEat[i]);  
            }
		}

		// m_attackTarget = null;
	}

	protected virtual float GetBitePlayerDamage()
	{
		return 0;
	}

	/// <summary>
	/// Returns the distance this entity can eat something
	/// </summary>
	/// <returns>The find eating distance.</returns>
	protected virtual float GetEatDistance()
	{
		return m_eatDistance * transform.localScale.x;
	}


	private void StartBlood(){
		m_holdingBlood = 0.5f;

		if (!m_useBlood)return;

		Vector3 bloodPos = m_mouth.position;
		bloodPos.z = -50f;
		GameObject go = m_holdingBloodParticle.Spawn(bloodPos + m_holdingBloodParticle.offset);

        if (!ParticleManager.IsBloodOverrided()) {
            if (go != null) {
                FollowTransform ft = go.GetComponent<FollowTransform>();
                if (ft != null) {
                    ft.m_follow = m_mouth;
                    ft.m_offset = m_holdingBloodParticle.offset;
                }

            }
            m_bloodEmitter.Add(go);
        }
	}
    
	private void UpdateBlood() {
		if ( !m_useBlood ) return;
		if (m_bloodEmitter.Count > 0) {
			bool empty = true;
			Vector3 bloodPos = m_mouth.position;
			//bloodPos.z = 0f;

			for (int i = 0; i < m_bloodEmitter.Count; i++) {
				if (m_bloodEmitter[i] != null && m_bloodEmitter[i].activeInHierarchy) {
					m_bloodEmitter[i].transform.position = bloodPos;
					empty = false;
				} else {
					m_bloodEmitter[i] = null;
				}
			}

			if (empty) {
				m_bloodEmitter.Clear();
			}
		}
	}

    private void StartFreezing() {
        Vector3 bloodPos = m_mouth.position;
        bloodPos.z = -50f;
        GameObject go = m_holdingFreezeParticle.Spawn(bloodPos + m_holdingFreezeParticle.offset);
        if (go != null) {
            FollowTransform ft = go.GetComponent<FollowTransform>();
            if (ft != null) {
                ft.m_follow = m_mouth;
                ft.m_offset = m_holdingFreezeParticle.offset;
            }

        }
        m_freezeEmitter.Add(go);
    }

    private void UpdateFreezing() {
		if (m_freezeEmitter.Count > 0) {
			bool empty = true;
			Vector3 bloodPos = m_mouth.position;
			//bloodPos.z = 0f;

			for (int i = 0; i < m_freezeEmitter.Count; i++) {
				if (m_freezeEmitter[i] != null && m_freezeEmitter[i].activeInHierarchy) {
					m_freezeEmitter[i].transform.position = bloodPos;
					empty = false;
				} else {
					m_freezeEmitter[i] = null;
				}
			}

			if (empty) {
				m_freezeEmitter.Clear();
			}
		}
	}

	/// On kill function over prey. Eating or holding
	protected void StartSwallow(AI.IMachine _prey) {
		_prey.BeginSwallowed(m_mouth, m_rewardsPlayer, m_type);//( m_mouth );
	}

	private void EndSwallow(AI.IMachine _prey){
		_prey.EndSwallowed(m_mouth);
	}

	/// <summary>
	/// GIZMO
	/// </summary>
	protected virtual void OnDrawGizmos() {
		if (m_suction == null) {
			MouthCache();
		}
		if ( m_motion == null || m_mouth == null)
			return;

		float angularSpeed = m_motion.angularVelocity.magnitude;

		// Eating Distance
		float eatRadius = GetEatDistance();
		eatRadius = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, eatRadius, eatRadius * m_angleSpeedMultiplier);

		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(m_mouth.position, eatRadius);

		// Eat Detect Distance
		float speed = m_motion.velocity.magnitude;
		float arcRadius = eatRadius * m_eatDetectionRadiusMultiplier;
		arcRadius = arcRadius + speed *  m_eatDetectionSpeedRadiusMultiplier;

		// Eating arc
		float arcAngle = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, m_minArcAngle, m_maxArcAngle);
		Vector2 dir = (Vector2)m_motion.direction;
		dir.Normalize();
		Vector3 arcOrigin = m_mouth.position - (Vector3)(dir * eatRadius);

		// Draw Arc
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(arcOrigin, arcRadius);
		Gizmos.color = Color.red;
		Debug.DrawLine(arcOrigin, arcOrigin + (Vector3)(dir * arcRadius));

		Vector2 dUp = dir.RotateDegrees(arcAngle/2.0f);
		Debug.DrawLine( arcOrigin, arcOrigin + (Vector3)(dUp * arcRadius) );
		Vector2 dDown = dir.RotateDegrees(-arcAngle/2.0f);
		Debug.DrawLine( arcOrigin, arcOrigin + (Vector3)(dDown * arcRadius) );

	}
}
