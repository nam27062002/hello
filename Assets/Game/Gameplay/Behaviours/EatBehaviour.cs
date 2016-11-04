using UnityEngine;
using System.Collections.Generic;

public abstract class EatBehaviour : MonoBehaviour {
	protected struct PreyData {		
		public float absorbTimer;
		public float eatingAnimationTimer;
		public float eatingAnimationDuration;
		public Transform startParent;
		public Vector3 startScale;
		public AI.Machine prey;
		public Quaternion dyingRotation;
	};

	private const float m_rotateToMouthSpeed = 800;
	private const float m_rotateToMouthThreshold = 5;

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------	

	[SerializeField]private float m_absorbTime = 1;
	[SerializeField]private float m_minEatAnimTime = 1;
	[SerializeField]protected float m_eatDistance = 1;
	public float eatDistanceSqr { get { return (m_eatDistance * transform.localScale.x) * (m_eatDistance * transform.localScale.x); } }

	protected List<PreyData> m_prey;// each prey that falls near the mouth while running the eat animation, will be swallowed at the same time
    private List<AI.Machine> m_preysToEat; // Temporary list needed when eating. It's defined here to prevent memory from being generated when eating

	protected DragonTier m_tier;
	protected float m_eatSpeedFactor = 1f;	// eatTime (s) = eatSpeedFactor * preyResistance

	// Hold stuff
	private float m_holdPreyTimer = 0;
	protected AI.Machine m_holdingPrey = null;
	protected DragonPlayer m_holdingPlayer = null;
	protected DragonHealthBehaviour m_holdingPlayerHealth = null;
	protected Transform m_holdTransform = null;
	public Transform holdTransform{ get{ return m_holdTransform; } }
	protected bool m_grabbingPrey = false;

	// Attacking/Targeting
	protected Transform m_attackTarget = null;
	protected float m_attackTimer = 0;

	// First position when swallowing. Rotation has to end here
	protected Transform m_suction;
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

	protected MotionInterface m_motion;

	private List<GameObject> m_bloodEmitter;

	public List<string> m_burpSounds = new List<string>();
	// public AudioSource m_burpAudio;

	private float m_noAttackTime = 0;
	private float m_holdingBlood = 0;

	// config
	protected bool m_isPlayer = true;		// If eating entity is the player
	protected bool m_rewardsPlayer = false;	
	protected bool m_canLatchOnPlayer = false;
	protected bool m_canEatEntities = true;
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
	private const float m_minArcAngle = 60;
	private const float m_maxArcAngle = 180;
		// Multiplies eating distance to detect targets
	private const float m_eatDetectionRadiusMultiplier = 4;
		// Increases bite distance based on angular speed
	private const float m_angleSpeedMultiplier = 1.2f;
		// Inceases bite distance based on speed
	private const float m_speedRadiusMultiplier = 0.1f;

	// This are tmp variables we reuse every time we need to find targets
	private Entity[] m_checkEntities = new Entity[20];
	private Collider[] m_checkPlayer = new Collider[2];
	private int m_numCheckEntities = 0;
	private int m_playerColliderMask = -1;

	protected bool m_pauseEating = false;

	public delegate void OnEvent();
	public OnEvent onBiteKill;
	public OnEvent onEndEating;
	public OnEvent onEndLatching;


	private List<string> m_ignoreTierList = new List<string>();

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	protected virtual void Awake () {

		m_prey = new List<PreyData>();
        m_preysToEat = new List<AI.Machine>();
        m_bloodEmitter = new List<GameObject>();

		MouthCache();
		m_holdStunTime = 0.5f;
		m_holdDamage = 10;
		m_holdBoostDamageMultiplier = 3;
		m_holdHealthGainRate = 10;
		m_holdDuration = 1;

		m_playerColliderMask = 1 << LayerMask.NameToLayer("Player");
	}

	// find mouth transform 
	protected virtual void MouthCache() 
	{
		m_mouth = transform.FindTransformRecursive("Fire_Dummy");// SuctionPoint
		m_bite = transform.FindTransformRecursive("BitePoint");
		m_swallow = transform.FindTransformRecursive("Dragon_Head");// SwallowPoint
		m_suction = transform.FindTransformRecursive("SuctionPoint");

		if ( m_bite == null )
			m_bite = m_mouth;	
		if (m_suction == null)
			m_suction = m_mouth;
	}

