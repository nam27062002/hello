using UnityEngine;
using System.Collections;

public class AttackBehaviour : Initializable {

	private enum State {
		None = 0,
		Pursuit,
		Attack
	};

	[SerializeField] private float m_damage;
	[SerializeField] private float m_attackDelay;

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
				break;

			case State.Attack:
				m_motion.velocity = Vector2.zero;

				// should walk while shooting?
				Vector3 player = m_dragon.transform.position;
				if (player.x < m_motion.position.x) {
					m_motion.direction = Vector2.left;
				} else {
					m_motion.direction = Vector2.right;
				}
				break;
		}

		m_motion.ApplySteering();
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
		m_dragon.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, transform);

		/* Move this to the Tactics layer
		Vector3 dir = m_gunShot.position - m_gun.position;
		dir.Normalize();

		GameObject spark = PoolManager.GetInstance("PF_Spark");
		spark.transform.position = m_gunShot.position + (dir * 0.1f);
		spark.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector3.Angle(dir, Vector3.up)));
		*/
	}
}
