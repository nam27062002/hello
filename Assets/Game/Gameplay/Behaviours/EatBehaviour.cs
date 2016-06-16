using UnityEngine;
using System.Collections.Generic;

public abstract class EatBehaviour : MonoBehaviour {
	struct PreyData {		
		public float absorbTimer;
		public float eatingAnimationTimer;
		public Transform startParent;
		public Vector3 startScale;
		public EdibleBehaviour prey;
	};

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------	

	[SerializeField]private float m_absorbTime;
	[SerializeField]private float m_minEatAnimTime;
	[SerializeField]private float m_eatDistance;
	public float eatDistanceSqr { get { return (m_eatDistance * transform.localScale.x) * (m_eatDistance * transform.localScale.x); } }
	public DragonTier tier { get { return m_tier; } }

	private List<PreyData> m_prey;// each prey that falls near the mouth while running the eat animation, will be swallowed at the same time

	protected DragonTier m_tier;
	protected float m_eatSpeedFactor = 1f;	// eatTime (s) = eatSpeedFactor * preyResistance

	private float m_eatingTimer;
	private float m_eatingTime;
	protected bool m_slowedDown;
	private float m_holdPreyTimer = 0;
	protected EdibleBehaviour m_holdingPrey = null;
	protected Transform m_holdTransform = null;

	protected Transform m_attackTarget = null;
	protected float m_attackTimer = 0;


	private Transform m_suction;
	private Transform m_mouth;
	public Transform mouth
	{
		get{ return m_mouth; }
	}
	private Transform m_head;
	protected Animator m_animator;

	protected MotionInterface m_motion;

	private List<GameObject> m_bloodEmitter;

	public List<string> m_burpSounds = new List<string>();
	// public AudioSource m_burpAudio;

	private float m_noAttackTime = 0;
	private float m_holdingBlood = 0;

	protected bool m_canHold = true;		// if this eater can hold a prey
	protected bool m_limitEating = false;	// If there is a limit on eating preys at a time
	protected int m_limitEatingValue = 1;	// limit value
	protected bool m_isPlayer = true;


	protected float m_holdStunTime;
	protected float m_holdDamage;
	protected float m_holdBoostDamageMultiplier;
	protected float m_holdHealthGainRate;
	protected float m_holdDuration;

	protected DragonBoostBehaviour m_boost;

	// Arc detection values

	private const float m_minAngularSpeed = 0;
	private const float m_maxAngularSpeed = 12;
	private const float m_minArcAngle = 60;
	private const float m_maxArcAngle = 180;
	private const float m_eatDetectionRadiusMultiplier = 4;
	private const float m_angleSpeedMultiplier = 1.2f;
	private const float m_speedRadiusMultiplier = 0.1f;

	protected bool m_waitJawsEvent = false;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake () {
		m_eatingTimer = 0;
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_boost = GetComponent<DragonBoostBehaviour>();

		m_prey = new List<PreyData>();
		m_bloodEmitter = new List<GameObject>();

		m_slowedDown = false;

		GetMouth();
		m_holdStunTime = 0.5f;
		m_holdDamage = 10;
		m_holdBoostDamageMultiplier = 3;
		m_holdHealthGainRate = 10;
		m_holdDuration = 1;
	}

	protected void SetupHoldParametersForTier( string tierSku)
	{
		DefinitionNode def = DefinitionsManager.GetDefinitionByVariable(DefinitionsCategory.HOLD_PREY_TIER, "tier", tierSku);

		if ( def != null)
		{
			m_holdStunTime = def.GetAsFloat("stunTime");
			m_holdDamage = def.GetAsFloat("damage");
			m_holdBoostDamageMultiplier = def.GetAsFloat("boostMultiplier");
			m_holdHealthGainRate = def.GetAsFloat("healthDrain");
			m_holdDuration = def.GetAsFloat("duration");
		}
	}

	void OnDisable() {
		m_eatingTimer = 0;
		if (m_slowedDown) {
			SlowDown(false);
		}

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

		if (m_animator && m_animator.isInitialized) {
			m_animator.SetBool("eat", false);
		}
	}

	public bool IsEating() {
		return enabled && m_prey.Count > 0;
	}

