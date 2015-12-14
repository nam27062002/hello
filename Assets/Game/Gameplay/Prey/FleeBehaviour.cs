using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public class FleeBehaviour : Initializable {

	private enum State {
		None = 0,
		Idle,
		Move,
		Afraid
	};

	[SerializeField] private bool m_canBeAfraid = false;

	private Transform m_dragonMouth;
			
	private PreyMotion m_motion;
	private Animator m_animator;
	private SensePlayer m_sensor;
	
	private State m_state;
	private State m_nextState;



	// Use this for initialization
	void Awake () {
		m_dragonMouth = InstanceManager.player.GetComponent<DragonMotion>().tongue;

		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_sensor = GetComponent<SensePlayer>();
	}
	
	public override void Initialize() {
		m_state = State.None;
		m_nextState = State.Idle;
	}

	void OnEnable() {
		m_state = State.None;
		m_nextState = State.Idle;
	}

	void OnDisable() {
		m_nextState = State.None;
		ChangeState();
	}

	void Update() {
		if (m_state != m_nextState) {
			ChangeState();
		}

		if (m_state == State.Move) {
			if (!m_area.Contains(m_motion.position)) {
				if (m_canBeAfraid) {
					m_nextState = State.Afraid;
				} else {
					m_nextState = State.Idle;
				}
			}
		}

		if (m_state == State.Idle) {			
			if (m_sensor.alert && m_area.Contains(m_motion.position)) {
				m_nextState = State.Move;
			}
		} else {
			if (!m_sensor.alert) {
				m_nextState = State.Idle;
			}
		}
	}

	// Update is called once per frame
	void FixedUpdate() {
		switch (m_state) {
			case State.Move:
				if (m_sensor.alert) {
					m_motion.Flee(m_dragonMouth.position);
				}
				break;

			case State.Afraid:
				Vector3 player = m_dragonMouth.position;
				if (player.x < m_motion.position.x) {
					m_motion.direction = Vector2.left;
				} else {
					m_motion.direction = Vector2.right;
				}
				break;
		}
	}

	private void ChangeState() {
		// exit State
		switch (m_state) {
			case State.Move:
				m_animator.SetBool("move", false);
				break;
				
			case State.Afraid:
				m_animator.SetBool("scared", false);
				break;
		}
		
		// enter State
		switch (m_nextState) {
			case State.Move:
				m_animator.SetBool("move", true);
				break;
				
			case State.Afraid:
				m_animator.SetBool("scared", true);
				m_motion.velocity = Vector2.zero;
				break;

			case State.Idle:
				m_motion.velocity = Vector2.zero;
				break;
		}
		
		m_state = m_nextState;
	}
}
