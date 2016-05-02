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
	private float m_burpTime;
	private float m_holdPreyTimer = 0;
	private EdibleBehaviour m_holdingPrey = null;

	private Transform m_suction;
	private Transform m_mouth;
	private Transform m_head;
	protected Animator m_animator;

	protected MotionInterface m_motion;

	private List<GameObject> m_bloodEmitter;

	bool m_almostEat = false;

	public List<string> m_burpSounds = new List<string>();
	// public AudioSource m_burpAudio;

	private float m_noAttackTime = 0;
	private float m_holdingBlood = 0;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake () {
		m_eatingTimer = 0;
		m_burpTime = 0;
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		m_prey = new List<PreyData>();
		m_bloodEmitter = new List<GameObject>();

		m_slowedDown = false;

		GetMouth();
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
		if ( m_noAttackTime > 0 )
		{
			m_noAttackTime -= Time.deltaTime;
		}

		// if not holding
		if (m_holdingPrey == null && m_noAttackTime <= 0)
		{
			FindSomethingToEat( m_prey.Count <= 0 );
		}
		else
		{
			if ( m_holdingPrey != null )
				UpdateHoldingPrey();	
		}

		if (m_prey.Count > 0) 
		{	
			Chew();
		}

		UpdateBlood();

		m_animator.SetBool("almostEat", m_almostEat);
		m_almostEat = false;
	}

	private void Burp()
	{
		if ( m_burpSounds.Count > 0 )
		{
			string name = m_burpSounds[ Random.Range( 0, m_burpSounds.Count ) ];
			AudioManager.instance.PlayClip(name);
		}
	}

	public void AlmostEat(EdibleBehaviour _prey) {
		m_almostEat = true;
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
		_prey.OnHoldBy(this);
		m_holdingPrey = _prey;
		m_holdPreyTimer = 2.0f;
		m_animator.SetBool("eat", true);
		// TODO (miguel) this has to be adapted to the pet
		DragonMotion motion = GetComponent<DragonMotion>();
		motion.StartHoldPreyMovement( _prey.transform);
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
		float m_holdDamage = 10;
		m_holdingPrey.HoldingDamage( m_holdDamage * Time.deltaTime);
		if ( m_holdingPrey.isDead() )
		{
			m_holdingPrey.OnSwallow( m_mouth );
			EndHold();
		}
		else
		{
			// Swallow
			m_holdPreyTimer -= Time.deltaTime;
			if ( m_holdPreyTimer <= 0 ) // or prey is death
			{
				// release prey
				m_holdingPrey.ReleaseHold();
				EndHold();
			}	
		}
	}

	private void EndHold()
	{
		// TODO (miguel) this has to be adapted to the pet
		DragonMotion motion = GetComponent<DragonMotion>();
		motion.EndHoldMovement();
		m_holdingPrey = null;
		m_noAttackTime = 1.0f;
		m_animator.SetBool("eat", false);
	}

	private void Swallow(EdibleBehaviour _prey) {
		_prey.OnSwallow( m_mouth );
	}

	private void FindSomethingToEat( bool _canHold = true ) 
	{
		float eatDistance = m_eatDistance * transform.localScale.x;
		if (DebugSettings.eatDistancePowerUp) {
			eatDistance *= 2;
		}

		EdibleBehaviour preyToHold = null;
		List<EdibleBehaviour> preysToEat = new List<EdibleBehaviour>();
		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_suction.position, eatDistance);
		for (int e = 0; e < preys.Length; e++) {
			Entity entity = preys[e];
			/*
			if (entity.edibleFromTier <= m_tier) 
			{
				EdibleBehaviour edible = entity.GetComponent<EdibleBehaviour>();
				if (edible.CanBeEaten(m_motion.direction)) 
				{
					preysToEat.Add(edible);
				}
			}
			else if (_canHold && entity.canBeHolded && (entity.holdFromTier <= m_tier) )
			*/
			{
				EdibleBehaviour edible = entity.GetComponent<EdibleBehaviour>();
				if (edible.CanBeEaten(m_motion.direction)) 
				{
					preyToHold = edible;
					break;
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

		Gizmos.DrawWireSphere(m_suction.position, m_eatDistance * transform.localScale.x);
	}
}
