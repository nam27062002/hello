using UnityEngine;
using System.Collections;

public class SoldierTactics : Initializable {

	private enum State {
		None = 0,
		Wander,
		Attack
	};

	private FollowPathBehaviour m_wander;
	private AttackBehaviour m_attack;
	private SensePlayer m_sensor;

	private State m_state;
	private State m_nextState;


	public override void Initialize() {
		m_wander = GetComponent<FollowPathBehaviour>();
		m_attack = GetComponent<AttackBehaviour>();
		m_sensor = GetComponent<SensePlayer>();

		m_attack.enabled = false;
		m_wander.enabled = false;

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
				m_sensor.dragonTarget = InstanceManager.player.GetComponent<DragonMotion>().GetAttackPointNear(transform.position);

				m_attack.enabled = false;
				m_wander.enabled = true;
				break;

			case State.Attack:
				m_attack.enabled = true;
				m_wander.enabled = false;
				break;
		}

		m_state = m_nextState;
	}
}
