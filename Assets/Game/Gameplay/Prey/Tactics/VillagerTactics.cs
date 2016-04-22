using UnityEngine;
using System.Collections;

public class VillagerTactics : Initializable {

	private enum State {
		None = 0,
		Wander,
		Flee
	};

	private FollowPathBehaviour m_wander;
	private FleePathBehaviour m_flee;
	private SensePlayer m_sensor;

	private State m_state;
	private State m_nextState;


	public override void Initialize() {
		m_wander = GetComponent<FollowPathBehaviour>();
		m_flee = GetComponent<FleePathBehaviour>();
		m_sensor = GetComponent<SensePlayer>();

		m_flee.enabled = false;
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
					m_nextState = State.Flee;
				}
				break;

			case State.Flee:
				if (!m_sensor.isInsideMaxArea) {
					m_nextState = State.Wander;
				}
				break;
		}
	}

	private void ChangeState() {
		switch (m_nextState) {
			case State.Wander:
				m_flee.enabled = false;
				m_wander.enabled = true;
				break;

			case State.Flee:
				m_flee.enabled = true;
				m_wander.enabled = false;
				break;
		}

		m_state = m_nextState;
	}
}
