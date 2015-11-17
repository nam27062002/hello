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

	// Use this for initialization
	void Start () {
		m_sensor = GetComponent<SensePlayer>();
	}

	public override void Initialize() {
		//start at random anim position
		m_state = State.None;
		m_nextState = State.FollowPath;
		
		GetComponent<FollowPathBehaviour>().enabled = false;
		GetComponent<AttackBehaviour>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		if (m_sensor.isInsideMaxArea) {
			m_nextState = State.Attack;
		} else {
			m_nextState = State.FollowPath;
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
