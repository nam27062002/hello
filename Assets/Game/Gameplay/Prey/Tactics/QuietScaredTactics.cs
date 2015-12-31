using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Quiet and Scared Tactics")]
[RequireComponent(typeof(FleeBehaviour))]
public class QuietScaredTactics : Initializable {

	private enum State {
		None = 0,
		Idle,
		Scared
	};

	private State m_state;
	private State m_nextState;

	private SensePlayer m_sensor;
	private PreyMotion m_motion;
	private FleeBehaviour m_fleeBehaviour;


	public override void Initialize() {
		m_motion = GetComponent<PreyMotion>();
		m_sensor = GetComponent<SensePlayer>();
		m_fleeBehaviour = GetComponent<FleeBehaviour>();
		m_fleeBehaviour.enabled = false;

		m_state = State.None;
		m_nextState = State.Idle;
	}

	void Update() {
		if (m_state != m_nextState) {
			ChangeState();
		}

		switch (m_state) {
			case State.Idle:
				if (m_sensor.alert) {
					m_nextState = State.Scared;
				}
				m_motion.Stop();
				break;

			case State.Scared:
				if (!m_sensor.alert) {
					m_nextState = State.Idle;
				}
				break;
		}
	}

	private void ChangeState() {		
		// exit State
		switch (m_state) {
			case State.Idle:
				break;
									
			case State.Scared:
				m_fleeBehaviour.enabled = false;
				break;
		}

		// enter State
		switch (m_nextState) {
			case State.Idle:
				break;

			case State.Scared:
				m_fleeBehaviour.enabled = true;
				break;
		}

		m_state = m_nextState;
	}
}