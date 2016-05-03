using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Dragon Tactics")]
[RequireComponent(typeof(WanderBehaviour))]
[RequireComponent(typeof(AttackBehaviour))]
[RequireComponent(typeof(EvadeBehaviour))]
public class DragonTactics : Initializable {

	private enum State {
		None = 0,
		Wander,
		Attack
	};

	private State m_state;
	private State m_nextState;

	private WanderBehaviour m_wander;
	private AttackBehaviour m_attack;
	private EvadeBehaviour m_evade;
	private SensePlayer m_sensor;


	public override void Initialize() {
		m_wander = GetComponent<WanderBehaviour>();
		m_attack = GetComponent<AttackBehaviour>();
		m_evade = GetComponent<EvadeBehaviour>();
		m_sensor = GetComponent<SensePlayer>();

		m_attack.enabled = false;
		m_wander.enabled = false;
		m_evade.enabled = false;

		m_state = State.None;
		m_nextState = State.Wander;
	}

	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		switch (m_nextState) {
			case State.Wander:
				if (m_sensor.isInsideMaxArea) {
					m_nextState = State.Attack;
				}
				break;

			case State.Attack:
				if (!m_sensor.isInsideMaxArea) {
					m_nextState = State.Wander;
				}
				break;
		}
	}

	private void ChangeState() {
		switch (m_nextState) {
			case State.Wander:
				m_attack.enabled = false;
				m_wander.enabled = true;
				m_evade.enabled = true;
				break;

			case State.Attack:
				m_attack.enabled = true;
				m_wander.enabled = false;
				m_evade.enabled = false;
				break;
		}

		m_state = m_nextState;
	}
}
