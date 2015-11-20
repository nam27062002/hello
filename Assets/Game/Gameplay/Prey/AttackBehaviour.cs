using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public class AttackBehaviour : Initializable {

	// Delegates
	delegate void StartFX();
	StartFX m_startFX;

	// Constants
	private enum State {
		None = 0,
		Pursuit,
		Attack
	};


	[SerializeField] private float m_damage;
	[SerializeField] private float m_attackDelay;
	[SerializeField] private GameObject m_projectilePrefab;

	private Animator m_animator;
	private PreyMotion m_motion;
	private SensePlayer m_sensor;
	private DragonMotion m_dragon; // all the attacks will aim to this target

	private State m_state;
	private State m_nextState;

	private float m_timer;


	// Use this for initialization
	void Start () {
		m_motion = GetComponent<PreyMotion>();
		m_sensor = GetComponent<SensePlayer>();
		m_dragon = InstanceManager.player.GetComponent<DragonMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		if (m_projectilePrefab != null) {
			// create a pool of projectiles
			PoolManager.CreatePool(m_projectilePrefab, 2, true);
		}

		PreyAnimationEvents animEvents = transform.FindChild("view").GetComponent<PreyAnimationEvents>();
		animEvents.onAttackDealDamage += new PreyAnimationEvents.Attack_DealDamage(OnAttack);
	}

	void OnDestroy() {
		Transform view = transform.FindChild("view");
		if (view != null) {
			PreyAnimationEvents animEvents = view.GetComponent<PreyAnimationEvents>();
			if (animEvents != null) {
				animEvents.onAttackDealDamage -= new PreyAnimationEvents.Attack_DealDamage(OnAttack);
			}
		}
	}

	public override void Initialize() {
		m_state = State.None;
		m_nextState = State.Pursuit;
	}

	void OnEnable() {
		m_state = State.None;
		m_nextState = State.Pursuit;
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

		switch (m_state) {
			case State.Pursuit:				
				if (m_sensor.isInsideMinArea) {
					m_nextState = State.Attack;
				}
				break;
				
			case State.Attack:
				if (m_sensor.isInsideMinArea) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0) {
						//do attack
						if (m_startFX != null) {
							m_startFX();
						}
						m_animator.SetTrigger("attack");
						m_timer = m_attackDelay;
					}
				} else {
					m_nextState = State.Pursuit;
				}
				break;
		}
	}

	void FixedUpdate() {
		switch (m_state) {
			case State.Pursuit:
				m_motion.Pursuit(m_dragon.transform.position, m_dragon.GetVelocity(), m_dragon.GetMaxSpeed());
				m_motion.ApplySteering();
				break;

			case State.Attack:
				m_motion.velocity = Vector2.zero;
				m_motion.ApplySteering();

				if (m_motion.faceDirection) {
					m_motion.direction = m_dragon.transform.position - (Vector3)m_motion.position;
				} else {
					Vector3 player = m_dragon.transform.position;
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
					break;
					
				case State.Attack:
					m_motion.velocity = Vector2.zero;
					m_timer = 0;
					break;
			}

			m_state = m_nextState;
		}
	}

	public void OnAttack() {
		// do stuff - this will be called from animation events "PreyAnimationEvents"

		if (m_projectilePrefab != null) {
			ProjectileBehaviour projectile = PoolManager.GetInstance(m_projectilePrefab.name).GetComponent<ProjectileBehaviour>();
			if (projectile != null) {
				projectile.Shoot(transform, m_damage);
			}
		} else {
			m_dragon.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, transform);
		}
	}
}
