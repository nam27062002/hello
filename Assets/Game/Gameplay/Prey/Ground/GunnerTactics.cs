using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Behaviour/Prey/Gunner Tactics")]
[RequireComponent(typeof(WanderBehaviour))]
[RequireComponent(typeof(AttackBehaviour))]
public class GunnerTactics : Initializable {

	private enum State {
		None = 0,
		Wander,
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
		m_nextState = State.Wander;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_state != m_nextState) {
			ChangeState();
		}

		if (m_sensor.alert) {
			m_nextState = State.Attack;
		} else {
			m_nextState = State.Wander;
		}
	}

	private void ChangeState() {
		// exit State
		switch (m_state) {
			case State.Wander:
				GetComponent<WanderBehaviour>().enabled = false;
				break;
				
			case State.Attack:
				GetComponent<AttackBehaviour>().enabled = false;
				break;
		}
		
		// enter State
		switch (m_nextState) {
			case State.Wander:
				GetComponent<WanderBehaviour>().enabled = true;
				break;
				
			case State.Attack:
				GetComponent<AttackBehaviour>().enabled = true;
				break;
		}
		
		m_state = m_nextState;
	}
}