	/// <summary>
	/// Adds to ignore tier list. Adds and sku to an ignore tier list. This eating behaviour will be able to eat this entities event if it doesn't meet the tier requierement
	/// </summary>
	/// <param name="entitySku">Entity sku.</param>
	public void AddToIgnoreTierList( string entitySku )
	{
		m_ignoreTierList.Add( entitySku );
	}

	protected void SetupHoldParametersForTier( string tierSku )
	{
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.HOLD_PREY_TIER, "tier", tierSku);
		if ( def != null)
		{
			m_holdStunTime = def.GetAsFloat("stunTime");
			m_holdDamage = def.GetAsFloat("damage");
			m_holdBoostDamageMultiplier = def.GetAsFloat("boostMultiplier");
			m_holdHealthGainRate = def.GetAsFloat("healthDrain");
			m_holdDuration = def.GetAsFloat("duration");
		}
	}

	protected virtual void OnDisable() {

		for (int i = 0; i < m_prey.Count; i++) {	
			if (m_prey[i].prey != null) {
				PreyData prey = m_prey[i];
				prey.prey.transform.parent = prey.startParent;
				Swallow(m_prey[i].prey);
				prey.prey = null;
				prey.startParent = null;
			}
		}

		m_prey.Clear();
	}

	public bool IsEating() {
		return enabled && m_prey.Count > 0;
	}

	public bool IsLatching()
	{
		return m_holdingPlayer != null || (!m_grabbingPrey && m_holdingPrey);
	}

	public bool IsGrabbing()
	{
		return m_grabbingPrey;
	}

	// Update is called once per frame
	void Update() 
	{
		if (m_prey.Count > 0)
		{
			UpdateEating();
		}

		if ( m_attackTarget != null )
		{
			m_attackTimer -= Time.deltaTime;
			if ( m_attackTimer <= 0 && !m_waitJawsEvent)
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
		// Bite kill!
		if ( m_holdingPrey == null && !m_pauseEating && m_holdingPlayer == null)
		{
			StopAttackTarget();
			BiteKill(m_prey.Count <= 0 && m_canHold);
			if (onBiteKill != null)
				onBiteKill();
			// if ( m_holdingPrey == null )
			// 	TargetSomethingToEat();	// Buscar target -> al hacer el bite mirar si entran presas
		}
	}

	public Transform GetAttackTarget()
	{
		return m_attackTarget;
	}

	protected void Burp()
	{
		if ( m_burpSounds.Count > 0 )
		{
			string name = m_burpSounds[ Random.Range( 0, m_burpSounds.Count ) ];
			AudioManager.instance.PlayClip(name);
		}
	}

	/// <summary>
	/// EATING FUNCTIONS
	/// </summary>

	/// Start Eating _prey
	protected virtual void Eat(AI.Machine _prey) {
		
		_prey.Bite();

		PreyData preyData = new PreyData();

		float eatTime = Mathf.Max(m_minEatAnimTime, m_eatSpeedFactor * _prey.biteResistance);

		preyData.startParent = _prey.transform.parent;
		_prey.transform.parent = m_mouth;
		preyData.startScale = _prey.transform.localScale;
		preyData.absorbTimer = eatTime * 0.5f;
		preyData.eatingAnimationTimer = preyData.absorbTimer;
		preyData.eatingAnimationDuration = preyData.eatingAnimationTimer;
		preyData.prey = _prey;
		preyData.dyingRotation = _prey.GetDyingFixRot();

		m_prey.Add(preyData);
	}

	/// <summary>
	/// Eating Update
	/// </summary>
	protected virtual void UpdateEating() {
		bool empty = true;

		for (int i = 0; i < m_prey.Count; i++) {
			if (m_prey[i].prey != null) {
				PreyData prey = m_prey[i];

				Quaternion localRot = prey.prey.transform.localRotation;
				float fixRotStep = m_rotateToMouthSpeed * Time.deltaTime;
				prey.prey.transform.localRotation = Quaternion.RotateTowards( localRot, prey.dyingRotation, fixRotStep);
				// prey.prey.transform.rotation = Quaternion.Lerp(prey.prey.transform.rotation, Quaternion.AngleAxis(-90f, tongueDir), 0.25f);

				if ( prey.absorbTimer > 0 )
				{
					prey.absorbTimer -= Time.deltaTime;
					float t = 1 - Mathf.Max(0, prey.absorbTimer / prey.eatingAnimationDuration);
					// swallow entity
					prey.prey.transform.position = Vector3.Lerp(prey.prey.transform.position, m_suction.position, t);
					prey.prey.transform.localScale = Vector3.Lerp(prey.prey.transform.localScale, prey.startScale * 0.75f, t);
				}
				else
				{
					prey.eatingAnimationTimer -= Time.deltaTime;
					float t = Mathf.Max(0, prey.eatingAnimationTimer / prey.eatingAnimationDuration);
					prey.prey.transform.position = Vector3.Lerp(prey.prey.transform.position, m_swallow.position, t);
					// remaining time eating
					if (prey.eatingAnimationTimer <= 0) 
					{
						prey.prey.transform.parent = prey.startParent;
						Swallow(prey.prey);
						prey.prey = null;
						prey.startParent = null;
					}
				}



				m_prey[i] = prey;
				empty = false;
			}
		}

		if (empty) {
			m_prey.Clear();
			if ( onEndEating != null)
				onEndEating();
		}
	}

	///
	/// END EATING
	///

	/// <summary>
	/// HOLDING FUNCTIONS
	/// </summary>

	/// Start holding a prey machine
	virtual protected void StartHold( AI.Machine _prey, bool grab = false) 
	{
		m_grabbingPrey = grab;
		// look for closer hold point
		SerchClosestTransform( _prey.holdPreyPoints );

		if ( m_holdTransform == null )
			m_holdTransform = _prey.transform;

		// TODO (MALH): Check if bite and grab or bite and hold
		_prey.BiteAndHold();

		m_holdingPrey = _prey;
		m_holdingPlayer = null;
		m_holdPreyTimer = m_holdDuration;
	}

	virtual protected void StartLatchOnPlayer( DragonPlayer player )
	{
		m_grabbingPrey = false;

		SerchClosestTransform( player.holdPreyPoints );

		if ( m_holdTransform == null )
			m_holdTransform = player.transform;

		// TODO (MALH): Check if bite and grab or bite and hold

		m_holdingPlayer = player;
		m_holdingPlayer.StartLatchedOn();

		m_holdingPrey = null;
		m_holdPreyTimer = m_holdDuration;

	}

	virtual protected void SerchClosestTransform( List<Transform> holdPreyPoints )
	{
		float distance = float.MaxValue;
		List<Transform> points = holdPreyPoints;
		m_holdTransform = null;
		for( int i = 0; i<points.Count; i++ )
		{
			if ( Vector3.SqrMagnitude( m_mouth.position - points[i].position) < distance )
			{
				distance = Vector3.SqrMagnitude( m_mouth.position - points[i].position);
				m_holdTransform = points[i];
			}
		}
	}

	/// <summary>
	/// Update holding a prey
	/// </summary>
	protected virtual void UpdateHoldingPrey()
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

		m_holdingPrey.ReceiveDamage(damage * Time.deltaTime);
		if (m_holdingPrey.IsDead())
		{
			Swallow(m_holdingPrey);
			StartBlood();
			EndHold();
		}
		else
		{
			// Swallow
			m_holdPreyTimer -= Time.deltaTime;
			if ( m_holdPreyTimer <= 0 ) // or prey is death
			{
				// release prey
				// Escaped Event
				if ( m_isPlayer )
					Messenger.Broadcast<Transform>(GameEvents.ENTITY_ESCAPED, m_holdingPrey.transform);
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

		m_holdingPlayer.dragonHealthBehaviour.ReceiveDamage( damage * Time.deltaTime, DamageType.LATCH, transform, false);
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
				onEndLatching();
				
			m_holdingPrey.ReleaseHold();
			m_holdingPrey.SetVelocity(m_motion.velocity * 2f);
			m_holdingPrey = null;
		}
		else if ( m_holdingPlayer != null)
		{
			if (onEndLatching != null)
				onEndLatching();
			m_holdingPlayer.EndLatchedOn();
			m_holdingPlayer = null;
		}

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
		arcRadius = arcRadius + speed * m_speedRadiusMultiplier;

		Vector3 dir = m_motion.direction;
		dir.z = 0;
		dir.Normalize();
		float arcAngle = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, m_minArcAngle, m_maxArcAngle);
		Vector3 mouthPos = m_mouth.position;
		mouthPos.z = 0;
		Vector3 arcOrigin = mouthPos - (dir * eatDistance);

		if (m_canLatchOnPlayer) {
			Vector3 heading = (InstanceManager.player.transform.position - arcOrigin);
			float dot = Vector3.Dot(heading, dir);
			if ( dot > 0)
			{
				SphereCollider sc = InstanceManager.player.dragonMotion.groundCollider;

				// Check arc
				Vector3 circleCenter = sc.transform.TransformPoint(sc.center);
				circleCenter.z = 0;
				if (MathUtils.TestCircleVsArc( arcOrigin, arcAngle, arcRadius, dir, circleCenter, sc.radius))
				{
					StartAttackTarget( sc.transform );
				}
			}
		}

		if (m_canEatEntities && m_attackTarget == null) {
			m_numCheckEntities = EntityManager.instance.GetOverlapingEntities( arcOrigin, arcRadius, m_checkEntities);
			for (int e = 0; e < m_numCheckEntities; e++) 
			{
				Entity entity = m_checkEntities[e];
				if (entity.IsEdible())
				{
					// Start bite attempt
					Vector3 heading = (entity.transform.position - arcOrigin);
					float dot = Vector3.Dot(heading, dir);
					if ( dot > 0)
					{
						// Check arc
						Vector3 circleCenter = entity.circleArea.center;
						circleCenter.z = 0;
						if (MathUtils.TestCircleVsArc( arcOrigin, arcAngle, arcRadius, dir, circleCenter, entity.circleArea.radius))
						{
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
		m_attackTimer = 0.2f;
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

		if (m_canLatchOnPlayer && InstanceManager.player != null && !InstanceManager.player.BeingLatchedOn())
		{
			m_numCheckEntities = Physics.OverlapSphereNonAlloc( m_mouth.position, eatDistance, m_checkPlayer, m_playerColliderMask);
			if ( m_numCheckEntities > 0 )
			{
				// Sart latching on player
				StartLatchOnPlayer( InstanceManager.player );
			}
		}

		if ( m_canEatEntities && m_holdingPlayer == null )
		{
			AI.Machine preyToHold = null;
			Entity entityToHold = null;
            
			m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities(m_mouth.position, eatDistance, m_checkEntities);
			for (int e = 0; e < m_numCheckEntities; e++) {
				Entity entity = m_checkEntities[e];
				if ( entity.IsEdible() )
				{
					if (entity.IsEdible(m_tier) || m_ignoreTierList.Contains( entity.sku ))
					{
						if (m_limitEating && (m_preysToEat.Count + m_prey.Count) < m_limitEatingValue || !m_limitEating)
						{
							AI.Machine machine = entity.GetComponent<AI.Machine>();
							if (!machine.IsDead() && !machine.IsDying()) {
                                m_preysToEat.Add(machine);
							}
						}
					}
					else if (entity.CanBeHolded(m_tier))
					{
						if (_canHold)
						{
							AI.Machine machine = entity.GetComponent<AI.Machine>();
							preyToHold = machine;
							entityToHold = entity;
						}
					}
					else 
					{
						if (m_isPlayer)
							Messenger.Broadcast<DragonTier, string>(GameEvents.BIGGER_DRAGON_NEEDED, entity.edibleFromTier, entity.sku);
					}
				}
			}

			if ( preyToHold != null )
			{
				StartHold(preyToHold, entityToHold.CanBeGrabbed( m_tier));
			}
			else if (m_preysToEat.Count > 0 )
			{
				for( int i = 0; i< m_preysToEat.Count; i++ )
					Eat(m_preysToEat[i]);

                // Clears it up since it's a temporary list. It's defined as global to prevent memory from being generated every time the creature eats
                m_preysToEat.Clear();
            }
		}

		m_attackTarget = null;
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
		Vector3 bloodPos = m_mouth.position;
		bloodPos.z = -50f;
		m_bloodEmitter.Add(ParticleManager.Spawn("PS_Blood_Explosion_Medium", bloodPos, "Blood"));
		m_holdingBlood = 0.5f;
	}

	private void UpdateBlood() {
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


	/// On kill function over prey. Eating or holding
	private void Swallow(AI.Machine _prey) {
		_prey.BeingSwallowed(m_mouth, m_rewardsPlayer);//( m_mouth );
	}

	/// <summary>
	/// GIZMO
	/// </summary>
	protected virtual void OnDrawGizmos() {
		if (m_suction == null) {
			MouthCache();
		}
		if ( m_motion == null )
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
		arcRadius = arcRadius + speed * m_speedRadiusMultiplier;

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
