using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Drone Tactics")]
[RequireComponent(typeof(FollowPathBehaviour))]
[RequireComponent(typeof(AttackBehaviour))]
public class DroneTactics : Initializable {

	private enum State {
		None = 0,
		FollowPath,
		Attack
	};

	private State m_state;
	private State m_nextState;
	private SensePlayer m_sensor;

	private float m_timer;

	// Use this for initialization
	void Start () {

	}

	public override void Initialize() {
		//start at random anim position
		m_state = State.None;
		m_nextState = State.FollowPath;
		
		GetComponent<FollowPathBehaviour>().enabled = false;
		GetComponent<AttackBehaviour>().enabled = false;

		m_sensor = GetComponent<SensePlayer>();
		m_sensor.enabled = true;

		m_timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		switch (m_state) {
			case State.FollowPath:
				if (m_timer > 0) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0) {
						m_timer = 0;
						m_sensor.enabled = true;
					}
				}
				if (m_sensor.isInsideMaxArea) {
					m_nextState = State.Attack;
				}
				break;

			case State.Attack:
				if (!m_area.Contains(transform.position)) {
					m_sensor.enabled = false;
					m_timer = 5f;
					m_nextState = State.FollowPath;
				}

				if (!m_sensor.isInsideMaxArea) {
					m_nextState = State.FollowPath;
				}
				break;
		}
	}

	private void ChangeState() {
		// exit State
		switch (m_state) {
			case State.FollowPath:
				GetComponent<FollowPathBehaviour>().enabled = false;
				break;
				
			case State.Attack:
				GetComponent<AttackBehaviour>().enabled = false;
				break;
		}
		
		// enter State
		switch (m_nextState) {
			case State.FollowPath:
				GetComponent<FollowPathBehaviour>().enabled = true;
				break;
				
			case State.Attack:
				GetComponent<AttackBehaviour>().enabled = true;
				break;
		}
		
		m_state = m_nextState;
	}
}