	// Update is called once per frame
	void Update() 
	{
		if (m_prey.Count > 0)
		{
			Chew();
		}

		if ( m_attackTarget != null )
		{
			m_attackTimer -= Time.deltaTime;
			if ( m_attackTimer <= 0 && !m_waitJawsEvent)
			{
				// Bite kill!
				FindSomethingToEat(m_prey.Count <= 0 && m_canHold);
				m_attackTarget = null;

				if ( m_prey.Count <= 0 )
				{
					m_animator.SetBool("eat", false);
				}

				if (m_holdingPrey == null)
					TargetSomethingToEat();	// Buscar target -> al hacer el bite mirar si entran presas
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
			else 
			{
				if (m_noAttackTime <= 0)	// no attack time y no estoy intentando comerme algo? y si ya estoy comiendo? empiezo un nuevo bite?
					TargetSomethingToEat();	// Buscar target -> al hacer el bite mirar si entran presas
				
			}
		}

		UpdateBlood();

	}

	public void OnJawsClose()
	{
		// Bite kill!
		FindSomethingToEat(m_prey.Count <= 0 && m_canHold);
		m_attackTarget = null;

		if ( m_prey.Count <= 0 )
		{
			m_animator.SetBool("eat", false);
		}

		if (m_holdingPrey == null)
			TargetSomethingToEat();	// Buscar target -> al hacer el bite mirar si entran presas
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

	private void Eat(EdibleBehaviour _prey) {
		
		_prey.OnEat();

		// Yes!! Eat it!!
		m_eatingTimer = m_eatingTime = (m_eatSpeedFactor * _prey.biteResistance);

		if (m_eatingTime >= 0.5f) {
			SlowDown(true);
		}

		PreyData preyData = new PreyData();

		preyData.startParent = _prey.transform.parent;
		_prey.transform.parent = transform;
		preyData.startScale = _prey.transform.localScale;
		preyData.absorbTimer = m_absorbTime;
		preyData.eatingAnimationTimer = Mathf.Max(m_minEatAnimTime, m_eatingTimer);
		preyData.prey = _prey;

		m_prey.Add(preyData);

		m_animator.SetBool("eat", true);

		if (m_eatingTime >= 0.5f || m_prey.Count > 2) 
		{
			m_animator.SetTrigger("eat crazy");
		}
		/*
		Vector3 bloodPos = m_mouth.position;
		bloodPos.z = -50f;
		m_bloodEmitter.Add(ParticleManager.Spawn("bloodchurn-large", bloodPos));
		*/
	}

	virtual protected void StartHold(EdibleBehaviour _prey) 
	{
		// look for closer hold point
		float distance = float.MaxValue;
		List<Transform> points = _prey.holdPreyPoints;
		m_holdTransform = null;
		for( int i = 0; i<points.Count; i++ )
		{
			if ( Vector3.SqrMagnitude( m_mouth.position - points[i].position) < distance )
			{
				distance = Vector3.SqrMagnitude( m_mouth.position - points[i].position);
				m_holdTransform = points[i];
			}
		}

		if ( m_holdTransform == null )
			m_holdTransform = _prey.transform;

		_prey.OnHoldBy(this);
		m_holdingPrey = _prey;
		m_holdPreyTimer = m_holdDuration;
		m_animator.SetBool("eatHold", true);

	}

	private void UpdateHoldingPrey()
	{
		if (m_holdingBlood <= 0)
		{
			Vector3 bloodPos = m_mouth.position;
			bloodPos.z = -50f;
			m_bloodEmitter.Add(ParticleManager.Spawn("bloodchurn-large", bloodPos));
			m_holdingBlood = 0.5f;
		}
		else
		{
			m_holdingBlood -= Time.deltaTime;
		}

		// damage prey

		float damage = m_holdDamage;
		// if active boost
		if (m_boost.IsBoostActive())
		{
			damage *= m_holdBoostDamageMultiplier;
			// Increase eating speed
			m_animator.SetFloat("eatingSpeed", m_holdBoostDamageMultiplier / 2.0f);
		}
		else
		{
			// Change speed back
			m_animator.SetFloat("eatingSpeed", 1);
		}

		m_holdingPrey.HoldingDamage( damage * Time.deltaTime);
		if ( m_holdingPrey.isDead() )
		{
			m_holdingPrey.OnSwallow( m_mouth , true);
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
				Messenger.Broadcast<Transform>(GameEvents.ENTITY_ESCAPED, m_holdingPrey.transform);
				m_holdingPrey.ReleaseHold();
				EndHold();
			}	
		}
	}

	virtual protected void EndHold()
	{
		m_holdingPrey = null;
		m_noAttackTime = m_holdStunTime;
		m_animator.SetBool("eatHold", false);

		// Set back default speed

		// Check if boosting!!
		if (m_boost.IsBoostActive())
		{
			m_animator.SetFloat("eatingSpeed", m_boost.boostMultiplier);
		}
		else
		{
			m_animator.SetFloat("eatingSpeed", 1);
		}
	}

	private void Swallow(EdibleBehaviour _prey) {
		_prey.OnSwallow( m_mouth );
	}

	private void TargetSomethingToEat()
	{
		float arcRadius = m_eatDistance * transform.localScale.x ;
		if (DebugSettings.eatDistancePowerUp) {
			arcRadius *= 2;
		}

		float speed = m_motion.velocity.magnitude;
		float angularSpeed = m_motion.angularVelocity.magnitude;

		float eatRadius = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, arcRadius, arcRadius * m_angleSpeedMultiplier);
		arcRadius = eatRadius * m_eatDetectionRadiusMultiplier;
		arcRadius = arcRadius + speed * m_speedRadiusMultiplier;

		float arcAngle = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, m_minArcAngle, m_maxArcAngle);
		Vector3 arcOrigin = m_suction.position - (Vector3)(m_motion.direction * eatRadius);

		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(arcOrigin, arcRadius);
		for (int e = 0; e < preys.Length; e++) 
		{
			Entity entity = preys[e];
			if (entity.isEdible) 
			{
				if (entity.edibleFromTier <= m_tier || ( entity.canBeHolded && (entity.holdFromTier <= m_tier) )) 	// if we find a prey we can eat
				{
					// Start bite attempt
					Vector3 heading = (entity.transform.position - arcOrigin);
					float dot = Vector3.Dot(heading, m_motion.direction);
					if ( dot > 0)
					{
						// Check arc
						Vector3 circleCenter = entity.circleArea.center;
						circleCenter.z = 0;
						if (MathUtils.TestCircleVsArc( arcOrigin, arcAngle, arcRadius, m_motion.direction, circleCenter, entity.circleArea.radius))
						{
							m_attackTarget = entity.transform;
							m_attackTimer = 0.2f;

							// Start attack animation
							m_animator.SetBool("eat", true);
						}
					}
					break;
				}
			}
		}
	}

	private void FindSomethingToEat( bool _canHold = true ) 
	{
		float eatDistance = m_eatDistance * transform.localScale.x;
		if (DebugSettings.eatDistancePowerUp) {
			eatDistance *= 2;
		}

		float angularSpeed = m_motion.angularVelocity.magnitude;
		eatDistance = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, eatDistance, eatDistance * m_angleSpeedMultiplier);

		EdibleBehaviour preyToHold = null;
		List<EdibleBehaviour> preysToEat = new List<EdibleBehaviour>();
		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_suction.position, eatDistance);
		for (int e = 0; e < preys.Length; e++) {
			Entity entity = preys[e];
			if (entity.isEdible) {
				if (entity.edibleFromTier <= m_tier) 
				{
					if ( m_limitEating && preysToEat.Count < m_limitEatingValue || !m_limitEating)
					{
						EdibleBehaviour edible = entity.GetComponent<EdibleBehaviour>();
						if (edible.CanBeEaten(m_motion.direction)) 
						{
							preysToEat.Add(edible);
						}
					}
				}
				else if ( entity.canBeHolded && (entity.holdFromTier <= m_tier) )
				{
					if (_canHold)
					{
						EdibleBehaviour edible = entity.GetComponent<EdibleBehaviour>();
						if (edible.CanBeEaten(m_motion.direction)) 
						{
							preyToHold = edible;
							break;
						}
					}
				}
				else 
				{
					if ( m_isPlayer )
						Messenger.Broadcast<DragonTier>(GameEvents.BIGGER_DRAGON_NEEDED, entity.edibleFromTier);
				}
			}
		}

		if ( preyToHold != null )
		{
			StartHold(preyToHold);
		}
		else if ( preysToEat.Count > 0 )
		{
			for( int i = 0; i<preysToEat.Count; i++ )
				Eat(preysToEat[i]);
		}
	}

	private void Chew() {
		bool empty = true;
		for (int i = 0; i < m_prey.Count; i++) {
			if (m_prey[i].prey != null) {
				PreyData prey = m_prey[i];

				prey.absorbTimer -= Time.deltaTime;
				prey.eatingAnimationTimer -= Time.deltaTime;

				float t = 1 - Mathf.Max(0, m_prey[i].absorbTimer / m_absorbTime);
				Vector3 tongueDir = (m_mouth.position - m_head.position).normalized;

				// swallow entity
				prey.prey.transform.position = Vector3.Lerp(prey.prey.transform.position, m_mouth.position, t);
				prey.prey.transform.localScale = Vector3.Lerp(prey.prey.transform.localScale, prey.startScale * 0.75f, t);
				prey.prey.transform.rotation = Quaternion.Lerp(prey.prey.transform.rotation, Quaternion.AngleAxis(-90f, tongueDir), 0.25f);

				// remaining time eating
				if (m_prey[i].eatingAnimationTimer <= 0) 
				{
					prey.prey.transform.parent = prey.startParent;
					Swallow(prey.prey);
					prey.prey = null;
					prey.startParent = null;
				}

				m_prey[i] = prey;
				empty = false;
			}
		}

		if (empty) {
			m_prey.Clear();

			if (m_slowedDown) {
				SlowDown(false);
			}

			m_animator.SetBool("eat", false);
		}
	}

	private void UpdateBlood() {
		if (m_bloodEmitter.Count > 0) {
			bool empty = true;
			Vector3 bloodPos = m_mouth.position;
			bloodPos.z = -1f;

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

	protected abstract void SlowDown(bool _enable);


	// find mouth transform 
	private void GetMouth() {
		m_mouth = transform.FindTransformRecursive("Fire_Dummy");
		m_head = transform.FindTransformRecursive("Dragon_Head");

		m_suction = transform.FindTransformRecursive("Fire_Dummy");
		if (m_suction == null) {
			m_suction = m_mouth;
		}
	}

	void OnDrawGizmos() {
		if (m_suction == null) {
			GetMouth();
		}
		if ( m_motion == null )
			return;

		float speed = m_motion.velocity.magnitude;
		float angularSpeed = m_motion.angularVelocity.magnitude;

		// Eating Distance
		float eatRadius = m_eatDistance * transform.localScale.x;
		if (DebugSettings.eatDistancePowerUp) {
			eatRadius *= 2;
		}
		eatRadius = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, eatRadius, eatRadius * m_angleSpeedMultiplier);

		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(m_suction.position, eatRadius);

		// Eat Detect Distance
		float arcRadius = eatRadius * m_eatDetectionRadiusMultiplier;
		arcRadius = arcRadius + speed * m_speedRadiusMultiplier;

		// Eating arc
		float arcAngle = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, m_minArcAngle, m_maxArcAngle);
		Vector3 arcOrigin = m_suction.position - (Vector3)(m_motion.direction * eatRadius);

		// Draw Arc
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(arcOrigin, arcRadius);
		Gizmos.color = Color.red;
		Debug.DrawLine(arcOrigin, arcOrigin + (Vector3)(m_motion.direction * arcRadius));

		Vector2 dUp = m_motion.direction.RotateDegrees(arcAngle/2.0f);
		Debug.DrawLine( arcOrigin, arcOrigin + (Vector3)(dUp * arcRadius) );
		Vector2 dDown = m_motion.direction.RotateDegrees(-arcAngle/2.0f);
		Debug.DrawLine( arcOrigin, arcOrigin + (Vector3)(dDown * arcRadius) );

	}
}
