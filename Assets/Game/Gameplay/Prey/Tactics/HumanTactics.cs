using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Human Tactics")]
public class HumanTactics : Initializable {

	private enum State {
		Wander = 0,
		Afraid
	};

	private State m_state;
	private State m_nextState;

	private float m_timer;
	private bool m_playerDetected;

	private PreyMotion m_motion;
	private Animator m_animator;
	private SensePlayer m_sensor;
	private Transform m_dragon;

	

	public override void Initialize() {

		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_sensor = GetComponent<SensePlayer>();
		m_dragon = InstanceManager.player.transform;

		//start at random anim position
		ChangeState(State.Wander);

		m_playerDetected = false;
	}

	void Update() {
		bool playerDetected = m_sensor.alert;

		switch (m_state) {
			case State.Wander:
				if (m_playerDetected) {
					if (!playerDetected || !m_area.Contains(m_motion.position)) {
						ChangeState(State.Afraid);
					}
				} 

				m_playerDetected = playerDetected;
				break;

			case State.Afraid:
				Vector3 player = m_dragon.position;
				if (player.x < m_motion.position.x) {
					m_motion.direction = Vector2.left;
				} else {
					m_motion.direction = Vector2.right;
				}

				if (!playerDetected) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0) {
						ChangeState(State.Wander);
					}
				}
				break;
		}
	}

	private void ChangeState(State _newSate) {

		if (m_state != _newSate) {
			// exit State
			switch (m_state) {
				case State.Wander:
					GetComponent<WanderBehaviour>().enabled = false;
					break;
										
				case State.Afraid:
					m_animator.SetBool("scared", false);
					break;
			}

			// enter State
			switch (_newSate) {
				case State.Wander:
					GetComponent<WanderBehaviour>().enabled = true;
					break;

				case State.Afraid:
					m_animator.SetBool("scared", true);
					m_motion.Stop();
					m_timer = 5f; // 5 sconds before wandering around
					break;
			}

			m_state = _newSate;
		}
	}
}