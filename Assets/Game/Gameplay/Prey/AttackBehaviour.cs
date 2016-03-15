using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public class AttackBehaviour : Initializable {
	
	// Constants
	public enum State {
		None = 0,
		Idle,
		Pursuit,
		Attack,
		AttackRetreat,
	};


	[SerializeField] private float m_damage;
	[SerializeField] private float m_attackDelay;
	[SerializeField] private int m_consecutiveAttacks;
	[SerializeField] private float m_retreatingTime;
	[SerializeField] private bool m_hasAnimation = true;
	[SerializeField] private GameObject m_projectilePrefab;
	[SerializeField] private Transform m_projectileSpawnPoint;

	private Animator m_animator;
	private PreyMotion m_motion;
	private PreyOrientation m_orientation;
	private SensePlayer m_sensor;
	private DragonMotion m_dragon;
	private Transform m_target; // all the attacks will aim to this target

	private State m_state;
	private State m_nextState;

	private float m_timer;
	private int m_attackCount;


	// Use this for initialization
	void Start () {
		m_motion = GetComponent<PreyMotion>();
		m_orientation = GetComponent<PreyOrientation>();
		m_sensor = GetComponent<SensePlayer>();
		m_dragon = InstanceManager.player.GetComponent<DragonMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		m_target = m_dragon.GetAttackPointNear(transform.position);

		if (m_projectilePrefab != null) {
			// create a pool of projectiles
			PoolManager.CreatePool(m_projectilePrefab, 2, true);
		}

		PreyAnimationEvents animEvents = transform.FindChild("view").GetComponent<PreyAnimationEvents>();
		if (animEvents != null) {
			animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAttack);
		}
	}

	void OnDestroy() {
		Transform view = transform.FindChild("view");
		if (view != null) {
			PreyAnimationEvents animEvents = view.GetComponent<PreyAnimationEvents>();
			if (animEvents != null) {
				animEvents.onAttackDealDamage -= new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAttack);
			}
		}
	}

	public override void Initialize() {
		m_state = State.None;
		m_nextState = State.Pursuit;
		m_attackCount = 0;
	}

	public State state
	{
		get
		{
			return m_state;
		}
	}

	void OnEnable() {
		m_state = State.None;
		m_nextState = State.Pursuit;

		if (m_dragon != null)
			m_target = m_dragon.GetAttackPointNear(transform.position);
	}

	void OnDisable() {
		if (m_animator && m_animator.isInitialized) {
			m_animator.SetBool("move", false);
			m_animator.SetBool("fast", false);
		}
	}

	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		Vector2 v = transform.position - m_target.position;
		switch (m_state) {
			case State.Idle:
			{
				if ( m_timer > 0 )
				{
					m_timer -= Time.deltaTime;
				}
				else if (m_sensor.isInsideMaxArea) 
				{
					m_nextState = State.Pursuit;
				}
			}break;
			case State.Pursuit:				
				if (v.sqrMagnitude <= m_sensor.sensorMinRadius * m_sensor.sensorMinRadius) {
					m_nextState = State.Attack;
				}
				if (m_area != null && !m_area.Contains(transform.position)) {
					m_timer = 5.0f;
					m_nextState = State.Idle;
				}
				break;
				
			case State.Attack:
				if (v.sqrMagnitude <= m_sensor.sensorMinRadius * m_sensor.sensorMinRadius) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0) {
						//do attack
						if (m_hasAnimation) {
							m_animator.SetTrigger("attack");
						} else {
							OnAttack();
						}

						m_timer = m_attackDelay;
 
						if (m_consecutiveAttacks > 0) {
							m_attackCount++;
							if (m_attackCount >= m_consecutiveAttacks) {
								m_timer = m_retreatingTime;
								m_nextState = State.AttackRetreat;	// While retreating normal movement should do its thing. Its set on tactics
							}
						}
					}
				} else {
					m_nextState = State.Pursuit;
				}
				break;
			case State.AttackRetreat:
			{
				m_timer -= Time.deltaTime;
				if ( m_timer <= 0 )
				{
					// Check Sensor
					m_nextState = State.Idle;
					m_timer = 5.0f;
				}
			}break;
		}
	}

	void FixedUpdate() {
		switch (m_state) {
			case State.Pursuit:
				m_motion.Pursuit(m_target.position, m_dragon.velocity, m_dragon.maxSpeed);
				//m_motion.ApplySteering();
				break;

			case State.Attack:
				m_motion.Stop();

				if (m_orientation.faceDirection) {
					m_motion.direction = m_target.position - (Vector3)m_motion.position;
				} else {
					Vector3 player = m_target.position;
					if (player.x < m_motion.position.x) {
						m_motion.direction = Vector2.left;
					} else {
						m_motion.direction = Vector2.right;
					}
				}
				break;
		}
	}

	private void ChangeState() {
		if (m_state != m_nextState) {
			switch (m_state) {
				case State.Pursuit:
					m_animator.SetBool("move", false);
					m_animator.SetBool("fast", false);
					break;

				case State.Attack:
					break;
			}

			switch (m_nextState) {
				case State.Pursuit:
					m_animator.SetBool("move", true);
					m_animator.SetBool("fast", true);
					m_attackCount = 0;
					break;
					
				case State.Attack:
					m_motion.Stop();
					m_timer = 0;
					break;
			}

			m_state = m_nextState;
		}
	}

	public void OnAttack() {
		// do stuff - this will be called from animation events "PreyAnimationEvents"

		if (m_projectilePrefab != null) {
			GameObject go = PoolManager.GetInstance(m_projectilePrefab.name);
			if ( go != null )
			{
				ProjectileBehaviour projectile = go.GetComponent<ProjectileBehaviour>();
				if (projectile != null) 
				{
					if ( m_projectileSpawnPoint != null )
					{
						projectile.Shoot(m_projectileSpawnPoint, m_damage);
					}
					else
					{
						projectile.Shoot(transform, m_damage);
					}
				}
			}
		} else {
			m_dragon.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, transform);
		}
	}
}
