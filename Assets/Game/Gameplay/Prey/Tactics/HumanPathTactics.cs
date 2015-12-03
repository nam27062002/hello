using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Human Path Tactics")]
[RequireComponent(typeof(FleeBehaviour))]
[RequireComponent(typeof(FollowPathBehaviour))]
public class HumanPathTactics : Initializable {

	private enum State {
		Wander = 0,
		Flee
	};

	private FollowPathBehaviour m_followPathBehaviour;
	private FleeBehaviour m_fleeBehaviour;

	private State m_state;
	private State m_nextState;
	
	private SensePlayer m_sensor;
		

	public override void Initialize() {
		m_sensor = GetComponent<SensePlayer>();

		m_followPathBehaviour = GetComponent<FollowPathBehaviour>();
		m_fleeBehaviour = GetComponent<FleeBehaviour>();

		//start at random anim position
		m_state = State.Flee;
		ChangeState(State.Wander);
	}

	void Update() {
		bool playerDetected = m_sensor.alert;

		switch (m_state) {
			case State.Wander:
				if (playerDetected) {
					ChangeState(State.Flee);
				}
				break;

			case State.Flee:
				if (!playerDetected) {
					ChangeState(State.Wander);
				}
				break;
		}
	}

	private void ChangeState(State _newSate) {

		if (m_state != _newSate) {
			// exit State
			switch (m_state) {
				case State.Wander:
					GetComponent<FollowPathBehaviour>().enabled = false;
					break;

				case State.Flee:
					GetComponent<FleeBehaviour>().enabled = false;
					break;
			}

			// enter State
			switch (_newSate) {
				case State.Wander:
					GetComponent<FollowPathBehaviour>().enabled = true;
					break;

				case State.Flee:
					GetComponent<FleeBehaviour>().enabled = true;
					break;
			}

			m_state = _newSate;
		}
	}
}