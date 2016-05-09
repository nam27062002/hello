using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public abstract class AttackBehaviour : Initializable {
	
	// Constants
	public enum State {
		None = 0,
		Idle,
		Pursuit,
		Attack,
		AttackRetreat,
		BeingHeld
	};

	[SerializeField] protected float m_damage;
	[SerializeField] private float m_attackDelay;
	[SerializeField] private int m_consecutiveAttacks;
	[SerializeField] private float m_retreatingTime;
	[SerializeField] private bool m_hasAnimation = true;
	[SerializeField] private bool m_invulnerableWhileAttacking = false;

	protected Animator m_animator;
	protected PreyMotion m_motion;
	protected PreyOrientation m_orientation;
	protected SensePlayer m_sensor;
	protected DragonMotion m_dragon;
	protected Transform m_target; // all the attacks will aim to this target

	private EdibleBehaviour m_edible;

	private bool m_playingAttackAnimation;
	private bool m_onAttachEventDone;
	private bool m_onDamageEventDone;
	private bool m_onAttackEndEventDone;

	private State m_state;
	private State m_nextState;
	public State state { get { return m_state; } }

	private float m_timer;
	private int m_attackCount;


	// Use this for initialization
	protected virtual void Start () {
		m_motion = GetComponent<PreyMotion>();
		m_orientation = GetComponent<PreyOrientation>();
		m_sensor = GetComponent<SensePlayer>();
		m_dragon = InstanceManager.player.GetComponent<DragonMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		m_edible = GetComponent<EdibleBehaviour>();
	

		m_playingAttackAnimation = false;
		m_onAttachEventDone = false;
     	m_onDamageEventDone = false;
     	m_onAttackEndEventDone = false;

		m_target = m_dragon.GetAttackPointNear(transform.position);
		m_sensor.dragonTarget = m_target;

		PreyAnimationEvents animEvents = transform.FindChild("view").GetComponent<PreyAnimationEvents>();
		if (animEvents != null) {
			animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAttack);
			animEvents.onAttackEnd += new PreyAnimationEvents.OnAttackEndDelegate(OnAttackEnd);
			animEvents.onAttachProjectile += new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
		} else {
			m_hasAnimation = false;
		}
	}

	void OnDestroy() {
		Transform view = transform.FindChild("view");
		if (view != null) {
			PreyAnimationEvents animEvents = view.GetComponent<PreyAnimationEvents>();
			if (animEvents != null) {
				animEvents.onAttackDealDamage -= new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAttack);
				animEvents.onAttackEnd -= new PreyAnimationEvents.OnAttackEndDelegate(OnAttackEnd);
				animEvents.onAttachProjectile -= new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
			}
		}
	}

	public override void Initialize() {
		m_state = State.None;
		m_nextState = State.Pursuit;
		m_attackCount = 0;
	}

	void OnEnable() {
		m_state = State.None;
		m_nextState = State.Pursuit;

		if (m_dragon != null) {
			m_target = m_dragon.GetAttackPointNear(transform.position);
			m_sensor.dragonTarget = m_target;
		}
	}

	protected virtual void OnDisable() {
		if (m_animator && m_animator.isInitialized) {
			m_animator.SetBool("move", false);
			m_animator.SetBool("fast", false);
			m_animator.SetBool("attack", false);
		}

		if (m_edible != null) m_edible.enabled = true;
	}

	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		switch (m_state) 
		{
			case State.Idle:
				if (!m_edible.IsBeingHeld())
				{
					m_motion.Stop();
					if (m_sensor.isInsideMaxArea) {
						if (m_area == null || m_area.Contains(transform.position)) {
							m_nextState = State.Pursuit;
						}
					}
				}
				else
				{
					m_nextState = State.BeingHeld;
				}
			break;

			case State.Pursuit:
				if ( !m_edible.IsBeingHeld() )
				{
					bool goToIdle = false;

					if (m_sensor.isInsideMaxArea) {
						if (m_sensor.isInsideMinArea) {
							m_nextState = State.Attack;
						} else {
							if (m_area != null && !m_area.Contains(transform.position)) {
								goToIdle = true;
							} else {
								m_motion.Pursuit(m_target.position, m_dragon.velocity, m_dragon.maxSpeed);
							}
						}
					} else {
						goToIdle = true;
					}

					if (goToIdle) {
						if (m_retreatingTime > 0) {
							m_sensor.Shutdown(m_retreatingTime);
						}
						m_nextState = State.Idle;
					}
				}
				else
				{
					m_nextState = State.BeingHeld;
				}
				break;
				
			case State.Attack:
				m_motion.Stop();
				if ( m_edible.IsBeingHeld() )
				{
					m_nextState = State.BeingHeld;
				}
				else
				{
					m_timer -= Time.deltaTime;
					if (m_timer <= 0) {
						m_timer = 0;

						if (!m_playingAttackAnimation) {
							if (m_sensor.isInsideMinArea) {
								//do attack
								m_playingAttackAnimation = true;

								m_onAttachEventDone = false;
		                       	m_onDamageEventDone = false;
		                       	m_onAttackEndEventDone = false;

								OnAttackStart_Extended();

								if (m_hasAnimation) {
									m_animator.SetBool("attack", true);
								} else {
									m_dragon.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, transform);
									OnAttackEnd();
								}
								m_timer = m_attackDelay;
							} else {
								m_animator.SetBool("attack", false);
								if (m_sensor.isInsideMaxArea) {
									m_nextState = State.Pursuit;
								} else {
									m_nextState = State.Idle;
								}
							}
						}
					}
				}
				break;
			case State.BeingHeld:
			{
				if (!m_edible.IsBeingHeld())
					m_nextState = State.Idle;
			}break;
		}

		UpdateOrientation();
	}
		
	protected virtual void UpdateOrientation() {
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
	}


	//------------------------------------------------------------------------------------------------------------------------------------------------------------//
	//
	//------------------------------------------------------------------------------------------------------------------------------------------------------------//

	private void ChangeState() {
		if (m_state != m_nextState) {
			switch (m_state) {
				case State.Pursuit:
					m_animator.SetBool("move", false);
					m_animator.SetBool("fast", false);
					break;

				case State.Attack:
					m_animator.SetBool("attack", false);
					if (m_edible != null && m_invulnerableWhileAttacking) 
						m_edible.enabled = true;
					break;
				case State.BeingHeld:
				{
					m_animator.SetBool("hold", false);
				}break;
			}

			switch (m_nextState) {
				case State.Pursuit:
					m_animator.SetBool("move", true);
					m_animator.SetBool("fast", true);
					m_attackCount = 0;
					break;
					
				case State.Attack:
					m_playingAttackAnimation = false;
					if (m_edible != null && m_invulnerableWhileAttacking) 
						m_edible.enabled = false;

					m_motion.Stop();
					m_timer = 0;
					break;
				case State.BeingHeld:
				{
					m_animator.SetBool("hold", true);
				}break;
			}

			m_state = m_nextState;
		}
	}


	//------------------------------------------------------------------------------------------------------------------------------------------------------------//
	// Events
	//------------------------------------------------------------------------------------------------------------------------------------------------------------//

	public void OnAttachProjectile() {
		// do stuff - this will be called from animation events "PreyAnimationEvents"
		if (!m_onAttachEventDone) {
			
			OnAttachProjectile_Extended();

			m_onAttachEventDone = true;
		}
	}

	public void OnAttack() {
		// do stuff - this will be called from animation events "PreyAnimationEvents"
		if (!m_onDamageEventDone) {

			OnAttack_Extended();

			m_onDamageEventDone = true;
		}
	}

	public void OnAttackEnd() {		
		// do stuff - this will be called from animation events "PreyAnimationEvents"
		if (!m_onAttackEndEventDone) {

			OnAttackEnd_Extended();

			if (m_consecutiveAttacks > 0) {
				m_attackCount++;
				if (m_attackCount >= m_consecutiveAttacks) {
					m_animator.SetBool("attack", false);
					if (m_retreatingTime > 0) {
						m_sensor.Shutdown(m_retreatingTime);
					}
					m_nextState = State.Idle;
				}
			}

			// if this prey has to wait more before attacking again, stop the animation
			if (m_timer > 0) {
				m_animator.SetBool("attack", false);
			}

			m_playingAttackAnimation = false;
			m_onAttackEndEventDone = true;
		}
	}

	protected abstract void OnAttackStart_Extended();
	protected abstract void OnAttachProjectile_Extended();
	protected abstract void OnAttack_Extended();
	protected abstract void OnAttackEnd_Extended();
}
